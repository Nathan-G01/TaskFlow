using TaskFlow.Providers.Abstraction.Core;

using System.Threading;
using System.Threading.Tasks;

namespace TaskFlow.Providers.Abstraction.Engines;

public interface IEngine
{
    EngineStatus Status { get; }

    ValueTask<TaskResult> ExecuteAsync(TaskAssignment assignment, CancellationToken cancellationToken = default);
}
