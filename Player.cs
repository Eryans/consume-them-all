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
	[Export]
	private float maxAliveTime = 30f;
	private Node3D visual;
	private Timer stunedTimer = new();
	private Timer stayAliveTimer = new();
	private Vector3 stunedDirection = new();
	private State currentState = State.Normal;
	private Tween tw;
	private Tween rtw;
	public event Action OnAteRewardableEntity;
	public event Action OnPlayerDeath;
	public override void _Ready()
	{
		visual = GetNode<Node3D>("%Visual");
		EntityDetector entityDetector = GetNode<EntityDetector>("EntityDetector");
		entityDetector.OnHitObstacle += OnHitObstacle;
		entityDetector.OnConsumeEntity += OnConsumeEntity;
		AddChild(stunedTimer);
		AddChild(stayAliveTimer);
		stayAliveTimer.Start(maxAliveTime);
		stunedTimer.Timeout += OnStunedTimerTimeout;
		stayAliveTimer.Timeout += OnStayAliveTimerTimeout;
	}

	private void OnStayAliveTimerTimeout()
	{
		currentState = State.Dead;
		OnPlayerDeath?.Invoke();
	}

	private void OnConsumeEntity(Node3D entity)
	{
		if (entity.IsInGroup("Consumable"))
		{
			OnAteRewardableEntity?.Invoke();
		}
	}

	private void OnStunedTimerTimeout()
	{
		currentState = State.Normal;
		stunedDirection = Vector3.Zero;
	}

	private void OnHitObstacle(Vector3 vector)
	{
		stunedTimer.Start(stunedDuration);
		stunedDirection = (GlobalPosition - vector).Normalized();
		stunedDirection.Y = 0;
		currentState = State.Stuned;
		tw?.Kill();
		tw = CreateTween();
		rtw?.Kill();
		rtw = CreateTween();
		Vector3 originalScale = visual.Scale;
		float originalRotation = visual.Rotation.Y;
		int rotationLoop = 3;
		rtw.TweenProperty(visual, "rotation:y", originalRotation + Mathf.DegToRad(360) * rotationLoop, 1).SetTrans(Tween.TransitionType.Sine);
		tw.TweenProperty(visual, "scale", visual.Scale * 1.25f, .1f);
		tw.TweenProperty(visual, "scale", originalScale, .5f).SetTrans(Tween.TransitionType.Bounce);
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
				// visual.RotateY((float)delta * stunnedRotationForce);
				break;
			case State.Dead:
				Velocity = Vector3.Zero;
				break;
		}
		MoveAndSlide();
	}
}
