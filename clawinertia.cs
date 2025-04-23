using Godot;

public partial class clawinertia : Node2D
{
    // Movement properties
    [Export] public float Acceleration = 200.0f; // How quickly the claw speeds up
    [Export] public float Deceleration = 200.0f; // How quickly the claw slows down
    [Export] public float MaxSpeed = 300.0f; // Maximum speed of the claw

    // Reference to the Sprite2D (claw)
    private Sprite2D _claw;

    // Current velocity of the claw
    private Vector2 _currentVelocity = Vector2.Zero;

    public override void _Ready()
    {
        // Get the Sprite2D node (child of the main node)
        _claw = GetNode<Sprite2D>("Kauha");

        // Debug: Check if _claw is null
        if (_claw == null)
        {
            GD.Print("Error: Sprite2D node not found!");
        }
        else
        {
            GD.Print("Sprite2D node found!");
        }
    }

    public override void _Process(double delta)
    {
        // Ensure _claw is not null before using it
        if (_claw == null)
        {
            return; // Exit early to avoid NullReferenceException
        }

        // Get input for movement
        Vector2 inputDirection = Vector2.Zero;

        if (Input.IsActionPressed("Left"))
        {
            inputDirection.X -= 1;
        }
        if (Input.IsActionPressed("Right"))
        {
            inputDirection.X += 1;
        }
        if (Input.IsActionPressed("Up"))
        {
            inputDirection.Y -= 1;
        }
        if (Input.IsActionPressed("Down"))
        {
            inputDirection.Y += 1;
        }

        // Normalize the input direction to prevent faster diagonal movement
        inputDirection = inputDirection.Normalized();

        // Calculate target velocity based on input
        Vector2 targetVelocity = inputDirection * MaxSpeed;

        // Smoothly interpolate the current velocity toward the target velocity
        if (inputDirection != Vector2.Zero)
        {
            // Accelerate
            _currentVelocity = _currentVelocity.MoveToward(targetVelocity, Acceleration * (float)delta);
        }
        else
        {
            // Decelerate
            _currentVelocity = _currentVelocity.MoveToward(Vector2.Zero, Deceleration * (float)delta);
        }

        // Move the claw
        _claw.Position += _currentVelocity * (float)delta;

        // Clamp the claw's position to stay within the screen bounds
        var viewportSize = GetViewportRect().Size;
        _claw.Position = new Vector2(
            Mathf.Clamp(_claw.Position.X, 0, viewportSize.X),
            Mathf.Clamp(_claw.Position.Y, 0, viewportSize.Y)
        );
    }
}