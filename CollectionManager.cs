using Godot;
using System;
using System.Collections.Generic;

public partial class CollectionManager : Node2D
{
    [Export] public float RequiredWeight = 200.0f;
    [Export] public float TruckSpeed = 200.0f;
    [Export] public float GameDuration = 60.0f;
    [Export] public bool DebugMode = true;
    [Export] public float WeightIncreasePerRound = 50.0f;
    [Export] public float TimeIncreasePerRound = 10.0f;

    public bool IsTruckMoving { get; private set; }
    private StaticBody2D _truckBody;
    private Label _scoreLabel;
    private Label _timerLabel;
    private Timer _gameTimer;
    private float _currentWeight = 0;
    private bool _gameOver = false;
    private HashSet<Block> _collectedBlocks = new();
    private Area2D _collectionArea;
    private Vector2 _truckStartPos;
    private int _currentRound = 1;
    private ProgressBar _weightProgressBar;

    public override void _Ready()
    {
        _truckBody = GetNode<StaticBody2D>("StaticBody2D");
        _truckStartPos = _truckBody.Position;
        _scoreLabel = GetNode<Label>("ScoreLabel");
        _timerLabel = GetNode<Label>("TimerLabel");
        _gameTimer = GetNode<Timer>("GameTimer");
        _weightProgressBar = GetNode<ProgressBar>("WeightProgressBar");
        _weightProgressBar.MaxValue = RequiredWeight;

        _scoreLabel.Text = $"Round: {_currentRound}";
        _gameTimer.Timeout += OnGameTimerTimeout;
        _gameTimer.Start(GameDuration);

        _collectionArea = GetNode<Area2D>("StaticBody2D/CollectionArea");
        _collectionArea.BodyEntered += OnBlockEnteredCollectionArea;

        PrintDebug($"Game started! Required weight: {RequiredWeight}");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_gameOver) return;
        UpdateTimer();
        MoveTruck(delta);
    }

    public void AddBlock(Block block)
    {
        if (_collectedBlocks.Contains(block)) return;

        _collectedBlocks.Add(block);
        block.SetCollectionManager(this);
        MonitorBlockWeight(block);
    }

    private async void MonitorBlockWeight(Block block)
    {
        // Wait for block to settle
        while (IsInstanceValid(block) && block.LinearVelocity.Length() > 5f)
        {
            await ToSignal(GetTree().CreateTimer(0.2f), Timer.SignalName.Timeout);
            if (!IsInstanceValid(block)) return;
        }

        if (!IsInstanceValid(block) || !_collectedBlocks.Contains(block)) return;

        float weight = block.BlockType switch
        {
            BlockType.Metal => 80f,
            BlockType.Plastic => 50f,
            BlockType.Glass => 60f,
            BlockType.Wood => 30f,
            _ => 30f
        };

        _currentWeight += weight;
        _weightProgressBar.Value = _currentWeight;
        block.IsCollected = true;
        block.CollectedWeight = weight;

        PrintDebug($"Block settled! Weight: {weight} | Total: {_currentWeight}/{RequiredWeight}");

        if (_currentWeight >= RequiredWeight && !IsTruckMoving)
        {
            SendTruckAway();
        }
    }

    public void RemoveCollectedBlock(Block block)
    {
        if (!_collectedBlocks.Contains(block)) return;

        _currentWeight -= block.CollectedWeight;
        _weightProgressBar.Value = _currentWeight;
        _collectedBlocks.Remove(block);

        PrintDebug($"Collected block destroyed! (-{block.CollectedWeight}) | Total: {_currentWeight}/{RequiredWeight}");
    }

    private void UpdateTimer()
    {
        _timerLabel.Text = $"Time: {Mathf.Ceil(_gameTimer.TimeLeft)}";
    }

    private void MoveTruck(double delta)
    {
        if (!IsTruckMoving) return;

        _truckBody.Position += new Vector2(TruckSpeed * (float)delta, 0);

        if (_truckBody.Position.X > GetViewportRect().Size.X + 100)
        {
            ResetTruck();
        }
    }

    private void SendTruckAway()
    {
        IsTruckMoving = true;
        _gameTimer.Stop();
        PrintDebug("Truck is leaving!");
    }

    private void ResetTruck()
    {
        if (_gameOver) return;

        _truckBody.Position = _truckStartPos;
        IsTruckMoving = false;
        _currentWeight = 0;

        foreach (var block in _collectedBlocks)
        {
            if (IsInstanceValid(block))
                block.QueueFree();
        }
        _collectedBlocks.Clear();

        _currentRound++;
        RequiredWeight += WeightIncreasePerRound;
        GameDuration += TimeIncreasePerRound;

        _weightProgressBar.Value = 0;
        _weightProgressBar.MaxValue = RequiredWeight;

        _scoreLabel.Text = $"Round: {_currentRound}";
        _timerLabel.Text = $"Time: {Mathf.Ceil(GameDuration)}";

        _gameTimer.Start(GameDuration);
        PrintDebug($"Truck reset! Round {_currentRound} started | New weight: {RequiredWeight}, Time: {GameDuration}");
    }

    private void OnGameTimerTimeout()
    {
        _gameOver = true;
        GetTree().ChangeSceneToFile("res://GameLostScreen.tscn");
    }

    private void OnBlockEnteredCollectionArea(Node2D body)
    {
        if (body is Block block)
        {
            PrintDebug($"Block entered: {block.Name} (Type: {block.BlockType})");
            AddBlock(block);
        }
    }

    private void PrintDebug(string message)
    {
        if (DebugMode)
            GD.Print($"[COLLECT] {message}");
    }
}