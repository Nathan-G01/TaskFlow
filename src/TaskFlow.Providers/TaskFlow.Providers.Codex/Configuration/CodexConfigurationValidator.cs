using TaskFlow.Providers.Codex.Support;

namespace TaskFlow.Providers.Codex.Configuration;

public static class CodexConfigurationValidator
{
    public static CodexProviderOptions Validate(CodexProviderOptions? options, string repositoryRoot)
    {
        if (options is null)
        {
            throw new CodexProviderException("Missing TaskFlow:Providers:Codex configuration in appsettings.json.");
        }

        options.Command = string.IsNullOrWhiteSpace(options.Command) ? "codex" : options.Command;
        options.Supervisor ??= new CodexRoleOptions
        {
            Sandbox = CodexSandboxMode.ReadOnly,
            Approval = CodexApprovalMode.Never,
            Ephemeral = true,
            WorkingDirectory = "."
        };
        options.Agent ??= new CodexRoleOptions
        {
            Sandbox = CodexSandboxMode.WorkspaceWrite,
            Approval = CodexApprovalMode.OnRequest,
            Ephemeral = true,
            WorkingDirectory = "."
        };
        options.Supervisor = NormalizeRole(options.Supervisor, repositoryRoot);
        options.Agent = NormalizeRole(options.Agent, repositoryRoot);
        return options;
    }

    private static CodexRoleOptions NormalizeRole(CodexRoleOptions options, string repositoryRoot)
    {
        options.WorkingDirectory = ResolvePath(options.WorkingDirectory, repositoryRoot);
        options.AdditionalWritableDirectories = options.AdditionalWritableDirectories
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => ResolvePath(path, repositoryRoot))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return options;
    }

    private static string ResolvePath(string? path, string repositoryRoot)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return repositoryRoot;
        }

        return Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(path, repositoryRoot);
    }
}
