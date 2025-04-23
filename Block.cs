using Godot;

public enum BlockType
{
    Wood,
    Metal,
    Glass,
    Plastic
}

public partial class Block : RigidBody2D
{
    [Export] public BlockType BlockType;
    [Export] public PackedScene WoodParticles;
    [Export] public PackedScene MetalParticles;
    [Export] public PackedScene GlassParticles;
    [Export] public PackedScene PlasticParticles;
    [Export] public float DestructionForceThreshold = 200f;

    public bool IsCollected { get; set; }
    public float CollectedWeight { get; set; }

    private CollectionManager _collectionManager;
    private SoundManager _soundManager;

    public void SetCollectionManager(CollectionManager manager)
    {
        _collectionManager = manager;
    }
    public void SetSoundManager(SoundManager soundManager)
{
    _soundManager = soundManager;
}

    public override void _Ready()
    {
        ContactMonitor = true;
        MaxContactsReported = 10;
        BodyEntered += OnCollision;



    }

    private void OnCollision(Node body)
    {
        float relativeForce = (body is RigidBody2D otherBody)
            ? (LinearVelocity - otherBody.LinearVelocity).Length()
            : LinearVelocity.Length();

        if (relativeForce < DestructionForceThreshold)
            return;


        _soundManager?.PlayBlockImpact(BlockType);

        if (body is Block otherBlock)
        {
            if (BlockType == BlockType.Metal && otherBlock.BlockType != BlockType.Metal)
                DestroyBlock(otherBlock);

            else if ((BlockType == BlockType.Plastic || BlockType == BlockType.Wood || BlockType == BlockType.Metal)
                  && otherBlock.BlockType == BlockType.Glass)
                DestroyBlock(otherBlock);

            else if ((BlockType == BlockType.Wood || BlockType == BlockType.Metal)
                  && otherBlock.BlockType == BlockType.Plastic)
                DestroyBlock(otherBlock);
        }
        else if (body is StaticBody2D && BlockType != BlockType.Metal)
        {
            DestroyBlock(this);
        }
    }

    private void DestroyBlock(Block block)
    {
        if (!IsInstanceValid(block)) return;


        _soundManager?.PlayBlockBreak(block.BlockType);

        if (block.IsCollected && block._collectionManager != null)
            block._collectionManager.RemoveCollectedBlock(block);

        var sprite = block.GetNodeOrNull<Sprite2D>("Sprite2D");
        if (sprite != null)
            sprite.Modulate = Colors.Red;

        GetTree().CreateTimer(0.1f).Timeout += () =>
        {
            if (!IsInstanceValid(block)) return;

            block.QueueFree();
        };
    }
}




