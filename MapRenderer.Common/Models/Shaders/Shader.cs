
namespace MapRenderer.Common.Models.Shaders;

public class Shader
{
    public uint ShaderHandle { get; set; }
    public Shader(ShaderType type, string source)
    {
        // Create and compile a shader
        ShaderHandle = Gl.CreateShader(type);
        Gl.ShaderSource(ShaderHandle, source);
        Gl.CompileShader(ShaderHandle);


        // Check the log
        Gl.GetShader(ShaderHandle, ShaderParameterName.InfoLogLength, out var logLen);

        if (logLen > 0)
        {
            Gl.GetShaderInfoLog(ShaderHandle, (uint)logLen, out _, out string log);

            if (!string.IsNullOrEmpty(log))
                Console.WriteLine($"Shader compile log:\n{log}");
        }


        // Check it compiled successfully
        Gl.GetShader(ShaderHandle, ShaderParameterName.CompileStatus, out var status);

        if (status == (int)GLEnum.True)
            return;



        // Delete it
        Gl.DeleteShader(ShaderHandle);
        ShaderHandle = 0;

        AssertFalse();
    }

    public void Dispose()
    {
        // Already disposed
        if (ShaderHandle == 0)
        {
            AssertFalse();
            return;
        }

        Gl.DeleteShader(ShaderHandle);
        ShaderHandle = 0;
    }

}
