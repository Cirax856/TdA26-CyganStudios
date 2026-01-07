using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TdA26_CyganStudios.Models.Db;

public class DbQuizSubmision
{
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid QuizId { get; set; }

    [ForeignKey(nameof(QuizId))]
    public DbQuiz Quiz { get; set; }

    public IList<DbQuizAnswer> Answers { get; set; }
}
