using TdA26_CyganStudios;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios;

public sealed record QuizCardModel(DbQuiz Quiz, Guid CourseUuid, bool Editable);