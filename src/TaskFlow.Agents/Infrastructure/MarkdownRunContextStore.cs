using TaskFlow.Agents.Abstractions;
using TaskFlow.Agents.Models;
using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Agents.Infrastructure;

public sealed class MarkdownRunContextStore : IRunContextStore
{
    private readonly RunFilesOptions _options;

    public MarkdownRunContextStore(RunFilesOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async ValueTask<RunContext> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_options.PlanPath))
        {
            throw new FileNotFoundException("TaskFlow plan file was not found.", _options.PlanPath);
        }

        var planMarkdown = await File.ReadAllTextAsync(_options.PlanPath, cancellationToken);
        var objective = ExtractObjective(planMarkdown);
        var history = await LoadHistoryAsync(cancellationToken);

        return new RunContext(objective, planMarkdown, history);
    }

    public async ValueTask AppendHistoryAsync(TaskHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var directory = Path.GetDirectoryName(_options.ProgressPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(_options.ProgressPath) || string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(_options.ProgressPath, cancellationToken)))
        {
            await File.WriteAllTextAsync(_options.ProgressPath, "# Progress" + Environment.NewLine + Environment.NewLine, cancellationToken);
        }

        var section = string.Join(
            Environment.NewLine,
            [
                $"## Task {entry.TaskId:D}",
                $"- Timestamp: {entry.Timestamp:O}",
                $"- ValidationStatus: {entry.ValidationStatus}",
                $"- ExecutionStatus: {entry.ExecutionStatus}",
                $"- Title: {Sanitize(entry.Title)}",
                $"- Instructions: {Sanitize(entry.Instructions)}",
                $"- ExpectedOutput: {Sanitize(entry.ExpectedOutput)}",
                $"- ResultSummary: {Sanitize(entry.ResultSummary)}",
                $"- ValidationSummary: {Sanitize(entry.ValidationSummary)}",
                string.Empty
            ]);

        await File.AppendAllTextAsync(_options.ProgressPath, section + Environment.NewLine, cancellationToken);
    }

    private async ValueTask<IReadOnlyList<TaskHistoryEntry>> LoadHistoryAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_options.ProgressPath))
        {
            return [];
        }

        var lines = await File.ReadAllLinesAsync(_options.ProgressPath, cancellationToken);
        var history = new List<TaskHistoryEntry>();
        Guid? currentTaskId = null;
        Dictionary<string, string>? fields = null;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.StartsWith("## Task ", StringComparison.Ordinal))
            {
                if (TryCreateHistoryEntry(currentTaskId, fields) is { } completedEntry)
                {
                    history.Add(completedEntry);
                }

                currentTaskId = Guid.TryParse(line["## Task ".Length..].Trim(), out var parsedTaskId) ? parsedTaskId : null;
                fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            if (currentTaskId is not null && fields is not null && line.StartsWith("- ", StringComparison.Ordinal))
            {
                var separatorIndex = line.IndexOf(':');
                if (separatorIndex > 2)
                {
                    var key = line[2..separatorIndex].Trim();
                    var value = line[(separatorIndex + 1)..].Trim();
                    fields[key] = value;
                }
            }
        }

        if (TryCreateHistoryEntry(currentTaskId, fields) is { } finalEntry)
        {
            history.Add(finalEntry);
        }

        return history;
    }

    private static string ExtractObjective(string planMarkdown)
    {
        var lines = planMarkdown
            .Split(["\r\n", "\n"], StringSplitOptions.None)
            .Select(line => line.Trim());

        var firstHeading = lines.FirstOrDefault(line => line.StartsWith("# ", StringComparison.Ordinal));
        if (!string.IsNullOrWhiteSpace(firstHeading))
        {
            return firstHeading[2..].Trim();
        }

        var firstContentLine = lines.FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
        return Guard.NotNullOrWhiteSpace(firstContentLine, nameof(planMarkdown));
    }

    private static TaskHistoryEntry? TryCreateHistoryEntry(Guid? taskId, IReadOnlyDictionary<string, string>? fields)
    {
        if (taskId is null || fields is null)
        {
            return null;
        }

        if (!fields.TryGetValue("Timestamp", out var timestampValue) ||
            !DateTimeOffset.TryParse(timestampValue, out var timestamp) ||
            !fields.TryGetValue("ValidationStatus", out var validationStatusValue) ||
            !Enum.TryParse<TaskValidationStatus>(validationStatusValue, true, out var validationStatus) ||
            !fields.TryGetValue("ExecutionStatus", out var executionStatusValue) ||
            !Enum.TryParse<TaskExecutionStatus>(executionStatusValue, true, out var executionStatus) ||
            !fields.TryGetValue("Title", out var title) ||
            !fields.TryGetValue("Instructions", out var instructions) ||
            !fields.TryGetValue("ExpectedOutput", out var expectedOutput) ||
            !fields.TryGetValue("ResultSummary", out var resultSummary) ||
            !fields.TryGetValue("ValidationSummary", out var validationSummary))
        {
            return null;
        }

        return new TaskHistoryEntry(
            taskId.Value,
            timestamp,
            title,
            instructions,
            expectedOutput,
            executionStatus,
            validationStatus,
            resultSummary,
            validationSummary);
    }

    private static string Sanitize(string value) =>
        value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("|", "/", StringComparison.Ordinal)
            .Trim();
}
