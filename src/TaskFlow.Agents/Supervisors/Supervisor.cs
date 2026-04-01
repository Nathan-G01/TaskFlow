using TaskFlow.Agents.Abstractions;
using TaskFlow.Agents.Models;
using TaskFlow.Providers.Abstraction.Core;
using TaskFlow.Providers.Abstraction.Engines;

namespace TaskFlow.Agents.Supervisors;

public sealed class Supervisor : ISupervisor
{
    private readonly ISupervisorEngine _engine;

    public Supervisor(ISupervisorEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    public ValueTask<SupervisorTaskDecision> CreateNextTaskAsync(RunContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        return _engine.CreateNextTaskAsync(context.ToSupervisorContext(), cancellationToken);
    }

    public ValueTask<TaskReview> ReviewTaskAsync(
        RunContext context,
        TaskAssignment assignment,
        TaskResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(assignment);
        ArgumentNullException.ThrowIfNull(result);

        return _engine.ReviewTaskAsync(context.ToSupervisorContext(), assignment, result, cancellationToken);
    }
}
