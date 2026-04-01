using TaskFlow.Agents.Models;
using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Agents.Abstractions;

public interface IRunContextStore
{
    ValueTask<RunContext> LoadAsync(CancellationToken cancellationToken = default);

    ValueTask AppendHistoryAsync(TaskHistoryEntry entry, CancellationToken cancellationToken = default);
}
