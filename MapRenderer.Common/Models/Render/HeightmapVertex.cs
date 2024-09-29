using System;

namespace MapRenderer.Common.Models.Render;

public struct HeightmapVertex
{
    public float Altitude { get; set; }
    
    public void Reset(float altitude)
    {
        Altitude = altitude;
    }
}