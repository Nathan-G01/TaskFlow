using System.Text.Json.Serialization;
using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Providers.Abstraction.Protocol;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ReviewTaskRequest(
    string Objective,
    string PlanMarkdown,
    TaskAssignmentPayload Assignment,
    ExecuteTaskResponse Result,
    IReadOnlyList<TaskHistoryEntryPayload> TaskHistory);
