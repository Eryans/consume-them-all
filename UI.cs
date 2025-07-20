using Godot;
using System;

public partial class UI : Control
{
	private Player player;
	private RichTextLabel tinyDudeCount;
	private ProgressBar ProgressBar;
	public override void _Ready()
	{
		GameManager.Instance.UpdateGameDataEvent += OnUpdateGameData;
		player = (Player)GetTree().GetFirstNodeInGroup("Player");
		tinyDudeCount = GetNode<RichTextLabel>("%TinyDudeCounter");
		ProgressBar = GetNode<ProgressBar>("%ProgressBar");
	}
	public override void _ExitTree()
	{
		GameManager.Instance.UpdateGameDataEvent -= OnUpdateGameData;
	}
	private void OnUpdateGameData(object sender, GameManager.GameDataEventArgs e)
	{
		GD.Print("update");
		tinyDudeCount.Text = $"{e.consumablesLeft} / {e.maxConsumables}";
	}
	public override void _Process(double delta)
	{
		ProgressBar.MaxValue = player.MaxAliveTime;
		ProgressBar.Value = player.StayAliveTimer.TimeLeft;
	}
}
