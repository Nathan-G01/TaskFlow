namespace TaskFlow.Providers.Abstraction.Protocol;

public sealed class ProviderProtocolException : InvalidOperationException
{
    public ProviderProtocolException(string message)
        : base(message)
    {
    }

    public ProviderProtocolException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
