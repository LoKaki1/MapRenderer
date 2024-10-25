using System;
using System.Numerics;
using MapRenderer.Camera;
using Silk.NET.Input;

namespace MapRenderer.Map;

public class DynamicMap
{
    private readonly List<StaticMap> m_Meshes;
    private readonly CameraController m_Camera;
    private readonly Vector3 m_MapPos;
    private readonly IKeyboard m_Keyboard;
    private readonly int m_MapSize;

    private float m_Update;


    public DynamicMap(CameraController camera, IKeyboard keyboard)
    {
        m_Camera = camera;
        m_Keyboard = keyboard;
        m_Meshes = [];
        m_Update = 0.1f;
        m_MapPos = new Vector3();
        m_MapSize = 256;
    }

    public void Update()
    {
        var level = GetLevelOfDetail();
        for (int i = m_Meshes.Count - 1; i >= 0; i--)
        {
            var mesh = m_Meshes[i];

            if (mesh.IsRendering || mesh.IsUpdating)
            {
                continue;
            }

            m_Update += 0.1f;
            mesh.Update(m_Update, level);
            var temp = m_Meshes[0];
            m_Meshes[0] = mesh;
            m_Meshes[i] = temp;

            return;
        }

        var newChunk = new StaticMap(m_Camera, m_Keyboard ,levelOfDetails: level, heightMapSize: m_MapSize);
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

    private int GetLevelOfDetail()
    {
        var middle = new Vector3(m_MapPos.X + m_MapSize / 2, 0, m_MapPos.Z + m_MapSize / 2);
        var distance =Vector3.Distance(m_Camera.CameraPos, middle) - 180;
        int lodLevel;
        if (distance < 50)
            lodLevel = 1; // High detail
        else if (distance < 400)
            lodLevel = 2; // Medium detail
        else if (distance < 800)
            lodLevel = 3; // Low detail
        else if (distance < 1600)
            lodLevel = 4;
        else if (distance < 3200)
            lodLevel = 5;
        else
            lodLevel = 6;
        return lodLevel;
    }
}
