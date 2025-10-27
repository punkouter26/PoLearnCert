using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Client.Features.Quiz;

/// <summary>
/// State management for quiz session.
/// </summary>
public class QuizSessionState
{
    private QuizSessionDto? _session;
    private QuestionDto? _currentQuestion;
    private List<string> _answeredQuestionIds = new();

    public event Action? OnStateChanged;

    /// <summary>
    /// Gets the current quiz session.
    /// </summary>
    public QuizSessionDto? Session => _session;

    /// <summary>
    /// Gets the current question being displayed.
    /// </summary>
    public QuestionDto? CurrentQuestion => _currentQuestion;

    /// <summary>
    /// Gets the list of already answered question IDs.
    /// </summary>
    public IReadOnlyList<string> AnsweredQuestionIds => _answeredQuestionIds.AsReadOnly();

    /// <summary>
    /// Gets whether there is an active session.
    /// </summary>
    public bool HasActiveSession => _session != null && !_session.IsCompleted;

    /// <summary>
    /// Gets the current question number (1-based).
    /// </summary>
    public int CurrentQuestionNumber => _session?.CurrentQuestionIndex + 1 ?? 0;

    /// <summary>
    /// Gets the total number of questions in the session.
    /// </summary>
    public int TotalQuestions => _session?.QuestionIds.Count ?? 0;

    /// <summary>
    /// Gets the progress percentage.
    /// </summary>
    public int ProgressPercentage => TotalQuestions > 0 
        ? (int)Math.Round((double)_session!.CurrentQuestionIndex / TotalQuestions * 100) 
        : 0;

    /// <summary>
    /// Initializes a new quiz session.
    /// </summary>
    public void StartSession(QuizSessionDto session)
    {
        _session = session;
        _currentQuestion = null;
        _answeredQuestionIds.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Sets the current question being displayed.
    /// </summary>
    public void SetCurrentQuestion(QuestionDto question)
    {
        _currentQuestion = question;
        NotifyStateChanged();
    }

    /// <summary>
    /// Records that a question has been answered.
    /// </summary>
    public void RecordAnswer(string questionId)
    {
        if (!_answeredQuestionIds.Contains(questionId))
        {
            _answeredQuestionIds.Add(questionId);
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Updates the session after an answer is submitted.
    /// </summary>
    public void UpdateSession(QuizSessionDto updatedSession)
    {
        _session = updatedSession;
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears the current session.
    /// </summary>
    public void ClearSession()
    {
        _session = null;
        _currentQuestion = null;
        _answeredQuestionIds.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Gets the next question ID to display.
    /// </summary>
    public string? GetNextQuestionId()
    {
        if (_session == null || _session.CurrentQuestionIndex >= _session.QuestionIds.Count)
        {
            return null;
        }

        return _session.QuestionIds[_session.CurrentQuestionIndex];
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
