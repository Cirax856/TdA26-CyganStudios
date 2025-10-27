using System.Text.Json.Serialization;

namespace TdA26_CyganStudios.Models.Api;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(FileMaterial), "file")]
[JsonDerivedType(typeof(UrlMaterial), "url")]
public abstract record Material(Guid Uuid, string Name, string? Description);

public sealed record FileMaterial(Guid Uuid, string Name, string? Description, string FileUrl, string? MimeType, int? SizeInBytes)
    : Material(Uuid, Name, Description);

public sealed record UrlMaterial(Guid Uuid, string Name, string? Description, string Url, string? FaviconUrl)
    : Material(Uuid, Name, Description);