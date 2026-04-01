namespace TaskFlow.Providers.Abstraction.Core;

public sealed record SupervisorContext
{
    public SupervisorContext(
        string objective,
        string planMarkdown,
        string supervisorInstructionsMarkdown,
        IReadOnlyList<TaskHistoryEntry>? taskHistory = null)
    {
        Objective = Guard.NotNullOrWhiteSpace(objective, nameof(objective));
        PlanMarkdown = Guard.NotNullOrWhiteSpace(planMarkdown, nameof(planMarkdown));
        SupervisorInstructionsMarkdown = Guard.NotNullOrWhiteSpace(supervisorInstructionsMarkdown, nameof(supervisorInstructionsMarkdown));
        TaskHistory = taskHistory ?? Array.Empty<TaskHistoryEntry>();
    }

    public string Objective { get; }

    public string PlanMarkdown { get; }

    public string SupervisorInstructionsMarkdown { get; }

    public IReadOnlyList<TaskHistoryEntry> TaskHistory { get; }
}
