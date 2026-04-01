namespace TaskFlow.Agents.Models;

public sealed record RunFilesOptions
{
    public RunFilesOptions(string planPath, string progressPath, string logPath, int maxAgentTrials)
    {
        PlanPath = Guard.NotNullOrWhiteSpace(planPath, nameof(planPath));
        ProgressPath = Guard.NotNullOrWhiteSpace(progressPath, nameof(progressPath));
        LogPath = Guard.NotNullOrWhiteSpace(logPath, nameof(logPath));
        MaxAgentTrials = maxAgentTrials < 1
            ? throw new ArgumentOutOfRangeException(nameof(maxAgentTrials), "Max agent trials must be at least 1.")
            : maxAgentTrials;
    }

    public string PlanPath { get; }

    public string ProgressPath { get; }

    public string LogPath { get; }

    public int MaxAgentTrials { get; }
}
