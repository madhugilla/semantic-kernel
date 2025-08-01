// Copyright (c) Microsoft. All rights reserved.

namespace AzureAIAgentSamples.Utilities;

/// <summary>
/// Simple utility to read resource files from the Resources directory.
/// </summary>
internal static class ResourceHelper
{
    /// <summary>
    /// Read a text file from the Resources directory.
    /// </summary>
    /// <param name="fileName">The name of the file to read.</param>
    /// <returns>The content of the file as a string.</returns>
    internal static string Read(string fileName)
    {
        var resourcePath = Path.Combine("Resources", fileName);
        if (!File.Exists(resourcePath))
        {
            throw new FileNotFoundException($"Resource file not found: {resourcePath}");
        }
        return File.ReadAllText(resourcePath);
    }

    /// <summary>
    /// Read a file from the Resources directory as a stream.
    /// </summary>
    /// <param name="fileName">The name of the file to read.</param>
    /// <returns>A stream for the file.</returns>
    internal static Stream ReadStream(string fileName)
    {
        var resourcePath = Path.Combine("Resources", fileName);
        if (!File.Exists(resourcePath))
        {
            throw new FileNotFoundException($"Resource file not found: {resourcePath}");
        }
        return File.OpenRead(resourcePath);
    }

    /// <summary>
    /// Read a file from the Resources directory as bytes.
    /// </summary>
    /// <param name="fileName">The name of the file to read.</param>
    /// <returns>The content of the file as a ReadOnlyMemory&lt;byte&gt;.</returns>
    internal static async Task<ReadOnlyMemory<byte>> ReadAllAsync(string fileName)
    {
        var resourcePath = Path.Combine("Resources", fileName);
        if (!File.Exists(resourcePath))
        {
            throw new FileNotFoundException($"Resource file not found: {resourcePath}");
        }
        var bytes = await File.ReadAllBytesAsync(resourcePath);
        return new ReadOnlyMemory<byte>(bytes);
    }
}