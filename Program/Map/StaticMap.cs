using System.Runtime.InteropServices;
using System;
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


    public HeightmapVertexBuffer Buffer { get; set; }

    private int m_LevelOfDetails;
    private int m_HeightmapSize;
    private int m_HeightmapSizeLod;
    private int m_VerticiesPerRun;
    private int m_VerticiesPerChunk;
    private int m_VerticiesPerRunNotDegenerate;
    private readonly CameraController m_Camera;

    private readonly IKeyboard m_Keyboard;

    public HeightMapShader HeightMapShader { get; set; }
    public bool IsUpdating { get; private set; }
    public bool IsRendering { get; private set; }

    public StaticMap(CameraController cameraController,
                     IKeyboard keyboard,
                     int levelOfDetails = 1,
                     int heightMapSize = 256)
    {
        Buffer = new();
        m_LevelOfDetails = levelOfDetails;
        m_HeightmapSize = heightMapSize;
        m_HeightmapSizeLod = heightMapSize / m_LevelOfDetails;
        m_VerticiesPerRun = m_HeightmapSizeLod * 2 + 4;
        m_VerticiesPerChunk = m_VerticiesPerRun * m_HeightmapSizeLod;
        m_VerticiesPerRunNotDegenerate = m_VerticiesPerRun - 3;

        m_Camera = cameraController;

        m_Keyboard = keyboard;
        HeightMapShader = ShaderLoader.CreateHeightmap("VertexShader.glsl", "FragmentShader.glsl");
    }

    float GetHeight(int x, int z, float height) => MathF.Sin((x + height) * m_LevelOfDetails * 0.5f) + MathF.Cos((z + height) * m_LevelOfDetails * 0.25f) * m_LevelOfDetails;

    void GenerateBuffer(float height, int levelOfDetails = -1)
    {
        IsUpdating = true;

        if (levelOfDetails != -1)
        {
            m_LevelOfDetails = levelOfDetails;
            m_HeightmapSizeLod = m_HeightmapSize / m_LevelOfDetails;
            m_VerticiesPerRun = m_HeightmapSizeLod * 2 + 4;
            m_VerticiesPerChunk = m_VerticiesPerRun * m_HeightmapSizeLod;
            m_VerticiesPerRunNotDegenerate = m_VerticiesPerRun - 3;
        }

        var bytes_vertexData = Marshal.SizeOf<HeightmapVertex>() * m_VerticiesPerChunk;
        var offset = (HeightmapVertex*)Allocator.Alloc(bytes_vertexData);
        var write = offset;


        for (int z = 0; z < m_HeightmapSizeLod; z++)
        {
            int x = 0;

            var altitude0 = GetHeight(x, z, height);
            var altitude1 = GetHeight(x, z + 1, height);
            var altitude2 = GetHeight(x + 1, z, height);


            // First vertex is a degenerate
            write++->Reset(altitude0);


            // Create the first triangle
            write++->Reset(altitude0);
            write++->Reset(altitude1);
            write++->Reset(altitude2);
            // Rest of the strip
            x += 1;
            var altitude = GetHeight(x, z + 1, height);
            write++->Reset(altitude);

            x += 1;

            for (; x <= m_HeightmapSizeLod; x++)
            {
                altitude = GetHeight(x, z, height);
                write++->Reset(altitude);

                altitude = GetHeight(x, z + 1, height);
                write++->Reset(altitude);

            }

            // Degenerate
            altitude = GetHeight(x - 1, z + 1, height);
            write++->Reset(altitude);
        }

        DispatcherQueue.Enqueue(() => BufferData(offset, bytes_vertexData));
    }
    public void Update(float height, int levelOfDetails = -1)
    {
        Task.Run(() => GenerateBuffer(height, levelOfDetails));
    }

    public void BufferData(HeightmapVertex* offset, int bytes_vertexData)

    {
        Buffer.BufferData(m_VerticiesPerChunk, offset);
        Allocator.Free(ref offset, ref bytes_vertexData);
        IsUpdating = false;
    }

    public void Render()
    {
        // Prepare the shader
        IsRendering = true;

        HeightMapShader.UseProgram();
        var projection = m_Camera.GetViewProjection();
        HeightMapShader.ModelViewProjectionMatrix.Set(projection);
        var isSpacePressed = m_Keyboard.IsKeyPressed(Key.Space);
        HeightMapShader.ShowWireframe.Set(isSpacePressed);
        HeightMapShader.PerRun.Set(m_VerticiesPerRun);
        HeightMapShader.PerRunNotDeg.Set(m_VerticiesPerRunNotDegenerate);
        HeightMapShader.Lod.Set(m_LevelOfDetails);

        Gl.FrontFace(FrontFaceDirection.Ccw);
        Buffer.primitiveType = PrimitiveType.TriangleStrip;
        Buffer.BindAndDraw();
        IsRendering = false;
    }

}
