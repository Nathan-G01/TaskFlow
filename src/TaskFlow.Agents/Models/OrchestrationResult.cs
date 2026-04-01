using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Agents.Models;

public sealed record OrchestrationResult(
    RunContext InitialContext,
    RunContext FinalContext,
    IReadOnlyList<TaskHistoryEntry> AppendedHistory);
