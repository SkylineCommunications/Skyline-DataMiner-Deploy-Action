﻿namespace Package.Domain.Models;

public class CreatedPackage
{
    public FileInfo Package { get; }
    public string Name { get; } 
    public string Type { get; } 
    public string Version { get; }

    public CreatedPackage(FileInfo package, string name, string type, string version)
    {
        Package = package;
        Name = name;
        Type = type;
        Version = version;
    }
}