using System;

namespace Chivas.Exceptions;

internal class ChecksumFailedException(string message) : Exception(message)
{
}
