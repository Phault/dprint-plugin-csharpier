using System.Collections.Generic;
using System.Linq;
using CSharpier;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dprint.Plugins.Csharpier.Configuration;

public record CsharpierConfiguration
{
    public int? PrintWidth { get; init; }
    public EndOfLine? EndOfLine { get; init; }
    public IndentStyle? IndentStyle { get; init; }
    public int? IndentSize { get; init; }

    [JsonExtensionData(ReadData = true, WriteData = false)]
    private IDictionary<string, JToken> UnknownOptions { get; init; } =
        new Dictionary<string, JToken>();

    [JsonIgnore]
    public bool IsEmpty =>
        PrintWidth == null
        && EndOfLine == null
        && IndentStyle == null
        && IndentSize == null
        && UnknownOptions.Count == 0;

    private static readonly CodeFormatterOptions DefaultCodeFormatterOptions = new();

    public CodeFormatterOptions ToCodeFormatterOptions()
    {
        return new CodeFormatterOptions
        {
            Width = PrintWidth ?? DefaultCodeFormatterOptions.Width,
            EndOfLine = EndOfLine ?? DefaultCodeFormatterOptions.EndOfLine,
            IndentStyle = IndentStyle ?? DefaultCodeFormatterOptions.IndentStyle,
            IndentSize = IndentSize ?? DefaultCodeFormatterOptions.IndentSize
        };
    }

    public CsharpierConfiguration Combine(CsharpierConfiguration other)
    {
        return new CsharpierConfiguration
        {
            PrintWidth = other.PrintWidth ?? PrintWidth,
            EndOfLine = other.EndOfLine ?? EndOfLine,
            IndentStyle = other.IndentStyle ?? IndentStyle,
            IndentSize = other.IndentSize ?? IndentSize,
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
