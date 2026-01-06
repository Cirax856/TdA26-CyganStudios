namespace TdA26_CyganStudios.Models.Api;

public sealed record QuizAnswer(Guid? Uuid, int? SelectedIndex, IEnumerable<int>? SelectedIndices, string? Comment);