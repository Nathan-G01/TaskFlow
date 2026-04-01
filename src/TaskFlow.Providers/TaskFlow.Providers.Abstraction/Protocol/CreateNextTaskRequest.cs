using System.Text.Json.Serialization;
using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Providers.Abstraction.Protocol;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateNextTaskRequest(
    string Objective,
    string PlanMarkdown,
    IReadOnlyList<TaskHistoryEntryPayload> TaskHistory);
