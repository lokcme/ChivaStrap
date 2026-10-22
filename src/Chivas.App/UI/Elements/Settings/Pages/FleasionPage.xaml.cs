using System.Windows;
using System.Windows.Controls;
using Chivas.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Chivas.UI.Elements.Settings.Pages;

public partial class FleasionPage : UiPage
{
	private readonly FleasionViewModel _vm;

	public FleasionPage()
	{
		_vm = new FleasionViewModel();
		base.DataContext = _vm;
		InitializeComponent();
		Loaded += FleasionPage_Loaded;
	}

	private void FleasionPage_Loaded(object sender, RoutedEventArgs e)
	{
		_vm.Refresh();
	}

	private void EnableMaster_Click(object sender, RoutedEventArgs e)
	{
		_vm.MasterEnabled = true;
	}

	private void OpenFleasion_Click(object sender, RoutedEventArgs e)
	{
		_vm.OpenFleasion();
	}

	private void ClearCache_Click(object sender, RoutedEventArgs e)
	{
		_vm.ClearCache();
	}

	private void PickFile_Click(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement { Tag: FleasionFunction function })
		{
			_vm.PickFile(function);
		}
	}
}
