using TaskFlow.Providers.Abstraction.Core;
using TaskFlow.Providers.Abstraction.Engines;
using TaskFlow.Providers.Abstraction.Protocol;
using TaskFlow.Providers.Codex.Client;
using TaskFlow.Providers.Codex.Configuration;
using TaskFlow.Providers.Codex.Support;

namespace TaskFlow.Providers.Codex.Engines;

public sealed class CodexSupervisorEngine : ISupervisorEngine
{
    private readonly CodexCliClient _client;
    private readonly CodexRoleOptions _options;
    private readonly IProviderProtocol _protocol;

    public CodexSupervisorEngine(CodexCliClient client, CodexRoleOptions options, IProviderProtocol protocol)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
    }

    public async ValueTask<SupervisorTaskDecision> CreateNextTaskAsync(SupervisorContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var request = ProviderProtocolMapper.ToCreateNextTaskRequest(context);
        var prompt = ProviderPromptFactory.CreateNextTaskPrompt(request, _protocol);
        var result = await _client.ExecuteAsync(prompt, CodexSchemaFactory.CreateNextTaskResponseSchema, _options, "create-next-task", cancellationToken);
        var response = _protocol.ParseCreateNextTaskResponse(result.FinalMessage);
        return ProviderProtocolMapper.ToSupervisorTaskDecision(response);
    }

    public async ValueTask<TaskReview> ReviewTaskAsync(
        SupervisorContext context,
        TaskAssignment assignment,
        TaskResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(assignment);
        ArgumentNullException.ThrowIfNull(result);

        var request = ProviderProtocolMapper.ToReviewTaskRequest(context, assignment, result);
        var prompt = ProviderPromptFactory.ReviewTaskPrompt(request, _protocol);
        var cliResult = await _client.ExecuteAsync(prompt, CodexSchemaFactory.ReviewTaskResponseSchema, _options, "review-task", cancellationToken);
        var response = _protocol.ParseReviewTaskResponse(cliResult.FinalMessage);
        return ProviderProtocolMapper.ToTaskReview(response, assignment.Id);
    }
}
