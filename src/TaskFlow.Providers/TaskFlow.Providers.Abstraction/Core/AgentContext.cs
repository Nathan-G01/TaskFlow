namespace TaskFlow.Providers.Abstraction.Core;

public sealed record AgentContext
{
    public AgentContext(
        string objective,
        string planMarkdown,
        string agentInstructionsMarkdown)
    {
        Objective = Guard.NotNullOrWhiteSpace(objective, nameof(objective));
        PlanMarkdown = Guard.NotNullOrWhiteSpace(planMarkdown, nameof(planMarkdown));
        AgentInstructionsMarkdown = Guard.NotNullOrWhiteSpace(agentInstructionsMarkdown, nameof(agentInstructionsMarkdown));
    }

    public string Objective { get; }

    public string PlanMarkdown { get; }

    public string AgentInstructionsMarkdown { get; }
}
