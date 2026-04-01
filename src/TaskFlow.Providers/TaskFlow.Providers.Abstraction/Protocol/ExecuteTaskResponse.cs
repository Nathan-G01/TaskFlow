using System.Text.Json.Serialization;
using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Providers.Abstraction.Protocol;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ExecuteTaskResponse(
    Guid TaskId,
    TaskExecutionStatus Status,
    string Summary,
    string? Output = null,
    string? RawPayload = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
