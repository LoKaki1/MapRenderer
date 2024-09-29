using static MapRenderer.Common.Global;
using Silk.NET.OpenGL;

namespace MapRenderer.Common.Models.Shaders;

public class ShaderProgram
{
    public static string VERSION = "#version 330\n";
    static uint CurrentHandle;
    public bool Active => CurrentHandle == programHandle;

    Shader? m_VertexShader;
    Shader? m_FragmentShader;
    protected uint programHandle;

    public ShaderProgram(string vertexShaderSource, string fragmentShaderSource)
    {
        Initialise(vertexShaderSource, fragmentShaderSource);
    }

    void Initialise(string vertexShaderSource, string fragmentShaderSource)
    {
        try
        {
            m_VertexShader = new Shader(ShaderType.VertexShader, VERSION + vertexShaderSource);
            m_FragmentShader = new Shader(ShaderType.FragmentShader, VERSION + fragmentShaderSource);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Dispose();
            throw;
        }

        // Create shader program.
        programHandle = Gl.CreateProgram();


        // Attach shaders to the program
        Gl.AttachShader(programHandle, m_VertexShader.ShaderHandle);
        Gl.AttachShader(programHandle, m_FragmentShader.ShaderHandle);
        Gl.LinkProgram(programHandle);


        // Get the log
        Gl.GetProgram(programHandle, ProgramPropertyARB.InfoLogLength, out int logLen);

        if (logLen > 0)
        {
            Gl.GetProgramInfoLog(programHandle, (uint)logLen, out _, out string log);

            if (!string.IsNullOrEmpty(log))
                Console.WriteLine($"Program link log:\n{log}");
        }


        // Clean up
        Gl.DetachShader(programHandle, m_VertexShader.ShaderHandle);
        Gl.DetachShader(programHandle, m_FragmentShader.ShaderHandle);


        // Ensure the shaders were linked correctly
        Gl.GetProgram(programHandle, ProgramPropertyARB.LinkStatus, out int status);

        if (status == 0)
        {
            Console.WriteLine($"Shader link failed. Status: {status}");
            AssertFalse();
        }

        Unbind();
    }

    public virtual void UseProgram()
    {
        // Already current, all good
        if (Active)
            return;

        Gl.UseProgram(programHandle);
        CurrentHandle = programHandle;
    }


    public static void Unbind()
    {
        Gl.UseProgram(0);
        CurrentHandle = 0;
    }

    public int GetUniformLocation(string name) => Gl.GetUniformLocation(programHandle, name);

    public void Dispose()
    {
        if (m_VertexShader != null)
        {
            m_VertexShader.Dispose();
            m_VertexShader = null;
        }

        if (m_FragmentShader != null)
        {
            m_FragmentShader.Dispose();
            m_FragmentShader = null;
        }

        if (programHandle != 0)
        {
            Gl.DeleteProgram(programHandle);
            programHandle = 0;
        }
    }


    // Remember which is the current shader program
}
