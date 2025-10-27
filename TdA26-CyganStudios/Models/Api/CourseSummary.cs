using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Models.Api;

public sealed record CourseSummary(Guid Uuid, string Name, string Description, DateTime CreatedAt, DateTime UpdatedAt)
{
    internal static CourseSummary FromCourse(DbCourse course)
        => new CourseSummary(course.Uuid, course.Name, course.Description, course.CreatedAt.UtcDateTime, course.UpdatedAt.UtcDateTime);
}
