using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Models.Api;

public sealed record CourseDetail(Guid Uuid, string Name, string? Description, IEnumerable<Material> Materials, IEnumerable<Quiz> Quizzes, IEnumerable<FeedItem> Feed)
{
    public static CourseDetail FromCourse(DbCourse course)
        => new CourseDetail(course.Uuid, course.Name, course.Description, [], [], []);
}
