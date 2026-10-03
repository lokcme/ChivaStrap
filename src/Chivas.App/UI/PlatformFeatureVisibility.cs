using System.Windows;
using Chivas.Platform;

namespace Chivas.UI;

public static class PlatformFeatureVisibility
{
	public static Visibility Overlay { get; } = Resolve(FeatureId.Overlay);

	public static Visibility GlobalInput { get; } = Resolve(FeatureId.GlobalInput);

	public static Visibility AudioSession { get; } = Resolve(FeatureId.AudioSession);

	public static Visibility Tray { get; } = Resolve(FeatureId.Tray);

	public static Visibility ResourceOptimization { get; } = Resolve(FeatureId.ResourceOptimization);

	public static Visibility FrameGeneration { get; } = Resolve(FeatureId.FrameGeneration);

	public static Visibility VirtualController { get; } = Resolve(FeatureId.VirtualController);

	public static Visibility WindowsIntegration { get; } = Chivas.Utility.Platform.IsLinux ? Visibility.Collapsed : Visibility.Visible;

	public static Visibility NetworkMtu { get; } = Chivas.Utility.Platform.IsWindows ? Visibility.Visible : Visibility.Collapsed;

	public static Visibility DesktopBackdrop { get; } = Chivas.Utility.Platform.IsLinux ? Visibility.Collapsed : Visibility.Visible;

	public static Visibility LinuxIntegration { get; } = Chivas.Utility.Platform.IsLinux ? Visibility.Visible : Visibility.Collapsed;

	public static bool IsSupported(FeatureId feature)
	{
		return Resolve(feature) == Visibility.Visible;
	}

	private static Visibility Resolve(FeatureId feature)
	{
		if (!Chivas.Utility.Platform.IsLinux)
		{
			return Visibility.Visible;
		}

		try
		{
			IPlatformHost? host = Chivas.Utility.Platform.RuntimeHost;
			return Chivas.Core.PlatformFeatureGate.IsHidden(host?.Capabilities, feature)
				? Visibility.Collapsed
				: Visibility.Visible;
		}
		catch
		{
			return Visibility.Visible;
		}
	}
}
