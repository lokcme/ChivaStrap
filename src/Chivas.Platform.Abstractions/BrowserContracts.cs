using System;

namespace Chivas.Platform;

public sealed class BrowserMessageReceivedEventArgs : EventArgs
{
	public BrowserMessageReceivedEventArgs(string message)
	{
		Message = message;
	}

	public string Message { get; }
}
