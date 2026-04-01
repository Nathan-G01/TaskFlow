using System.Collections.ObjectModel;

namespace TaskFlow.Providers.Abstraction.Core;

public sealed record TaskResult
{
    private static readonly IReadOnlyDictionary<string, string> EmptyMetadata =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    public TaskResult(
        Guid taskId,
        TaskExecutionStatus status,
        string summary,
        string? output = null,
        string? rawPayload = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        TaskId = Guard.NotEmpty(taskId, nameof(taskId));
        Summary = Guard.NotNullOrWhiteSpace(summary, nameof(summary));
        Status = status;
        Output = output;
        RawPayload = rawPayload;
        Metadata = metadata is null
            ? EmptyMetadata
            : new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(metadata));
    }

    public Guid TaskId { get; }

    public TaskExecutionStatus Status { get; }

    public string Summary { get; }

    public string? Output { get; }

    public string? RawPayload { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
