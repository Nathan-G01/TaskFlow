namespace TaskFlow.Providers.Abstraction.Core;

public sealed record TaskHistoryEntry
{
    public TaskHistoryEntry(
        Guid taskId,
        DateTimeOffset timestamp,
        string title,
        string instructions,
        string expectedOutput,
        TaskExecutionStatus executionStatus,
        TaskValidationStatus validationStatus,
        string resultSummary,
        string validationSummary)
    {
        TaskId = Guard.NotEmpty(taskId, nameof(taskId));
        Timestamp = timestamp;
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
        Instructions = Guard.NotNullOrWhiteSpace(instructions, nameof(instructions));
        ExpectedOutput = Guard.NotNullOrWhiteSpace(expectedOutput, nameof(expectedOutput));
        ExecutionStatus = executionStatus;
        ValidationStatus = validationStatus;
        ResultSummary = Guard.NotNullOrWhiteSpace(resultSummary, nameof(resultSummary));
        ValidationSummary = Guard.NotNullOrWhiteSpace(validationSummary, nameof(validationSummary));
    }

    public Guid TaskId { get; }

    public DateTimeOffset Timestamp { get; }

    public string Title { get; }

    public string Instructions { get; }

    public string ExpectedOutput { get; }

    public TaskExecutionStatus ExecutionStatus { get; }

    public TaskValidationStatus ValidationStatus { get; }

    public string ResultSummary { get; }

    public string ValidationSummary { get; }
}
