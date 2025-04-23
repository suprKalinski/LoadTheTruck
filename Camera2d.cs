using Godot;
using System;
public partial class GameCamera : Camera2D
{
    [Export] public Vector2 GameAreaSize = new Vector2(1920, 1080); // Set your game area size

    public override void _Ready()
    {
        UpdateZoom();
    }

    public override void _Process(double delta)
    {
        UpdateZoom();
    }

    private void UpdateZoom()
    {
        var viewportSize = GetViewportRect().Size;
        float zoomX = viewportSize.X / GameAreaSize.X;
        float zoomY = viewportSize.Y / GameAreaSize.Y;
        Zoom = new Vector2(zoomX, zoomY);
    }
}

public partial class Camera2d : Camera2D
{
}
