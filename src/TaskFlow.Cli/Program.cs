using Microsoft.Extensions.Configuration;
using TaskFlow.Agents.Factories;
using TaskFlow.Agents.Infrastructure;
using TaskFlow.Agents.Models;
using TaskFlow.Agents.Orchestration;
using TaskFlow.Agents.Supervisors;
using TaskFlow.Providers.Abstraction.Core;
using TaskFlow.Providers.Abstraction.Protocol;
using TaskFlow.Providers.Codex.Configuration;
using TaskFlow.Providers.Codex.Support;

namespace TaskFlow.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var options = CreateRunFilesOptions(repositoryRoot, args);
        var providerOptions = CreateProviderOptions(args);

        try
        {
            var codexOptions = LoadCodexOptions(repositoryRoot);
            var providerRegistry = ProviderRegistry.CreateDefault(codexOptions);
            var supervisorEngineFactory = providerRegistry.GetSupervisorFactory(providerOptions.SupervisorProvider);
            var agentEngineFactory = providerRegistry.GetAgentFactory(providerOptions.AgentProvider);

            var orchestrator = new Orchestrator(
                new Supervisor(supervisorEngineFactory.CreateEngine()),
                new MarkdownRunContextStore(options),
                new AgentFactory(agentEngineFactory),
                new FileConsoleTaskLogger(options.LogPath));

            var result = await orchestrator.RunAsync();

            Console.WriteLine($"Objective: {result.FinalContext.Objective}");
            Console.WriteLine($"Reviewed tasks: {result.AppendedHistory.Count}");
            Console.WriteLine($"Supervisor provider: {providerOptions.SupervisorProvider}");
            Console.WriteLine($"Agent provider: {providerOptions.AgentProvider}");

            if (result.AppendedHistory.Count > 0)
            {
                var lastEntry = result.AppendedHistory[^1];
                Console.WriteLine($"Last task: {lastEntry.TaskId:D} -> {lastEntry.ValidationStatus} ({lastEntry.ExecutionStatus})");
                Console.WriteLine(lastEntry.ValidationSummary);
            }

            return result.AppendedHistory.Any(entry => entry.ValidationStatus != TaskValidationStatus.Validated) ? 1 : 0;
        }
        catch (FileNotFoundException exception)
        {
            Console.Error.WriteLine(exception.Message);
            Console.Error.WriteLine("Create a plan file before running TaskFlow. Expected files can be overridden with CLI arguments: <planPath> <progressPath> [--supervisor-provider <name>] [--agent-provider <name>].");
            Console.Error.WriteLine($"Missing path: {exception.FileName}");
            return 1;
        }
        catch (ProviderProtocolException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
        catch (InvalidOperationException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static RunFilesOptions CreateRunFilesOptions(string repositoryRoot, string[] args)
    {
        var positionalArgs = GetPositionalArgs(args);

        if (positionalArgs.Length >= 2)
        {
            return new RunFilesOptions(
                Path.GetFullPath(positionalArgs[0]),
                Path.GetFullPath(positionalArgs[1]),
                ResolveLogPath(repositoryRoot, args));
        }

        return new RunFilesOptions(
            Path.Combine(repositoryRoot, "plan.md"),
            Path.Combine(repositoryRoot, "progress.md"),
            ResolveLogPath(repositoryRoot, args));
    }

    private static ProviderOptions CreateProviderOptions(string[] args)
    {
        string? supervisorProvider = null;
        string? agentProvider = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--supervisor-provider", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                supervisorProvider = args[i + 1];
                i++;
                continue;
            }

            if (string.Equals(args[i], "--agent-provider", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                agentProvider = args[i + 1];
                i++;
            }
        }

        return new ProviderOptions(
            string.IsNullOrWhiteSpace(supervisorProvider) ? "codex" : supervisorProvider,
            string.IsNullOrWhiteSpace(agentProvider) ? "codex" : agentProvider);
    }

    private static string[] GetPositionalArgs(string[] args)
    {
        var positionalArgs = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--supervisor-provider", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(args[i], "--agent-provider", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(args[i], "--log-path", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                continue;
            }

            if (!args[i].StartsWith("--", StringComparison.Ordinal))
            {
                positionalArgs.Add(args[i]);
            }
        }

        return positionalArgs.ToArray();
    }

    private static string ResolveLogPath(string repositoryRoot, string[] args)
    {
        string? configuredLogPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--log-path", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                configuredLogPath = args[i + 1];
                break;
            }
        }

        return string.IsNullOrWhiteSpace(configuredLogPath)
            ? Path.Combine(repositoryRoot, "taskflow.log")
            : Path.GetFullPath(configuredLogPath);
    }

    private static CodexProviderOptions LoadCodexOptions(string repositoryRoot)
    {
        var appSettingsPath = Path.Combine(repositoryRoot, "src", "TaskFlow.Cli", "appsettings.json");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(appSettingsPath)!)
            .AddJsonFile(Path.GetFileName(appSettingsPath), optional: false, reloadOnChange: false)
            .Build();

        var options = configuration
            .GetSection("TaskFlow:Providers:Codex")
            .Get<CodexProviderOptions>();

        return CodexConfigurationValidator.Validate(options, repositoryRoot);
    }

    private static string ResolveRepositoryRoot()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            var solutionPath = Path.Combine(currentDirectory.FullName, "TaskFlow.slnx");
            if (File.Exists(solutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
