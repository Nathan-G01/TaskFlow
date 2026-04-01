using TaskFlow.Providers.Abstraction.Core;
using TaskFlow.Providers.Abstraction.Engines;
using TaskFlow.Providers.Abstraction.Protocol;
using TaskFlow.Providers.Codex.Client;
using TaskFlow.Providers.Codex.Configuration;
using TaskFlow.Providers.Codex.Support;

namespace TaskFlow.Providers.Codex.Engines;

public sealed class CodexAgentEngine : IEngine
{
    private readonly CodexCliClient _client;
    private readonly CodexRoleOptions _options;
    private readonly IProviderProtocol _protocol;

    public CodexAgentEngine(CodexCliClient client, CodexRoleOptions options, IProviderProtocol protocol)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
    }

    public EngineStatus Status => new("Codex", true, 128000, 0, $"Model={_options.Model ?? "default"}, Sandbox={_options.Sandbox}");

    public async ValueTask<TaskResult> ExecuteAsync(AgentContext context, TaskAssignment assignment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(assignment);

        var request = ProviderProtocolMapper.ToExecuteTaskRequest(context, assignment);
        var prompt = ProviderPromptFactory.ExecuteTaskPrompt(request, _protocol);
        var cliResult = await _client.ExecuteAsync(prompt, CodexSchemaFactory.ExecuteTaskResponseSchema, _options, "execute-task", cancellationToken);
        var response = _protocol.ParseExecuteTaskResponse(cliResult.FinalMessage);
        return ProviderProtocolMapper.ToTaskResult(response, assignment.Id);
    }
}
