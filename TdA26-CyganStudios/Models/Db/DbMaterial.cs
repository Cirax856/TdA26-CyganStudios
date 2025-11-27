using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TdA26_CyganStudios.Models.Db;

public abstract class DbMaterial
{
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public DbCourse Course { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }
}

public sealed class DbFileMaterial : DbMaterial
{
    public Guid FileUuid { get; set; }

    public string MimeType { get; set; }

    public int SizeInBytes { get; set; }
}

public sealed class DbUrlMaterial : DbMaterial
{
    public string Url { get; set; }
}