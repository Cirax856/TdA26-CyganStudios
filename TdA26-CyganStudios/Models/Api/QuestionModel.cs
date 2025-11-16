using System.Text.Json.Serialization;

namespace TdA26_CyganStudios.Models.Api;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SingleChoiceQuestion), "singleChoice")]
[JsonDerivedType(typeof(MultipleChoiceQuestion), "multipleChoice")]
public abstract record QuestionModel(Guid Uuid, string Name, string Question, IEnumerable<string> Options);

public sealed record SingleChoiceQuestion(Guid Uuid, string Name, string Question, IEnumerable<string> Options, int CorrectIndex)
    : QuestionModel(Uuid, Name, Question, Options);

public sealed record MultipleChoiceQuestion(Guid Uuid, string Name, string Question, IEnumerable<string> Options, IEnumerable<int> CorrectIndices)
    : QuestionModel(Uuid, Name, Question, Options);