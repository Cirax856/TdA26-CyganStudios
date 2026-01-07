using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TdA26_CyganStudios.Models.Db;

public class DbFeedItem
{
    [Key]
    public Guid Uuid { get; set; }

    public Guid CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public DbCourse Course { get; set; }

    public DbFeedItemType Type { get; set; }

    public string Message { get; set; }

    public bool Edited { get; set; }

    public long CreatedAt { get; set; }

    public long? UpdatedAt { get; set; }

    [NotMapped]
    public DateTimeOffset CreatedAtDT
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(CreatedAt);
        set
        {
            CreatedAt = value.ToUnixTimeMilliseconds();
        }
    }

    [NotMapped]
    public DateTimeOffset? UpdatedAtDT
    {
        get => UpdatedAt is { } updatedAt ? DateTimeOffset.FromUnixTimeMilliseconds(updatedAt) : null;
        set
        {
            UpdatedAt = value?.ToUnixTimeMilliseconds();
        }
    }
}

public enum DbFeedItemType
{
    Manual,
    System,
}