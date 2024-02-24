using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpier;
using Dprint.Plugins.Csharpier.Communication;
using Dprint.Plugins.Csharpier.Configuration;
using Dprint.Plugins.Csharpier.Serialization;
using Dprint.Plugins.Csharpier.Utils;

namespace Dprint.Plugins.Csharpier;

public class MessageProcessor(StdoutWriter writer)
{
    private readonly JsonSerializer _serializer = new();
    private readonly ConcurrentStorage<CancellationTokenSource> _tokens = new();
    private readonly ConcurrentDictionary<uint, StoredConfig> _configs = new();

    public ShutdownMessage RunStdinMessageLoop(MessageReader reader)
    {
        while (true)
        {
            var receivedMessage = Message.Read(reader);
            switch (receivedMessage)
            {
                case ErrorResponseMessage:
                    break;
                case ShutdownMessage message:
                    return message; // exit
                case ActiveMessage message:
                    writer.SendSuccessResponse(message.MessageId);
                    break;
                case GetPluginInfoMessage message:
                    writer.SendDataResponse(message.MessageId, GetPluginInfo());
                    break;
                case GetLicenseTextMessage message:
                    writer.SendDataResponse(message.MessageId, ReadLicenseText());
                    break;
                case RegisterConfigMessage message:
                    TryAction(
                        message.MessageId,
                        () =>
                        {
                            var globalConfig = _serializer.Deserialize<DprintGlobalConfiguration>(
                                message.GlobalConfigData
                            );
                            var pluginConfig = _serializer.Deserialize<CsharpierConfiguration>(
                                message.PluginConfigData
                            );
                            _configs.TryAdd(
                                message.ConfigId,
                                new StoredConfig(globalConfig, pluginConfig)
                            );
                            writer.SendSuccessResponse(message.MessageId);
                        }
                    );
                    break;
                case ReleaseConfigMessage message:
                    TryAction(
                        message.MessageId,
                        () =>
                        {
                            _configs.TryRemove(message.ConfigId, out _);
                            writer.SendSuccessResponse(message.MessageId);
                        }
                    );
                    break;
                case GetConfigDiagnosticsMessage message:
                    TryAction(
                        message.MessageId,
                        () =>
                        {
                            var config = GetConfig(message.ConfigId);
                            var diagnostics = config.CombinedConfig.GetDiagnostics();
                            var jsonDiagnostics = _serializer.Serialize(diagnostics);
                            writer.SendDataResponse(message.MessageId, jsonDiagnostics);
                        }
                    );
                    break;
                case GetFileMatchingInfo message:
                    writer.SendDataResponse(message.MessageId, GetFileMatchingInfo());
                    break;
                case GetResolvedConfigMessage message:
                    TryAction(
                        message.MessageId,
                        () =>
                        {
                            var config = GetConfig(message.ConfigId).CombinedConfig;
                            var jsonConfig = _serializer.Serialize(config);
                            writer.SendDataResponse(message.MessageId, jsonConfig);
                        }
                    );
                    break;
                case CheckConfigUpdatesMessage message:
                    TryAction(
                        message.MessageId,
                        () =>
                        {
                            writer.SendDataResponse(message.MessageId, "{ \"changes\": [] }");
                        }
                    );
                    break;
                case FormatTextMessage message:
                    TryAction(message.MessageId, () => StartFormatText(message).Wait());
                    break;
                case CancelFormatMessage message:
                    _tokens.Take(message.OriginalMessageId)?.Cancel();
                    break;
                case FormatTextResponseMessage:
                    // ignore, host formatting is not used by this plugin
                    break;
                case SuccessResponseMessage:
                case DataResponseMessage:
                    // ignore
                    break;
                case HostFormatMessage message:
                    writer.SendError(message.MessageId, "Cannot host format with a plugin.");
                    break;
                default:
                    // exit the process
                    throw new NotImplementedException(
                        $"Unimplemented message: {receivedMessage.GetType().FullName}"
                    );
            }
        }
    }

    private StoredConfig GetConfig(uint configId)
    {
        if (_configs.TryGetValue(configId, out var config))
            return config;

        throw new ArgumentOutOfRangeException(
            nameof(config),
            $"Could not find configuration id: {configId}"
        );
    }

    private async Task StartFormatText(FormatTextMessage message)
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        _tokens.StoreValue(message.MessageId, cts);

        try
        {
            var config = GetConfig(message.ConfigId).CombinedConfig;

            var overrideConfig = _serializer.Deserialize<CsharpierConfiguration?>(
                message.OverrideConfig
            );
            if (overrideConfig is { IsEmpty: false })
                config = config.Combine(overrideConfig);

            var fileText = Encoding.UTF8.GetString(message.FileText);
            var result = await CodeFormatter.FormatAsync(
                fileText,
                config.ToCodeFormatterOptions(),
                token
            );
            writer.SendFormatTextResponse(
                message.MessageId,
                result.Code == fileText ? null : result.Code
            );
        }
        catch (Exception ex)
        {
            writer.SendError(message.MessageId, ex);
        }
        finally
        {
            _tokens.Take(message.MessageId);
        }
    }

    private void TryAction(uint originalMessageId, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            writer.SendError(originalMessageId, ex);
        }
    }

    private static string GetPluginInfo()
    {
        return $$"""
            {
              "name": "dprint-plugin-csharpier",
              "version": "{{GetAssemblyVersion()}}",
              "configKey": "csharpier",
              "helpUrl": "https://github.com/Phault/dprint-plugin-csharpier",
              "configSchemaUrl": "",
              "updateUrl": "https://plugins.dprint.dev/Phault/dprint-plugin-csharpier/latest.json"
            }
            """;
    }

    private static string GetFileMatchingInfo()
    {
        return """
            {
              "fileExtensions": ["cs", "csx"]
            }
            """;
    }

    private static string GetAssemblyVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        return $"{fileVersionInfo.FileMajorPart}.{fileVersionInfo.FileMinorPart}.{fileVersionInfo.FileBuildPart}";
    }

    private static string ReadLicenseText()
    {
        using var stream =
            Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("Dprint.Plugins.Csharpier.LICENSE")
            ?? throw new Exception("Could not find license text.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
