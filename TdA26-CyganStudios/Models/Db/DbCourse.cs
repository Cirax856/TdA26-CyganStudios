using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TdA26_CyganStudios.Models.Db;

public class DbCourse
{
    [Key]
    public Guid Uuid { get; set; }

    public int LecturerId { get; set; }

    [ForeignKey("LecturerId")]
    public IdentityUser<int> Lecturer { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DbCourse(IdentityUser<int> lecturer, string name, string? description, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(lecturer);
        ArgumentNullException.ThrowIfNull(name);

        Lecturer = lecturer;
        LecturerId = lecturer.Id;
        Name = name;
        Description = description;
        CreatedAt = now;
        UpdatedAt = now;
    }

    private DbCourse()
    {
        Lecturer = null!;
        Name = null!;
        Description = null!;
    }
}
