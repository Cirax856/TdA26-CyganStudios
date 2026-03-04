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

    public CourseState State { get; set; }

    public ICollection<DbMaterial> Materials { get; set; } = [];

    public ICollection<DbQuiz> Quizzes { get; set; } = [];

    public ICollection<DbFeedItem> FeedItems { get; set; } = [];

    // if the lecturer schedules the course to be published in the future
    // the state remains Draft until the specified time has passed. once
    // the timestamp is reached a background task (or page hit) will flip
    // the state to Published and clear the value.
    public long? ScheduledPublishAt { get; set; }

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
    public DateTimeOffset UpdatedAtDT
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(UpdatedAt);
        set
        {
            UpdatedAt = value.ToUnixTimeMilliseconds();
        }
    }

    [NotMapped]
    public DateTimeOffset? ScheduledPublishAtDT
    {
        get => ScheduledPublishAt is { } ts ? DateTimeOffset.FromUnixTimeMilliseconds(ts) : null;
        set
        {
            if (value.HasValue)
            {
                ScheduledPublishAt = value.Value.ToUnixTimeMilliseconds();
            }
            else
            {
                ScheduledPublishAt = null;
            }
        }
    }

    public DbCourse(IdentityUser<int> lecturer, string name, string? description, CourseState state, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(lecturer);
        ArgumentNullException.ThrowIfNull(name);

        Lecturer = lecturer;
        LecturerId = lecturer.Id;
        Name = name;
        Description = description;
        State = state;
        CreatedAtDT = now;
        UpdatedAtDT = now;
    }

    private DbCourse()
    {
        Lecturer = null!;
        Name = null!;
        Description = null!;
    }
}
