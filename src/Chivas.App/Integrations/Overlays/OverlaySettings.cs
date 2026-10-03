using Chivas.Integrations.AntiAliasing;
using Chivas.Integrations.FrameGeneration;
using Chivas.Integrations.MotionBlur;
using Chivas.Integrations.RiShade;

namespace Chivas.Integrations.Overlays
{
    public static class OverlaySettings
    {
        public static bool GameEffectsEnabled =>
            App.Settings.Prop.RiShadeEnabled
            || AntiAliasingSettings.MethodIndex > 0
            || MotionBlurSettings.StrengthIndex > 0
            || FrameGenSettings.ModeIndex > 0;

        public static bool HomepageBackgroundEnabled => App.Settings.Prop.HomepageBackgroundOverlayEnabled;

		public static string HomepageBackgroundMode
		{
			get
			{
				string mode = App.Settings.Prop.HomepageBackgroundOverlayMode ?? "Auto";
				if (mode is "Solid" or "Gradient" or "Media")
					return mode;
				if (!string.IsNullOrWhiteSpace(App.Settings.Prop.HomepageBackgroundOverlayMediaPath))
					return "Media";
				return App.Settings.Prop.HomepageBackgroundOverlayGradientEnabled ? "Gradient" : "Solid";
			}
		}

        public static bool AnyEnabled => OverlayHub.InGame ? GameEffectsEnabled : HomepageBackgroundEnabled;

		public static bool RequiresLinuxX11Session => Chivas.Utility.Platform.IsLinux
			&& (App.Settings.Prop.FakeExclusiveFullscreen
				|| App.Settings.Prop.FakeBorderlessFullscreen
				|| App.Settings.Prop.SoberAutoFullscreen
				|| App.Settings.Prop.SnapTapEnabled
				|| App.Settings.Prop.DuckRobloxAudioOnUnfocus
				|| LinuxRobloxWindow.IsEnabled
				|| HomepageBackgroundEnabled
				|| LinuxCustomCursorNeedsX11);

		public static bool LinuxCustomCursorNeedsX11 => Chivas.Utility.Platform.IsLinux
			&& App.Settings.Prop.CursorType != Chivas.Enums.CursorType.Default
			&& App.Settings.Prop.ModApplyTarget is Chivas.Enums.ModApplyTarget.Both or Chivas.Enums.ModApplyTarget.Player;
    }
}
