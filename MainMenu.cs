using Godot;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        // Get the TouchScreenButton (now direct child of MainMenu)
        var playButton = GetNodeOrNull<TouchScreenButton>("PlayButton");

        if (playButton == null)
        {
            GD.PrintErr("Error: PlayButton (TouchScreenButton) not found as direct child");
            // Debug: Print all child nodes
            GD.Print("Current children:");
            foreach (Node child in GetChildren())
            {
                GD.Print($"- {child.Name} ({child.GetType()})");
            }
            return;
        }

        // Connect the pressed signal
        playButton.Pressed += OnPlayButtonPressed;
        GD.Print("PlayButton connected successfully");
    }

    private void OnPlayButtonPressed()
    {
        GD.Print("PlayButton pressed - attempting to load game scene");

        string scenePath = "res://Claw.tscn";

        // Check if scene exists before loading
        if (!ResourceLoader.Exists(scenePath))
        {
            GD.PrintErr($"Error: Game scene file not found at path '{scenePath}'");
            // Debug: Check what files exist
            GD.Print("Checking for files in res://");
            // Note: In C# you might need to use Godot's Directory class to list files
            return;
        }

        // Load and change to game scene
        var gameScene = GD.Load<PackedScene>(scenePath);
        if (gameScene == null)
        {
            GD.PrintErr("Error: Failed to load the scene despite it existing");
            return;
        }

        GetTree().ChangeSceneToPacked(gameScene);
        GD.Print("Scene change initiated");
    }
}