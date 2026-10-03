using System;
using Chivas.Enums;
using Chivas.Integrations;

namespace Chivas.Integrations.Studio
{
    public static class StudioIntegration
    {
        private const string LogTag = "StudioIntegration";

        private static readonly object _lock = new object();

        private static StudioRichPresence? _rpc;

        public static void Start(bool studioSession = false)
        {
            try
            {
                LaunchSettings launchSettings = App.LaunchSettings;
                if (!studioSession && (launchSettings == null || launchSettings.RobloxLaunchMode != LaunchMode.Studio))
                {
                    return;
                }
                if (Chivas.Utility.Platform.IsLinux)
                {
                    StartLinux();
                    return;
                }
                lock (_lock)
                {
                    StudioBridge.Start();
                    if (_rpc == null)
                    {
                        _rpc = new StudioRichPresence();
                    }
                }
                App.Logger.WriteLine(LogTag, "Studio integration started");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LogTag, "Start failed: " + ex.Message);
            }
        }

        private static void StartLinux()
        {
            bool presence = App.Settings.Prop.UseDiscordRichPresence;
            lock (_lock)
            {
                StudioBridge.Start();
                if (presence && _rpc == null)
                {
                    _rpc = new StudioRichPresence();
                }
            }
            App.Logger.WriteLine(LogTag, "Studio integration started for Vinegar, presence " + (presence ? "on" : "off"));
        }

        public static void Shutdown()
        {
            lock (_lock)
            {
                try
                {
                    _rpc?.Dispose();
                }
                catch
                {
                }
                _rpc = null;
                StudioBridge.Shutdown();
            }
        }
    }
}
