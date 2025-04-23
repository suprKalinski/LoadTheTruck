using Godot;
using System;

public partial class clawdrop : Node2D
{
    [Export] public float Acceleration = 75.0f;
    [Export] public float Deceleration = 75.0f;
    [Export] public float MaxSpeed = 300.0f;
    [Export] private float _pickupRadius = 50.0f;
    [Export] private float _conveyorSpeed = 80.0f;

    [Export] public PackedScene WoodBlockScene;
    [Export] public PackedScene MetalBlockScene;
    [Export] public PackedScene PlasticBlockScene;
    [Export] public PackedScene GlassBlockScene;

    private AnimationPlayer _grabAnimPlayer;
    private AnimationPlayer _releaseAnimPlayer;
    private Sprite2D _claw;
    private RigidBody2D _currentBlock;
    private Area2D _blockSpawnArea;
    private Timer _blockSpawnTimer;
    private CollectionManager _collectionManager;
    private SoundManager _soundManager;

    private BlockType _currentBlockType;
    private Vector2 _currentVelocity = Vector2.Zero;
    private bool _isHoldingBlock = false;
    private bool _isAnimating = false;

    private const float CLAW_BLOCK_OFFSET_Y = 30f;
    private const float CONVEYOR_END_OFFSET = 50f;

    public override void _Ready()
    {
        _claw = GetNode<Sprite2D>("Kauha");
        _grabAnimPlayer = GetNode<AnimationPlayer>("Grab");
        _releaseAnimPlayer = GetNode<AnimationPlayer>("Release");
        _blockSpawnArea = GetParent().GetNode<Area2D>("BlockSpawnArea");
        _blockSpawnTimer = GetNode<Timer>("BlockSpawnTimer");
        _collectionManager = GetParent().GetNode<CollectionManager>("CollectionManager");
        _soundManager = GetParent().GetNode<SoundManager>("SoundManager"); // 🔈 Get SoundManager

        _blockSpawnTimer.Timeout += LoadRandomBlock;
        CallDeferred(MethodName.LoadRandomBlock);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("pickup")) TryPickupBlock();
        if (Input.IsActionJustPressed("Drop")) DropBlock();

    }

    public override void _PhysicsProcess(double delta)
    {
        ProcessConveyorMovement((float)delta);
        ProcessClawMovement((float)delta);
    }

    private void ProcessConveyorMovement(float delta)
    {
        foreach (Node node in GetTree().GetNodesInGroup("ConveyorBlocks"))
        {
            if (node is RigidBody2D body && body.Freeze)
            {
                body.GlobalPosition += new Vector2(_conveyorSpeed * delta, 0);

                float conveyorEnd = _blockSpawnArea.GlobalPosition.X +
                    (_blockSpawnArea.GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D).Size.X - CONVEYOR_END_OFFSET;

                body.GlobalPosition = new Vector2(
                    Mathf.Min(body.GlobalPosition.X, conveyorEnd),
                    body.GlobalPosition.Y
                );
            }
        }
    }

    private void ProcessClawMovement(float delta)
    {
        Vector2 inputDirection = GetInputDirection();
        UpdateVelocity(inputDirection, delta);
        _claw.Position += _currentVelocity * delta;
        ClampClawPosition();

        if (_isHoldingBlock && _currentBlock != null)
            _currentBlock.GlobalPosition = _claw.GlobalPosition + new Vector2(0, CLAW_BLOCK_OFFSET_Y);
    }

    private async void TryPickupBlock()
    {

        if (_isHoldingBlock || _isAnimating) return;

        Vector2 pickupPosition = _claw.GlobalPosition + new Vector2(0, CLAW_BLOCK_OFFSET_Y);
        RigidBody2D closestBlock = FindClosestBlock(pickupPosition);

        if (closestBlock != null)
        {
            _isAnimating = true;
            _grabAnimPlayer.SpeedScale = 4.0f;
            _grabAnimPlayer.Play("Grab");
            await ToSignal(_grabAnimPlayer, "animation_finished");

            PickupBlock(closestBlock);
            _isAnimating = false;
            _soundManager?.PlayClawCloseSound();
        }
    }

    private RigidBody2D FindClosestBlock(Vector2 pickupPosition)
    {
        foreach (Node node in GetTree().GetNodesInGroup("ConveyorBlocks"))
        {
            if (node is RigidBody2D body &&
                body.GlobalPosition.DistanceTo(pickupPosition) < _pickupRadius)
            {
                return body;
            }
        }
        return null;
    }

    private void PickupBlock(RigidBody2D block)
    {
        block.RemoveFromGroup("ConveyorBlocks");
        block.Freeze = true;
        block.GravityScale = 0;
        _currentBlock = block;
        _isHoldingBlock = true;
        _currentBlockType = (BlockType)(int)block.GetMeta("BlockType");
    }

    private async void DropBlock()
    {
        if (!_isHoldingBlock || _isAnimating || _currentBlock == null || !IsInstanceValid(_currentBlock))
            return;

        var block = _currentBlock;

        _isAnimating = true;
        _releaseAnimPlayer.SpeedScale = 4.0f;
        _releaseAnimPlayer.Play("Release");
        await ToSignal(_releaseAnimPlayer, "animation_finished");
        _soundManager?.PlayClawOpenSound();

        _currentBlock = null;
        _isHoldingBlock = false;
        _isAnimating = false;

        block.Freeze = false;
        block.GravityScale = 1.0f;
        block.LinearVelocity = _currentVelocity * 0.7f;

        _blockSpawnTimer.Start();
    }

    private void LoadRandomBlock()
    {
        if (GetTree().GetNodesInGroup("ConveyorBlocks").Count > 0)
            return;

        _currentBlockType = (BlockType)(GD.Randi() % 4);
        PackedScene selectedScene = _currentBlockType switch
        {
            BlockType.Wood => WoodBlockScene,
            BlockType.Metal => MetalBlockScene,
            BlockType.Plastic => PlasticBlockScene,
            BlockType.Glass => GlassBlockScene,
            _ => WoodBlockScene,
        };

        var newBlock = selectedScene.Instantiate<RigidBody2D>();

        // Inject SoundManager into Block
        if (newBlock is Block blockScript)
        {
            blockScript.SetSoundManager(_soundManager);
        }

        newBlock.GlobalPosition = GetConveyorStartPosition();
        newBlock.Freeze = true;
        newBlock.GravityScale = 0;
        newBlock.CollisionLayer = 1;
        newBlock.CollisionMask = 1;
        newBlock.AddToGroup("ConveyorBlocks");
        newBlock.SetMeta("BlockType", (int)_currentBlockType);
        newBlock.Mass = _currentBlockType == BlockType.Wood ? 0.5f : 3.0f;

        GetParent().CallDeferred(Node.MethodName.AddChild, newBlock);
    }

    private Vector2 GetConveyorStartPosition()
    {
        if (_blockSpawnArea != null)
        {
            var shape = _blockSpawnArea.GetNode<CollisionShape2D>("CollisionShape2D");
            if (shape?.Shape is RectangleShape2D rect)
            {
                return new Vector2(
                    _blockSpawnArea.GlobalPosition.X + 20,
                    _blockSpawnArea.GlobalPosition.Y - 30
                );
            }
        }
        return GetViewportRect().Size / 2;
    }

    private Vector2 GetInputDirection()
    {
        Vector2 input = Vector2.Zero;
        if (Input.IsActionPressed("Left")) input.X -= 1;
        if (Input.IsActionPressed("Right")) input.X += 1;
        if (Input.IsActionPressed("Up")) input.Y -= 1;
        if (Input.IsActionPressed("Down")) input.Y += 1;
        return input.Normalized();
    }

    private void UpdateVelocity(Vector2 inputDirection, float delta)
    {
        Vector2 targetVelocity = inputDirection * MaxSpeed;
        _currentVelocity = inputDirection != Vector2.Zero
            ? _currentVelocity.MoveToward(targetVelocity, Acceleration * delta)
            : _currentVelocity.MoveToward(Vector2.Zero, Deceleration * delta);
    }

    private void ClampClawPosition()
    {
        var viewportSize = GetViewportRect().Size;
        var clawPos = _claw.GlobalPosition;
        var clawSize = _claw.Texture.GetSize();

        float leftBoundary = clawSize.X / 2;
        float rightBoundary = viewportSize.X - clawSize.X / 2;
        float topBoundary = clawSize.Y / 2;
        float bottomBoundary = viewportSize.Y - clawSize.Y / 2;

        _claw.GlobalPosition = new Vector2(
            Mathf.Clamp(clawPos.X, leftBoundary, rightBoundary),
            Mathf.Clamp(clawPos.Y, topBoundary, bottomBoundary)
        );
    }
}
