using Godot;

public partial class PauseButton : TouchScreenButton
{
    private bool _isPaused = false;

    public override void _Ready()
    {
        // Ensure the button is interactive during pause, so we don't need PauseMode.
        // TouchScreenButton should naturally work even when the game is paused.
    }

    // Called when the pause button is pressed
    public void _on_PauseTouchButton_pressed()
    {
        _isPaused = !_isPaused;

        // Pause or unpause the game
        GetTree().Paused = _isPaused;

        GD.Print("Game Paused: " + _isPaused);

        // Optional: You can show a pause menu or overlay when paused
        // If you have a pause menu, you can set its visibility here:
        // PauseMenu.Visible = _isPaused;
    }
}




