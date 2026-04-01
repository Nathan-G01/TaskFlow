using System.Collections.ObjectModel;

namespace TaskFlow.Providers.Abstraction.Core;

public sealed record TaskAssignment
{
    private static readonly IReadOnlyDictionary<string, string> EmptyMetadata =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    public TaskAssignment(
        Guid id,
        string title,
        string instructions,
        string expectedOutput,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Id = Guard.NotEmpty(id, nameof(id));
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
        Instructions = Guard.NotNullOrWhiteSpace(instructions, nameof(instructions));
        ExpectedOutput = Guard.NotNullOrWhiteSpace(expectedOutput, nameof(expectedOutput));
        Metadata = metadata is null
            ? EmptyMetadata
            : new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(metadata));
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Instructions { get; }

    public string ExpectedOutput { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
