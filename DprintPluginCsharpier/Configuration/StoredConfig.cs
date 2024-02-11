using System;

namespace Dprint.Plugins.Csharpier.Configuration;

public class StoredConfig
{
    public CsharpierConfiguration PluginConfig { get; }
    public DprintGlobalConfiguration GlobalConfig { get; }

    private readonly Lazy<CsharpierConfiguration> _lazyCombined;
    public CsharpierConfiguration CombinedConfig => _lazyCombined.Value;

    public StoredConfig(DprintGlobalConfiguration globalConfig, CsharpierConfiguration pluginConfig)
    {
        GlobalConfig = globalConfig;
        PluginConfig = pluginConfig;
        _lazyCombined = new Lazy<CsharpierConfiguration>(
            () => GlobalConfig.ToCsharpierConfiguration().Combine(PluginConfig)
        );
    }
}
