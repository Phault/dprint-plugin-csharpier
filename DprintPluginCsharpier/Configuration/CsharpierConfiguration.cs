using System.Collections.Generic;
using System.Linq;
using CSharpier;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dprint.Plugins.Csharpier.Configuration;

/**
 * Represents the subset of Csharpier options we support.
 * Oddly the Csharpier library only exposes the very limited CodeFormatterOptions class.
 *
 * TODO: open upstream issue requesting full configurability
 */
public record CsharpierConfiguration
{
    public int? PrintWidth { get; init; }

    [JsonExtensionData(ReadData = true, WriteData = false)]
    private IDictionary<string, JToken> UnknownOptions { get; init; } =
        new Dictionary<string, JToken>();

    [JsonIgnore]
    public bool IsEmpty => PrintWidth == null && UnknownOptions.Count == 0;

    private static readonly CodeFormatterOptions DefaultCodeFormatterOptions = new();

    public CodeFormatterOptions ToCodeFormatterOptions()
    {
        return new CodeFormatterOptions { Width = PrintWidth ?? DefaultCodeFormatterOptions.Width };
    }

    public CsharpierConfiguration Combine(CsharpierConfiguration other)
    {
        return new CsharpierConfiguration
        {
            PrintWidth = other.PrintWidth ?? PrintWidth,
            UnknownOptions = UnknownOptions.Concat(other.UnknownOptions).ToDictionary()
        };
    }

    public IReadOnlyList<ConfigurationDiagnostic> GetDiagnostics()
    {
        return UnknownOptions
            .Select(x => new ConfigurationDiagnostic(x.Key, "Unknown configuration property name."))
            .ToList();
    }
}

public record ConfigurationDiagnostic(string PropertyName, string Message);
