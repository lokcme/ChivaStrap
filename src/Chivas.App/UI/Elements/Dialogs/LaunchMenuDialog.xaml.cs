using System;
using Chivas.Enums;
using Chivas.UI.Elements.Base;
using Chivas.UI.ViewModels.Installer;

namespace Chivas.UI.Elements.Dialogs;

public partial class LaunchMenuDialog : WpfUiWindow
{
	private readonly LaunchMenuViewModel _viewModel;

	public NextAction CloseAction;

	public LaunchMenuDialog()
	{
		_viewModel = new LaunchMenuViewModel();
		_viewModel.CloseWindowRequest += OnCloseWindowRequest;
		base.DataContext = _viewModel;
		InitializeComponent();
		base.Closed += OnClosed;
	}

	private void OnCloseWindowRequest(object? sender, NextAction closeAction)
	{
		CloseAction = closeAction;
		Close();
	}

	private void OnClosed(object? sender, EventArgs e)
	{
		_viewModel.CloseWindowRequest -= OnCloseWindowRequest;
		base.Closed -= OnClosed;
	}
}
