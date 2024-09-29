using System;
using Silk.NET.OpenGL;
using static MapRenderer.Common.Global;

namespace MapRenderer.Common.Models.Render;

public unsafe abstract class VertexBuffer<T> where T : unmanaged
{
    public VertexBuffer(uint vertexSize)
    {
        m_VertexSize = vertexSize;

        // Create a VAO
        vaoHandle = Gl.GenVertexArray();
        Gl.BindVertexArray(vaoHandle);


        // Create a VBO
        vboHandle = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vboHandle);


        // Set up attribs
        SetupVAO();


        // Clean up
        UnbindVAO();
        UnbindVBO();
    }

    public void BindAndDraw()
    {
        BindVAO();
        Gl.DrawArrays(primitiveType, 0, (uint)size);
        UnbindVAO();
    }

    public void BufferData(int size, T* data)
    {
        this.size = size;

        BindVBO();
        Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(size * m_VertexSize), data, BufferUsageARB.StaticDraw);
        UnbindVBO();
    }


    // GPU data
    public int size;
    protected uint m_VertexSize;


    // Rendering settings
    public PrimitiveType primitiveType = PrimitiveType.Triangles;


    // Buffer handles
    uint vaoHandle;
    uint vboHandle;


    // Abstracts
    protected abstract void SetupVAO();


    // Shortcut functions
    public void BindVAO() => Gl.BindVertexArray(vaoHandle);
    public void BindVBO() => Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vboHandle);

    public static void UnbindVAO() => Gl.BindVertexArray(0);
    public static void UnbindVBO() => Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
}