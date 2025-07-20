using Godot;
using System;

public partial class UI : Control
{
	private Player player;
	private RichTextLabel tinyDudeCount;
	public override void _Ready()
	{
		GameManager.Instance.OnUpdateGameData += OnUpdateGameData;
		tinyDudeCount = GetNode<RichTextLabel>("%TinyDudeCounter");
		GD.Print(GameManager.Instance);
	}

	private void OnUpdateGameData(object sender, GameManager.GameDataEventArgs e)
	{
		GD.Print("update");
		tinyDudeCount.Text = $"{e.consumablesLeft} / {e.maxConsumables}";
	}
}
