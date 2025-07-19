using Godot;
using System;

public partial class CameraFollow : SpringArm3D
{
	[Export]
	private CharacterBody3D target;
	[Export]
	private float speed = 5;
	public override void _PhysicsProcess(double delta)
	{
		if (target != null)
		{
			GlobalPosition = GlobalPosition.Lerp(target.GlobalPosition, (float)delta * speed);
		}
	}
}
