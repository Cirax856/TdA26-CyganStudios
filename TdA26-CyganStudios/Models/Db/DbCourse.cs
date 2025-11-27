using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TdA26_CyganStudios.Models.Db;

public class DbCourse
{
    [Key]
    public Guid Uuid { get; set; }

    public int LecturerId { get; set; }

    [ForeignKey(nameof(LecturerId))]
    public IdentityUser<int> Lecturer { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public long CreatedAt { get; set; }

    public long UpdatedAt { get; set; }

    public ICollection<DbMaterial> Materials { get; set; } = [];

    [NotMapped]
    public DateTimeOffset CreatedAtDT
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(UpdatedAt);
        set
        {
            CreatedAt = value.ToUnixTimeMilliseconds();
        }
    }

    [NotMapped]
    public DateTimeOffset UpdatedAtDT
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(UpdatedAt);
        set
        {
            UpdatedAt = value.ToUnixTimeMilliseconds();
        }
    }

    public DbCourse(IdentityUser<int> lecturer, string name, string? description, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(lecturer);
        ArgumentNullException.ThrowIfNull(name);

        Lecturer = lecturer;
        LecturerId = lecturer.Id;
        Name = name;
        Description = description;
        CreatedAtDT = now.UtcDateTime;
        UpdatedAtDT = now.UtcDateTime;
    }

    private DbCourse()
    {
        Lecturer = null!;
        Name = null!;
        Description = null!;
    }
}
