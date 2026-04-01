using TaskFlow.Providers.Abstraction.Factories;
using TaskFlow.Providers.Codex.Configuration;
using TaskFlow.Providers.Codex.Factories;

namespace TaskFlow.Cli;

internal sealed class ProviderRegistry
{
    private readonly IReadOnlyDictionary<string, IAgentEngineFactory> _agentFactories;
    private readonly IReadOnlyDictionary<string, ISupervisorEngineFactory> _supervisorFactories;

    public ProviderRegistry(IEnumerable<IAgentEngineFactory> agentFactories, IEnumerable<ISupervisorEngineFactory> supervisorFactories)
    {
        _agentFactories = agentFactories.ToDictionary(factory => factory.ProviderName, StringComparer.OrdinalIgnoreCase);
        _supervisorFactories = supervisorFactories.ToDictionary(factory => factory.ProviderName, StringComparer.OrdinalIgnoreCase);
    }

    public static ProviderRegistry CreateDefault(CodexProviderOptions codexOptions)
    {
        var codexFactory = new CodexEngineFactory(codexOptions);
        return new ProviderRegistry([codexFactory], [codexFactory]);
    }

    public IAgentEngineFactory GetAgentFactory(string providerName)
    {
        if (_agentFactories.TryGetValue(providerName, out var factory))
        {
            return factory;
        }

        throw new InvalidOperationException($"Unknown agent provider '{providerName}'. Registered providers: {string.Join(", ", _agentFactories.Keys.Order())}.");
    }

    public ISupervisorEngineFactory GetSupervisorFactory(string providerName)
    {
        if (_supervisorFactories.TryGetValue(providerName, out var factory))
        {
            return factory;
        }

        throw new InvalidOperationException($"Unknown supervisor provider '{providerName}'. Registered providers: {string.Join(", ", _supervisorFactories.Keys.Order())}.");
    }
}
