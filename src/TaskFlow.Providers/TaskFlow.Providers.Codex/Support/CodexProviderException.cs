namespace TaskFlow.Providers.Codex.Support;

public sealed class CodexProviderException : InvalidOperationException
{
    public CodexProviderException(string message)
        : base(message)
    {
    }

    public CodexProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
