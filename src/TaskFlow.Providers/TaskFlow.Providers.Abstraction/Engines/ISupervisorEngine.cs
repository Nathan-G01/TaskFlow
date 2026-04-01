using TaskFlow.Providers.Abstraction.Core;

using System.Threading;
using System.Threading.Tasks;

namespace TaskFlow.Providers.Abstraction.Engines;

public interface ISupervisorEngine
{
    ValueTask<SupervisorTaskDecision> CreateNextTaskAsync(SupervisorContext context, CancellationToken cancellationToken = default);

    ValueTask<TaskReview> ReviewTaskAsync(
        SupervisorContext context,
        TaskAssignment assignment,
        TaskResult result,
        CancellationToken cancellationToken = default);
}
