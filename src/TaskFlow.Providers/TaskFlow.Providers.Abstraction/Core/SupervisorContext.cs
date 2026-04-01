namespace TaskFlow.Providers.Abstraction.Core;

public sealed record SupervisorContext
{
    public SupervisorContext(
        string objective,
        string planMarkdown,
        IReadOnlyList<TaskHistoryEntry>? taskHistory = null)
    {
        Objective = Guard.NotNullOrWhiteSpace(objective, nameof(objective));
        PlanMarkdown = Guard.NotNullOrWhiteSpace(planMarkdown, nameof(planMarkdown));
        TaskHistory = taskHistory ?? Array.Empty<TaskHistoryEntry>();
    }

    public string Objective { get; }

    public string PlanMarkdown { get; }

    public IReadOnlyList<TaskHistoryEntry> TaskHistory { get; }
}
