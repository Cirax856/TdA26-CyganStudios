using System.ComponentModel.DataAnnotations;

namespace TdA26_CyganStudios.Models.Api;

public sealed record CourseCreateRequest([property: Required] string Name, string? Description);