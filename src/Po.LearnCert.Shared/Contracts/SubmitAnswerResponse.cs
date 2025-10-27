namespace Po.LearnCert.Shared.Contracts;

/// <summary>
/// Response after submitting an answer.
/// </summary>
public class SubmitAnswerResponse
{
    /// <summary>
    /// Whether the answer was correct.
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// The correct answer choice ID.
    /// </summary>
    public string CorrectChoiceId { get; set; } = default!;

    /// <summary>
    /// Explanation for the answer.
    /// </summary>
    public string Explanation { get; set; } = default!;

    /// <summary>
    /// Updated session progress.
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// Updated incorrect count.
    /// </summary>
    public int IncorrectAnswers { get; set; }

    /// <summary>
    /// Current question index after this answer.
    /// </summary>
    public int CurrentQuestionIndex { get; set; }

    /// <summary>
    /// Whether this was the last question.
    /// </summary>
    public bool IsSessionComplete { get; set; }
}
