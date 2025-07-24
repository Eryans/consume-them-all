using Godot;

public partial class Gps : Node3D
{
	public Node3D Target { get; private set; }
	private Tween tw;
	// public override void _Process(double delta)
	// {
	// 	if (IsInstanceValid(Target))
	// 	{
	// 		Vector3 lookAtDirection = Target.GlobalPosition - GlobalPosition;
	// 		Rotation = Rotation with { Y = Mathf.LerpAngle(Rotation.Y, Mathf.Atan2(lookAtDirection.X, lookAtDirection.Z), (float)GetPhysicsProcessDeltaTime()) };
	// 	}
	// }

	public void SetTarget(Node3D newTarget)
	{
		if (IsInstanceValid(newTarget))
		{
			Target = newTarget;
			Vector3 lookAtDirection = Target.GlobalPosition - GlobalPosition;

			tw?.Kill();
			tw = CreateTween();
			tw.SetEase(Tween.EaseType.Out);
			tw.TweenProperty(this,
			 "rotation:y",
			  Mathf.Atan2(lookAtDirection.X, lookAtDirection.Z),
			   1f)
			.SetTrans(Tween.TransitionType.Bounce);
		}
		else
		{
			Visible = false;
		}
	}
}
