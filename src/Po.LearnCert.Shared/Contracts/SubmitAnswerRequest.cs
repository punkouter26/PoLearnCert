using System.ComponentModel.DataAnnotations;

namespace Po.LearnCert.Shared.Contracts;

/// <summary>
/// Request to submit an answer for a question.
/// </summary>
public class SubmitAnswerRequest
{
    /// <summary>
    /// Session ID.
    /// </summary>
    [Required]
    public string SessionId { get; set; } = default!;

    /// <summary>
    /// Question ID being answered.
    /// </summary>
    [Required]
    public string QuestionId { get; set; } = default!;

    /// <summary>
    /// Selected answer choice ID.
    /// </summary>
    [Required]
    public string SelectedChoiceId { get; set; } = default!;
}
