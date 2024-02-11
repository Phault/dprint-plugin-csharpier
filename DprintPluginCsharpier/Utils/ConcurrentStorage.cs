using System.Collections.Generic;

namespace Dprint.Plugins.Csharpier.Utils;

/// <summary>
/// Concurrent dictionary but less verbose API.
/// </summary>
public class ConcurrentStorage<TValue>
{
    private readonly object _lock = new();
    private readonly Dictionary<uint, TValue> _values = new();

    public void StoreValue(uint messageId, TValue value)
    {
        lock (_lock)
            _values[messageId] = value;
    }

    public TValue? Take(uint messageId)
    {
        lock (_lock)
        {
            return _values.Remove(messageId, out var value) ? value : default;
        }
    }
}
