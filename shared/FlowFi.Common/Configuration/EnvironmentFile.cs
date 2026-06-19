using System.Collections;

namespace FlowFi.Common.Configuration;

public static class EnvironmentFile
{
    private const string SharedPrefix = "FLOWFI_SHARED__";

    public static void Load(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var processEnvironmentKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = FindEnvironmentFile();
        if (path is not null)
        {
            foreach (var pair in Parse(path))
            {
                values[pair.Key] = pair.Value;
            }
        }

        foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
        {
            if (variable.Key is string key && variable.Value is string value)
            {
                values[key] = value;
                processEnvironmentKeys.Add(key);
            }
        }

        var servicePrefix = $"FLOWFI_{Normalize(serviceName)}__";
        Apply(values, SharedPrefix, processEnvironmentKeys, overwrite: false);
        Apply(values, servicePrefix, processEnvironmentKeys, overwrite: true);
    }

    private static void Apply(
        IReadOnlyDictionary<string, string> values,
        string prefix,
        IReadOnlySet<string> processEnvironmentKeys,
        bool overwrite)
    {
        foreach (var pair in values)
        {
            if (!pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var configurationKey = pair.Key[prefix.Length..];
            if (string.IsNullOrWhiteSpace(configurationKey) ||
                processEnvironmentKeys.Contains(configurationKey) ||
                (!overwrite && Environment.GetEnvironmentVariable(configurationKey) is not null))
            {
                continue;
            }

            Environment.SetEnvironmentVariable(configurationKey, pair.Value);
        }
    }

    private static IEnumerable<KeyValuePair<string, string>> Parse(string path)
    {
        foreach (var sourceLine in File.ReadLines(path))
        {
            var line = sourceLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
            {
                line = line[7..].TrimStart();
            }

            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') ||
                 (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }

            yield return new KeyValuePair<string, string>(key, value);
        }
    }

    private static string? FindEnvironmentFile()
    {
        var explicitPath = Environment.GetEnvironmentVariable("FLOWFI_ENV_FILE");
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            var fullPath = Path.GetFullPath(explicitPath);
            return File.Exists(fullPath) ? fullPath : null;
        }

        return SearchParents(Directory.GetCurrentDirectory()) ??
               SearchParents(AppContext.BaseDirectory);
    }

    private static string? SearchParents(string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string Normalize(string serviceName)
        => new(serviceName
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());
}
