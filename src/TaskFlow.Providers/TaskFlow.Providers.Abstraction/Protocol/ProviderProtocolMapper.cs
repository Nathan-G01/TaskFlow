using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Providers.Abstraction.Protocol;

public static class ProviderProtocolMapper
{
    public static CreateNextTaskRequest ToCreateNextTaskRequest(SupervisorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new CreateNextTaskRequest(
            context.Objective,
            context.PlanMarkdown,
            context.SupervisorInstructionsMarkdown,
            context.TaskHistory.Select(ToTaskHistoryEntryPayload).ToArray());
    }

    public static SupervisorTaskDecision ToSupervisorTaskDecision(CreateNextTaskResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.IsComplete)
        {
            if (response.Assignment is not null)
            {
                throw new ProviderProtocolException("CreateNextTask response cannot include an assignment when isComplete is true.");
            }

            return SupervisorTaskDecision.Complete(response.Summary);
        }

        if (response.Assignment is null)
        {
            throw new ProviderProtocolException("CreateNextTask response must include an assignment when isComplete is false.");
        }

        return SupervisorTaskDecision.CreateTask(
            new TaskAssignment(
                response.Assignment.Id,
                response.Assignment.Title,
                response.Assignment.Instructions,
                response.Assignment.ExpectedOutput,
                response.Assignment.Metadata),
            response.Summary);
    }

    public static ReviewTaskRequest ToReviewTaskRequest(SupervisorContext context, TaskAssignment assignment, TaskResult result)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(assignment);
        ArgumentNullException.ThrowIfNull(result);

        return new ReviewTaskRequest(
            context.Objective,
            context.PlanMarkdown,
            context.SupervisorInstructionsMarkdown,
            ToTaskAssignmentPayload(assignment),
            ToExecuteTaskResponse(result),
            context.TaskHistory.Select(ToTaskHistoryEntryPayload).ToArray());
    }

    public static TaskReview ToTaskReview(ReviewTaskResponse response, Guid expectedTaskId)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.TaskId != expectedTaskId)
        {
            throw new ProviderProtocolException($"ReviewTask response taskId '{response.TaskId}' does not match expected taskId '{expectedTaskId}'.");
        }

        return new TaskReview(response.TaskId, response.Status, response.Summary);
    }

    public static ExecuteTaskRequest ToExecuteTaskRequest(AgentContext context, TaskAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(assignment);

        return new ExecuteTaskRequest(
            context.Objective,
            context.PlanMarkdown,
            context.AgentInstructionsMarkdown,
            ToTaskAssignmentPayload(assignment),
            "Execute exactly one bounded task and respond with strict JSON only.");
    }

    public static TaskResult ToTaskResult(ExecuteTaskResponse response, Guid expectedTaskId)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.TaskId != expectedTaskId)
        {
            throw new ProviderProtocolException($"ExecuteTask response taskId '{response.TaskId}' does not match expected taskId '{expectedTaskId}'.");
        }

        return new TaskResult(
            response.TaskId,
            response.Status,
            response.Summary,
            response.Output,
            response.RawPayload,
            response.Metadata);
    }

    public static ExecuteTaskResponse ToExecuteTaskResponse(TaskResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ExecuteTaskResponse(
            result.TaskId,
            result.Status,
            result.Summary,
            result.Output,
            result.RawPayload,
            result.Metadata);
    }

    public static TaskHistoryEntryPayload ToTaskHistoryEntryPayload(TaskHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new TaskHistoryEntryPayload(
            entry.TaskId,
            entry.Timestamp,
            entry.Title,
            entry.Instructions,
            entry.ExpectedOutput,
            entry.ExecutionStatus,
            entry.ValidationStatus,
            entry.ResultSummary,
            entry.ValidationSummary);
    }

    public static TaskAssignmentPayload ToTaskAssignmentPayload(TaskAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        return new TaskAssignmentPayload(
            assignment.Id,
            assignment.Title,
            assignment.Instructions,
            assignment.ExpectedOutput,
            assignment.Metadata);
    }
}
