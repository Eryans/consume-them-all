using Godot;

[GlobalClass]
public partial class Consumable : Node
{
	[Export]
	public int BonusTime { get; private set; } = 0;
	private PackedScene explosionPackedScene;

	public override void _Ready()
	{
		explosionPackedScene = GD.Load<PackedScene>("uid://br3qnla0f77d7");
	}
	public void Consume()
	{
		Node3D explosion = explosionPackedScene.Instantiate() as Node3D;
		GetTree().CurrentScene.AddChild(explosion);
		explosion.GlobalPosition = GetOwner<Node3D>().GlobalPosition;
		Owner.QueueFree();

	}
}
