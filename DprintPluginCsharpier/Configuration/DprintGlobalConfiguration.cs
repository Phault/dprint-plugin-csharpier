namespace Dprint.Plugins.Csharpier.Configuration;

public record DprintGlobalConfiguration
{
    public int? LineWidth { get; init; }

    public CsharpierConfiguration ToCsharpierConfiguration()
    {
        return new CsharpierConfiguration { PrintWidth = LineWidth };
    }
}
