using System;
using MapRenderer.Client;

namespace MapRenderer;

public class Program
{
    public static void Main(string[] args)
    {
        var client = new CameraMapClient();

        client.Run();
    }
}
