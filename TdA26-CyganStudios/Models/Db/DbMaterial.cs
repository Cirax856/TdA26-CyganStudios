using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TdA26_CyganStudios.Models.Db;

public abstract class DbMaterial
{
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public DbCourse Course { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required long CreatedAt { get; set; }

    [NotMapped]
    public DateTimeOffset CreatedAtDT
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(CreatedAt);
        set
        {
            CreatedAt = value.ToUnixTimeMilliseconds();
        }
    }
}

public sealed class DbFileMaterial : DbMaterial
{
    public required Guid FileUuid { get; set; }

    public required string MimeType { get; set; }

    public required int SizeInBytes { get; set; }
}

public sealed class DbUrlMaterial : DbMaterial
{
    public required string Url { get; set; }
}