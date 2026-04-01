using TaskFlow.Providers.Abstraction.Engines;
using TaskFlow.Providers.Abstraction.Factories;
using TaskFlow.Providers.Abstraction.Protocol;
using TaskFlow.Providers.Codex.Client;
using TaskFlow.Providers.Codex.Configuration;
using TaskFlow.Providers.Codex.Engines;

namespace TaskFlow.Providers.Codex.Factories;

public sealed class CodexEngineFactory : IAgentEngineFactory, ISupervisorEngineFactory
{
    private readonly CodexCliClient _client;
    private readonly CodexProviderOptions _options;
    private readonly IProviderProtocol _protocol;

    public CodexEngineFactory(CodexProviderOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _client = new CodexCliClient(_options.Command);
        _protocol = new JsonProviderProtocol();
    }

    public string ProviderName => "codex";

    public IEngine CreateEngine() => new CodexAgentEngine(_client, _options.Agent, _protocol);

    ISupervisorEngine ISupervisorEngineFactory.CreateEngine() => new CodexSupervisorEngine(_client, _options.Supervisor, _protocol);
}
