using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Agents.Models;

public sealed record RunContext
{
    public RunContext(
        string objective,
        string planMarkdown,
        string supervisorInstructionsMarkdown,
        string agentInstructionsMarkdown,
        IReadOnlyList<TaskHistoryEntry>? taskHistory = null)
    {
        Objective = Guard.NotNullOrWhiteSpace(objective, nameof(objective));
        PlanMarkdown = Guard.NotNullOrWhiteSpace(planMarkdown, nameof(planMarkdown));
        SupervisorInstructionsMarkdown = Guard.NotNullOrWhiteSpace(supervisorInstructionsMarkdown, nameof(supervisorInstructionsMarkdown));
        AgentInstructionsMarkdown = Guard.NotNullOrWhiteSpace(agentInstructionsMarkdown, nameof(agentInstructionsMarkdown));
        TaskHistory = taskHistory ?? Array.Empty<TaskHistoryEntry>();
    }

    public string Objective { get; }

    public string PlanMarkdown { get; }

    public string SupervisorInstructionsMarkdown { get; }

    public string AgentInstructionsMarkdown { get; }

    public IReadOnlyList<TaskHistoryEntry> TaskHistory { get; }

    public RunContext AddHistoryEntry(TaskHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return new RunContext(
            Objective,
            PlanMarkdown,
            SupervisorInstructionsMarkdown,
            AgentInstructionsMarkdown,
            TaskHistory.Concat([entry]).ToArray());
    }

    public SupervisorContext ToSupervisorContext() => new(Objective, PlanMarkdown, SupervisorInstructionsMarkdown, TaskHistory);

    public AgentContext ToAgentContext() => new(Objective, PlanMarkdown, AgentInstructionsMarkdown);
}
