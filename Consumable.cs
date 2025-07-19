using Godot;

[GlobalClass]
public partial class Consumable : Node
{
	public void Consume()
	{
		GD.Print("Nom nom nom nom nom !");
		Owner.QueueFree();
	}
}
