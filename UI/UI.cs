using Godot;
using System;

public partial class UI : Control
{
	private Player player;
	private RichTextLabel tinyDudeCount;
	private ProgressBar ProgressBar;
	private Panel panel;
	private RichTextLabel winOrGameOverText;
	public override void _Ready()
	{
		GameManager.Instance.UpdateGameDataEvent += OnUpdateGameData;
		GameManager.Instance.WinOrGameOverEvent += OnWinOrGameOver;
		player = (Player)GetTree().GetFirstNodeInGroup("Player");
		tinyDudeCount = GetNode<RichTextLabel>("%TinyDudeCounter");
		ProgressBar = GetNode<ProgressBar>("%ProgressBar");
		panel = GetNode<Panel>("Panel");
		winOrGameOverText = GetNode<RichTextLabel>("%WinOrGameOverText");
		panel.Visible = false;
	}

	private void OnWinOrGameOver(bool trueWinFalseGameover)
	{
		Tween panelTw = CreateTween();
		panel.Visible = true;
		float originalPanelPosY = panel.Position.Y;
		panel.Position = panel.Position with { Y = -(GetViewport().GetVisibleRect().Size.Y + 200) };
		winOrGameOverText.Text = trueWinFalseGameover ? "You consumed all the tiny dudes ! \n You win !" : "Game over ! \n You didn't consume enough tiny dudes >:( !";
		panelTw.TweenProperty(panel, "position:y", originalPanelPosY, 2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
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
		if (!GameManager.Instance.StayAliveTimer.Paused)
		{
			ProgressBar.MaxValue = GameManager.Instance.MaxAliveTime;
			ProgressBar.Value = GameManager.Instance.StayAliveTimer.TimeLeft;
		}
	}
}
