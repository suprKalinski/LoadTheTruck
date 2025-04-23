using Godot;

public partial class GameLostScreen : CanvasLayer
{
    // Reference to the touch buttons
    private TouchScreenButton _restartButton;
    private TouchScreenButton _quitButton;

    public override void _Ready()
    {
        // Get the touch buttons
        _restartButton = GetNode<TouchScreenButton>("RestartButton");
        _quitButton = GetNode<TouchScreenButton>("QuitButton");

        // Connect button signals
        if (_restartButton != null)
        {
            _restartButton.Connect("pressed", new Callable(this, nameof(OnRestartButtonPressed)));
        }
        else
        {
            GD.PrintErr("Error: RestartButton node not found!");
        }

        if (_quitButton != null)
        {
            _quitButton.Connect("pressed", new Callable(this, nameof(OnQuitButtonPressed)));
        }
        else
        {
            GD.PrintErr("Error: QuitButton node not found!");
        }
    }

    private void OnRestartButtonPressed()
    {
        GD.Print("RestartButton pressed!");

        // Unpause the game
        GetTree().Paused = false;

        // Reload the current scene
        GetTree().ChangeSceneToFile("res://Claw.tscn");
    }

    private void OnQuitButtonPressed()
    {
        GD.Print("QuitButton pressed!");

        // Unpause the game
        GetTree().Paused = false;

        // Change to the main menu scene
        GetTree().ChangeSceneToFile("res://Mainmenu.tscn"); // Replace with the path to your main menu scene
    }
}