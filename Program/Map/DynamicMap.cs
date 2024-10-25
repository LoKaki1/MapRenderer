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
    public int MapSize { get; private init; }

    private float m_Update;
    private readonly float m_Updater;

    public DynamicMap(CameraController camera, IKeyboard keyboard)
    {
        m_Camera = camera;
        m_Keyboard = keyboard;
        m_Meshes = [];
        m_Update = 0f;
        m_Updater = 0.1f;
        m_MapPos = new Vector3();
        MapSize = 128;
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

            m_Update += m_Updater;
            mesh.Update(m_Update, level);
            var temp = m_Meshes[0];
            m_Meshes[0] = mesh;
            m_Meshes[i] = temp;

            return;
        }

        var newChunk = new StaticMap(m_Camera, m_Keyboard ,levelOfDetails: level, heightMapSize: MapSize);
        m_Update += m_Updater;
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
        var chunkCenter = new Vector3(m_MapPos.X + MapSize / 2, 0, m_MapPos.Z + MapSize / 2);
        float halfSize = MapSize / 2;
        Vector3 min = chunkCenter - new Vector3(halfSize, halfSize, halfSize);
        Vector3 max = chunkCenter + new Vector3(halfSize, halfSize, halfSize);
        var cameraPosition = m_Camera.CameraPos;
        Vector3 closestPoint = new Vector3(
            Math.Clamp(cameraPosition.X, min.X, max.X),
            Math.Clamp(cameraPosition.Y, min.Y, max.Y),
            Math.Clamp(cameraPosition.Z, min.Z, max.Z));

        var distance =Vector3.Distance(m_Camera.CameraPos, closestPoint);
        int lodLevel;
        if (distance < 250)
            lodLevel = 1; // High detail
        else if (distance < 450)
            lodLevel = 2; // Medium detail
        else if (distance < 550)
            lodLevel = 3; // Low detail
        else if (distance < 800)
            lodLevel = 4;
        else 
            lodLevel = 5;
        
        return lodLevel;
    }
}
