using System;
using System.Numerics;

namespace MapRenderer.Common.Models.Shaders;

public class HeightMapShader
{
    public ShaderProgram Shader { get; set; }
    public bool Active => Shader?.Active ?? false;

    public ShaderValue<Matrix4x4> ModelViewProjectionMatrix { get; set; }
    public ShaderValue<bool> ShowWireframe { get; set; }
    public HeightMapShader(string vertexShader, string fragmentShader)
    {
        Shader = new(vertexShader, fragmentShader);

        ModelViewProjectionMatrix = new(Shader, "mvp");
        ShowWireframe = new(Shader, "showWireframe");
    }

    public void UseProgram()
    {
        Shader.UseProgram();
    }
}
