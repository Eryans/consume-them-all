using Godot;

[GlobalClass]
public partial class Consumable : Node
{
	[Export]
	public int BonusTime { get; private set; } = 0;
	public void Consume()
	{
		GD.Print("Nom nom nom nom nom !");
		Owner.QueueFree();
	}
}
