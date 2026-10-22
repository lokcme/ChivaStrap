using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Markup;
using Chivas.Resources;
using Chivas.UI.Elements.Base;
using Chivas.UI.Elements.Controls;
using Windows.Win32;
using Windows.Win32.Foundation;
using Wpf.Ui.Controls;

namespace Chivas.UI.Elements.Dialogs;

public partial class ExceptionDialog : WpfUiWindow{
	private readonly string _issueUrl;
	private readonly string _backupIssueUrl;

	public ExceptionDialog(Exception exception)
	{
		InitializeComponent();
		AddExceptionToTextBox(exception);
		if (!App.Logger.Initialized)
		{
			LocateLogFileButton.Content = Strings.Dialog_Exception_CopyLogContents;
		}
		string wikiUrl = App.ProjectHelpLink;
		_issueUrl = App.ProjectSupportLink;
		HelpMessageMDTextBlock.MarkdownText = GetHelpMessage(wikiUrl, _issueUrl);
		VersionText.Text = string.Format(Strings.Dialog_Exception_Version, App.Version);
		ReportExceptionButton.Click += OnReportExceptionClick;
		_backupIssueUrl = App.ProjectFallbackRepository + "/issues/new";
		ReportExceptionBackupButton.Click += OnReportExceptionBackupClick;
		LocateLogFileButton.Click += OnLocateLogFileClick;
		CopyDetailsButton.Click += OnCopyDetailsClick;
		CloseButton.Click += OnCloseClick;
		base.Closed += OnDialogClosed;
		Chivas.Utility.SafeSystemSounds.Play(Chivas.Utility.SafeSystemSounds.Get(MessageBoxImage.Hand));
		base.Loaded += OnLoaded;
	}

	private string _exceptionText = string.Empty;

	private System.Windows.Threading.DispatcherTimer? _copyFeedbackTimer;

	private void AddExceptionToTextBox(Exception exception)
	{
		AppendException(exception, isInner: false);
		_exceptionText = ErrorRichTextBox.Selection.Text;
		void AppendException(Exception ex, bool isInner)
		{
			if (ex != null)
			{
				if (!isInner)
				{
					ErrorRichTextBox.Selection.Text = $"{ex.GetType()}: {ex.Message}";
				}
				else
				{
					ErrorRichTextBox.Selection.Text += $"\n\n[Inner Exception]\n{ex.GetType()}: {ex.Message}";
				}
				AppendException(ex.InnerException!, isInner: true);
			}
		}
	}

	private void OnCopyDetailsClick(object sender, RoutedEventArgs e)
	{
		string details = _exceptionText;
		if (string.IsNullOrWhiteSpace(details))
		{
			details = new TextRange(ErrorRichTextBox.Document.ContentStart, ErrorRichTextBox.Document.ContentEnd).Text;
		}
		string payload = string.Format(Strings.Dialog_Exception_Version, App.Version) + Environment.NewLine + Environment.NewLine + details.Trim();
		try
		{
			Chivas.Utility.ClipboardService.SetDataObject(payload, copy: true);
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("ExceptionDialog::Copy", "Clipboard write failed: " + ex.Message);
			return;
		}
		ShowCopyFeedback();
	}

	private void ShowCopyFeedback()
	{
		CopyDetailsButton.Content = "Copied";
		if (_copyFeedbackTimer == null)
		{
			_copyFeedbackTimer = new System.Windows.Threading.DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(1600.0)
			};
			_copyFeedbackTimer.Tick += OnCopyFeedbackTick;
		}
		_copyFeedbackTimer.Stop();
		_copyFeedbackTimer.Start();
	}

	private void OnCopyFeedbackTick(object? sender, EventArgs e)
	{
		_copyFeedbackTimer?.Stop();
		CopyDetailsButton.Content = Strings.Menu_Dialog_ExceptionCopy;
	}

	private void OnDialogClosed(object? sender, EventArgs e)
	{
		base.Closed -= OnDialogClosed;
		base.Loaded -= OnLoaded;
		ReportExceptionButton.Click -= OnReportExceptionClick;
		ReportExceptionBackupButton.Click -= OnReportExceptionBackupClick;
		CloseButton.Click -= OnCloseClick;
		CopyDetailsButton.Click -= OnCopyDetailsClick;
		LocateLogFileButton.Click -= OnLocateLogFileClick;
		if (_copyFeedbackTimer != null)
		{
			_copyFeedbackTimer.Stop();
			_copyFeedbackTimer.Tick -= OnCopyFeedbackTick;
			_copyFeedbackTimer = null;
		}
	}

	private void OnReportExceptionClick(object sender, RoutedEventArgs e)
	{
		Utilities.ShellExecute(_issueUrl);
	}

	private void OnReportExceptionBackupClick(object sender, RoutedEventArgs e)
	{
		Utilities.ShellExecute(_backupIssueUrl);
	}

	private void OnCloseClick(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		FlashWindowOnLoad();
	}

	private static string GetHelpMessage(string wikiUrl, string issueUrl)
	{
		if (!App.IsActionBuild && !App.BuildMetadata.Machine.Contains("pizzaboxer", StringComparison.Ordinal))
		{
			return string.Format(Strings.Dialog_Exception_Info_2_Alt, wikiUrl);
		}
		return string.Format(Strings.Dialog_Exception_Info_2, wikiUrl, issueUrl);
	}

	private void OnLocateLogFileClick(object sender, RoutedEventArgs e)
	{
		if (App.Logger.Initialized && !string.IsNullOrEmpty(App.Logger.FileLocation))
		{
			Utilities.OpenTextFile(App.Logger.FileLocation);
		}
		else
		{
			Chivas.Utility.ClipboardService.SetDataObject(App.Logger.AsDocument);
		}
	}

	private void FlashWindowOnLoad()
	{
		if (Chivas.Utility.Platform.IsWindows) { Windows.Win32.PInvoke.FlashWindow((HWND)new WindowInteropHelper(this).Handle, true); }
	}
}
