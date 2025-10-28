namespace Po.LearnCert.Shared.Models;

/// <summary>
/// User's performance statistics for a specific subtopic within a certification.
/// </summary>
public class SubtopicPerformanceDto
{
    /// <summary>
    /// Subtopic ID.
    /// </summary>
    public string SubtopicId { get; set; } = default!;

    /// <summary>
    /// Subtopic name.
    /// </summary>
    public string SubtopicName { get; set; } = default!;

    /// <summary>
    /// Certification ID this subtopic belongs to.
    /// </summary>
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Certification name.
    /// </summary>
    public string CertificationName { get; set; } = default!;

    /// <summary>
    /// Number of questions answered for this subtopic.
    /// </summary>
    public int QuestionsAnswered { get; set; }

    /// <summary>
    /// Number of correct answers for this subtopic.
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// Accuracy percentage for this subtopic.
    /// </summary>
    public decimal Accuracy { get; set; }

    /// <summary>
    /// Performance level based on accuracy.
    /// </summary>
    public PerformanceLevel PerformanceLevel { get; set; }

    /// <summary>
    /// Number of times this subtopic has been practiced.
    /// </summary>
    public int PracticeCount { get; set; }

    /// <summary>
    /// Date when this subtopic was last practiced.
    /// </summary>
    public DateTime? LastPracticedDate { get; set; }

    /// <summary>
    /// Average time spent per question for this subtopic.
    /// </summary>
    public TimeSpan AverageTimePerQuestion { get; set; }
}

/// <summary>
/// Performance level categories based on accuracy percentage.
/// </summary>
public enum PerformanceLevel
{
    /// <summary>
    /// Below 50% accuracy - needs significant improvement.
    /// </summary>
    NeedsImprovement = 0,

    /// <summary>
    /// 50-70% accuracy - basic understanding.
    /// </summary>
    Basic = 1,

    /// <summary>
    /// 70-85% accuracy - good understanding.
    /// </summary>
    Good = 2,

    /// <summary>
    /// 85-95% accuracy - excellent understanding.
    /// </summary>
    Excellent = 3,

    /// <summary>
    /// 95%+ accuracy - mastered the topic.
    /// </summary>
    Mastered = 4
}