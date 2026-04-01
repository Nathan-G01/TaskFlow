namespace TaskFlow.Agents.Models;

public sealed record RunFilesOptions
{
    public RunFilesOptions(string planPath, string progressPath, string logPath)
    {
        PlanPath = Guard.NotNullOrWhiteSpace(planPath, nameof(planPath));
        ProgressPath = Guard.NotNullOrWhiteSpace(progressPath, nameof(progressPath));
        LogPath = Guard.NotNullOrWhiteSpace(logPath, nameof(logPath));
    }

    public string PlanPath { get; }

    public string ProgressPath { get; }

    public string LogPath { get; }
}
