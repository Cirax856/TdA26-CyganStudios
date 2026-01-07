using System.Text.Json.Serialization;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Models.Api;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SingleChoiceQuestion), "singleChoice")]
[JsonDerivedType(typeof(MultipleChoiceQuestion), "multipleChoice")]
public abstract record QuestionModel(Guid? Uuid, string Name, string Question, IEnumerable<string> Options)
{
    public static QuestionModel FromQuestion(DbQuestion question)
    {
        return question.IsMultiChoice
            ? new MultipleChoiceQuestion(question.Uuid, question.Name, question.Question, question.Options, question.CorrectIndices)
            : new SingleChoiceQuestion(question.Uuid, question.Name, question.Question, question.Options, question.CorrectIndices.First());
    }

    public DbQuestion ToQuestion()
        => new DbQuestion(Uuid ?? Guid.NewGuid(), Name, Question, Options, this is MultipleChoiceQuestion, (this as MultipleChoiceQuestion)?.CorrectIndices ?? [((SingleChoiceQuestion)this).CorrectIndex]);
}

public sealed record SingleChoiceQuestion(Guid? Uuid, string Name, string Question, IEnumerable<string> Options, int CorrectIndex)
    : QuestionModel(Uuid, Name, Question, Options);

public sealed record MultipleChoiceQuestion(Guid? Uuid, string Name, string Question, IEnumerable<string> Options, IEnumerable<int> CorrectIndices)
    : QuestionModel(Uuid, Name, Question, Options);