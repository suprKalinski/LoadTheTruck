using Godot;

public partial class claw : Node2D
{
    // Movement speed of the claw
    [Export] public float Speed = 100.0f;

    // Reference to the Sprite2D (claw)
    private Sprite2D _claw;

    public override void _Ready()
    {
        
        _claw = GetNode<Sprite2D>("Kauha");
        if (_claw == null)
        {
        GD.Print("Error: Kauha node not found");
        }
        else
        {
            GD.Print("Kauha node found");
        } 
    }

    public override void _Process(double delta)
    {
        // Get input for movement
        Vector2 velocity = Vector2.Zero;

        if (Input.IsActionPressed("Left"))
        {
            velocity.X -= 1;
        }
        if (Input.IsActionPressed("Right"))
        {
            velocity.X += 1;
        }
        if (Input.IsActionPressed("Up"))
        {
            velocity.Y -= 1;
        }
        if (Input.IsActionPressed("Down"))
        {
            velocity.Y += 1;
        }

        velocity = velocity.Normalized();
		
        _claw.Position += velocity * Speed * (float)delta;

        var viewportSize = GetViewportRect().Size;
        _claw.Position = new Vector2(
            Mathf.Clamp(_claw.Position.X, 0, viewportSize.X),
            Mathf.Clamp(_claw.Position.Y, 0, viewportSize.Y)
        );
    }
}
