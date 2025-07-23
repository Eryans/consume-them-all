using Godot;
using System;

public partial class MovingEntity : Area3D
{
	[Export]
	private float maxDistanceToPlayer = 100;
	[Export]
	private float roamSpeed = 5;
	[Export]
	private float fleeSpeed = 15;
	private float speed;
	private Vector3 direction = Vector3.Zero;
	private Player player;
	private RandomNumberGenerator rdm = new();
	private AnimationPlayer animationPlayer;
	private RayCast3D rayCast3D;
	private NavigationAgent3D navigationAgent3D;
	private enum State
	{
		Normal,
		Fleeing
	}
	private State currentState = State.Normal;


	public override void _Ready()
	{
		player = (Player)GetTree().GetFirstNodeInGroup("Player");
		rayCast3D = GetNode<RayCast3D>("RayCast3D");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		navigationAgent3D = GetNode<NavigationAgent3D>("NavigationAgent3D");
		navigationAgent3D.TargetReached += OnNavAgentTargetReached;
		SetRoamOrFleeTarget();
	}

	private void OnNavAgentTargetReached()
	{
		SetRoamOrFleeTarget();
	}

	private void SetRoamOrFleeTarget()
	{
		if (GlobalPosition.DistanceSquaredTo(player.GlobalPosition) > maxDistanceToPlayer)
		{
			currentState = State.Normal;
			float rdmAngle = rdm.Randf() * Mathf.Tau;
			float radius = 8;
			Vector3 rdmPointInCircle = new Vector3(Mathf.Cos(rdmAngle), 0f, Mathf.Sin(rdmAngle)) * radius;
			SetMovementTarget(GlobalPosition + rdmPointInCircle);
		}
		else
		{
			currentState = State.Fleeing;
			SetFleeingVelocity(null);
		}
	}

	private void SetMovementTarget(Vector3 target)
	{
		navigationAgent3D.TargetPosition = target;

	}
	private void OnVelocityComputed(Vector3 safeVelocity)
	{
		GlobalPosition = GlobalPosition.MoveToward(GlobalPosition + safeVelocity, speed * (float)GetPhysicsProcessDeltaTime());
	}
	public override void _PhysicsProcess(double delta)
	{

		if (NavigationServer3D.MapGetIterationId(navigationAgent3D.GetNavigationMap()) == 0)
		{
			return;
		}
		if (navigationAgent3D.IsNavigationFinished())
		{
			SetRoamOrFleeTarget();
			return;
		}


		Vector3 nextPathPosition = navigationAgent3D.GetNextPathPosition();
		Vector3 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * speed * (float)delta;
		if (navigationAgent3D.AvoidanceEnabled)
		{
			navigationAgent3D.Velocity = newVelocity;
		}
		else
		{
			OnVelocityComputed(newVelocity);
		}

		if (newVelocity != Vector3.Zero)
		{
			Rotation = Rotation with { Y = Mathf.LerpAngle(Rotation.Y, MathF.Atan2(newVelocity.X, newVelocity.Z), .25f) };
			animationPlayer.Play("MovementAnim");
		}
		else
		{
			animationPlayer.Play("Idle");
		}

		switch (currentState)
		{
			case State.Normal:
				speed = roamSpeed;
				if (player != null)
				{
					if (GlobalPosition.DistanceSquaredTo(player.GlobalPosition) < maxDistanceToPlayer)
					{
						SetFleeingVelocity(null);
					}
				}
				break;
			case State.Fleeing:
				speed = fleeSpeed;
				break;
		}
	}

	private void SetFleeingVelocity(float? angle)
	{
		currentState = State.Fleeing;
		float length = 20;
		// fleeingDirectionTimer.Start(rdm.Randf() * 2);
		float randomAngleDeg = rdm.Randf() * 180f - 90f;
		float angleToApply = angle != null ? (float)angle : Mathf.DegToRad(randomAngleDeg);
		Vector3 direction = (GlobalPosition - player.GlobalPosition).Normalized();
		direction.Y = 0;
		float x = direction.X;
		float z = direction.Z;
		direction.X = x * Mathf.Cos(angleToApply) - z * Mathf.Sin(angleToApply);
		direction.Z = x * Mathf.Sin(angleToApply) + z * Mathf.Cos(angleToApply);
		Vector3 destinationTarget = GlobalPosition + direction * length;
		SetMovementTarget(destinationTarget);

	}
}
