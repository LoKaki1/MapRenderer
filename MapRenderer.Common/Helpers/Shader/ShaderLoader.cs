using System;
using MapRenderer.Common.Models.Shaders;

namespace MapRenderer.Common.Helpers.Shader;

public static class ShaderLoader
{

    public static ShaderProgram CreateProgram(string vertexShaderPath,
                                              string fragmentShaderPath)
    {
        var vertexShaderSource = File.ReadAllText(vertexShaderPath);
        var fragmentShaderSource = File.ReadAllText(fragmentShaderPath);

        var program = new ShaderProgram(vertexShaderSource, fragmentShaderSource);

        return program;
    }

    public static HeightMapShader CreateHeightmap(string verteciesShaderPath, string fractShaderPath)
    {
        var vertexShaderSource = File.ReadAllText(verteciesShaderPath);
        var fragmentShaderSource = File.ReadAllText(fractShaderPath);

        var program = new HeightMapShader(vertexShaderSource, fragmentShaderSource);

        return program;
    }
    public static async Task<ShaderProgram> CreateProgramAsync(string vertexShaderPath,
                                                               string fragmentShaderPath)
    {
        var vertexShaderSource = await File.ReadAllTextAsync(vertexShaderPath);
        var fragmentShaderSource = await File.ReadAllTextAsync(fragmentShaderPath);

        var program = new ShaderProgram(vertexShaderSource, fragmentShaderSource);

        return program;
    }

    public static HeightMapShader CreateDemo()
    {
        return new HeightMapShader(Constants.VERTEX_SHADER, Constants.FRAGMENT_SHADER);
    }

}