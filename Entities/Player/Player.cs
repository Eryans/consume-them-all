using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CharacterBody3D
{
	private enum State
	{
		Normal,
		Stuned,
		Inactive
	}
	[Export]
	private float RayLength = 1000f;
	[Export]
	public float Speed = 5.0f;
	[Export]
	private float stunedDuration = 2;

	private Node3D visual;
	private Timer stunedTimer = new();
	private Timer gpsTimer = new();
	private Vector3 stunedDirection = new();
	private State currentState = State.Normal;
	private Tween tw;
	private Tween rtw;
	private AnimationPlayer animationPlayer;
	private EntityDetector entityDetector;
	private Gps gps;
	public event Action<int> AteRewardableEntityEvent;

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
		visual = GetNode<Node3D>("%Visual");
		gps = GetNode<Gps>("GPS");
		entityDetector = GetNode<EntityDetector>("EntityDetector");
		entityDetector.HitObstacleEvent += OnHitObstacle;
		entityDetector.ConsumeEntityEvent += OnConsumeEntity;
		AddChild(stunedTimer);
		AddChild(gpsTimer);
		gpsTimer.Timeout += OnGpsTimerTimeout;
		stunedTimer.Timeout += OnStunedTimerTimeout;
		GameManager.Instance.WinOrGameOverEvent += OnWinOrGameOver;
		SetNewGPSNearestTarget();
		gpsTimer.Start(1);
	}

	private void OnGpsTimerTimeout()
	{
		SetNewGPSNearestTarget();
		gpsTimer.Start(1);
	}

	private void OnWinOrGameOver(bool obj)
	{
		stunedTimer.Stop();
		entityDetector.SetProcess(false);
		currentState = State.Inactive;
	}
	public override void _ExitTree()
	{
		GameManager.Instance.WinOrGameOverEvent -= OnWinOrGameOver;
	}


	private void OnConsumeEntity(Node3D entity, int bonusTime)
	{
		animationPlayer.Play("Eat");
		if (entity.IsInGroup("Consumable"))
		{
			entity.RemoveFromGroup("Consumable");
			AteRewardableEntityEvent?.Invoke(bonusTime);
		}
		SetNewGPSNearestTarget();
	}
	private void SetNewGPSNearestTarget()
	{
		List<Node3D> consumables = [.. GetTree().GetNodesInGroup("Consumable").Cast<Node3D>()];
		Node3D nearest = null;
		foreach (Node3D consumable in consumables)
		{
			nearest ??= consumable;
			if (GlobalPosition.DistanceSquaredTo(consumable.GlobalPosition) < GlobalPosition.DistanceSquaredTo(nearest.GlobalPosition))
			{
				nearest = consumable;
			}
		}
		gps.SetTarget(nearest);
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
		tw.SetEase(Tween.EaseType.Out);
		rtw.SetEase(Tween.EaseType.Out);
		rtw.TweenProperty(visual, "rotation:y", originalRotation + Mathf.Tau * rotationLoop, 1).SetTrans(Tween.TransitionType.Back);
		tw.TweenProperty(visual, "scale", visual.Scale * 1.25f, .1f);
		tw.TweenProperty(visual, "scale", originalScale, .75f).SetTrans(Tween.TransitionType.Back);
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
			case State.Inactive:
				Velocity = Vector3.Zero;
				break;
		}
		MoveAndSlide();
	}
}
