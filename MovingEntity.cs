using Godot;
using System;
using System.Threading.Tasks;

public partial class MovingEntity : Area3D
{
	[Export]
	private float maxDistanceToPlayer = 100;
	[Export]
	private float speed = 10;
	private Vector3 direction = Vector3.Zero;
	private Player player;
	private RandomNumberGenerator rdm = new();
	private AnimationPlayer animationPlayer;
	private Timer fleeingDirectionTimer = new();
	private RayCast3D rayCast3D;
	private enum State
	{
		Normal,
		Fleeing
	}
	private State currentState = State.Normal;


	public override void _Ready()
	{

		AddChild(fleeingDirectionTimer);
		fleeingDirectionTimer.Timeout += OnFleeingDirectionTimerTimeout;
		player = (Player)GetTree().GetFirstNodeInGroup("Player");
		rayCast3D = GetNode<RayCast3D>("RayCast3D");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	private void OnFleeingDirectionTimerTimeout()
	{
		SetFleeingDirection(null);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (direction != Vector3.Zero)
		{
			animationPlayer.Play("MovementAnim");
		}
		else
		{
			animationPlayer.Play("Idle");
		}
		switch (currentState)
		{
			case State.Normal:

				if (player != null)
				{
					if (GlobalPosition.DistanceSquaredTo(player.GlobalPosition) < maxDistanceToPlayer)
					{
						SetFleeingDirection(null);
						currentState = State.Fleeing;
					}
				}
				break;
			case State.Fleeing:
				CheckForIncomingObstacle();
				GlobalPosition += direction * speed * (float)delta;
				Rotation = Rotation with { Y = Mathf.LerpAngle(Rotation.Y, MathF.Atan2(direction.X, direction.Z), .25f) };
				if (GlobalPosition.DistanceSquaredTo(player.GlobalPosition) > maxDistanceToPlayer)
				{
					currentState = State.Normal;
					direction = Vector3.Zero;
				}
				break;
		}
	}
	private void CheckForIncomingObstacle()
	{
		if (rayCast3D.GetCollider() is Node3D collider)
		{
			SetFleeingDirection(Mathf.DegToRad(rdm.RandiRange(-1, 1) * 90));
		}
	}
	private void SetFleeingDirection(float? angle)
	{
		fleeingDirectionTimer.Start(rdm.Randf() * 2);
		float randomAngleDeg = rdm.Randf() * 90f - 45f;
		float angleToApply = angle != null ? (float)angle : Mathf.DegToRad(randomAngleDeg);
		direction = (GlobalPosition - player.GlobalPosition).Normalized();
		direction.Y = 0;
		float x = direction.X;
		float z = direction.Z;
		direction.X = x * Mathf.Cos(angleToApply) - z * Mathf.Sin(angleToApply);
		direction.Z = x * Mathf.Sin(angleToApply) + z * Mathf.Cos(angleToApply);
	}
}
