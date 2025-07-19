using Godot;
using System;

public partial class MovingEntity : Area3D
{
	[Export]
	private float maxDistanceToPlayer = 100;
	[Export]
	private float speed = 10;
	private Vector3 direction = Vector3.Zero;
	private Player player;
	private RandomNumberGenerator rdm = new();
	private enum State
	{
		Normal,
		Fleeing
	}
	private State currentState = State.Normal;


	public override void _Ready()
	{
		player = (Player)GetTree().GetFirstNodeInGroup("Player");
	}

	public override void _Process(double delta)
	{
		switch (currentState)
		{
			case State.Normal:

				if (player != null)
				{
					if (GlobalPosition.DistanceSquaredTo(player.GlobalPosition) < maxDistanceToPlayer)
					{
						float randomAngle = rdm.Randf() * 90f - 45f;
						direction = (GlobalPosition - player.GlobalPosition).Normalized();
						direction.Y = 0;
						float x = direction.X;
						float z = direction.Z;
						direction.X = x * Mathf.Cos(randomAngle) - z * Mathf.Sin(randomAngle);
						direction.Z = x * Mathf.Sin(randomAngle) + z * Mathf.Cos(randomAngle);
						currentState = State.Fleeing;
					}
				}
				break;
			case State.Fleeing:
				GlobalPosition += direction * speed * (float)delta;
				if (GlobalPosition.DistanceSquaredTo(player.GlobalPosition) > maxDistanceToPlayer)
				{
					currentState = State.Normal;
				}
				break;
		}
	}
}
