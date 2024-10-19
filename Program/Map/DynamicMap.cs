using System;
using MapRenderer.Camera;
using Silk.NET.Input;

namespace MapRenderer.Map;

public class DynamicMap
{
    private readonly List<StaticMap> m_Chunks;
    private readonly CameraController m_Camera;

    private readonly IKeyboard m_Keyboard;

    private float m_Update;
    public DynamicMap(CameraController camera, IKeyboard keyboard)
    {
        m_Camera = camera;
        m_Keyboard = keyboard;
        m_Chunks = new List<StaticMap>();
        m_Update = 0.1f;
    }

    public void Update()
    {
        foreach (var chunk in m_Chunks)
        {
            if (chunk.IsRendering || chunk.IsUpdating)
            {
                continue;
            }

            chunk.Update(m_Update);

            m_Update += 0.01f;
            return;
        }

        var newChunk = new StaticMap(m_Camera, m_Keyboard);
        m_Chunks.Add(newChunk);

    }

    public void Render()
    {
        foreach (var chunk in m_Chunks)
        {
            if (!chunk.IsUpdating || !chunk.IsRendering)
            {
                chunk.Render();

                return;
            }
        }
    }
}
