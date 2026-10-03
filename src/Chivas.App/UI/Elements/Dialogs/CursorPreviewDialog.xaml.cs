using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Chivas.Enums;
using Chivas.UI.Elements.Base;

namespace Chivas.UI.Elements.Dialogs;

public partial class CursorPreviewDialog : WpfUiWindow{

	public Chivas.Enums.CursorType? SelectedCursor { get; private set; }

	public CursorPreviewDialog()
	{
		InitializeComponent();
		LoadCursorPreviews();
		base.Closed += OnClosed;
	}

	private void LoadCursorPreviews()
	{
		Chivas.Enums.CursorType[] array = new Chivas.Enums.CursorType[9]
		{
			Chivas.Enums.CursorType.Default,
			Chivas.Enums.CursorType.FPSCursor,
			Chivas.Enums.CursorType.CleanCursor,
			Chivas.Enums.CursorType.DotCursor,
			Chivas.Enums.CursorType.StoofsCursor,
			Chivas.Enums.CursorType.From2006,
			Chivas.Enums.CursorType.From2013,
			Chivas.Enums.CursorType.WhiteDotCursor,
			Chivas.Enums.CursorType.VerySmallWhiteDot
		};
		foreach (Chivas.Enums.CursorType cursor in array)
		{
			FrameworkElement element = CreateCursorPreviewItem(cursor);
			CursorStackPanel.Children.Add(element);
		}
	}

	private Border CreateCursorPreviewItem(Chivas.Enums.CursorType cursor)
	{
		Border border = new Border
		{
			BorderBrush = new SolidColorBrush(Colors.Gray),
			BorderThickness = new Thickness(1.0),
			Margin = new Thickness(5.0),
			Padding = new Thickness(10.0),
			Background = new SolidColorBrush(Colors.Transparent),
			Cursor = Cursors.Hand
		};
		border.Tag = cursor;
		StackPanel stackPanel = new StackPanel
		{
			Orientation = Orientation.Horizontal
		};
		Image image = new Image
		{
			Width = 32.0,
			Height = 32.0,
			Margin = new Thickness(0.0, 0.0, 10.0, 0.0)
		};
		try
		{
			string cursorImagePath = GetCursorImagePath(cursor);
			if (!string.IsNullOrEmpty(cursorImagePath))
			{
				Uri uriSource = new Uri("pack://application:,,,/Resources/Mods/" + cursorImagePath);
				image.Source = Chivas.Utility.SafeImaging.FromUri(uriSource);
			}
		}
		catch
		{
			image.Source = null;
		}
		TextBlock element = new TextBlock
		{
			Text = GetCursorDisplayName(cursor),
			VerticalAlignment = VerticalAlignment.Center,
			FontSize = 14.0
		};
		stackPanel.Children.Add(image);
		stackPanel.Children.Add(element);
		border.Child = stackPanel;
		border.MouseLeftButtonUp += OnCursorClick;
		border.MouseEnter += OnCursorMouseEnter;
		border.MouseLeave += OnCursorMouseLeave;
		return border;
	}

	private void OnCursorClick(object sender, MouseButtonEventArgs e)
	{
		if (sender is not Border { Tag: Chivas.Enums.CursorType cursor })
			return;
		SelectedCursor = cursor;
		base.DialogResult = true;
		Close();
	}

	private static void OnCursorMouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is Border border)
			border.Background = new SolidColorBrush(Color.FromArgb(50, 100, 149, 237));
	}

	private static void OnCursorMouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is Border border)
			border.Background = new SolidColorBrush(Colors.Transparent);
	}

	private void OnClosed(object? sender, EventArgs e)
	{
		foreach (Border border in CursorStackPanel.Children.OfType<Border>())
		{
			border.MouseLeftButtonUp -= OnCursorClick;
			border.MouseEnter -= OnCursorMouseEnter;
			border.MouseLeave -= OnCursorMouseLeave;
		}
		base.Closed -= OnClosed;
	}

	private static string GetCursorImagePath(Chivas.Enums.CursorType cursor)
	{
		return cursor switch
		{
			Chivas.Enums.CursorType.FPSCursor => "Cursor/FPSCursor/ArrowCursor.png", 
			Chivas.Enums.CursorType.CleanCursor => "Cursor/CleanCursor/ArrowCursor.png", 
			Chivas.Enums.CursorType.DotCursor => "Cursor/DotCursor/ArrowCursor.png", 
			Chivas.Enums.CursorType.StoofsCursor => "Cursor/StoofsCursor/ArrowCursor.png", 
			Chivas.Enums.CursorType.From2006 => "Cursor/From2006/ArrowCursor.png", 
			Chivas.Enums.CursorType.From2013 => "Cursor/From2013/ArrowCursor.png", 
			Chivas.Enums.CursorType.WhiteDotCursor => "Cursor/WhiteDotCursor/ArrowCursor.png", 
			Chivas.Enums.CursorType.VerySmallWhiteDot => "Cursor/VerySmallWhiteDot/ArrowCursor.png", 
			_ => string.Empty, 
		};
	}

	private static string GetCursorDisplayName(Chivas.Enums.CursorType cursor)
	{
		return cursor switch
		{
			Chivas.Enums.CursorType.Default => "Default", 
			Chivas.Enums.CursorType.FPSCursor => "FPS Cursor (V1)", 
			Chivas.Enums.CursorType.CleanCursor => "Clean Cursor", 
			Chivas.Enums.CursorType.DotCursor => "Dot Cursor", 
			Chivas.Enums.CursorType.StoofsCursor => "Stoofs Cursor", 
			Chivas.Enums.CursorType.From2006 => "2006 Legacy Cursor", 
			Chivas.Enums.CursorType.From2013 => "2013 Legacy Cursor", 
			Chivas.Enums.CursorType.WhiteDotCursor => "White Dot Cursor", 
			Chivas.Enums.CursorType.VerySmallWhiteDot => "Very Small White Dot", 
			_ => cursor.ToString(), 
		};
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}
}
