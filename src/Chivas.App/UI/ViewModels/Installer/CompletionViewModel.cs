using Timer = System.Timers.Timer;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Chivas.Enums;
using Chivas.Resources;
using Chivas.Models;
using Chivas.Models.APIs.Roblox;
using Chivas.RobloxInterfaces;

namespace Chivas.UI.ViewModels.Installer;

public class CompletionViewModel : ObservableObject, IDisposable
{
	private readonly Timer _saveTimer = new Timer(5000.0);

	private bool _disposed;

	private int _loadGeneration;

	private bool _showLoadingError;

	private string _channelInfoLoadingText = "";

	private DeployInfo? _channelDeployInfo;

	private bool _showChannelWarning;

	private string _viewChannel;

	public ICommand LaunchSettingsCommand { get; }

	public ICommand LaunchRobloxCommand { get; }

	public ICommand OpenAboutCommand { get; }

	public bool ShowLoadingError
	{
		get
		{
			return _showLoadingError;
		}
		set
		{
			SetProperty(ref _showLoadingError, value, nameof(ShowLoadingError));
		}
	}

	public string ChannelInfoLoadingText
	{
		get
		{
			return _channelInfoLoadingText;
		}
		set
		{
			SetProperty(ref _channelInfoLoadingText, value, nameof(ChannelInfoLoadingText));
		}
	}

	public DeployInfo? ChannelDeployInfo
	{
		get
		{
			return _channelDeployInfo;
		}
		set
		{
			SetProperty(ref _channelDeployInfo, value, nameof(ChannelDeployInfo));
		}
	}

	public bool ShowChannelWarning
	{
		get
		{
			return _showChannelWarning;
		}
		set
		{
			SetProperty(ref _showChannelWarning, value, nameof(ShowChannelWarning));
		}
	}

	public string ViewChannel
	{
		get
		{
			return _viewChannel;
		}
		set
		{
			string text = value?.Trim() ?? "production";
			if (!(_viewChannel == text))
			{
				_viewChannel = text;
				OnPropertyChanged(nameof(ViewChannel));
				_ = LoadChannelDeployInfoAsync(text);
				App.Settings.Prop.Channel = text;
				_saveTimer.Stop();
				_saveTimer.Start();
			}
		}
	}

	public event EventHandler<NextAction>? CloseWindowRequest;

	public CompletionViewModel()
	{
		LaunchSettingsCommand = new RelayCommand(delegate
		{
			if (RestartInto("-settings"))
				return;
			this.CloseWindowRequest?.Invoke(this, NextAction.LaunchSettings);
		});
		LaunchRobloxCommand = new RelayCommand(delegate
		{
			if (RestartInto("-player"))
				return;
			this.CloseWindowRequest?.Invoke(this, NextAction.LaunchRoblox);
		});
		OpenAboutCommand = new RelayCommand(delegate
		{
			new Chivas.UI.Elements.About.MainWindow().ShowOwnedDialog();
		});
		_viewChannel = App.Settings.Prop.Channel;
		_ = LoadChannelDeployInfoAsync(App.Settings.Prop.Channel);
		_saveTimer.Elapsed += OnSaveTimerElapsed;
		_saveTimer.AutoReset = false;
	}

	private static bool RestartInto(string flags)
	{
		try
		{
			App.State.Save();
			App.Settings.Save();
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("CompletionViewModel", "Could not flush settings before restarting: " + ex.Message);
		}

		string executable = "";
		try
		{
			executable = Paths.Application ?? "";
		}
		catch
		{
		}

		if (executable.Length == 0 || !System.IO.File.Exists(executable))
		{
			App.Logger.WriteLine("CompletionViewModel", "No installed executable to restart, using the in process path");
			return false;
		}

		try
		{
			System.Diagnostics.ProcessStartInfo startInfo = new()
			{
				FileName = executable,
				UseShellExecute = !Chivas.Utility.Platform.IsLinux,
				WorkingDirectory = System.IO.Path.GetDirectoryName(executable) ?? ""
			};
			if (Chivas.Utility.Platform.IsLinux)
			{
				string? setsid = new Chivas.Core.SystemProcessService().FindExecutable("setsid");
				if (!string.IsNullOrWhiteSpace(setsid))
				{
					startInfo.FileName = setsid;
					startInfo.CreateNoWindow = true;
					startInfo.ArgumentList.Add("--fork");
					startInfo.ArgumentList.Add(executable);
				}
				startInfo.ArgumentList.Add(flags);
			}
			else
			{
				startInfo.Arguments = flags;
			}

			using System.Diagnostics.Process? successor = System.Diagnostics.Process.Start(startInfo);
			if (successor == null)
			{
				throw new InvalidOperationException("The installed Chivas executable did not start");
			}
			App.Logger.WriteLine("CompletionViewModel", "Restarting Chivas with " + flags);
			App.Terminate();
			return true;
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("CompletionViewModel", "Could not restart Chivas: " + ex.Message);
			return false;
		}
	}

	private void OnSaveTimerElapsed(object? sender, ElapsedEventArgs e)
	{
		try
		{
			if (!_disposed)
			{
				App.State.Save();
			}
		}
		catch (Exception ex)
		{
			App.Logger.WriteException("CompletionViewModel::OnSaveTimerElapsed", ex);
		}
	}

	private async Task LoadChannelDeployInfoAsync(string channel)
	{
		int generation = ++_loadGeneration;
		try
		{
			ShowLoadingError = false;
			ChannelDeployInfo = null;
			ChannelInfoLoadingText = Strings.Installer_Channel_DeployInfo_Loading;
			OnPropertyChanged(nameof(ShowLoadingError));
			OnPropertyChanged(nameof(ChannelDeployInfo));
			OnPropertyChanged(nameof(ChannelInfoLoadingText));
			ClientVersion clientVersion = await Deployment.GetInfo(channel);
			if (_disposed || generation != _loadGeneration)
			{
				return;
			}
			ShowChannelWarning = clientVersion.IsBehindDefaultChannel;
			ChannelDeployInfo = new DeployInfo
			{
				Version = clientVersion.Version,
				VersionGuid = clientVersion.VersionGuid
			};
			App.State.Prop.IgnoreOutdatedChannel = true;
			OnPropertyChanged(nameof(ShowChannelWarning));
			OnPropertyChanged(nameof(ChannelDeployInfo));
		}
		catch (HttpRequestException)
		{
			ShowLoadingError = true;
			ChannelInfoLoadingText = Strings.Installer_Channel_DeployInfo_Unreachable;
		}
		catch (TaskCanceledException)
		{
			ShowLoadingError = true;
			ChannelInfoLoadingText = Strings.Installer_Channel_DeployInfo_Timeout;
		}
		catch (Exception ex3)
		{
			ShowLoadingError = true;
			ChannelInfoLoadingText = string.Format(Strings.Installer_Channel_DeployInfo_Error, ex3.Message);
		}
		finally
		{
			if (!_disposed && generation == _loadGeneration)
			{
				OnPropertyChanged(nameof(ShowLoadingError));
				OnPropertyChanged(nameof(ChannelInfoLoadingText));
			}
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		_loadGeneration++;
		_saveTimer.Stop();
		_saveTimer.Elapsed -= OnSaveTimerElapsed;
		_saveTimer.Dispose();
		CloseWindowRequest = null;
		GC.SuppressFinalize(this);
	}
}
