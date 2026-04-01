using System.Text.Json.Serialization;

namespace TaskFlow.Providers.Abstraction.Protocol;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ExecuteTaskRequest(
    TaskAssignmentPayload Assignment,
    string ExecutionDirective);
