using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Agents.Models;

public sealed record RunContext
{
    public RunContext(
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

    public RunContext AddHistoryEntry(TaskHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return new RunContext(Objective, PlanMarkdown, TaskHistory.Concat([entry]).ToArray());
    }

    public SupervisorContext ToSupervisorContext() => new(Objective, PlanMarkdown, TaskHistory);
}
