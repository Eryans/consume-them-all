using Godot;
using System;

public partial class BloodExplosion : GpuParticles3D
{
	public override void _Ready()
	{
		Emitting = true;
		Finished += QueueFree;
	}
}
