namespace TaskFlow.Providers.Codex.Configuration;

public sealed class CodexRoleOptions
{
    public string? Model { get; set; }

    public string? Profile { get; set; }

    public CodexSandboxMode Sandbox { get; set; }

    public CodexApprovalMode Approval { get; set; }

    public bool Ephemeral { get; set; } = true;

    public string WorkingDirectory { get; set; } = ".";

    public List<string> AdditionalWritableDirectories { get; set; } = [];
}
