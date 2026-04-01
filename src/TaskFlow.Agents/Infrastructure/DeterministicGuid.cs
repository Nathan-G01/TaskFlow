using System.Security.Cryptography;
using System.Text;

namespace TaskFlow.Agents.Infrastructure;

internal static class DeterministicGuid
{
    public static Guid Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = MD5.HashData(bytes);
        return new Guid(hash);
    }
}
