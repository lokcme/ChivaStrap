using System.Windows;
using Chivas.UI.ViewModels.About;
using Wpf.Ui.Controls;

namespace Chivas.UI.Elements.About.Pages;

public partial class AboutPage : UiPage{
	public AboutPage()
	{
		base.DataContext = new AboutViewModel();
		InitializeComponent();
	}

	private void ThirdParty_Click(object sender, RoutedEventArgs e)
	{
		NavigationService?.Navigate(new LicensesPage());
	}
}
