using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.StaticFiles;

namespace TdA26_CyganStudios.Services;

public sealed class MimeTypeToExtensionProvider
{
    private readonly Dictionary<string, string> _reverse;

    public MimeTypeToExtensionProvider(FileExtensionContentTypeProvider forwardProvider)
    {
        _reverse = forwardProvider.Mappings
            .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
            .ToDictionary(
                g => g.Key,
                g => g.First()
            );
    }

    public string GetExtension(string contentType)
    {
        if (TryGetExtensions(contentType, out var extension))
        {
            return extension;
        }

        throw new KeyNotFoundException();
    }

    public bool TryGetExtensions(string contentType, [NotNullWhen(true)] out string? extension)
    {
        if (_reverse.TryGetValue(contentType, out var value))
        {
            extension = value;
            return true;
        }

        extension = null;
        return false;
    }
}