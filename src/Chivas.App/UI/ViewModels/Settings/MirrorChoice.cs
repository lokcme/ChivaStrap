using System;
using Chivas.Resources;

namespace Chivas.UI.ViewModels.Settings;

public class MirrorChoice
{
    public MirrorChoice(string display, string url)
    {
        Display = display;
        Url = url;
    }

    public string Display { get; }

    public string Url { get; }

    public static string Describe(string url)
    {
        if (string.IsNullOrEmpty(url))
            return Strings.Installer_Channel_Mirror_Auto;
        try
        {
            return new Uri(url).Host;
        }
        catch
        {
            return url;
        }
    }
}
