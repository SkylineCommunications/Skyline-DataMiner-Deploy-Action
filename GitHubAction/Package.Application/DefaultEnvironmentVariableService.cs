﻿using Package.Domain.Services;

namespace Package.Application;

public class DefaultEnvironmentVariableService : IEnvironmentVariableService
{
    /// <inheritdoc />
    public Uri? GetSourceUri()
    {
        return null;
    }

    /// <inheritdoc />
    public Uri? GetReleaseUri()
    {
        return null;
    }

    public string GetBranch()
    {
        return string.Empty;
    }
}