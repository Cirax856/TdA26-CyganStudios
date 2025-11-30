namespace TdA26_CyganStudios.Models.Api;

public sealed record class FileMaterialUpdateRequest(string? Name, string? Description, IFormFile? File);