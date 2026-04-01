using TaskFlow.Agents.Logging;

namespace TaskFlow.Agents.Abstractions;

public interface ITaskLogger
{
    ValueTask LogAsync(TaskLogEntry entry, CancellationToken cancellationToken = default);
}
