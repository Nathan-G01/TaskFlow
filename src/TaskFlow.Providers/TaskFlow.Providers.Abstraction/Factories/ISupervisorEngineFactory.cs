using TaskFlow.Providers.Abstraction.Engines;

namespace TaskFlow.Providers.Abstraction.Factories;

public interface ISupervisorEngineFactory
{
    string ProviderName { get; }

    ISupervisorEngine CreateEngine();
}
