using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Chivas.UI.ViewModels;
using Chivas.Utility;

namespace Chivas.UI.Elements.Settings.Pages
{
    public class ExtensionViewModel : NotifyPropertyChangedViewModel
    {
        public const string TypeAll = "All";
        public const string TypeExtensions = "Roblox Extensions";
        public const string TypeStudio = "Roblox Studio Extensions";

        private static readonly HttpClient client = CreateClient();

        private static HttpClient CreateClient()
        {
            var http = Chivas.Utility.VpnHttpClient.Create(TimeSpan.FromMinutes(10));
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Chivas");
            return http;
        }

        private static string fleasionDir => Paths.Fleasion;
        private const string ApiDumpSha256 = "C79D898BCEA32693BDB96100DE01F47F9CD018A0CCB8346C038E92DC2A1F6FB8";
        private const long MaxDownloadBytes = 268435456L;

        private static string apiDumpDir => Paths.ApiDumpTool;

        private static readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private static CancellationTokenSource? _activeCts;
        private static readonly object CancelSync = new();
        private static readonly HashSet<Action> CancelActions = [];
        private string _selectedExtensionType = TypeAll;

        private int _fleasionRev;
        private int _apiDumpRev;
        private const int ToggleDebounceMs = 450;
        private static volatile bool _userCancel;

        public event Action<string, double, bool> OnProgressChanged;

        public static event Action<string, double, bool>? AnyProgressChanged;

        public static void ReportProgress(string title, double fraction, bool show)
        {
            AnyProgressChanged?.Invoke(title, fraction, show);
        }

        public static void CancelActiveDownload()
        {
            _userCancel = true;
            try
            {
                _activeCts?.Cancel();
            }
            catch
            {
            }
            Action[] actions;
            lock (CancelSync)
            {
                actions = CancelActions.ToArray();
            }
            foreach (Action action in actions)
            {
                try
                {
                    action();
                }
                catch
                {
                }
            }
        }

        public static void RegisterCancelAction(Action action)
        {
            lock (CancelSync)
            {
                CancelActions.Add(action);
            }
        }

        public static void UnregisterCancelAction(Action action)
        {
            lock (CancelSync)
            {
                CancelActions.Remove(action);
            }
        }

        private void RelayProgress(string title, double fraction, bool show)
        {
            AnyProgressChanged?.Invoke(title, fraction, show);
        }

        private static ExtensionViewModel? _current;

        public static ExtensionViewModel EnsureCurrent()
        {
            return _current ??= new ExtensionViewModel();
        }

        /// <summary>
        /// While the Fleasion app is the selected delivery engine, turning the
        /// master toggle off must only stop managing it, never wipe its binary:
        /// uninstalling the engine the rules depend on silently breaks every
        /// replacement on the page.
        /// </summary>
        private static bool CanUninstallFleasion()
        {
            return !Chivas.UI.ViewModels.Settings.FleasionViewModel.UseFleasionProxyOnly;
        }

        public ExtensionViewModel()
        {
            _current ??= this;
            OnProgressChanged += RelayProgress;
            if (App.Settings.Prop.Fleasion && !App.Settings.Prop.FleasionNativeEngineEnabled)
            {
                string exePath = Path.Combine(fleasionDir, "Fleasion.exe");
                if (!File.Exists(exePath))
                    _ = DownloadFleasionAsync();
                else if (!IsFleasionRunning())
                {
                    SeedFleasionConfig();
                    LaunchFleasionSilent();
                }
            }
            else if (CanUninstallFleasion())
            {
                _ = UninstallFleasionAsync();
            }

            if (App.Settings.Prop.RojoEnabled && !Chivas.Integrations.Rojo.RojoManager.IsInstalled)
                _ = DownloadRojoAsync();
        }

        public ObservableCollection<string> ExtensionTypes { get; } = new ObservableCollection<string>
        {
            TypeAll, TypeExtensions, TypeStudio
        };

        public ObservableCollection<string> ExtensionStatuses { get; } = new ObservableCollection<string>
        {
            Chivas.Resources.Strings.Menu_Extensions_Page_StatusAll,
            Chivas.Resources.Strings.Menu_Extensions_Page_StatusEnabled,
            Chivas.Resources.Strings.Menu_Extensions_Page_StatusDisabled
        };

        public ObservableCollection<string> ExtensionSortOptions { get; } = new ObservableCollection<string>
        {
            Chivas.Resources.Strings.Menu_Extensions_Page_SortDefault,
            Chivas.Resources.Strings.Menu_Extensions_Page_SortNameAsc,
            Chivas.Resources.Strings.Menu_Extensions_Page_SortNameDesc,
            Chivas.Resources.Strings.Menu_Extensions_Page_SortEnabledFirst
        };

        private string _selectedExtensionSort = "";

        public string SelectedExtensionSort
        {
            get => string.IsNullOrEmpty(_selectedExtensionSort) ? ExtensionSortOptions[0] : _selectedExtensionSort;
            set
            {
                string next = value ?? ExtensionSortOptions[0];
                if (_selectedExtensionSort == next)
                    return;
                _selectedExtensionSort = next;
                OnPropertyChanged();
            }
        }

        private bool _isGridView = true;

        public bool IsGridView
        {
            get => _isGridView;
            set
            {
                if (_isGridView == value)
                    return;
                _isGridView = value;
                OnPropertyChanged();
            }
        }

        private int _statEnabledCount;

        public int StatEnabledCount
        {
            get => _statEnabledCount;
            private set
            {
                if (_statEnabledCount == value)
                    return;
                _statEnabledCount = value;
                OnPropertyChanged();
            }
        }

        private int _statDisabledCount;

        public int StatDisabledCount
        {
            get => _statDisabledCount;
            private set
            {
                if (_statDisabledCount == value)
                    return;
                _statDisabledCount = value;
                OnPropertyChanged();
            }
        }

        private int _statTotalCount;

        public int StatTotalCount
        {
            get => _statTotalCount;
            private set
            {
                if (_statTotalCount == value)
                    return;
                _statTotalCount = value;
                OnPropertyChanged();
            }
        }

        public void UpdateStats(int total, int enabled)
        {
            StatTotalCount = total;
            StatEnabledCount = enabled;
            StatDisabledCount = Math.Max(0, total - enabled);
        }

        private string _selectedExtensionStatus = "";

        public string SelectedExtensionStatus
        {
            get => string.IsNullOrEmpty(_selectedExtensionStatus) ? ExtensionStatuses[0] : _selectedExtensionStatus;
            set
            {
                string next = value ?? ExtensionStatuses[0];
                if (_selectedExtensionStatus == next)
                    return;
                _selectedExtensionStatus = next;
                OnPropertyChanged();
                RaiseVisibilities();
            }
        }

        public string SelectedExtensionType
        {
            get => _selectedExtensionType;
            set
            {
                if (string.IsNullOrEmpty(value) || _selectedExtensionType == value)
                    return;
                _selectedExtensionType = value;
                OnPropertyChanged();
                RaiseVisibilities();
            }
        }

        private string _searchText = "";

        public string SearchText
        {
            get => _searchText;
            set
            {
                string next = value ?? "";
                if (_searchText == next)
                    return;
                _searchText = next;
                OnPropertyChanged();
                RaiseVisibilities();
            }
        }

        private void RaiseVisibilities()
        {
            OnPropertyChanged(nameof(FleasionVisibility));
            OnPropertyChanged(nameof(RiShadeVisibility));
            OnPropertyChanged(nameof(ApiDumpVisibility));
            OnPropertyChanged(nameof(RojoVisibility));
            OnPropertyChanged(nameof(EmptyStateVisibility));
            OnPropertyChanged(nameof(EmptyStateTitle));
            OnPropertyChanged(nameof(EmptyStateSubtitle));
        }

        private bool ShowType(string type) => _selectedExtensionType == TypeAll || _selectedExtensionType == type;

        private bool MatchesSearch(string haystack)
        {
            string q = _searchText.Trim();
            return q.Length == 0 || haystack.Contains(q, StringComparison.OrdinalIgnoreCase);
        }

        private bool FleasionVisible => ShowType(TypeExtensions) && MatchesSearch("Fleasion replace Roblox game assets textures audio meshes animations dump Roblox Extension");

        private bool RiShadeVisible => ShowType(TypeExtensions) && MatchesSearch("RiShade shaders visuals effects bloom tonemap panel F8 Roblox Extension not recommended taking images");

        private bool ApiDumpVisible => ShowType(TypeExtensions) && MatchesSearch("Roblox API Dump Tool MaximumADHD api dump diff classes members enums Roblox");

        private bool RojoVisible => ShowType(TypeStudio) && MatchesSearch("Rojo filesystem sync studio project git version control serve build init plugin Roblox Studio Extensions");

        private bool AnyCardVisible => FleasionVisible || RiShadeVisible || ApiDumpVisible || RojoVisible;

        public Visibility FleasionVisibility => FleasionVisible ? Visibility.Visible : Visibility.Collapsed;

        public Visibility RiShadeVisibility => RiShadeVisible ? Visibility.Visible : Visibility.Collapsed;

        public Visibility EmptyStateVisibility => AnyCardVisible ? Visibility.Collapsed : Visibility.Visible;

        public string EmptyStateTitle
        {
            get
            {
                if (AnyCardVisible)
                    return "";
                string q = _searchText.Trim();
                if (q.Length > 0)
                {
                    if (q.Length > 40)
                        q = string.Concat(q.AsSpan(0, 40), "...");
                    return "No results for \"" + q + "\"";
                }
                return "Nothing to show here";
            }
        }

        public string EmptyStateSubtitle
        {
            get
            {
                if (AnyCardVisible)
                    return "";
                if (_searchText.Trim().Length > 0)
                    return "Try a different search, or change the type filter.";
                return "Change the type filter to see more extensions.";
            }
        }

        private string _riShadeOpenLabel = Chivas.Integrations.RiShade.RiShadePanel.IsOpen ? "Close" : "Open";

        public string RiShadeOpenLabel
        {
            get => _riShadeOpenLabel;
            set
            {
                if (_riShadeOpenLabel == value)
                    return;
                _riShadeOpenLabel = value;
                OnPropertyChanged();
            }
        }

        public bool rishadeenabler
        {
            get => App.Settings.Prop.RiShadeEnabled;
            set
            {
                if (App.Settings.Prop.RiShadeEnabled == value)
                    return;
                Chivas.Integrations.RiShade.RiShadeManager.SetEnabled(value);
                OnPropertyChanged();
            }
        }

        public Visibility ApiDumpVisibility => ApiDumpVisible ? Visibility.Visible : Visibility.Collapsed;

        public Visibility RojoVisibility => RojoVisible ? Visibility.Visible : Visibility.Collapsed;

        private int _rojoRev;

        public bool rojoenabler
        {
            get => App.Settings.Prop.RojoEnabled;
            set
            {
                if (App.Settings.Prop.RojoEnabled == value)
                    return;
                App.Settings.Prop.RojoEnabled = value;
                App.Settings.Save();
                OnPropertyChanged();
                _ = DebounceRojoAsync();
            }
        }

        public string RojoProjectPath
        {
            get => App.Settings.Prop.RojoProjectPath ?? "";
            set
            {
                string next = value ?? "";
                if ((App.Settings.Prop.RojoProjectPath ?? "") == next)
                    return;
                App.Settings.Prop.RojoProjectPath = next;
                App.Settings.Save();
                OnPropertyChanged();
            }
        }

        public string RojoServeLabel => Chivas.Integrations.Rojo.RojoManager.IsServing ? Chivas.Resources.Strings.Menu_Extensions_Page_RojoServeStop : Chivas.Resources.Strings.Menu_Extensions_Page_RojoServeStart;

        private async Task DebounceRojoAsync()
        {
            int rev = Interlocked.Increment(ref _rojoRev);
            if (!App.Settings.Prop.RojoEnabled)
                StopAllDownloads();
            try { await Task.Delay(ToggleDebounceMs); } catch { }
            if (rev != Volatile.Read(ref _rojoRev))
                return;
            if (App.Settings.Prop.RojoEnabled)
                await DownloadRojoAsync();
            else
                await UninstallRojoAsync();
        }

        private void RevertRojoToggle()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (App.Settings.Prop.RojoEnabled)
                {
                    App.Settings.Prop.RojoEnabled = false;
                    App.Settings.Save();
                    OnPropertyChanged(nameof(rojoenabler));
                }
            });
        }

        private async Task DownloadRojoAsync()
        {
            var (cts, ct) = BeginOperation();
            try
            {
                await _gate.WaitAsync(ct);
                try
                {
                    await Chivas.Integrations.Rojo.RojoManager.EnsureInstalledAsync(
                        (t, f, s) => OnProgressChanged?.Invoke(t, f, s), ct);
                    OnProgressChanged?.Invoke("Rojo ready", 1.0, true);
                    await Task.Delay(700, ct);
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (OperationCanceledException)
            {
                if (_userCancel)
                    RevertRojoToggle();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_RojoDlFail, ex.Message), MessageBoxImage.Error);
                RevertRojoToggle();
            }
            finally
            {
                OnProgressChanged?.Invoke("", -1.0, false);
                EndOperation(cts);
            }
        }

        private async Task UninstallRojoAsync()
        {
            var (cts, ct) = BeginOperation();
            try
            {
                await _gate.WaitAsync(ct);
                try
                {
                    OnProgressChanged?.Invoke("Removing Rojo", -1.0, true);
                    await Chivas.Integrations.Rojo.RojoManager.UninstallAsync();
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_RojoUninstFail, ex.Message), MessageBoxImage.Error);
            }
            finally
            {
                OnProgressChanged?.Invoke("", -1.0, false);
                EndOperation(cts);
                OnPropertyChanged(nameof(RojoServeLabel));
            }
        }

        private bool EnsureRojoReady()
        {
            if (!Chivas.Integrations.Rojo.RojoManager.IsInstalled)
            {
                Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_RojoEnableFirst, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(RojoProjectPath) || !Directory.Exists(RojoProjectPath))
            {
                Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_RojoChooseFolder, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        public void PickRojoFolder()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFolderDialog { Title = Strings.Menu_Dialogs_Code_RojoChooseFolderTitle };
                if (!string.IsNullOrWhiteSpace(RojoProjectPath) && Directory.Exists(RojoProjectPath))
                    dialog.InitialDirectory = RojoProjectPath;
                if (dialog.ShowDialog() == true)
                    RojoProjectPath = dialog.FolderName;
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_RojoFolderFail, ex.Message), MessageBoxImage.Error);
            }
        }

        public async Task RojoInitAsync()
        {
            if (!EnsureRojoReady())
                return;
            var (cts, ct) = BeginOperation();
            try
            {
                OnProgressChanged?.Invoke("Creating Rojo project", -1.0, true);
                var (ok, output) = await Chivas.Integrations.Rojo.RojoManager.RunAsync("init", RojoProjectPath, ct);
                OnProgressChanged?.Invoke("", -1.0, false);
                if (ok)
                    Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_RojoCreated, RojoProjectPath), MessageBoxImage.Information);
                else
                    Frontend.ShowMessageBox("rojo init did not complete:\n" + output, MessageBoxImage.Warning);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox("rojo init failed: " + ex.Message, MessageBoxImage.Error);
            }
            finally
            {
                OnProgressChanged?.Invoke("", -1.0, false);
                EndOperation(cts);
            }
        }

        public void RojoToggleServe()
        {
            if (Chivas.Integrations.Rojo.RojoManager.IsServing)
            {
                Chivas.Integrations.Rojo.RojoManager.StopServe();
                OnPropertyChanged(nameof(RojoServeLabel));
                return;
            }
            if (!EnsureRojoReady())
                return;
            bool started = Chivas.Integrations.Rojo.RojoManager.StartServe(RojoProjectPath);
            if (!started)
                Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_RojoServeFail, MessageBoxImage.Error);
            OnPropertyChanged(nameof(RojoServeLabel));
        }

        public async Task RojoBuildAsync()
        {
            if (!EnsureRojoReady())
                return;
            var (cts, ct) = BeginOperation();
            try
            {
                OnProgressChanged?.Invoke("Building place with Rojo", -1.0, true);
                string outFile = Path.Combine(RojoProjectPath, "build.rbxlx");
                var (ok, output) = await Chivas.Integrations.Rojo.RojoManager.RunAsync($"build -o \"{outFile}\"", RojoProjectPath, ct);
                OnProgressChanged?.Invoke("", -1.0, false);
                if (ok)
                    Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_RojoBuilt, outFile), MessageBoxImage.Information);
                else
                    Frontend.ShowMessageBox("rojo build did not complete:\n" + output, MessageBoxImage.Warning);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox("rojo build failed: " + ex.Message, MessageBoxImage.Error);
            }
            finally
            {
                OnProgressChanged?.Invoke("", -1.0, false);
                EndOperation(cts);
            }
        }

        public async Task RojoInstallPluginAsync()
        {
            if (!Chivas.Integrations.Rojo.RojoManager.IsInstalled)
            {
                Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_RojoEnableFirst, MessageBoxImage.Warning);
                return;
            }
            var (cts, ct) = BeginOperation();
            try
            {
                OnProgressChanged?.Invoke("Installing the Rojo Studio plugin", -1.0, true);
                var (ok, output) = await Chivas.Integrations.Rojo.RojoManager.RunAsync("plugin install", RojoProjectPath, ct);
                OnProgressChanged?.Invoke("", -1.0, false);
                if (ok)
                    Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_RojoPluginInstalled, MessageBoxImage.Information);
                else
                    Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_RojoPluginFail, output), MessageBoxImage.Warning);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_RojoPluginFail2, ex.Message), MessageBoxImage.Error);
            }
            finally
            {
                OnProgressChanged?.Invoke("", -1.0, false);
                EndOperation(cts);
            }
        }

        public void OpenRojoFolder()
        {
            if (!string.IsNullOrWhiteSpace(RojoProjectPath) && Directory.Exists(RojoProjectPath))
                OpenFolder(RojoProjectPath);
            else
                Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_RojoChooseFolder, MessageBoxImage.Warning);
        }

        public bool apidumpenabler
        {
            get => App.Settings.Prop.RobloxApiDumpTool;
            set
            {
                if (App.Settings.Prop.RobloxApiDumpTool == value)
                    return;
                App.Settings.Prop.RobloxApiDumpTool = value;
                App.Settings.Save();
                OnPropertyChanged();
                _ = DebounceApiDumpAsync();
            }
        }

        private async Task DebounceApiDumpAsync()
        {
            int rev = Interlocked.Increment(ref _apiDumpRev);
            if (!App.Settings.Prop.RobloxApiDumpTool)
                StopAllDownloads();
            try { await Task.Delay(ToggleDebounceMs); } catch { }
            if (rev != Volatile.Read(ref _apiDumpRev))
                return;
            if (App.Settings.Prop.RobloxApiDumpTool)
                await DownloadApiDumpToolAsync();
            else
                await UninstallApiDumpToolAsync();
        }

        private void RevertApiDumpToggle()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (App.Settings.Prop.RobloxApiDumpTool)
                {
                    App.Settings.Prop.RobloxApiDumpTool = false;
                    App.Settings.Save();
                    OnPropertyChanged(nameof(apidumpenabler));
                }
            });
        }

        private async Task DownloadApiDumpToolAsync()
        {
            var (cts, ct) = BeginOperation();
            try
            {
                await _gate.WaitAsync(ct);
                try
                {
                    OnProgressChanged?.Invoke("Preparing Roblox API Dump Tool", -1.0, true);
                    Directory.CreateDirectory(apiDumpDir);
                    string outputPath = Path.Combine(apiDumpDir, "RobloxAPIDumpTool.exe");

                    await KillProcessAsync("RobloxAPIDumpTool", ct);

                    if (File.Exists(outputPath) && IsFileLocked(outputPath))
                    {
                        Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_ApiStillRunning, MessageBoxImage.Warning);
                        return;
                    }

                    var download = await ResolveApiDumpUrlsAsync(ct);
                    bool ok = await DownloadToFileAsync(download.Urls, outputPath, "Downloading Roblox API Dump Tool", ct, download.Digest);
                    if (ok)
                    {
                        OnProgressChanged?.Invoke("Roblox API Dump Tool ready", 1.0, true);
                        await Task.Delay(700, ct);
                    }
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (OperationCanceledException)
            {
                if (_userCancel)
                    RevertApiDumpToggle();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_ApiDlFail, ex.Message), MessageBoxImage.Error);
            }
            finally
            {
                OnProgressChanged?.Invoke("", -1.0, false);
                EndOperation(cts);
            }
        }

        private async Task UninstallApiDumpToolAsync()
        {
            var (cts, ct) = BeginOperation();
            try
            {
                await _gate.WaitAsync(ct);
                try
                {
                    if (!Directory.Exists(apiDumpDir))
                        return;

                    OnProgressChanged?.Invoke("Removing Roblox API Dump Tool", -1.0, true);
                    await KillProcessAsync("RobloxAPIDumpTool", ct);

                    bool deleted = false;
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            Directory.Delete(apiDumpDir, true);
                            deleted = true;
                            break;
                        }
                        catch
                        {
                            await Task.Delay(400, ct);
                        }
                    }

                    if (!deleted && Directory.Exists(apiDumpDir))
                    {
                        Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_ApiRemoveFail, MessageBoxImage.Warning);
                        OpenFolder(apiDumpDir);
                    }
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_ApiUninstFail, ex.Message), MessageBoxImage.Error);
            }
            finally
            {
                OnProgressChanged?.Invoke("", -1.0, false);
                EndOperation(cts);
            }
        }

        public bool fleasionenabler
        {
            get => App.Settings.Prop.Fleasion;
            set
            {
                if (App.Settings.Prop.Fleasion == value)
                    return;
                App.Settings.Prop.Fleasion = value;
                App.Settings.Save();
                OnPropertyChanged();
                _ = DebounceFleasionAsync();
            }
        }

        private async Task DebounceFleasionAsync()
        {
            int rev = Interlocked.Increment(ref _fleasionRev);
            if (!App.Settings.Prop.Fleasion)
                StopAllDownloads();
            try { await Task.Delay(ToggleDebounceMs); } catch { }
            if (rev != Volatile.Read(ref _fleasionRev))
                return;
            if (App.Settings.Prop.Fleasion)
                await DownloadFleasionAsync();
            else
                await UninstallFleasionAsync();
        }

        public void CancelDownload()
        {
            _userCancel = true;
            try
            {
                _activeCts?.Cancel();
            }
            catch
            {
            }
        }

        public void StopAllDownloads()
        {
            try
            {
                _activeCts?.Cancel();
            }
            catch
            {
            }
        }

        private static (CancellationTokenSource Cts, CancellationToken Token) BeginOperation()
        {
            _userCancel = false;
            var cts = new CancellationTokenSource();
            var old = Interlocked.Exchange(ref _activeCts, cts);
            try
            {
                old?.Cancel();
            }
            catch
            {
            }
            return (cts, cts.Token);
        }

        private static void EndOperation(CancellationTokenSource cts)
        {
            Interlocked.CompareExchange(ref _activeCts, null, cts);
            try
            {
                cts.Dispose();
            }
            catch
            {
            }
        }

        private void RevertFleasionToggle()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (App.Settings.Prop.Fleasion)
                {
                    App.Settings.Prop.Fleasion = false;
                    App.Settings.Save();
                    OnPropertyChanged(nameof(fleasionenabler));
                }
            });
        }

        private async Task<bool> DownloadToFileAsync(IReadOnlyList<string> urls, string outputPath, string label, CancellationToken ct, string expectedDigest = "")
        {
            await Chivas.Utility.ResilientDownload.DownloadAsync(client, urls, outputPath, MaxDownloadBytes, expectedDigest,
                (read, total) =>
                {
                    double fraction = total is > 0 ? Math.Min((double)read / total.Value, 0.99) : -1.0;
                    OnProgressChanged?.Invoke(fraction >= 0 ? $"{label} {fraction * 100:0}%" : label, fraction, true);
                }, ct);
            return true;
        }

        private const string FleasionApiFork = "https://api.github.com/repos/lokcme/Fleasion/releases/latest";

        private const string FleasionApiUpstream = "https://api.github.com/repos/fleasion/Fleasion/releases/latest";

        private static async Task<(IReadOnlyList<string> Urls, string Digest)> ResolveFleasionUrlsFromAsync(string apiUrl, CancellationToken ct)
        {
            var urls = new List<string>();
            string digest = "";
            try
            {
                using JsonDocument doc = JsonDocument.Parse(await Chivas.Utility.Http.GetStringBoundedAsync(client, apiUrl, token: ct));
                foreach (JsonElement asset in doc.RootElement.GetProperty("assets").EnumerateArray())
                {
                    string name = asset.GetProperty("name").GetString() ?? "";
                    if (name.Equals("Fleasion.exe", StringComparison.OrdinalIgnoreCase) || name.EndsWith("-Windows.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        string? url = asset.GetProperty("browser_download_url").GetString();
                        if (string.IsNullOrEmpty(url))
                        {
                            continue;
                        }
                        string assetDigest = asset.TryGetProperty("digest", out JsonElement digestElement) ? digestElement.GetString() ?? "" : "";
                        if (assetDigest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) && assetDigest.Length == 71)
                        {
                            urls.Add(url);
                            digest = assetDigest.Substring(7);
                        }
                        else if (apiUrl == FleasionApiFork)
                        {
                            urls.Add(url);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
            }
            return (urls.Distinct().ToList(), digest);
        }

        private static async Task<(IReadOnlyList<string> Urls, string Digest)> ResolveFleasionUrlsAsync(CancellationToken ct)
        {
            var fork = await ResolveFleasionUrlsFromAsync(FleasionApiFork, ct);
            if (fork.Urls.Count > 0)
            {
                return fork;
            }
            return await ResolveFleasionUrlsFromAsync(FleasionApiUpstream, ct);
        }

        private static async Task<(IReadOnlyList<string> Urls, string Digest)> ResolveApiDumpUrlsAsync(CancellationToken ct)
        {
            var urls = new List<string>();
            string digest = ApiDumpSha256;
            try
            {
                using JsonDocument doc = JsonDocument.Parse(await Chivas.Utility.Http.GetStringBoundedAsync(client, "https://api.github.com/repos/MaximumADHD/Roblox-API-Dump-Tool/releases/latest", token: ct));
                foreach (JsonElement asset in doc.RootElement.GetProperty("assets").EnumerateArray())
                {
                    string name = asset.GetProperty("name").GetString() ?? "";
                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        string? url = asset.GetProperty("browser_download_url").GetString();
                        string assetDigest = asset.TryGetProperty("digest", out JsonElement digestElement) ? digestElement.GetString() ?? "" : "";
                        if (!string.IsNullOrEmpty(url) && assetDigest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) && assetDigest.Length == 71)
                        {
                            urls.Add(url);
                            digest = assetDigest.Substring(7);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
            }
            if (urls.Count == 0)
            {
                urls.Add("https://raw.githubusercontent.com/MaximumADHD/Roblox-API-Dump-Tool/master/RobloxAPIDumpTool.exe");
                digest = ApiDumpSha256;
            }
            return (urls.Distinct().ToList(), digest);
        }

        private static List<Process> GetFleasionProcesses()
        {
            List<Process> found = new List<Process>();
            Process[] all;
            try
            {
                all = Process.GetProcesses();
            }
            catch
            {
                return found;
            }
            foreach (Process proc in all)
            {
                bool match = false;
                try
                {
                    string name = proc.ProcessName;
                    match = name.Equals("Fleasion", StringComparison.OrdinalIgnoreCase)
                        || name.StartsWith("Fleasion-", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    match = false;
                }
                if (match)
                {
                    found.Add(proc);
                }
                else
                {
                    try
                    {
                        proc.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
            return found;
        }

        public static bool IsFleasionRunning()
        {
            List<Process> found = GetFleasionProcesses();
            try
            {
                return found.Count > 0;
            }
            finally
            {
                foreach (Process proc in found)
                {
                    try
                    {
                        proc.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static async Task KillFleasionAsync(CancellationToken ct)
        {
            foreach (Process proc in GetFleasionProcesses())
            {
                try
                {
                    proc.Kill();
                    await proc.WaitForExitAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    proc.Dispose();
                    throw;
                }
                catch
                {
                }
                finally
                {
                    proc.Dispose();
                }
            }
        }

        public static void SeedFleasionConfig()
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FleasionNT");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, "settings.json");
                Dictionary<string, object?>? data = null;
                if (File.Exists(path))
                {
                    try
                    {
                        data = JsonSerializer.Deserialize<Dictionary<string, object?>>(File.ReadAllText(path));
                    }
                    catch
                    {
                        data = null;
                    }
                }
                data ??= new Dictionary<string, object?>();
                data["first_time_setup_complete"] = true;
                data["open_dashboard_on_launch"] = false;
                data["proxy_features_enabled"] = true;
                data["proxy_mode"] = "env";
                List<string> enabled = new List<string>();
                if (data.TryGetValue("enabled_configs", out object? existing) && existing is JsonElement array && array.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement el in array.EnumerateArray())
                    {
                        if (el.ValueKind == JsonValueKind.String && el.GetString() is string name && name.Length > 0 && !enabled.Contains(name))
                        {
                            enabled.Add(name);
                        }
                    }
                }
                if (!enabled.Contains("Default"))
                {
                    enabled.Insert(0, "Default");
                }
                if (!enabled.Contains("ChivaStrap"))
                {
                    enabled.Add("ChivaStrap");
                }
                data["enabled_configs"] = enabled;
                File.WriteAllText(path, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                App.Logger?.WriteLine("ExtensionViewModel::SeedFleasion", ex.Message);
            }
        }

        public static bool LaunchFleasionSilent()
        {
            try
            {
                if (App.Settings.Prop.FleasionNativeEngineEnabled)
                {
                    return true;
                }
                string exe = Path.Combine(Paths.Fleasion, "Fleasion.exe");
                if (!File.Exists(exe))
                {
                    return false;
                }
                if (IsFleasionRunning())
                {
                    return true;
                }
                ProcessStartInfo psi = new ProcessStartInfo(exe, "--no-dashboard")
                {
                    UseShellExecute = false,
                    WorkingDirectory = Paths.Fleasion
                };
                psi.Environment["FLEASION_NO_FILE_LOG"] = "1";
                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                App.Logger?.WriteLine("ExtensionViewModel::LaunchFleasion", ex.Message);
                return false;
            }
        }

        private async Task DownloadFleasionAsync()
        {
            var (cts, ct) = BeginOperation();
            try
            {
                await _gate.WaitAsync(ct);
                try
                {
                    OnProgressChanged?.Invoke("Preparing Fleasion", -1.0, true);
                    Directory.CreateDirectory(fleasionDir);
                    string outputPath = Path.Combine(fleasionDir, "Fleasion.exe");

                    await KillFleasionAsync(ct);

                    if (File.Exists(outputPath) && IsFileLocked(outputPath))
                    {
                        Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_FleasionRunning, MessageBoxImage.Warning);
                        return;
                    }

                    var download = await ResolveFleasionUrlsAsync(ct);
                    if (download.Urls.Count == 0)
                        throw new InvalidDataException("Fleasion has no verified executable release");
                    bool ok = await DownloadToFileAsync(download.Urls, outputPath, "Downloading Fleasion", ct, download.Digest);
                    if (ok)
                    {
                        OnProgressChanged?.Invoke("Fleasion ready", 1.0, true);
                        SeedFleasionConfig();
                        LaunchFleasionSilent();
                        await Task.Delay(700, ct);
                    }
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (OperationCanceledException)
            {
                if (_userCancel)
                    RevertFleasionToggle();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_FleasionDlFail, ex.Message), MessageBoxImage.Error);
            }
            finally
            {
                OnProgressChanged?.Invoke("", -1.0, false);
                EndOperation(cts);
            }
        }

        private async Task UninstallFleasionAsync()
        {
            var (cts, ct) = BeginOperation();
            try
            {
                await _gate.WaitAsync(ct);
                try
                {
                    if (!Directory.Exists(fleasionDir))
                        return;

                    OnProgressChanged?.Invoke("Removing Fleasion", -1.0, true);
                    await KillFleasionAsync(ct);

                    bool deleted = false;
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            Directory.Delete(fleasionDir, true);
                            deleted = true;
                            break;
                        }
                        catch
                        {
                            await Task.Delay(400, ct);
                        }
                    }

                    if (!deleted && Directory.Exists(fleasionDir))
                    {
                        Frontend.ShowMessageBox(Strings.Menu_Dialogs_Code_FleasionRemoveFail, MessageBoxImage.Warning);
                        OpenFolder(fleasionDir);
                    }
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(string.Format(Strings.Menu_Dialogs_Code_FleasionUninstFail, ex.Message), MessageBoxImage.Error);
            }
            finally
            {
                OnProgressChanged?.Invoke("", -1.0, false);
                EndOperation(cts);
            }
        }

        private static async Task KillProcessAsync(string name, CancellationToken ct)
        {
            foreach (Process proc in Process.GetProcessesByName(name))
            {
                try
                {
                    proc.Kill();
                    await proc.WaitForExitAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    proc.Dispose();
                    throw;
                }
                catch
                {
                }
                finally
                {
                    proc.Dispose();
                }
            }
        }

        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch
            {
                return true;
            }
        }

        private static void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
        }

        private static void SafeDeleteDir(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch
            {
            }
        }

        private static void OpenFolder(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = Path.GetFullPath(path), UseShellExecute = true });
            }
            catch
            {
            }
        }
    }
}
