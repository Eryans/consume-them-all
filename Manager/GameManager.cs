using System;
using Godot;

public partial class GameManager : Node
{
	[Export]
	public float MaxAliveTime { get; private set; } = 30f;
	public static GameManager Instance { get; private set; }
	public event Action<bool> WinOrGameOverEvent;
	public event EventHandler<GameDataEventArgs> UpdateGameDataEvent;
	public Timer StayAliveTimer { get; private set; } = new();

	private Player player;
	private int maxConsumables;

	public class GameDataEventArgs : EventArgs
	{
		public int consumablesLeft;
		public int maxConsumables;
	}
	public override void _Ready()
	{
		if (Instance != null && Instance != this)
		{
			GD.Print("WARNING : GameManager Instance alreadyExist !");
		}
		Instance = this;

		player = (Player)GetTree().GetFirstNodeInGroup("Player");
		maxConsumables = GetTree().GetNodeCountInGroup("Consumable");

		AddChild(StayAliveTimer);
		StayAliveTimer.Start(MaxAliveTime);

		player.AteRewardableEntityEvent += OnPlayerAteRewardableEntity;
		StayAliveTimer.Timeout += OnStayAliveTimerTimeout;

		Callable.From(() =>
		{
			UpdateGameDataEvent?.Invoke(this, new GameDataEventArgs { maxConsumables = maxConsumables, consumablesLeft = maxConsumables });
		}).CallDeferred();
	}

	private void OnPlayerAteRewardableEntity(int bonusTime)
	{
		int consumablesLeft = GetTree().GetNodeCountInGroup("Consumable");
		UpdateGameDataEvent?.Invoke(this, new GameDataEventArgs { maxConsumables = maxConsumables, consumablesLeft = consumablesLeft });
		float newTime = (float)Mathf.Clamp(StayAliveTimer.TimeLeft + bonusTime, 0, MaxAliveTime);
		StayAliveTimer.Stop();
		StayAliveTimer.Start(newTime);
		if (consumablesLeft <= 0)
		{
			// Win Condition
			StayAliveTimer.Paused = true;
			WinOrGameOverEvent?.Invoke(true);
		}
	}
	private void OnStayAliveTimerTimeout()
	{
		WinOrGameOverEvent?.Invoke(false);
	}
}
