using System.Threading;

namespace Dprint.Plugins.Csharpier.Utils;

/// <summary>
/// Thread safe counter.
/// </summary>
public class IdGenerator
{
    private uint _counter;

    public uint Next()
    {
        return Interlocked.Increment(ref _counter);
    }
}
