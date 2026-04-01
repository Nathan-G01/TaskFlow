using TaskFlow.Providers.Abstraction.Core;

using System.Threading;
using System.Threading.Tasks;

namespace TaskFlow.Providers.Abstraction.Engines;

public interface IEngine
{
    EngineStatus Status { get; }

    ValueTask<TaskResult> ExecuteAsync(AgentContext context, TaskAssignment assignment, CancellationToken cancellationToken = default);
}
