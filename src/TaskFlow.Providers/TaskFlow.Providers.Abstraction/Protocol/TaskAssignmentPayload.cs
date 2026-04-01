using System.Text.Json.Serialization;

namespace TaskFlow.Providers.Abstraction.Protocol;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record TaskAssignmentPayload(
    Guid Id,
    string Title,
    string Instructions,
    string ExpectedOutput,
    IReadOnlyDictionary<string, string>? Metadata = null);
