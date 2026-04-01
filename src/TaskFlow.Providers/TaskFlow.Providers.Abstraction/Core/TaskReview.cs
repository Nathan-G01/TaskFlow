namespace TaskFlow.Providers.Abstraction.Core;

public sealed record TaskReview
{
    public TaskReview(Guid taskId, TaskValidationStatus status, string summary)
    {
        TaskId = Guard.NotEmpty(taskId, nameof(taskId));
        Status = status;
        Summary = Guard.NotNullOrWhiteSpace(summary, nameof(summary));
    }

    public Guid TaskId { get; }

    public TaskValidationStatus Status { get; }

    public string Summary { get; }
}
