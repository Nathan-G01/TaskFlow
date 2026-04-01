using System.Text.Json.Serialization;
using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Providers.Abstraction.Protocol;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record TaskHistoryEntryPayload(
    Guid TaskId,
    DateTimeOffset Timestamp,
    string Title,
    string Instructions,
    string ExpectedOutput,
    TaskExecutionStatus ExecutionStatus,
    TaskValidationStatus ValidationStatus,
    string ResultSummary,
    string ValidationSummary);
