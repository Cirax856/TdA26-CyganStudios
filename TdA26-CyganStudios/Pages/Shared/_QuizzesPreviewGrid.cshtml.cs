using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios;

public sealed record QuizzesPreviewGridModel(Guid CourseUuid, ICollection<DbQuiz> Quizzes, bool Editable, int MaxPreviewCount = 8);