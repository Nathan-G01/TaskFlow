using TaskFlow.Providers.Codex.Configuration;
using TaskFlow.Providers.Codex.Support;

using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace TaskFlow.Providers.Codex.Client;

public sealed class CodexCliClient
{
    private readonly string _command;

    public CodexCliClient(string command)
    {
        _command = ResolveCommand(command);
    }

    public async ValueTask<CodexCliResult> ExecuteAsync(
        string prompt,
        string schemaJson,
        CodexRoleOptions options,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaJson);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var workingDirectory = ResolveWorkingDirectory(options.WorkingDirectory);
        var tempDirectory = Path.Combine(Path.GetTempPath(), "TaskFlow", "Codex", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var schemaPath = Path.Combine(tempDirectory, $"{operationName}.schema.json");
        var outputPath = Path.Combine(tempDirectory, $"{operationName}.output.json");
        await File.WriteAllTextAsync(schemaPath, schemaJson, cancellationToken);

        using var process = CreateProcess(workingDirectory, schemaPath, outputPath, options);

        try
        {
            try
            {
                if (!process.Start())
                {
                    throw new CodexProviderException("Failed to start the Codex CLI process.");
                }
            }
            catch (Win32Exception exception)
            {
                throw new CodexProviderException(
                    $"Unable to start Codex CLI using command '{_command}'. Install Codex CLI and ensure it is available on PATH.",
                    exception);
            }

            await process.StandardInput.WriteAsync(prompt);
            await process.StandardInput.FlushAsync(cancellationToken);
            process.StandardInput.Close();

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                throw CreateExecutionFailure(process.ExitCode, stdout, stderr);
            }

            if (!File.Exists(outputPath))
            {
                throw new CodexProviderException($"Codex CLI did not produce the expected output file for operation '{operationName}'.");
            }

            var finalMessage = await File.ReadAllTextAsync(outputPath, cancellationToken);
            if (string.IsNullOrWhiteSpace(finalMessage))
            {
                throw new CodexProviderException($"Codex CLI returned an empty final message for operation '{operationName}'.");
            }

            return new CodexCliResult(process.ExitCode, stdout, stderr, finalMessage);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private Process CreateProcess(string workingDirectory, string schemaPath, string outputPath, CodexRoleOptions options)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _command,
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        startInfo.ArgumentList.Add("exec");
        startInfo.ArgumentList.Add("-");
        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(workingDirectory);
        startInfo.ArgumentList.Add("--skip-git-repo-check");
        startInfo.ArgumentList.Add("--json");
        startInfo.ArgumentList.Add("--output-schema");
        startInfo.ArgumentList.Add(schemaPath);
        startInfo.ArgumentList.Add("-o");
        startInfo.ArgumentList.Add(outputPath);
        startInfo.ArgumentList.Add("-s");
        startInfo.ArgumentList.Add(ToCliValue(options.Sandbox));
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add($"approval_policy=\"{ToCliValue(options.Approval)}\"");

        if (!string.IsNullOrWhiteSpace(options.Model))
        {
            startInfo.ArgumentList.Add("-m");
            startInfo.ArgumentList.Add(options.Model);
        }

        if (!string.IsNullOrWhiteSpace(options.Profile))
        {
            startInfo.ArgumentList.Add("-p");
            startInfo.ArgumentList.Add(options.Profile);
        }

        if (options.Ephemeral)
        {
            startInfo.ArgumentList.Add("--ephemeral");
        }

        foreach (var additionalDirectory in options.AdditionalWritableDirectories.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            startInfo.ArgumentList.Add("--add-dir");
            startInfo.ArgumentList.Add(Path.GetFullPath(additionalDirectory, workingDirectory));
        }

        return new Process { StartInfo = startInfo };
    }

    private static string ResolveWorkingDirectory(string configuredPath) =>
        Path.GetFullPath(string.IsNullOrWhiteSpace(configuredPath) ? "." : configuredPath);

    private static string ResolveCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Codex command cannot be empty.", nameof(command));
        }

        if (Path.IsPathRooted(command) || command.Contains(Path.DirectorySeparatorChar) || command.Contains(Path.AltDirectorySeparatorChar))
        {
            return Path.GetFullPath(command);
        }

        var pathEntries = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Prefer directly launchable wrappers on Windows instead of PowerShell script resolution.
        var candidateNames = OperatingSystem.IsWindows()
            ? new[] { $"{command}.cmd", $"{command}.exe", command, $"{command}.bat", $"{command}.ps1" }
            : new[] { command };

        foreach (var directory in pathEntries)
        {
            foreach (var candidateName in candidateNames)
            {
                var candidatePath = Path.Combine(directory, candidateName);
                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }
        }

        return command;
    }

    private static string ToCliValue(CodexSandboxMode mode) => mode switch
    {
        CodexSandboxMode.ReadOnly => "read-only",
        CodexSandboxMode.WorkspaceWrite => "workspace-write",
        CodexSandboxMode.DangerFullAccess => "danger-full-access",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported Codex sandbox mode.")
    };

    private static string ToCliValue(CodexApprovalMode mode) => mode switch
    {
        CodexApprovalMode.Never => "never",
        CodexApprovalMode.OnRequest => "on-request",
        CodexApprovalMode.Untrusted => "untrusted",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported Codex approval mode.")
    };

    private static CodexProviderException CreateExecutionFailure(int exitCode, string stdout, string stderr)
    {
        var combinedOutput = (stdout + Environment.NewLine + stderr).Trim();
        var compactOutput = combinedOutput.Length > 800 ? combinedOutput[..800] + "..." : combinedOutput;

        if (combinedOutput.Contains("login", StringComparison.OrdinalIgnoreCase) ||
            combinedOutput.Contains("authenticated", StringComparison.OrdinalIgnoreCase) ||
            combinedOutput.Contains("credential", StringComparison.OrdinalIgnoreCase))
        {
            return new CodexProviderException(
                $"Codex CLI exited with code {exitCode}. The CLI appears to require authentication. Run `codex login` and try again. Details: {compactOutput}");
        }

        return new CodexProviderException(
            $"Codex CLI exited with code {exitCode}. Details: {compactOutput}");
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
        }
    }
}
