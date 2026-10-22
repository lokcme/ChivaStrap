using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Chivas.Integrations.AssetProxy;
using Chivas.UI.Elements.Settings.Pages;
using Chivas.Utility;
using Microsoft.Win32;

namespace Chivas.UI.ViewModels.Settings;

public enum FleasionFunctionKind
{
    Audio,
    Image,
    Pack
}

public sealed class FleasionFunction : NotifyPropertyChangedViewModel
{
    private readonly Action _onChanged;

    private readonly Func<FleasionFunction, bool> _confirmEnable;

    private bool _isEnabled;

    private string _filePath = "";

    private string _originalName = "";

    public FleasionFunction(string ruleName, string assetId, FleasionFunctionKind kind, string displayName, string kindDescription, string noFileLabel, Action onChanged, Func<FleasionFunction, bool> confirmEnable)
    {
        RuleName = ruleName;
        AssetId = assetId;
        Kind = kind;
        DisplayName = displayName;
        KindDescription = kindDescription;
        NoFileLabel = noFileLabel;
        _onChanged = onChanged;
        _confirmEnable = confirmEnable;
    }

    public string RuleName { get; }

    public string AssetId { get; }

    public FleasionFunctionKind Kind { get; }

    public string DisplayName { get; }

    public string KindDescription { get; }

    public string NoFileLabel { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
                return;
            if (value && Kind != FleasionFunctionKind.Pack && _filePath.Length == 0)
            {
                bool confirmed = false;
                try
                {
                    confirmed = _confirmEnable(this);
                }
                catch
                {
                    confirmed = false;
                }
                if (!confirmed || _filePath.Length == 0)
                {
                    OnPropertyChanged(nameof(IsEnabled));
                    return;
                }
            }
            _isEnabled = value;
            OnPropertyChanged(nameof(IsEnabled));
            _onChanged();
        }
    }

    public string FilePath
    {
        get => _filePath;
        set
        {
            string next = value ?? "";
            if (_filePath == next)
                return;
            _filePath = next;
            OnPropertyChanged(nameof(FilePath));
            OnPropertyChanged(nameof(FileName));
        }
    }

    public string FileName => _originalName.Length > 0 ? _originalName : (_filePath.Length > 0 ? Path.GetFileName(_filePath) : NoFileLabel);

    public string OriginalFileName
    {
        get => _originalName;
        set
        {
            string next = value ?? "";
            if (_originalName == next)
                return;
            _originalName = next;
            OnPropertyChanged(nameof(OriginalFileName));
            OnPropertyChanged(nameof(FileName));
        }
    }

    public Visibility FileRowVisibility => Kind == FleasionFunctionKind.Pack ? Visibility.Collapsed : Visibility.Visible;

    internal void SetEnabledSilent(bool value)
    {
        _isEnabled = value;
        OnPropertyChanged(nameof(IsEnabled));
    }

    internal void SetFileSilent(string value)
    {
        _filePath = value ?? "";
        OnPropertyChanged(nameof(FilePath));
        OnPropertyChanged(nameof(FileName));
    }

    internal void SetOriginalSilent(string value)
    {
        _originalName = value ?? "";
        OnPropertyChanged(nameof(OriginalFileName));
        OnPropertyChanged(nameof(FileName));
    }
}

public sealed class FleasionViewModel : NotifyPropertyChangedViewModel
{
    private const string ConfigFileName = "ChivaStrap.json";

    private const string MetaFileName = "ChivaStrap.meta.json";

    /// <summary>
    /// A single switch to park the whole Fleasion toolset while its delivery
    /// path is rebuilt. Every control is disabled, the toggles are forced
    /// off and any attempt to touch one answers with
    /// <see cref="MaintenanceMessage"/> instead of doing nothing silently.
    /// It is a property, not a const, on purpose: a const lets the compiler
    /// fold every guarded branch into dead code and warn about it.
    /// </summary>
    public static bool MaintenanceMode => false;

    public string MaintenanceMessage => Chivas.Resources.Strings.Menu_Fleasion_Page_Maintenance;

    /// <summary>Shows the maintenance notice. Bound from the page.</summary>
    public void ShowMaintenanceNotice()
    {
        Frontend.ShowMessageBox(MaintenanceMessage, MessageBoxImage.Information);
    }

    /// <summary>
    /// The native delivery engine is Fleasion's replacement protocol adapted
    /// into Chivas. It uses the same local interception path as the app proxy,
    /// but runs in-process and does not launch Fleasion.exe.
    /// </summary>
    public static bool UseFleasionProxyOnly => false;

    /// <summary>
    /// Forces the native engine off while the toolset is parked. Without this
    /// a previously enabled AssetWarp keeps routing Roblox through the proxy
    /// even though the page that manages it is disabled.
    /// </summary>
    private void DisableNativeEngine()
    {
        if (!App.Settings.Prop.AssetWarpEnabled)
            return;
        App.Settings.Prop.AssetWarpEnabled = false;
        App.Settings.Prop.AssetWarpCertificateApproved = false;
        AssetProxyServer.ReconcileRuntimeState();
        App.FastFlags.SaveDeferred();
        App.Settings.SaveDeferred();
    }

    private static string FleasionDataDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FleasionNT");

    private static string ConfigsDir => Path.Combine(Paths.AssetProxy, "Configs");

    private static string AssetsDir => Path.Combine(Paths.AssetProxy, "ChivaStrapAssets");

    private static string ConfigPath => Path.Combine(ConfigsDir, ConfigFileName);

    /// <summary>
    /// The Real Fleasion app loads replacement configs from
    /// %LOCALAPPDATA%\FleasionNT\configs and recompiles them on change, so the
    /// rules written here also land there as an enabled "ChivaStrap" profile.
    /// Without this mirror the app never sees rules from this page, because
    /// Its config folder is separate from the Chivas proxy folder.
    /// </summary>
    private static string FleasionSyncConfigPath => Path.Combine(FleasionDataDir, "configs", ConfigFileName);

    private bool _isRunning;

    public FleasionViewModel()
    {
        string audioDesc = Chivas.Resources.Strings.Menu_Fleasion_Page_KindAudioDesc;
        string imageDesc = Chivas.Resources.Strings.Menu_Fleasion_Page_KindImageDesc;
        string noFile = Chivas.Resources.Strings.Menu_Fleasion_Page_NoFile;
        Action onChanged = SaveFunctions;
        Func<FleasionFunction, bool> confirmEnable = ConfirmEnable;
        Functions.Add(new FleasionFunction("killfx", "296102734", FleasionFunctionKind.Audio, Chivas.Resources.Strings.Menu_Fleasion_Page_FnKill, audioDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("crosshair", "114987734743262", FleasionFunctionKind.Image, Chivas.Resources.Strings.Menu_Fleasion_Page_FnCrosshair, imageDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("missfx", "10209803", FleasionFunctionKind.Audio, Chivas.Resources.Strings.Menu_Fleasion_Page_FnMiss, audioDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("stabfx", "142247768", FleasionFunctionKind.Audio, Chivas.Resources.Strings.Menu_Fleasion_Page_FnStab, audioDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("winsoundfx", "113599363914218", FleasionFunctionKind.Audio, Chivas.Resources.Strings.Menu_Fleasion_Page_FnWin, audioDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("katana", "17732086382", FleasionFunctionKind.Image, Chivas.Resources.Strings.Menu_Fleasion_Page_FnKatana, imageDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("armamod", "17724623293", FleasionFunctionKind.Image, Chivas.Resources.Strings.Menu_Fleasion_Page_FnArma, imageDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("vengal", "17756582723", FleasionFunctionKind.Image, Chivas.Resources.Strings.Menu_Fleasion_Page_FnVengal, imageDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("flyswatter", "17808783437", FleasionFunctionKind.Image, Chivas.Resources.Strings.Menu_Fleasion_Page_FnFly, imageDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("bop", "17768575165", FleasionFunctionKind.Image, Chivas.Resources.Strings.Menu_Fleasion_Page_FnBop, imageDesc, noFile, onChanged, confirmEnable));
        Functions.Add(new FleasionFunction("basicsai", "18621040695", FleasionFunctionKind.Image, Chivas.Resources.Strings.Menu_Fleasion_Page_FnBasics, imageDesc, noFile, onChanged, confirmEnable));
        // "Textures limpias" removed: the "TexturePack" type-wildcard strips
        // every texture pack in every game, blanking all visuals. Upstream
        // removed their "No Textures" checkbox for the same reason. A safe
        // replacement (per-slot blank placeholders) would need real engine work.
        LoadFunctions();
        RefreshRunning();
        if (MaintenanceMode)
            DisableNativeEngine();
    }

    public ObservableCollection<FleasionFunction> Functions { get; } = new ObservableCollection<FleasionFunction>();

    public bool MasterEnabled
    {
        get => App.Settings.Prop.Fleasion;
        set
        {
            if (MaintenanceMode)
            {
                ShowMaintenanceNotice();
                OnPropertyChanged(nameof(MasterEnabled));
                return;
            }
            if (App.Settings.Prop.Fleasion == value)
                return;
            ExtensionViewModel.EnsureCurrent().fleasionenabler = value;
            OnPropertyChanged(nameof(MasterEnabled));
            RefreshRunning();
        }
    }

    public bool NativeEnabled
    {
        get => App.Settings.Prop.FleasionNativeEngineEnabled;
set
        {
            if (MaintenanceMode)
            {
                ShowMaintenanceNotice();
                OnPropertyChanged(nameof(NativeEnabled));
                return;
            }
            if (App.Settings.Prop.FleasionNativeEngineEnabled == value)
                return;
            if (value)
            {
                if (!FastFlagsViewModel.TryEnableFleasionNativeEngine())
                {
                    OnPropertyChanged(nameof(NativeEnabled));
                    OnPropertyChanged(nameof(StatusText));
                    return;
                }

                App.Settings.Prop.FleasionNativeEngineEnabled = true;
                TextureStripper.InvalidateRuntimeState();
                AssetProxyRouting.InvalidateCache();
                AssetProxyServer.ReconcileRuntimeState();
            }
            else
            {
                App.Settings.Prop.FleasionNativeEngineEnabled = false;
                if (!App.Settings.Prop.AssetWarpEnabled)
                    App.Settings.Prop.AssetWarpCertificateApproved = false;
                AssetProxyServer.ReconcileRuntimeState();
                App.FastFlags.SaveDeferred();
                App.Settings.SaveDeferred();
            }
            OnPropertyChanged(nameof(NativeEnabled));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(HasStripWarning));
            OnPropertyChanged(nameof(StripWarningVisibility));
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value)
                return;
            _isRunning = value;
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public string StatusText
    {
        get
        {
            if (App.Settings.Prop.FleasionNativeEngineEnabled)
            {
                return AssetProxyServer.IsRunning
                    ? Chivas.Resources.Strings.Menu_Fleasion_Page_NativeRunning
                    : Chivas.Resources.Strings.Menu_Fleasion_Page_NativeReady;
            }
            return IsRunning
                ? Chivas.Resources.Strings.Menu_Extensions_Page_EntryEnabled
                : Chivas.Resources.Strings.Menu_Extensions_Page_EntryDisabled;
        }
    }

    public void RefreshRunning()
    {
        IsRunning = ExtensionViewModel.IsFleasionRunning();
        OnPropertyChanged(nameof(StatusText));
    }

    public void Refresh()
    {
        LoadFunctions();
        if (MaintenanceMode)
            ApplyMaintenanceState();
        RefreshRunning();
        OnPropertyChanged(nameof(NativeEnabled));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(HasStripWarning));
        OnPropertyChanged(nameof(StripWarningVisibility));
    }

    /// <summary>
    /// Forces every per-function toggle off without writing the config, and
    /// tells the bindings the values changed. Done after LoadFunctions
    /// because that re-reads the saved state, which may still have toggles
    /// switched on from before the maintenance lock.
    /// </summary>
    private void ApplyMaintenanceState()
    {
        foreach (FleasionFunction function in Functions)
            function.SetEnabledSilent(false);
    }

    public bool HasStripWarning => App.Settings.Prop.AssetWarpDisableAllMeshes
        || App.Settings.Prop.AssetWarpDisableAllTextures
        || App.Settings.Prop.AssetWarpDisableAllImages
        || App.Settings.Prop.AssetWarpDisableAllDecals
        || App.Settings.Prop.AssetWarpDisableAllAnimations;

    public Visibility StripWarningVisibility => HasStripWarning ? Visibility.Visible : Visibility.Collapsed;

    public void OpenFleasion()
    {
        ExtensionViewModel.SeedFleasionConfig();
        ExtensionViewModel.LaunchFleasionSilent();
        RefreshRunning();
    }

    public void ClearCache()
    {
        try
        {
            AssetProxyRouting.InvalidateCache();
            bool robloxRunning = false;
            try
            {
                robloxRunning = Process.GetProcessesByName("RobloxPlayerBeta").Length > 0;
            }
            catch
            {
            }
            if (!robloxRunning)
            {
                AssetProxyRouting.ClearRobloxCache();
            }
            App.Logger?.WriteLine("FleasionViewModel::ClearCache", "Cache invalidated (immediate: " + (!robloxRunning) + ").");
        }
        catch (Exception ex)
        {
            App.Logger?.WriteLine("FleasionViewModel::ClearCache", ex.Message);
        }
    }

    private bool ConfirmEnable(FleasionFunction function)
    {
        if (function.FilePath.Length > 0)
        {
            return true;
        }
        PickFile(function);
        return function.FilePath.Length > 0;
    }

    public void PickFile(FleasionFunction function)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Title = Chivas.Resources.Strings.Menu_Fleasion_Page_ChooseFile,
            Filter = function.Kind == FleasionFunctionKind.Audio
                ? "Audio (*.mp3;*.ogg;*.wav)|*.mp3;*.ogg;*.wav"
                : "Imagen (*.png)|*.png",
            CheckFileExists = true
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }
        try
        {
            Directory.CreateDirectory(AssetsDir);
            string dest = Path.Combine(AssetsDir, function.RuleName + Path.GetExtension(dialog.FileName).ToLowerInvariant());
            File.Copy(dialog.FileName, dest, true);
            function.FilePath = dest;
            function.OriginalFileName = Path.GetFileName(dialog.FileName);
            SaveFunctions();
            SaveMeta();
        }
        catch (Exception ex)
        {
            App.Logger?.WriteLine("FleasionViewModel::PickFile", ex.Message);
        }
    }

    private void LoadFunctions()
    {
        Dictionary<string, string> meta = LoadMeta();
        Dictionary<string, JsonObject>? rules = ReadRules();
        if (rules == null)
        {
            rules = ImportRules() ?? new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        }
        foreach (FleasionFunction function in Functions)
        {
            function.SetOriginalSilent(meta.TryGetValue(function.RuleName, out string? original) ? original : "");
        }
        foreach (FleasionFunction function in Functions)
        {
            try
            {
                if (rules.TryGetValue(function.RuleName, out JsonObject? rule))
                {
                    function.SetEnabledSilent(SafeBool(rule["enabled"]));
                    function.SetFileSilent(rule["local_path"]?.GetValue<string>() ?? "");
                }
                else
                {
                    function.SetEnabledSilent(false);
                    function.SetFileSilent("");
                }
            }
            catch
            {
                function.SetEnabledSilent(false);
                function.SetFileSilent("");
            }
        }
    }

    private static bool SafeBool(JsonNode? node)
    {
        try
        {
            return node?.GetValue<bool>() ?? false;
        }
        catch
        {
            return false;
        }
    }

    private int _restartGen;

    private void ScheduleFleasionRestart()
    {
        if (!App.Settings.Prop.Fleasion || App.Settings.Prop.FleasionNativeEngineEnabled)
        {
            return;
        }
        int gen = Interlocked.Increment(ref _restartGen);
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2500);
            }
            catch
            {
                return;
            }
            if (gen != Volatile.Read(ref _restartGen))
            {
                return;
            }
            try
            {
                foreach (Process proc in Process.GetProcesses())
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
                        try
                        {
                            proc.Kill();
                        }
                        catch
                        {
                        }
                    }
                    try
                    {
                        proc.Dispose();
                    }
                    catch
                    {
                    }
                }
                await Task.Delay(1500);
                if (gen != Volatile.Read(ref _restartGen))
                {
                    return;
                }
                ExtensionViewModel.SeedFleasionConfig();
                ExtensionViewModel.LaunchFleasionSilent();
            }
            catch
            {
            }
            try
            {
                Application.Current?.Dispatcher.BeginInvoke(new Action(RefreshRunning));
            }
            catch
            {
            }
        });
    }

    private void SaveFunctions()
    {
        try
        {
            Directory.CreateDirectory(ConfigsDir);
            JsonArray array = new JsonArray();
            foreach (FleasionFunction function in Functions)
            {
                JsonObject rule = new JsonObject
                {
                    ["name"] = function.RuleName,
                    ["enabled"] = function.IsEnabled
                };
                JsonArray ids = new JsonArray();
                if (long.TryParse(function.AssetId, out long numericId))
                {
                    ids.Add(numericId);
                }
                else
                {
                    ids.Add(function.AssetId);
                }
                rule["replace_ids"] = ids;
                rule["mode"] = function.Kind == FleasionFunctionKind.Pack ? "id" : "local";
                if (function.Kind != FleasionFunctionKind.Pack && function.FilePath.Length > 0)
                {
                    rule["local_path"] = function.FilePath;
                }
                array.Add(rule);
            }
            JsonObject root = new JsonObject { ["replacement_rules"] = array };
            string json = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
            try
            {
                // The Real Fleasion app only reads its own configs folder, so
                // mirror the same file there. Its folder watcher recompiles
                // rules on change and its compiler understands this format
                // (enabled flip, local paths, type-name ids).
                Directory.CreateDirectory(Path.GetDirectoryName(FleasionSyncConfigPath)!);
                File.WriteAllText(FleasionSyncConfigPath, json);
            }
            catch (Exception ex)
            {
                App.Logger?.WriteLine("FleasionViewModel::SaveFunctions", "Could not sync rules to Fleasion: " + ex.Message);
            }
            // The native engine reads this shared replacement config directly
            // from Chivas' proxy rule directory. Do not seed or restart the
            // external Fleasion.exe when native mode is selected.
            if (App.Settings.Prop.FleasionNativeEngineEnabled)
            {
                TextureStripper.InvalidateRuntimeState();
                AssetProxyRouting.InvalidateCache();
                App.Logger?.WriteLine("FleasionViewModel::SaveFunctions", "Updated native Fleasion replacement rules without Fleasion.exe.");
            }
            else
            {
                ExtensionViewModel.SeedFleasionConfig();
                ScheduleFleasionRestart();
            }
        }
        catch (Exception ex)
        {
            App.Logger?.WriteLine("FleasionViewModel::SaveFunctions", ex.Message);
        }
    }

    private static Dictionary<string, string> LoadMeta()
    {
        try
        {
            string path = Path.Combine(ConfigsDir, MetaFileName);
            if (!File.Exists(path))
            {
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }
            Dictionary<string, string>? meta = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            return meta ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    private void SaveMeta()
    {
        try
        {
            Directory.CreateDirectory(ConfigsDir);
            Dictionary<string, string> meta = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (FleasionFunction function in Functions)
            {
                if (function.OriginalFileName.Length > 0)
                {
                    meta[function.RuleName] = function.OriginalFileName;
                }
            }
            File.WriteAllText(Path.Combine(ConfigsDir, MetaFileName), JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            App.Logger?.WriteLine("FleasionViewModel::SaveMeta", ex.Message);
        }
    }

    private static Dictionary<string, JsonObject>? ReadRules()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                return null;
            }
            JsonNode? root = JsonNode.Parse(File.ReadAllText(ConfigPath));
            JsonArray? array = root?["replacement_rules"]?.AsArray();
            if (array == null)
            {
                return null;
            }
            Dictionary<string, JsonObject> rules = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
            foreach (JsonNode? node in array)
            {
                if (node is JsonObject rule && rule["name"]?.GetValue<string>() is string name && name.Length > 0)
                {
                    rules[name] = rule;
                }
            }
            return rules;
        }
        catch
        {
            return null;
        }
    }

    private Dictionary<string, JsonObject> ImportRules()
    {
        Dictionary<string, JsonObject> adopted = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        try
        {
            string legacyDir = Path.Combine(FleasionDataDir, "configs");
            if (!Directory.Exists(legacyDir))
            {
                return adopted;
            }
            foreach (string file in Directory.GetFiles(legacyDir, "*.json"))
            {
                if (string.Equals(Path.GetFileName(file), ConfigFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                try
                {
                    JsonNode? root = JsonNode.Parse(File.ReadAllText(file));
                    JsonArray? array = root?["replacement_rules"]?.AsArray();
                    if (array == null)
                    {
                        continue;
                    }
                    foreach (JsonNode? node in array)
                    {
                        if (node is not JsonObject rule || rule["name"]?.GetValue<string>() is not string name)
                        {
                            continue;
                        }
                        FleasionFunction? target = Functions.FirstOrDefault(f => f.RuleName == name);
                        if (target == null || adopted.ContainsKey(name))
                        {
                            continue;
                        }
                        string source = rule["local_path"]?.GetValue<string>() ?? "";
                        if (target.Kind != FleasionFunctionKind.Pack && source.Length > 0 && File.Exists(source))
                        {
                            try
                            {
                                Directory.CreateDirectory(AssetsDir);
                                string dest = Path.Combine(AssetsDir, name + Path.GetExtension(source).ToLowerInvariant());
                                File.Copy(source, dest, true);
                                source = dest;
                            }
                            catch
                            {
                            }
                        }
                        else if (target.Kind != FleasionFunctionKind.Pack)
                        {
                            source = "";
                        }
                        target.SetEnabledSilent(SafeBool(rule["enabled"]));
                        target.SetFileSilent(source);
                        target.SetOriginalSilent(source.Length > 0 ? Path.GetFileName(source) : "");
                        try
                        {
                            rule["local_path"] = source;
                        }
                        catch
                        {
                        }
                        adopted[name] = rule;
                    }
                }
                catch
                {
                }
            }
        }
        catch (Exception ex)
        {
            App.Logger?.WriteLine("FleasionViewModel::ImportRules", ex.Message);
        }
        SaveMeta();
        return adopted;
    }
}
