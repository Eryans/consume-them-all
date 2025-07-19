using Godot;
using Godot.Collections;
using System;

public partial class Player : CharacterBody3D
{
	private enum State
	{
		Normal,
		Stuned,
		Dead
	}
	[Export]
	private float RayLength = 1000f;
	[Export]
	public float Speed = 5.0f;
	[Export]
	private float stunedDuration = 2;
	private const float stunnedRotationForce = 15;
	private Node3D visual;
	private Timer stunedTimer = new();
	private Vector3 stunedDirection = new();
	private State currentState = State.Normal;
	public override void _Ready()
	{
		visual = GetNode<Node3D>("%Visual");
		GetNode<EntityDetector>("EntityDetector").OnHitObstacle += OnHitObstacle;
		AddChild(stunedTimer);
		stunedTimer.Timeout += OnStunedTimerTimeout;
	}

	private void OnStunedTimerTimeout()
	{
		currentState = State.Normal;
		stunedDirection = Vector3.Zero;
	}

	private void OnHitObstacle(Vector3 vector)
	{
		stunedTimer.Start(3);
		stunedDirection = (GlobalPosition - vector).Normalized();
		stunedDirection.Y = 0;
		currentState = State.Stuned;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity;
		switch (currentState)
		{
			case State.Normal:

				Camera3D camera = GetViewport().GetCamera3D();
				Vector2 mousePos = GetViewport().GetMousePosition();
				Vector3 from = camera.ProjectRayOrigin(mousePos);
				Vector3 to = from + camera.ProjectRayNormal(mousePos) * RayLength;

				var spaceState = GetWorld3D().DirectSpaceState;
				var query = PhysicsRayQueryParameters3D.Create(from, to, 128);
				Dictionary result = spaceState.IntersectRay(query);
				if (result.TryGetValue("position", out var position))
				{
					Vector3 target = (Vector3)position;
					velocity = (target - GlobalPosition).Normalized();
					visual.Rotation = visual.Rotation with { Y = Mathf.LerpAngle(visual.Rotation.Y, MathF.Atan2(velocity.X, velocity.Z), .25f) };
					Velocity = velocity * Speed;
				}
				break;
			case State.Stuned:
				Velocity = stunedDirection * Speed;
				visual.RotateY((float)delta * stunnedRotationForce);
				break;
			case State.Dead:
				break;
		}
		MoveAndSlide();
	}
}
