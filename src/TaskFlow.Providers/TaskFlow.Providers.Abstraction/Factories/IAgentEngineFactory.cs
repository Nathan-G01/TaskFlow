using TaskFlow.Providers.Abstraction.Engines;

namespace TaskFlow.Providers.Abstraction.Factories;

public interface IAgentEngineFactory
{
    string ProviderName { get; }

    IEngine CreateEngine();
}
