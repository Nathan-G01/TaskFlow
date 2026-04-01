using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Agents.Abstractions;

public interface IAgent
{
    ValueTask<TaskResult> ExecuteAsync(TaskAssignment assignment, CancellationToken cancellationToken = default);
}
