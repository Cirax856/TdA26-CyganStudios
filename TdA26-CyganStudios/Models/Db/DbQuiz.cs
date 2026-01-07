using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TdA26_CyganStudios.Models.Db;

public class DbQuiz
{
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public DbCourse Course { get; set; }

    public string Title { get; set; }
    
    public int? AttemptsCount { get; set; }

    public IList<DbQuestion> Questions { get; set; }

    public ICollection<DbQuizSubmision> Submisions { get; set; } = [];
}
