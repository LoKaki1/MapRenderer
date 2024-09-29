using System;
using System.Runtime.InteropServices;
using MapRenderer.Camera;
using MapRenderer.Map;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using static MapRenderer.Common.Global;
namespace MapRenderer.Client;

public unsafe class CameraMapClient
{
    private readonly IWindow m_Window;
    private  CameraController? m_Camera;
    private  StaticMap? m_Map;
    
    public CameraMapClient()
    {
        // Create a Silk.NET window
        var options = WindowOptions.Default;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(3, 3));
        options.Position = new(200, 200);
        options.PreferredDepthBufferBits = 32;
        options.Title = "gl_VertexID";

        m_Window = Window.Create(options);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            var glfw = GlfwProvider.GLFW.Value;
            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 4); // Change version if needed
            glfw.WindowHint(WindowHintInt.ContextVersionMinor, 6); // or 4.5, depending on your machine
            glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
            glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
            glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
            glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
        }
        // Callback when the window is created
        m_Window.Load += () =>
        {
            // Create an OpenGL Context
            Gl = m_Window.CreateOpenGL();
            // SilkOnDidCreateOpenGLContext();
        
            // Precalculate input stuff
            var inputContext = m_Window.CreateInput();
            var keyboard = inputContext.Keyboards[0];
            var mouse = inputContext.Mice[0];

            m_Camera = new CameraController(keyboard, mouse, m_Window);
            m_Map = new StaticMap(m_Camera, keyboard);
        };

        m_Window.Render += (_) => Render();

        m_Window.Size = new(800, 600);
        m_Window.FramesPerSecond = 144;
        m_Window.UpdatesPerSecond = 144;
        m_Window.VSync = false;
        // m_Window.FocusChanged += SilkOnFocusChanged;

        // Initialise OpenGL and input context
        m_Window.Initialize();
    }
    public void Run()
    {
        m_Window.Run();
    }
    private void Render()
    {
        m_Camera?.Update();
        PreRenderSetup();
        m_Map?.Render();
    }

    void PreRenderSetup()
    {
        // Prepare rendering
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        Gl.Enable(EnableCap.DepthTest);
        Gl.Disable(EnableCap.Blend);
        Gl.Disable(EnableCap.StencilTest);
        Gl.Enable(EnableCap.CullFace);
        Gl.FrontFace(FrontFaceDirection.CW);


        // Clear everything
        Gl.ClearDepth(1.0f);
        Gl.DepthFunc(DepthFunction.Less);

        Gl.ColorMask(true, true, true, true);
        Gl.DepthMask(true);

        Gl.ClearColor(0, 0, 0, 0);
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);


        // Set the viewport to the window size
        Gl.Viewport(0, 0, (uint)m_Window.Size.X * 4, (uint)m_Window.Size.Y * 4);
    }
}
