using TaskFlow.Agents.Models;
using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Agents.Abstractions;

public interface ISupervisor
{
    ValueTask<SupervisorTaskDecision> CreateNextTaskAsync(RunContext context, CancellationToken cancellationToken = default);

    ValueTask<TaskReview> ReviewTaskAsync(
        RunContext context,
        TaskAssignment assignment,
        TaskResult result,
        CancellationToken cancellationToken = default);
}
