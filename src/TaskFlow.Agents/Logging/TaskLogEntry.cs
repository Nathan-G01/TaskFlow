namespace TaskFlow.Agents.Logging;

public sealed record TaskLogEntry(
    DateTimeOffset Timestamp,
    TaskLogLevel Level,
    string EventType,
    string Message,
    Guid? TaskId = null,
    string? Status = null);
