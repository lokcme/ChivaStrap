using System.Globalization;
using System.Windows;
using Chivas.Extensions;
using Chivas.Models.Attributes;
using Chivas.Resources;

namespace Chivas.UI.ViewModels.About;

public class AboutViewModel : NotifyPropertyChangedViewModel
{
	public string Version => string.Format(Strings.Menu_About_Version, App.Version);

	public string VersionTag => "v" + App.Version;

	public string VersionNumber => App.Version;

	public string BuildDate => BuildMetadata.Timestamp.ToString("dd MMM yyyy", new CultureInfo("es-DO")).Replace(".", "").ToUpperInvariant();

	public string BuildTime => BuildMetadata.Timestamp.ToString("h:mm:ss tt", CultureInfo.InvariantCulture);

	public BuildMetadataAttribute BuildMetadata => App.BuildMetadata;

	public string BuildTimestamp => BuildMetadata.Timestamp.ToFriendlyString();

	public string CommitHash => BuildMetadata.CommitHash;

	public string BuildCommitHashUrl => "https://github.com/lokcme/ChivaStrap/commit/" + BuildMetadata.CommitHash;

	public Visibility BuildInformationVisibility
	{
		get
		{
			if (!App.IsProductionBuild)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public Visibility BuildCommitVisibility
	{
		get
		{
			if (!App.IsActionBuild)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}
}
