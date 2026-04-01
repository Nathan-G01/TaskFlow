namespace TaskFlow.Providers.Codex.Client;

public sealed record CodexCliResult(
    int ExitCode,
    string StdOut,
    string StdErr,
    string FinalMessage);
