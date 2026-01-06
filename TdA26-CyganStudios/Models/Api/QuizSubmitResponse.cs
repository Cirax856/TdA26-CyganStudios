namespace TdA26_CyganStudios.Models.Api;

public sealed record QuizSubmitResponse(Guid QuizUuid, int Score, int MaxScore, bool[]? CorrectPerQuestion, DateTime SubmittedAt);
