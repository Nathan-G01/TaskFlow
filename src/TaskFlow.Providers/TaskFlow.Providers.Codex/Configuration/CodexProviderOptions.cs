namespace TaskFlow.Providers.Codex.Configuration;

public sealed class CodexProviderOptions
{
    public string Command { get; set; } = "codex";

    public CodexRoleOptions Supervisor { get; set; } = new()
    {
        Sandbox = CodexSandboxMode.ReadOnly,
        Approval = CodexApprovalMode.Never,
        Ephemeral = true,
        WorkingDirectory = "."
    };

    public CodexRoleOptions Agent { get; set; } = new()
    {
        Sandbox = CodexSandboxMode.WorkspaceWrite,
        Approval = CodexApprovalMode.OnRequest,
        Ephemeral = true,
        WorkingDirectory = "."
    };
}
