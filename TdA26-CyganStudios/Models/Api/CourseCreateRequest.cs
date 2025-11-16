using System.ComponentModel.DataAnnotations;

namespace TdA26_CyganStudios.Models.Api;

public sealed record CourseCreateRequest([Required] string Name, string? Description);