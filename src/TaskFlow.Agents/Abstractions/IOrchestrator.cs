using TaskFlow.Agents.Models;

namespace TaskFlow.Agents.Abstractions;

public interface IOrchestrator
{
    ValueTask<OrchestrationResult> RunAsync(CancellationToken cancellationToken = default);
}
