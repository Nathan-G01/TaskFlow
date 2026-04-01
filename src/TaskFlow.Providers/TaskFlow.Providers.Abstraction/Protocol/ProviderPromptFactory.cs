namespace TaskFlow.Providers.Abstraction.Protocol;

public static class ProviderPromptFactory
{
    public static string CreateNextTaskPrompt(CreateNextTaskRequest request, IProviderProtocol protocol) =>
        CreateNextTaskPrompt(protocol.SerializeCreateNextTaskRequest(request));

    public static string ReviewTaskPrompt(ReviewTaskRequest request, IProviderProtocol protocol) =>
        ReviewTaskPrompt(protocol.SerializeReviewTaskRequest(request));

    public static string ExecuteTaskPrompt(ExecuteTaskRequest request, IProviderProtocol protocol) =>
        ExecuteTaskPrompt(protocol.SerializeExecuteTaskRequest(request));

    private static string CreateNextTaskPrompt(string requestJson) =>
        $$"""
        You are the TaskFlow supervisor.
        Read the request JSON and decide the next bounded task to execute.
        You must follow the instructions provided in `supervisorInstructionsMarkdown`.
        Respond with strict JSON only. Do not wrap the response in markdown fences. Do not add commentary.

        Required response shape:
        {
          "isComplete": true|false,
          "summary": "string",
          "assignment": {
            "id": "guid",
            "title": "string",
            "instructions": "string",
            "expectedOutput": "string",
            "metadata": { "key": "value" }
          }
        }

        Rules:
        - If the objective is complete, set "isComplete" to true and set "assignment" to null.
        - If work remains, set "isComplete" to false and provide exactly one bounded assignment.
        - The assignment id must be a non-empty GUID.
        - Always include the "assignment" field.
        - When metadata is unavailable, set "metadata" to null.

        Request JSON:
        {{requestJson}}
        """;

    private static string ReviewTaskPrompt(string requestJson) =>
        $$"""
        You are the TaskFlow supervisor reviewing an agent result.
        Read the request JSON and decide whether the task result is validated or invalidated.
        You must follow the instructions provided in `supervisorInstructionsMarkdown`.
        Respond with strict JSON only. Do not wrap the response in markdown fences. Do not add commentary.

        Required response shape:
        {
          "taskId": "guid",
          "status": "Validated|Invalidated",
          "summary": "string"
        }

        Rules:
        - taskId must match the reviewed assignment.
        - Use Validated only if the result satisfies the expected output.
        - Use Invalidated otherwise and explain what is wrong in summary.

        Request JSON:
        {{requestJson}}
        """;

    private static string ExecuteTaskPrompt(string requestJson) =>
        $$"""
        You are the TaskFlow execution agent.
        Read the request JSON and execute exactly one bounded task.
        You must follow the instructions provided in `agentInstructionsMarkdown`.
        Respond with strict JSON only. Do not wrap the response in markdown fences. Do not add commentary.

        Required response shape:
        {
          "taskId": "guid",
          "status": "Succeeded|Failed|Cancelled",
          "summary": "string",
          "output": "string",
          "rawPayload": "string",
          "metadata": { "key": "value" }
        }

        Rules:
        - taskId must match the assignment.
        - summary is required.
        - Always include "output", "rawPayload", and "metadata".
        - Use null for "output", "rawPayload", or "metadata" when they are unavailable.
        - The response must be one valid JSON object and nothing else.
        - All string values must be valid JSON strings with escaped quotes and escaped backslashes.
        - Use forward slashes in file paths instead of backslashes.
        - Keep "output" concise and machine-safe. Do not paste code fences or conversational prose around the JSON object.

        Request JSON:
        {{requestJson}}
        """;
}
