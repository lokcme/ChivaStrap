using System.Windows;

namespace Chivas.Utility
{
    public static class ScreenMetrics
    {
        private const double MinimumUsableWidth = 320.0;

        private const double MinimumUsableHeight = 240.0;

        public static (double Width, double Height) GetPrimary()
        {
            if (Platform.IsLinux)
            {
                Chivas.Platform.Linux.LinuxDisplayInfo info = Chivas.Platform.Linux.LinuxDisplayMetrics.Current;
                if (info.Bounds.Width >= MinimumUsableWidth && info.Bounds.Height >= MinimumUsableHeight)
                    return (info.Bounds.Width, info.Bounds.Height);
            }

            return (SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
        }

        public static double PrimaryWidth => GetPrimary().Width;

        public static double PrimaryHeight => GetPrimary().Height;

        public static Rect WorkArea
        {
            get
            {
                if (Platform.IsLinux)
                {
                    Chivas.Platform.Linux.LinuxDisplayInfo info = Chivas.Platform.Linux.LinuxDisplayMetrics.Current;
                    if (info.WorkArea.Width >= MinimumUsableWidth && info.WorkArea.Height >= MinimumUsableHeight)
                    {
                        return new Rect(
                            info.WorkArea.Left,
                            info.WorkArea.Top,
                            info.WorkArea.Width,
                            info.WorkArea.Height);
                    }
                }

                return SystemParameters.WorkArea;
            }
        }
    }
}
