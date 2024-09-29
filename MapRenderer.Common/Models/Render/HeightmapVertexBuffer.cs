using System;
using Silk.NET.OpenGL;
using static MapRenderer.Common.Global;

namespace MapRenderer.Common.Models.Render;

public unsafe class HeightmapVertexBuffer : VertexBuffer<HeightmapVertex>
{
    public HeightmapVertexBuffer() : base(4) { }

    protected override void SetupVAO()
    {
        Gl.EnableVertexAttribArray(0);

        Gl.VertexAttribPointer(0,
                               1,
                               VertexAttribPointerType.Float,
                               false,
                               m_VertexSize,
                               null);
    }
}
