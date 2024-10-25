using System;
using System.Numerics;
using MapRenderer.Common.Helpers.Camera;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace MapRenderer.Camera;

public class CameraController
{
    private readonly IKeyboard m_Keyboard;
    private readonly IMouse m_Mouse;
    private readonly IWindow m_Window;
    // Camera
    private bool m_CaptureMouse;
    private Vector2 m_LastMouse;
    public Vector3 CameraPos { get; set; }
    public float Pitch { get; set; }
    public float Yaw { get; set; }

    float FieldOfView { get; set; } = 50.0f / 180.0f * MathF.PI;
    float Aspect => m_Window.Size.X / (float)m_Window.Size.Y;
    float NearPlane { get; set; } = 1.0f;
    float FarPlane { get; set; } = 16384f;

    public CameraController(IKeyboard m_Keyboard, IMouse Mouse, IWindow Window)
    {
        this.m_Keyboard = m_Keyboard;
        m_Mouse = Mouse;
        m_Window = Window;

        m_CaptureMouse = true;
        m_LastMouse = m_Mouse.Position;

        m_Keyboard.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape)
        {
            m_CaptureMouse = !m_CaptureMouse;

            // Don't snip the camera when capturing the mouse
            m_LastMouse = m_Mouse.Position;
        }
    }


    public Matrix4x4 GetViewProjection()
    {
        var view = CameraCauclations.CreateFPSView(CameraPos, Pitch, Yaw);
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, Aspect, NearPlane, FarPlane);

        return view * proj;
    }

    public void Update()
    {
        // Update camera roatation
        if (m_CaptureMouse)
        {
            var diff = m_LastMouse - m_Mouse.Position;

            Yaw -= diff.X * 0.003f;
            Pitch += diff.Y * 0.003f;

            m_Mouse.Position = new Vector2(m_Window.Size.X / 2, m_Window.Size.Y / 2);
            m_LastMouse = m_Mouse.Position;
            m_Mouse.Cursor.CursorMode = CursorMode.Hidden;
        }
        else
        {
            m_Mouse.Cursor.CursorMode = CursorMode.Normal;
        }

        // Update camera position
        // Fly camera movement
        float movementSpeed = 0.15f;

        if (m_Keyboard.IsKeyPressed(Key.ShiftLeft))
        {
            movementSpeed *= 16;
        }
        if (m_Keyboard.IsKeyPressed(Key.W))
            CameraPos += CameraCauclations.FromPitchYaw(Pitch, Yaw) * movementSpeed;
        else if (m_Keyboard.IsKeyPressed(Key.S))
            CameraPos -= CameraCauclations.FromPitchYaw(Pitch, Yaw) * movementSpeed;

        if (m_Keyboard.IsKeyPressed(Key.A))
            CameraPos += CameraCauclations.FromPitchYaw(0, Yaw - MathF.PI / 2) * movementSpeed;
        else if (m_Keyboard.IsKeyPressed(Key.D))
            CameraPos += CameraCauclations.FromPitchYaw(0, Yaw + MathF.PI / 2) * movementSpeed;

        if (m_Keyboard.IsKeyPressed(Key.E))
            CameraPos += CameraCauclations.FromPitchYaw(MathF.PI / 2, 0) * movementSpeed;
        else if (m_Keyboard.IsKeyPressed(Key.Q))
            CameraPos += CameraCauclations.FromPitchYaw(-MathF.PI / 2, 0) * movementSpeed;
    }
}
