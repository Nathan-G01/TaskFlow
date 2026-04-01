using TaskFlow.Agents.Abstractions;
using TaskFlow.Agents.Agents;
using TaskFlow.Providers.Abstraction.Factories;

namespace TaskFlow.Agents.Factories;

public sealed class AgentFactory : IAgentFactory
{
    private readonly IAgentEngineFactory _engineFactory;

    public AgentFactory(IAgentEngineFactory engineFactory)
    {
        _engineFactory = engineFactory ?? throw new ArgumentNullException(nameof(engineFactory));
    }

    public IAgent CreateAgent() => new Agent(_engineFactory.CreateEngine());
}
