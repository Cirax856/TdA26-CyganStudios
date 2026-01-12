using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Models.Api;

public sealed record Quiz(Guid? Uuid, string Title, int? AttemptsCount, IEnumerable<QuestionModel> Questions)
{
    public static Quiz FromQuiz(DbQuiz quiz)
        => new Quiz(quiz.Uuid, quiz.Title, quiz.AttemptsCount, quiz.Questions.Select(QuestionModel.FromQuestion));

    public DbQuiz ToQuiz(Guid courseId)
    {
        var dbQuiz = new DbQuiz()
        {
            CourseId = courseId,
            Title = Title,
            AttemptsCount = AttemptsCount,
            Questions = Questions.Select(q => q.ToQuestion()).ToList(),
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        if (Uuid is { } uuid)
        {
            dbQuiz.Uuid = uuid;
        }

        return dbQuiz;
    }

    public void AssignTo(DbQuiz quiz)
    {
        quiz.Title = Title;
        quiz.AttemptsCount = AttemptsCount;
        quiz.Questions = Questions.Select(q => q.ToQuestion()).ToList();
    }
}