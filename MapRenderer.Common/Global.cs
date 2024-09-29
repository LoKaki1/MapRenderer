global using System.Diagnostics;
global using Silk.NET.OpenGL;
global using static MapRenderer.Common.Global;

namespace MapRenderer.Common;

public static class Global
{
    public static GL Gl;

    public static void Assert(bool condition) => Debug.Assert(condition);
    public static void AssertFalse() => Debug.Assert(false);

}
