using System.Windows;
using Chivas.Resources;
using Chivas.UI.Elements.Base;
using Chivas.UI.Elements.ContextMenu;
using Chivas.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Chivas.UI.Elements.Installer.Pages;

public partial class InstallerAppearancePage : UiPage
{
	private readonly AppearanceViewModel _viewModel = new AppearanceViewModel();

	public InstallerAppearancePage()
	{
		DataContext = _viewModel;
		InitializeComponent();
	}

	private void UiPage_Loaded(object sender, RoutedEventArgs e)
	{
		if (Window.GetWindow(this) is MainWindow mainWindow)
		{
			mainWindow.SetNextButtonText(Strings.Common_Navigation_Next);
			mainWindow.NextPageCallback = NextPageCallback;
		}
	}

	private static bool NextPageCallback()
	{
		App.Settings.SaveDeferred();
		return true;
	}

	private void OpenCustomThemeEditor_Click(object sender, RoutedEventArgs e)
	{
		CustomThemeEditor editor = new()
		{
			Owner = Window.GetWindow(this)
		};
		editor.ShowOwnedDialog();

		if (Application.Current == null)
			return;

		_viewModel.OnPropertyChanged(nameof(AppearanceViewModel.Theme));
		foreach (WpfUiWindow window in Application.Current.Windows.OfType<WpfUiWindow>().ToArray())
			window.ApplyTheme();
	}
}
