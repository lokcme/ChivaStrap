using System;

namespace Chivas.Exceptions;

internal class AssertionException(string message) : Exception(message + "\n\nThis is very likely just an off-chance error. Please report this first, and then start Chivas again.")
{
}
