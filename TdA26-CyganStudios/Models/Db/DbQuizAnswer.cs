namespace TdA26_CyganStudios.Models.Db;

public sealed record DbQuizAnswer(Guid? Uuid, int[]? SelectedIndices, string? Comment);