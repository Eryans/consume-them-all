using Godot;

public partial class Gps : Node3D
{
	public Node3D Target { get; private set; }

	public override void _Process(double delta)
	{
		if (IsInstanceValid(Target))
		{
			Vector3 lookAtDirection = Target.GlobalPosition - GlobalPosition;
			Rotation = Rotation with { Y = Mathf.Atan2(lookAtDirection.X, lookAtDirection.Z) };
		}
	}

	public void SetTarget(Node3D newTarget)
	{
		Target = newTarget;
		if (!IsInstanceValid(Target))
		{
			Visible = false;
		}
		GD.Print(Target);
	}
}
