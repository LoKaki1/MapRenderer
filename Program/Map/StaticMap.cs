using System;
using System.Runtime.InteropServices;
using MapRenderer.Camera;
using MapRenderer.Common.Helpers;
using MapRenderer.Common.Helpers.Memory;
using MapRenderer.Common.Helpers.Shader;
using MapRenderer.Common.Models.Render;
using MapRenderer.Common.Models.Shaders;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using static MapRenderer.Common.Global;

namespace MapRenderer.Map;

public unsafe class StaticMap
{

    private readonly CameraController m_Camera;

    public HeightmapVertexBuffer Buffer { get; set; }

    private readonly IKeyboard m_Keyboard;

    public HeightMapShader HeightMapShader { get;  set; }
    public bool IsMapReady { get; private set; }

    public StaticMap(CameraController cameraController,
                     IKeyboard keyboard)
    {
        Buffer = new();
        
        m_Camera = cameraController;
        GenerateBuffer();
   
        m_Keyboard = keyboard;
        HeightMapShader = ShaderLoader.CreateDemo();
    }
    
    float GetHeight(int x, int z) => MathF.Sin(x * 0.5f) + MathF.Cos(z * 0.25f) * 2;

    void GenerateBuffer()
    {
        var bytes_vertexData = Marshal.SizeOf<HeightmapVertex>() * Constants.VERTICES_PER_CHUNK;
        var offset = (HeightmapVertex*)Allocator.Alloc(bytes_vertexData);
        var write = offset;
        for (int z = 0; z < Constants.HEIGHTMAP_SIZE; z++)
        {
        // Generate 32 triangle strips
            int x = 0;

            var altitude0 = GetHeight(x, z);
            var altitude1 = GetHeight(x, z + 1);
            var altitude2 = GetHeight(x + 1, z);


            // First vertex is a degenerate
            write++->Reset(altitude0); 


            // Create the first triangle
            write++->Reset(altitude0);
            write++->Reset(altitude1);
            write++->Reset(altitude2);

            // Rest of the strip
            x += 1;
            var altitude = GetHeight(x, z + 1);
            write++->Reset(altitude);

            x += 1;
            for (; x <= Constants.HEIGHTMAP_SIZE; x++)
            {
                altitude = GetHeight(x, z);
                write++->Reset(altitude);

                altitude = GetHeight(x, z + 1);
                write++->Reset(altitude);
            }


            // Degenerate
            altitude = GetHeight(x - 1, z + 1);
            write++->Reset(altitude);
        }

        Buffer.BufferData(Constants.VERTICES_PER_CHUNK, offset);
        Allocator.Free(ref offset, ref bytes_vertexData);

        IsMapReady = true;
    }

    public void Render()
    {
                // Prepare the shader
        HeightMapShader.UseProgram();
        var projection = m_Camera.GetViewProjection();
        HeightMapShader.ModelViewProjectionMatrix.Set(projection);
        var isSpacePressed = m_Keyboard.IsKeyPressed(Key.Space);
        HeightMapShader.ShowWireframe.Set(isSpacePressed);

        Gl.FrontFace(FrontFaceDirection.Ccw);
        Buffer.primitiveType = PrimitiveType.TriangleStrip;
        Buffer.BindAndDraw();
    }
}
