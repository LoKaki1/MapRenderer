global using System.Diagnostics;
global using Silk.NET.OpenGL;
global using static MapRenderer.Common.Global;
using System.Collections.Concurrent;

namespace MapRenderer.Common;

public static class Global
{
    public static GL Gl = new GL(null);
    public static ConcurrentQueue<Action> DispatcherQueue = new ConcurrentQueue<Action>();

    public static void Assert(bool condition) => Debug.Assert(condition);
    public static void AssertFalse() => Debug.Assert(false);

}
