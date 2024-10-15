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


    public HeightmapVertexBuffer Buffer { get; set; }
    private readonly CameraController m_Camera;

    private readonly IKeyboard m_Keyboard;

    public HeightMapShader HeightMapShader { get; set; }
    public bool IsUpdating { get; private set; }
    public bool IsRendering { get; private set; }

    public StaticMap(CameraController cameraController,
                     IKeyboard keyboard)
    {
        Buffer = new();

        m_Camera = cameraController;
        GenerateBuffer(MathF.PI / 2.0f);

        m_Keyboard = keyboard;
        HeightMapShader = ShaderLoader.CreateDemo();
    }

    float GetHeight(int x, int z, float height) => MathF.Sin((x + height) * 0.5f) + MathF.Cos((z + height) * 0.25f) * 8;

    void GenerateBuffer(float height)
    {
        IsUpdating = true;
        var lod = 4;
        var bytes_vertexData = Marshal.SizeOf<HeightmapVertex>() * Constants.VERTICES_PER_CHUNK / lod;
        var offset = (HeightmapVertex*)Allocator.Alloc(bytes_vertexData);
        var write = offset;
        // Here what we want is to create a for loop that iterate on all level of details
        // if the the size is 1024 
        // then we want the level of detail to be bigger then its previous,
        // For example -
        // All the chunk is 1024
        // the the first level of details we go throught each vertex lets assume it has 64 x 64
        // then we'll go on the second which will 128 x 128
        // then 256 x 256
        // etc
        // imagine this - 
        //
        // 3 3 3 3 3 3 3
        // 3 3 3 3 3 3 3
        // 3 3 3 3 3 3 3
        // 2 2 2 2 3 3 3
        // 2 2 2 2 3 3 3 
        // 1 1 2 2 3 3 3 
        // Y 1 2 2 3 3 3
        // 1 1 2 2 3 3 3
        // 2 2 2 2 3 3 3
        // 2 2 2 2 3 3 3
        // 3 3 3 3 3 3 3
        // 3 3 3 3 3 3 3
        // 3 3 3 3 3 3 3
        // assume I did the (3) 4 times
        // Each level of detail shows twices time from the level that was previous

        // To start we need to calculate how many levels we (that of course dependes on the chunk 
        // size and the camera pos from its origin to get the origin of course we need some how to get 
        // its gl_position and that what will do tommrow)

        // var levels = Math.Log2(Constants.HEIGHTMAP_SIZE);

        // for (int i = 0; i < levels; i++)
        // {
        //     // for start lets just caluculate the chunk size base on the idea above
        //     // every level should contains the same number of vertcies
        //     // how do we do that?
        //     // first lets say how many layers we want 

        //     // then to we need to calculate how many vertecies are in the layer


        //     int stepSize = 1 << i; // 1, 2, 4, 8, 16, ...

        //     var layers = stepSize * stepSize;
        //     for (int z = 0; z < layers; z += stepSize) {
        //         int x = 0;

        //         var altitude0 = GetHeight(x, z);
        //         var altitude1 = GetHeight(x, z + stepSize);
        //         var altitude2 = GetHeight(x + stepSize, z);

        //         // First vertex is a degenerate (to restart triangle strip)
        //         write++->Reset(altitude0);

        //         // Create the first triangle in the strip
        //         write++->Reset(altitude0);
        //         write++->Reset(altitude1);
        //         write++->Reset(altitude2);

        //         // Iterate through the rest of the strip with stepSize
        //         x += stepSize;
        //         var altitude = GetHeight(x, z + stepSize);
        //         write++->Reset(altitude);

        //         x += stepSize;
        //         for (; x <= layers; x += stepSize) {
        //             altitude = GetHeight(x, z);
        //             write++->Reset(altitude);

        //             altitude = GetHeight(x, z + stepSize);
        //             write++->Reset(altitude);
        //         }

        //         // Degenerate to end the strip
        //         altitude = GetHeight(x - stepSize, z + stepSize);
        //         write++->Reset(altitude);
        //     }

        //     // now we iterate on each of the rows and append this to the frame buffer
        // } 
        var heightMapSize = Constants.HEIGHTMAP_SIZE / lod;
        for (int z = 0; z < heightMapSize; z++)
        {
            // Generate 32 triangle strips
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
            for (; x <= heightMapSize; x++)
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

        DispatcherQueue.Enqueue(() => BufferData(offset, bytes_vertexData, lod));
    }
    public void Update(float height)
    {
        Task.Run(() => GenerateBuffer(height));
    }
    public void BufferData(HeightmapVertex* offset, int bytes_vertexData, int lod)

    {
        Buffer.BufferData(Constants.VERTICES_PER_CHUNK / lod, offset);
        Allocator.Free(ref offset, ref bytes_vertexData);
        IsUpdating = false;
    }
    //     int CalculateLODLevel(const Vector3& cameraPosition, const Vector3& chunkCenter) {
    //     float distance = length(cameraPosition - chunkCenter);

    //     // Determine LOD level based on distance (tune distance thresholds as needed)
    //     if (distance < 100.0f) return 0; // Highest detail
    //     else if (distance < 200.0f) return 1;
    //     else if (distance < 400.0f) return 2;
    //     else if (distance < 800.0f) return 3;
    //     else return 4; // Lowest detail
    // }

    public void Render()
    {
        // Prepare the shader
        IsRendering = true;
        HeightMapShader.UseProgram();
        var projection = m_Camera.GetViewProjection();
        HeightMapShader.ModelViewProjectionMatrix.Set(projection);
        var isSpacePressed = m_Keyboard.IsKeyPressed(Key.Space);
        HeightMapShader.ShowWireframe.Set(isSpacePressed);

        Gl.FrontFace(FrontFaceDirection.Ccw);
        Buffer.primitiveType = PrimitiveType.TriangleStrip;
        Buffer.BindAndDraw();
        IsRendering = false;
    }
}
