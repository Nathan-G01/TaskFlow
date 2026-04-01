using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using TaskFlow.Providers.Abstraction.Core;

namespace TaskFlow.Providers.Abstraction.Protocol;

public sealed class JsonProviderProtocol : IProviderProtocol
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    static JsonProviderProtocol()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public string SerializeCreateNextTaskRequest(CreateNextTaskRequest request) => Serialize(request, nameof(request));

    public string SerializeCreateNextTaskResponse(CreateNextTaskResponse response) => Serialize(response, nameof(response));

    public CreateNextTaskResponse ParseCreateNextTaskResponse(string json) => Deserialize<CreateNextTaskResponse>(json);

    public string SerializeReviewTaskRequest(ReviewTaskRequest request) => Serialize(request, nameof(request));

    public string SerializeReviewTaskResponse(ReviewTaskResponse response) => Serialize(response, nameof(response));

    public ReviewTaskResponse ParseReviewTaskResponse(string json) => Deserialize<ReviewTaskResponse>(json);

    public string SerializeExecuteTaskRequest(ExecuteTaskRequest request) => Serialize(request, nameof(request));

    public string SerializeExecuteTaskResponse(ExecuteTaskResponse response) => Serialize(response, nameof(response));

    public ExecuteTaskResponse ParseExecuteTaskResponse(string json) => Deserialize<ExecuteTaskResponse>(json);

    private static string Serialize<T>(T value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return JsonSerializer.Serialize(value, SerializerOptions);
    }

    private static T Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ProviderProtocolException("Provider response was empty. Expected strict JSON.");
        }

        foreach (var candidate in EnumerateCandidates(json))
        {
            try
            {
                var value = JsonSerializer.Deserialize<T>(candidate, SerializerOptions);
                if (value is null)
                {
                    continue;
                }

                return value;
            }
            catch (JsonException)
            {
            }
        }

        var excerpt = CreateExcerpt(json);
        throw new ProviderProtocolException($"Provider response was not valid strict JSON. Response excerpt: {excerpt}");
    }

    private static IEnumerable<string> EnumerateCandidates(string json)
    {
        var candidates = new List<string>();
        AddCandidate(candidates, json);

        var trimmed = json.Trim();
        AddCandidate(candidates, trimmed);

        var unfenced = TryUnwrapMarkdownFence(trimmed);
        AddCandidate(candidates, unfenced);

        var extracted = TryExtractJsonObject(trimmed);
        AddCandidate(candidates, extracted);

        AddCandidate(candidates, TryEscapeInvalidJsonStringEscapes(trimmed));

        if (unfenced is not null)
        {
            AddCandidate(candidates, TryExtractJsonObject(unfenced));
            AddCandidate(candidates, TryEscapeInvalidJsonStringEscapes(unfenced));
        }

        if (extracted is not null)
        {
            AddCandidate(candidates, TryEscapeInvalidJsonStringEscapes(extracted));
        }

        return candidates;
    }

    private static void AddCandidate(ICollection<string> candidates, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return;
        }

        if (!candidates.Contains(candidate, StringComparer.Ordinal))
        {
            candidates.Add(candidate);
        }
    }

    private static string? TryUnwrapMarkdownFence(string value)
    {
        var match = Regex.Match(
            value,
            @"^\s*```(?:json)?\s*(?<content>[\s\S]*?)\s*```\s*$",
            RegexOptions.CultureInvariant);

        return match.Success ? match.Groups["content"].Value.Trim() : null;
    }

    private static string? TryExtractJsonObject(string value)
    {
        var start = value.IndexOf('{');
        var end = value.LastIndexOf('}');
        return start >= 0 && end > start
            ? value[start..(end + 1)].Trim()
            : null;
    }

    private static string? TryEscapeInvalidJsonStringEscapes(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('\\', StringComparison.Ordinal))
        {
            return null;
        }

        var builder = new System.Text.StringBuilder(value.Length + 16);
        var insideString = false;
        var escapePending = false;

        foreach (var character in value)
        {
            if (!insideString)
            {
                builder.Append(character);
                if (character == '"')
                {
                    insideString = true;
                }

                continue;
            }

            if (escapePending)
            {
                if (!IsValidJsonEscape(character))
                {
                    builder.Append('\\');
                }

                builder.Append(character);
                escapePending = false;
                continue;
            }

            switch (character)
            {
                case '\\':
                    builder.Append(character);
                    escapePending = true;
                    break;
                case '"':
                    builder.Append(character);
                    insideString = false;
                    break;
                default:
                    builder.Append(character);
                    break;
            }
        }

        if (escapePending)
        {
            builder.Append('\\');
        }

        return builder.ToString();
    }

    private static bool IsValidJsonEscape(char character) =>
        character is '"' or '\\' or '/' or 'b' or 'f' or 'n' or 'r' or 't' or 'u';

    private static string CreateExcerpt(string value)
    {
        var singleLine = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();

        return singleLine.Length <= 480
            ? singleLine
            : singleLine[..480] + "...";
    }
}
