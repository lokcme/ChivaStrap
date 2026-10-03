using System;

namespace Chivas.Exceptions;

internal class InvalidHTTPResponseException(string message) : Exception(message)
{
}
