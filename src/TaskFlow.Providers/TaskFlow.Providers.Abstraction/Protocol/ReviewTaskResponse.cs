using System.Text.Json.Serialization;
using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Providers.Abstraction.Protocol;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ReviewTaskResponse(
    Guid TaskId,
    TaskValidationStatus Status,
    string Summary);
