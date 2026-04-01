using TaskFlow.Providers.Abstraction.Core;
using TaskFlow.Agents.Models;

namespace TaskFlow.Agents.Abstractions;

public interface IAgent
{
    ValueTask<TaskResult> ExecuteAsync(RunContext context, TaskAssignment assignment, CancellationToken cancellationToken = default);
}
