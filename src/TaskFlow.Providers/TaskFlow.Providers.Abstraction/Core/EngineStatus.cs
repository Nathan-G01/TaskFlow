namespace TaskFlow.Providers.Abstraction.Core;

public sealed record EngineStatus
{
    public EngineStatus(string engineName, bool isAvailable, int maxWindowSize, int currentWindowSize, string? detail = null)
    {
        EngineName = Guard.NotNullOrWhiteSpace(engineName, nameof(engineName));
        IsAvailable = isAvailable;
        Detail = detail;
        MaxWindowSize = maxWindowSize;
        CurrentWindowSize = currentWindowSize;
    }

    public string EngineName { get; }

    public bool IsAvailable { get; }

    public string? Detail { get; }

    public int MaxWindowSize { get; }

    public int CurrentWindowSize { get; }

}
