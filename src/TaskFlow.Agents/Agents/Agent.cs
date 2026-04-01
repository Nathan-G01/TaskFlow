using TaskFlow.Agents.Abstractions;
using TaskFlow.Agents.Models;
using TaskFlow.Providers.Abstraction.Core;
using TaskFlow.Providers.Abstraction.Engines;

namespace TaskFlow.Agents.Agents;

public sealed class Agent : IAgent
{
    private readonly IEngine _engine;

    public Agent(IEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    public async ValueTask<TaskResult> ExecuteAsync(RunContext context, TaskAssignment assignment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(assignment);

        return await _engine.ExecuteAsync(context.ToAgentContext(), assignment, cancellationToken);
    }
}
