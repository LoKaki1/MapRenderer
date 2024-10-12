namespace MapRenderer.Common.Models.Render;

public unsafe abstract class VertexBuffer<T> where T : unmanaged
{
    // Buffer handles
    protected uint m_VaoHandle;
    protected uint m_VboHandle;

    // GPU Data
    protected uint m_VertexSize;
    public int Symbol;
    public VertexBuffer(uint vertexSize)
    {
        m_VertexSize = vertexSize;

        // Create a VAO
        m_VaoHandle = Gl.GenVertexArray();
        Gl.BindVertexArray(m_VaoHandle);

        // Create a VBO
        m_VboHandle = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_VboHandle);

        // Set up attribs
        SetupVAO();
        // Clean up
        UnbindVAO();
        UnbindVBO();
    }

    public void BindAndDraw()
    {
        BindVAO();
        Gl.DrawArrays(primitiveType, 0, (uint)Symbol);
        UnbindVAO();
    }

    public void BufferData(int size, T* data)
    {
        Symbol = size;

        BindVBO();
        Gl.BufferData(BufferTargetARB.ArrayBuffer, 
                       (uint)(size * m_VertexSize), 
                        data,
                         BufferUsageARB.StaticDraw);
        UnbindVBO();
    }

    // Rendering settings
    public PrimitiveType primitiveType = PrimitiveType.Triangles;
     // Abstracts
    protected abstract void SetupVAO();
    // Shortcut functions
    public void BindVAO() => Gl.BindVertexArray(m_VaoHandle);
    public void BindVBO() => Gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_VboHandle);
    public static void UnbindVAO() => Gl.BindVertexArray(0);
    public static void UnbindVBO() => Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
}