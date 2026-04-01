using TaskFlow.Agents.Abstractions;
using TaskFlow.Agents.Logging;

namespace TaskFlow.Agents.Infrastructure;

public sealed class FileConsoleTaskLogger : ITaskLogger
{
    private readonly string _logPath;

    public FileConsoleTaskLogger(string logPath)
    {
        _logPath = string.IsNullOrWhiteSpace(logPath) ? throw new ArgumentException("Log path cannot be empty.", nameof(logPath)) : logPath;
    }

    public async ValueTask LogAsync(TaskLogEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var line = $"{entry.Timestamp:O} | {entry.Level} | {entry.EventType} | {(entry.TaskId is null ? "-" : entry.TaskId.Value.ToString("D"))} | {(string.IsNullOrWhiteSpace(entry.Status) ? "-" : entry.Status)} | {entry.Message}";

        if (entry.Level == TaskLogLevel.Error)
        {
            Console.Error.WriteLine(line);
        }
        else
        {
            Console.WriteLine(line);
        }

        var directory = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.AppendAllTextAsync(_logPath, line + Environment.NewLine, cancellationToken);
    }
}
