using System;

namespace MapRenderer.Map;

public class DynamicMap
{
    private readonly List<StaticMap> m_Chunks;

    public DynamicMap()
    {
        m_Chunks = new List<StaticMap>();
    }
}
