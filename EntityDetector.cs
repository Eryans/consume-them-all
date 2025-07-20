using Godot;
using System;

public partial class EntityDetector : Area3D
{
	public event Action<Vector3> OnHitObstacle;
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		AreaEntered += OnAreaEntered;
	}

	private void OnAreaEntered(Area3D area)
	{
		if (area.GetNodeOrNull<Consumable>("Consumable") is Consumable consumable1)
		{
			consumable1.Consume();
		}
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is Obstacle || body.IsInGroup("Obstacle"))
		{
			OnHitObstacle?.Invoke(body.GlobalPosition);
		}
	}
}
