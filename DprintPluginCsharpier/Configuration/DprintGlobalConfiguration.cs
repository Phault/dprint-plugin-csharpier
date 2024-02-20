using System;

namespace Dprint.Plugins.Csharpier.Configuration;

public enum NewLineKind
{
    Auto,
    Crlf,
    Lf,
    System
}

public record DprintGlobalConfiguration
{
    public int? LineWidth { get; init; }
    public int? IndentWidth { get; init; }
    public bool? UseTabs { get; init; }
    public NewLineKind? NewLineKind { get; init; }

    public CsharpierConfiguration ToCsharpierConfiguration()
    {
        return new CsharpierConfiguration
        {
            PrintWidth = LineWidth,
            IndentSize = IndentWidth,
            IndentStyle = UseTabs.HasValue
                ? UseTabs.Value
                    ? CSharpier.IndentStyle.Tabs
                    : CSharpier.IndentStyle.Spaces
                : null,
            EndOfLine = NewLineKind switch
            {
                // dprint's auto looks at line-ending for the last line, but csharpier's looks at first line
                // practically the difference should not matter
                Configuration.NewLineKind.Auto
                    => CSharpier.EndOfLine.Auto,
                Configuration.NewLineKind.Crlf => CSharpier.EndOfLine.CRLF,
                Configuration.NewLineKind.Lf => CSharpier.EndOfLine.LF,
                Configuration.NewLineKind.System
                    => Environment.NewLine switch
                    {
                        "\n" => CSharpier.EndOfLine.LF,
                        "\r\n" => CSharpier.EndOfLine.CRLF,
                        _ => null,
                    },
                _ => null,
            }
        };
    }
}
