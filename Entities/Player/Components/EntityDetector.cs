using Godot;
using System;

public partial class EntityDetector : Area3D
{
	public event Action<Vector3> HitObstacleEvent;
	public event Action<Node3D, int> ConsumeEntityEvent;
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
			ConsumeEntityEvent?.Invoke((Node3D)consumable1.Owner, consumable1.BonusTime);
		}
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is Obstacle || body.IsInGroup("Obstacle"))
		{
			HitObstacleEvent?.Invoke(body.GlobalPosition);
		}
	}
}
