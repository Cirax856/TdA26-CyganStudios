namespace TdA26_CyganStudios.Models.Db;

public sealed record DbQuestion(Guid Uuid, string Name, string Question, IEnumerable<string> Options, bool IsMultiChoice, IEnumerable<int> CorrectIndices);