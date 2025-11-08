using System;

namespace fluxel.API.Components;

public class CacheMissingException : Exception
{
    public CacheMissingException(string key)
        : base($"Key '{key}' was missing in validation cache.")
    {
    }
}
