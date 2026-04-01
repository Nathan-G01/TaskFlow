namespace TaskFlow.Providers.Codex.Support;

internal static class CodexSchemaFactory
{
    public static string CreateNextTaskResponseSchema => """
        {
          "type": "object",
          "additionalProperties": false,
          "required": ["isComplete", "summary", "assignment"],
          "properties": {
            "isComplete": { "type": "boolean" },
            "summary": { "type": "string", "minLength": 1 },
            "assignment": {
              "anyOf": [
                { "type": "null" },
                {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["id", "title", "instructions", "expectedOutput", "metadata"],
                  "properties": {
                    "id": { "type": "string", "format": "uuid" },
                    "title": { "type": "string", "minLength": 1 },
                    "instructions": { "type": "string", "minLength": 1 },
                    "expectedOutput": { "type": "string", "minLength": 1 },
                    "metadata": {
                      "anyOf": [
                        { "type": "null" },
                        {
                          "type": "object",
                          "additionalProperties": { "type": "string" }
                        }
                      ]
                    }
                  }
                }
              ]
            }
          }
        }
        """;

    public static string ReviewTaskResponseSchema => """
        {
          "type": "object",
          "additionalProperties": false,
          "required": ["taskId", "status", "summary"],
          "properties": {
            "taskId": { "type": "string", "format": "uuid" },
            "status": { "type": "string", "enum": ["Validated", "Invalidated"] },
            "summary": { "type": "string", "minLength": 1 }
          }
        }
        """;

    public static string ExecuteTaskResponseSchema => """
        {
          "type": "object",
          "additionalProperties": false,
          "required": ["taskId", "status", "summary", "output", "rawPayload", "metadata"],
          "properties": {
            "taskId": { "type": "string", "format": "uuid" },
            "status": { "type": "string", "enum": ["Succeeded", "Failed", "Cancelled"] },
            "summary": { "type": "string", "minLength": 1 },
            "output": { "type": ["string", "null"] },
            "rawPayload": { "type": ["string", "null"] },
            "metadata": {
              "anyOf": [
                { "type": "null" },
                {
                  "type": "object",
                  "additionalProperties": { "type": "string" }
                }
              ]
            }
          }
        }
        """;
}
