using Godot;
using System.Collections.Generic;

public partial class SoundManager : Node
{
    [Export] public AudioStream ClawOpenSound;
[Export] public AudioStream ClawCloseSound;
    [Export] public AudioStream WoodImpactSound;
    [Export] public AudioStream MetalImpactSound;
    [Export] public AudioStream GlassImpactSound;
    [Export] public AudioStream PlasticImpactSound;

    [Export] public AudioStream WoodBreakSound;
    [Export] public AudioStream MetalBreakSound;
    [Export] public AudioStream GlassBreakSound;
    [Export] public AudioStream PlasticBreakSound;

    [Export] public AudioStream TruckEngineSound;
    [Export] public AudioStream BackgroundMusic;

    private Queue<AudioStreamPlayer> _impactPlayers = new Queue<AudioStreamPlayer>();
    private AudioStreamPlayer _enginePlayer;
    private AudioStreamPlayer _musicPlayer;

    public override void _Ready()
    {
        // Remove singleton setup (no Instance = this;)
        for (int i = 0; i < 5; i++)
            _impactPlayers.Enqueue(CreatePlayer());

        _enginePlayer = CreatePlayer();
        _musicPlayer = CreatePlayer();

        PlayBackgroundMusic();
    }

    private AudioStreamPlayer CreatePlayer()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        return player;
    }

    public void PlayBlockImpact(BlockType type, float volume = 0f)
    {
        var sound = type switch {
            BlockType.Wood => WoodImpactSound,
            BlockType.Metal => MetalImpactSound,
            BlockType.Glass => GlassImpactSound,
            BlockType.Plastic => PlasticImpactSound,
            _ => null
        };
        PlayPooledSound(sound, volume);
    }

    public void PlayBlockBreak(BlockType type)
    {
        var sound = type switch {
            BlockType.Wood => WoodBreakSound,
            BlockType.Metal => MetalBreakSound,
            BlockType.Glass => GlassBreakSound,
            BlockType.Plastic => PlasticBreakSound,
            _ => null
        };
        PlayPooledSound(sound);
    }
    public void PlayClawOpenSound()
{
    PlayPooledSound(ClawOpenSound);
}

public void PlayClawCloseSound()
{
    PlayPooledSound(ClawCloseSound);
}





    private void PlayPooledSound(AudioStream sound, float volume = 0f)
    {
        if (sound == null) return;

        var player = _impactPlayers.Count > 0 ? _impactPlayers.Dequeue() : CreatePlayer();
        player.Stream = sound;
        player.VolumeDb = volume;
        player.PitchScale = GD.Randf() * 0.2f + 0.9f;
        player.Play();
        player.Finished += () => _impactPlayers.Enqueue(player);
    }

    public void PlayTruckEngine(bool play)
    {
        if (play && !_enginePlayer.Playing)
        {
            _enginePlayer.Stream = TruckEngineSound;
            _enginePlayer.Play();
        }
        else if (!play)
        {
            _enginePlayer.Stop();
        }
    }

    public void PlayBackgroundMusic()
    {
        _musicPlayer.Stream = BackgroundMusic;
        _musicPlayer.VolumeDb = -10f;
        _musicPlayer.Play();
    }
}
