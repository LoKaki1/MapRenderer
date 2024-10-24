using System;
using MapRenderer.Camera;
using Silk.NET.Input;

namespace MapRenderer.Map;

public class DynamicMap
{
    private readonly List<StaticMap> m_Meshes;
    private readonly CameraController m_Camera;

    private readonly IKeyboard m_Keyboard;

    private float m_Update;
    private StaticMap m_LastChunkRendered;

    public DynamicMap(CameraController camera, IKeyboard keyboard)
    {
        m_Camera = camera;
        m_Keyboard = keyboard;
        m_Meshes = [];
        m_Update = 0.1f;
    }

    public void Update()
    {
        int count = 0;

        for (int i = m_Meshes.Count - 1; i >= 0; i--)
        {
            var mesh = m_Meshes[i];

            if (mesh.IsRendering || mesh.IsUpdating)
            {
                continue;
            }

            m_Update += 0.1f;
            mesh.Update(m_Update);
            var temp = m_Meshes[0];
            m_Meshes[0] = mesh;
            m_Meshes[i] = temp;

            return;
        }
        var newChunk = new StaticMap(m_Camera, m_Keyboard ,levelOfDetails: 1);
        m_Update += 0.1f;
        newChunk.Update(m_Update);
        m_Meshes.Insert(0, newChunk);

    }

    public void Render()
    {
         foreach (var chunk in m_Meshes)
        {
            if (!chunk.IsUpdating || !chunk.IsRendering)
            {
                chunk.Render();

                return;
            }
        }

        Console.WriteLine("gon");
    }
}
