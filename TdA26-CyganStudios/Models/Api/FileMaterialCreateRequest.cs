using System.ComponentModel.DataAnnotations;

namespace TdA26_CyganStudios.Models.Api;

public sealed record class FileMaterialCreateRequest(string Type, [StringLength(100, MinimumLength = 1)] string Name, string? Description, IFormFile File);