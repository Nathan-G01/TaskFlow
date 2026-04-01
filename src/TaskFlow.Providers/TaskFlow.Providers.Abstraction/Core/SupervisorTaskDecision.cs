namespace TaskFlow.Providers.Abstraction.Core;

public sealed record SupervisorTaskDecision
{
    private SupervisorTaskDecision(TaskAssignment? assignment, bool isComplete, string summary)
    {
        if (isComplete && assignment is not null)
        {
            throw new ArgumentException("A complete decision cannot include an assignment.", nameof(assignment));
        }

        if (!isComplete && assignment is null)
        {
            throw new ArgumentException("A non-complete decision must include an assignment.", nameof(assignment));
        }

        Assignment = assignment;
        IsComplete = isComplete;
        Summary = Guard.NotNullOrWhiteSpace(summary, nameof(summary));
    }

    public TaskAssignment? Assignment { get; }

    public bool IsComplete { get; }

    public string Summary { get; }

    public static SupervisorTaskDecision Complete(string summary) => new(null, true, summary);

    public static SupervisorTaskDecision CreateTask(TaskAssignment assignment, string summary) => new(assignment, false, summary);
}
