using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios;

public sealed record QuizzesGridModel(Guid CourseUuid, ICollection<DbQuiz> Quizzes, bool Editable);