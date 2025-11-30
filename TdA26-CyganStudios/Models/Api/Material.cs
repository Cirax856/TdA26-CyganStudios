using System.Diagnostics;
using System.Text.Json.Serialization;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Models.Api;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(FileMaterial), "file")]
[JsonDerivedType(typeof(UrlMaterial), "url")]
public abstract record Material(Guid Uuid, string Name, string? Description)
{
    public static Material FromMaterial(DbMaterial material, string baseUrl)
        => material switch
        {
            DbFileMaterial fileMaterial => FromFileMaterial(fileMaterial, baseUrl),
            DbUrlMaterial urlMaterial => FromUrlMaterial(urlMaterial),
            _ => throw new UnreachableException(),
        };

    private static FileMaterial FromFileMaterial(DbFileMaterial material, string baseUrl)
        => new FileMaterial(material.Uuid, material.Name, material.Description, $"{baseUrl}/api/courses/{material.CourseId}/materials/{material.Uuid}/download", material.MimeType, material.SizeInBytes);

    private static UrlMaterial FromUrlMaterial(DbUrlMaterial material)
    {
        var uri = new Uri(material.Url);
        //var faviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico";
        var faviconUrl = $"https://www.google.com/s2/favicons?domain={uri.Host}";

        return new UrlMaterial(material.Uuid, material.Name, material.Description, material.Url, faviconUrl);
    }
}

public sealed record FileMaterial(Guid Uuid, string Name, string? Description, string FileUrl, string? MimeType, long? SizeInBytes)
    : Material(Uuid, Name, Description);

public sealed record UrlMaterial(Guid Uuid, string Name, string? Description, string Url, string? FaviconUrl)
    : Material(Uuid, Name, Description);