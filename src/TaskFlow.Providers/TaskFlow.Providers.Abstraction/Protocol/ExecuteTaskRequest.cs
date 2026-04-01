using System.Text.Json.Serialization;

namespace TaskFlow.Providers.Abstraction.Protocol;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ExecuteTaskRequest(
    string Objective,
    string PlanMarkdown,
    string AgentInstructionsMarkdown,
    TaskAssignmentPayload Assignment,
    string ExecutionDirective);
