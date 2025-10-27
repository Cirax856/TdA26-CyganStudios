namespace TdA26_CyganStudios.Models.Api;

public sealed record Quiz(Guid Uuid, string Title, int AttemptsCount, IEnumerable<QuestionModel> Questions);