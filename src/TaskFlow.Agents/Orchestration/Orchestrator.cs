using TaskFlow.Agents.Abstractions;
using TaskFlow.Agents.Logging;
using TaskFlow.Agents.Models;
using TaskFlow.Providers.Abstraction.Core;
using TaskFlow.Providers.Abstraction.Protocol;
using System.Diagnostics;

namespace TaskFlow.Agents.Orchestration;

public sealed class Orchestrator : IOrchestrator
{
    private static readonly TimeSpan OperationHeartbeatInterval = TimeSpan.FromSeconds(15);

    private readonly ISupervisor _supervisor;
    private readonly IRunContextStore _runContextStore;
    private readonly IAgentFactory _agentFactory;
    private readonly ITaskLogger _logger;
    private IAgent? _agent;

    public Orchestrator(ISupervisor supervisor, IRunContextStore runContextStore, IAgentFactory agentFactory, ITaskLogger logger)
    {
        _supervisor = supervisor ?? throw new ArgumentNullException(nameof(supervisor));
        _runContextStore = runContextStore ?? throw new ArgumentNullException(nameof(runContextStore));
        _agentFactory = agentFactory ?? throw new ArgumentNullException(nameof(agentFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<OrchestrationResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var initialContext = await _runContextStore.LoadAsync(cancellationToken);
        var context = initialContext;
        var appendedHistory = new List<TaskHistoryEntry>();
        await LogAsync(TaskLogLevel.Information, "run-start", "Loaded plan.md and progress.md.", cancellationToken: cancellationToken);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SupervisorTaskDecision decision;
            try
            {
                var createNextTask = await ObserveOperationAsync(
                    operation: token => _supervisor.CreateNextTaskAsync(context, token),
                    startEventType: "supervisor-create-start",
                    startMessage: "Supervisor is creating the next task.",
                    runningEventType: "supervisor-create-running",
                    runningMessageFactory: elapsed => $"Supervisor is still creating the next task. Elapsed: {FormatElapsed(elapsed)}.",
                    taskId: null,
                    status: "Running",
                    cancellationToken: cancellationToken);
                decision = createNextTask.Result;
            }
            catch (ProviderProtocolException exception)
            {
                await LogAsync(TaskLogLevel.Error, "run-stopped", $"Supervisor protocol error while creating next task: {exception.Message}", cancellationToken: cancellationToken);
                throw;
            }

            if (decision.IsComplete)
            {
                await LogAsync(TaskLogLevel.Information, "run-complete", decision.Summary, cancellationToken: cancellationToken);
                return new OrchestrationResult(initialContext, context, appendedHistory);
            }

            var assignment = decision.Assignment
                ?? throw new InvalidOperationException("Supervisor returned a non-complete decision without an assignment.");
            await LogAsync(TaskLogLevel.Information, "task-proposed", decision.Summary, assignment.Id, "Proposed", cancellationToken);
            await LogAsync(TaskLogLevel.Information, "task-dispatched", $"Dispatching task '{assignment.Title}'.", assignment.Id, "Running", cancellationToken);

            TaskResult result;
            TaskReview review;

            try
            {
                var execution = await ObserveOperationAsync(
                    operation: token => GetAgent().ExecuteAsync(assignment, token),
                    startEventType: "task-execution-start",
                    startMessage: $"Agent started task '{assignment.Title}'.",
                    runningEventType: "task-execution-running",
                    runningMessageFactory: elapsed => $"Agent is still running task '{assignment.Title}'. Elapsed: {FormatElapsed(elapsed)}.",
                    taskId: assignment.Id,
                    status: "Running",
                    cancellationToken: cancellationToken);
                result = execution.Result;
                await LogAsync(
                    TaskLogLevel.Information,
                    "task-completed",
                    $"{result.Summary} Duration: {FormatElapsed(execution.Elapsed)}",
                    assignment.Id,
                    result.Status.ToString(),
                    cancellationToken);
            }
            catch (ProviderProtocolException exception)
            {
                result = new TaskResult(
                    assignment.Id,
                    TaskExecutionStatus.Failed,
                    $"Agent protocol error: {exception.Message}",
                    rawPayload: exception.ToString());
                review = new TaskReview(assignment.Id, TaskValidationStatus.Invalidated, "Task invalidated because agent execution violated the strict JSON protocol.");
                await LogAsync(TaskLogLevel.Error, "task-completed", result.Summary, assignment.Id, result.Status.ToString(), cancellationToken);
                goto PersistReviewedTask;
            }

            try
            {
                var reviewOperation = await ObserveOperationAsync(
                    operation: token => _supervisor.ReviewTaskAsync(context, assignment, result, token),
                    startEventType: "task-review-start",
                    startMessage: $"Supervisor started reviewing task '{assignment.Title}'.",
                    runningEventType: "task-review-running",
                    runningMessageFactory: elapsed => $"Supervisor is still reviewing task '{assignment.Title}'. Elapsed: {FormatElapsed(elapsed)}.",
                    taskId: assignment.Id,
                    status: "Running",
                    cancellationToken: cancellationToken);
                review = reviewOperation.Result;
                await LogAsync(
                    TaskLogLevel.Information,
                    "task-reviewed",
                    $"{review.Summary} Review duration: {FormatElapsed(reviewOperation.Elapsed)}",
                    assignment.Id,
                    review.Status.ToString(),
                    cancellationToken);
            }
            catch (ProviderProtocolException exception)
            {
                review = new TaskReview(assignment.Id, TaskValidationStatus.Invalidated, $"Supervisor review protocol error: {exception.Message}");
                await LogAsync(TaskLogLevel.Error, "task-reviewed", review.Summary, assignment.Id, review.Status.ToString(), cancellationToken);
                goto PersistReviewedTask;
            }

        PersistReviewedTask:
            var entry = new TaskHistoryEntry(
                assignment.Id,
                DateTimeOffset.UtcNow,
                assignment.Title,
                assignment.Instructions,
                assignment.ExpectedOutput,
                result.Status,
                review.Status,
                result.Summary,
                review.Summary);

            appendedHistory.Add(entry);
            await _runContextStore.AppendHistoryAsync(entry, cancellationToken);
            await LogAsync(TaskLogLevel.Information, "task-persisted", "Persisted reviewed task to progress.md.", assignment.Id, review.Status.ToString(), cancellationToken);
            context = context.AddHistoryEntry(entry);

            if (review.Status != TaskValidationStatus.Validated)
            {
                await LogAsync(TaskLogLevel.Error, "run-stopped", review.Summary, assignment.Id, review.Status.ToString(), cancellationToken);
                return new OrchestrationResult(initialContext, context, appendedHistory);
            }
        }
    }

    private IAgent GetAgent()
    {
        _agent ??= _agentFactory.CreateAgent();
        return _agent;
    }

    private ValueTask LogAsync(
        TaskLogLevel level,
        string eventType,
        string message,
        Guid? taskId = null,
        string? status = null,
        CancellationToken cancellationToken = default) =>
        _logger.LogAsync(new TaskLogEntry(DateTimeOffset.UtcNow, level, eventType, message, taskId, status), cancellationToken);

    private async ValueTask<(T Result, TimeSpan Elapsed)> ObserveOperationAsync<T>(
        Func<CancellationToken, ValueTask<T>> operation,
        string startEventType,
        string startMessage,
        string runningEventType,
        Func<TimeSpan, string> runningMessageFactory,
        Guid? taskId,
        string? status,
        CancellationToken cancellationToken)
    {
        await LogAsync(TaskLogLevel.Information, startEventType, startMessage, taskId, status, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var pending = operation(cancellationToken).AsTask();

        while (!pending.IsCompleted)
        {
            var delayTask = Task.Delay(OperationHeartbeatInterval, cancellationToken);
            var completed = await Task.WhenAny(pending, delayTask);
            if (completed == pending)
            {
                break;
            }

            await LogAsync(
                TaskLogLevel.Information,
                runningEventType,
                runningMessageFactory(stopwatch.Elapsed),
                taskId,
                status,
                cancellationToken);
        }

        var result = await pending;
        stopwatch.Stop();

        return (result, stopwatch.Elapsed);
    }

    private static string FormatElapsed(TimeSpan elapsed) =>
        elapsed.TotalHours >= 1
            ? elapsed.ToString(@"hh\:mm\:ss")
            : elapsed.ToString(@"mm\:ss");
}
