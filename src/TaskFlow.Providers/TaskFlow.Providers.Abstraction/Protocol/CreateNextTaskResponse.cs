using System.Text.Json.Serialization;

namespace TaskFlow.Providers.Abstraction.Protocol;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateNextTaskResponse(
    bool IsComplete,
    string Summary,
    TaskAssignmentPayload? Assignment);
