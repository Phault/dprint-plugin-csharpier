using System;

namespace Dprint.Plugins.Csharpier.Communication;

public enum MessageKind
{
    Success = 0,
    DataResponse = 1,
    ErrorResponse = 2,
    Shutdown = 3,
    Active = 4,
    GetPluginInfo = 5,
    GetLicenseText = 6,
    RegisterConfig = 7,
    ReleaseConfig = 8,
    GetConfigDiagnostics = 9,
    GetFileMatchingInfo = 10,
    GetResolvedConfig = 11,
    CheckConfigUpdates = 12,
    FormatText = 13,
    FormatTextResponse = 14,
    CancelFormat = 15,
    HostFormat = 16,
}

public abstract class Message(uint messageId, MessageKind kind)
{
    public uint MessageId { get; set; } = messageId;
    public MessageKind Kind { get; set; } = kind;

    public void Write(MessageWriter writer)
    {
        writer.WriteUint(MessageId);
        writer.WriteUint((uint)Kind);
        WriteBody(writer);
        writer.WriteSuccessBytes();
    }

    protected abstract void WriteBody(MessageWriter writer);

    public static Message Read(MessageReader reader)
    {
        var messageId = reader.ReadUint();
        var kind = reader.ReadUint();

        var messageKind = (MessageKind)kind;
        Message result = messageKind switch
        {
            MessageKind.Success => new SuccessResponseMessage(messageId, reader.ReadUint()),
            MessageKind.DataResponse
                => new DataResponseMessage(messageId, reader.ReadUint(), reader.ReadVariableData()),
            MessageKind.ErrorResponse
                => new ErrorResponseMessage(
                    messageId,
                    reader.ReadUint(),
                    reader.ReadVariableData()
                ),
            MessageKind.Shutdown => new ShutdownMessage(messageId),
            MessageKind.Active => new ActiveMessage(messageId),
            MessageKind.GetPluginInfo => new GetPluginInfoMessage(messageId),
            MessageKind.GetLicenseText => new GetLicenseTextMessage(messageId),
            MessageKind.RegisterConfig
                => new RegisterConfigMessage(
                    messageId,
                    reader.ReadUint(),
                    reader.ReadVariableData(),
                    reader.ReadVariableData()
                ),
            MessageKind.ReleaseConfig => new ReleaseConfigMessage(messageId, reader.ReadUint()),
            MessageKind.GetConfigDiagnostics
                => new GetConfigDiagnosticsMessage(messageId, reader.ReadUint()),
            MessageKind.GetFileMatchingInfo
                => new GetFileMatchingInfo(messageId, reader.ReadUint()),
            MessageKind.GetResolvedConfig
                => new GetResolvedConfigMessage(messageId, reader.ReadUint()),
            MessageKind.CheckConfigUpdates
                => new CheckConfigUpdatesMessage(messageId, reader.ReadVariableData()),
            MessageKind.FormatText
                => new FormatTextMessage(
                    messageId,
                    reader.ReadVariableData(),
                    reader.ReadUint(),
                    reader.ReadUint(),
                    reader.ReadUint(),
                    reader.ReadVariableData(),
                    reader.ReadVariableData()
                ),
            MessageKind.FormatTextResponse
                => FormatTextResponseMessage.FromReader(messageId, reader),
            MessageKind.CancelFormat => new CancelFormatMessage(messageId, reader.ReadUint()),
            MessageKind.HostFormat
                => new HostFormatMessage(
                    messageId,
                    reader.ReadVariableData(),
                    reader.ReadUint(),
                    reader.ReadUint(),
                    reader.ReadVariableData(),
                    reader.ReadVariableData()
                ),
            _ => throw new ArgumentOutOfRangeException($"Unknown message kind: {messageKind}"),
        };

        reader.ReadSuccessBytes();

        return result;
    }
}

public class SuccessResponseMessage(uint messageId, uint originalMessageId)
    : Message(messageId, MessageKind.Success)
{
    public uint OriginalMessageId { get; } = originalMessageId;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(OriginalMessageId);
    }
}

public class DataResponseMessage(uint messageId, uint originalMessageId, byte[] data)
    : Message(messageId, MessageKind.DataResponse)
{
    public uint OriginalMessageId { get; } = originalMessageId;
    public byte[] Data { get; } = data;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(OriginalMessageId);
        writer.WriteVariableWidth(Data);
    }
}

public class ErrorResponseMessage(uint messageId, uint originalMessageId, byte[] data)
    : Message(messageId, MessageKind.ErrorResponse)
{
    public uint OriginalMessageId { get; } = originalMessageId;
    public byte[] Data { get; } = data;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(OriginalMessageId);
        writer.WriteVariableWidth(Data);
    }
}

public class ShutdownMessage(uint messageId) : Message(messageId, MessageKind.Shutdown)
{
    protected override void WriteBody(MessageWriter writer)
    {
        // no body
    }
}

public class ActiveMessage(uint messageId) : Message(messageId, MessageKind.Active)
{
    protected override void WriteBody(MessageWriter writer)
    {
        // no body
    }
}

public class GetPluginInfoMessage(uint messageId) : Message(messageId, MessageKind.GetPluginInfo)
{
    protected override void WriteBody(MessageWriter writer)
    {
        // no body
    }
}

public class GetLicenseTextMessage(uint messageId) : Message(messageId, MessageKind.GetLicenseText)
{
    protected override void WriteBody(MessageWriter writer)
    {
        // no body
    }
}

public class RegisterConfigMessage(
    uint messageId,
    uint configId,
    byte[] globalConfigData,
    byte[] pluginConfigData
) : Message(messageId, MessageKind.RegisterConfig)
{
    public uint ConfigId { get; } = configId;
    public byte[] GlobalConfigData { get; } = globalConfigData;
    public byte[] PluginConfigData { get; } = pluginConfigData;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(ConfigId);
        writer.WriteVariableWidth(GlobalConfigData);
        writer.WriteVariableWidth(PluginConfigData);
    }
}

public class ReleaseConfigMessage(uint messageId, uint configId)
    : Message(messageId, MessageKind.ReleaseConfig)
{
    public uint ConfigId { get; } = configId;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(ConfigId);
    }
}

public class GetConfigDiagnosticsMessage(uint messageId, uint configId)
    : Message(messageId, MessageKind.GetConfigDiagnostics)
{
    public uint ConfigId { get; } = configId;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(ConfigId);
    }
}

public class GetFileMatchingInfo(uint messageId, uint configId)
    : Message(messageId, MessageKind.GetFileMatchingInfo)
{
    public uint ConfigId { get; } = configId;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(ConfigId);
    }
}

public class GetResolvedConfigMessage(uint messageId, uint configId)
    : Message(messageId, MessageKind.GetResolvedConfig)
{
    public uint ConfigId { get; } = configId;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(ConfigId);
    }
}

public class CheckConfigUpdatesMessage(uint messageId, byte[] pluginConfig)
    : Message(messageId, MessageKind.CheckConfigUpdates)
{
    public byte[] PluginConfig { get; } = pluginConfig;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteVariableWidth(PluginConfig);
    }
}

public class FormatTextMessage(
    uint messageId,
    byte[] filePath,
    uint startByteIndex,
    uint endByteIndex,
    uint configId,
    byte[] overrideConfig,
    byte[] fileText
) : Message(messageId, MessageKind.FormatText)
{
    public byte[] FilePath { get; } = filePath;
    public uint StartByteIndex { get; } = startByteIndex;
    public uint EndByteIndex { get; } = endByteIndex;
    public uint ConfigId { get; } = configId;
    public byte[] OverrideConfig { get; } = overrideConfig;
    public byte[] FileText { get; } = fileText;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteVariableWidth(FilePath);
        writer.WriteUint(StartByteIndex);
        writer.WriteUint(EndByteIndex);
        writer.WriteUint(ConfigId);
        writer.WriteVariableWidth(OverrideConfig);
        writer.WriteVariableWidth(FileText);
    }
}

public class FormatTextResponseMessage(uint messageId, uint originalMessageId, byte[]? content)
    : Message(messageId, MessageKind.FormatTextResponse)
{
    public byte[]? Content { get; } = content;
    public uint OriginalMessageId { get; } = originalMessageId;

    public static FormatTextResponseMessage FromReader(uint messageId, MessageReader reader)
    {
        var originalMessageId = reader.ReadUint();
        var kind = reader.ReadUint();
        return kind switch
        {
            0 => new FormatTextResponseMessage(messageId, originalMessageId, null),
            1
                => new FormatTextResponseMessage(
                    messageId,
                    originalMessageId,
                    reader.ReadVariableData()
                ),
            _ => throw new Exception($"Unknown message kind: {kind}"),
        };
    }

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(OriginalMessageId);
        if (Content == null)
        {
            writer.WriteUint(0);
        }
        else
        {
            writer.WriteUint(1);
            writer.WriteVariableWidth(Content);
        }
    }
}

public class CancelFormatMessage(uint messageId, uint originalMessageId)
    : Message(messageId, MessageKind.CancelFormat)
{
    public uint OriginalMessageId { get; } = originalMessageId;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteUint(OriginalMessageId);
    }
}

public class HostFormatMessage(
    uint messageId,
    byte[] filePath,
    uint startByteIndex,
    uint endByteIndex,
    byte[] overrideConfig,
    byte[] fileText
) : Message(messageId, MessageKind.FormatText)
{
    public byte[] FilePath { get; } = filePath;
    public uint StartByteIndex { get; } = startByteIndex;
    public uint EndByteIndex { get; } = endByteIndex;
    public byte[] OverrideConfig { get; } = overrideConfig;
    public byte[] FileText { get; } = fileText;

    protected override void WriteBody(MessageWriter writer)
    {
        writer.WriteVariableWidth(FilePath);
        writer.WriteUint(StartByteIndex);
        writer.WriteUint(EndByteIndex);
        writer.WriteVariableWidth(OverrideConfig);
        writer.WriteVariableWidth(FileText);
    }
}
