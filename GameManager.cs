using System;
using Godot;

public partial class GameManager : Node
{
	private int maxConsumables;
	public static GameManager Instance { get; private set; }
	private Player player;
	public event EventHandler<GameDataEventArgs> UpdateGameDataEvent;
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
		player.AteRewardableEntityEvent += OnPlayerAteRewardableEntity;
		player.PlayerDeathEvent += OnPlayerDeath;
		Callable.From(() =>
		{
			UpdateGameDataEvent?.Invoke(this, new GameDataEventArgs { maxConsumables = maxConsumables, consumablesLeft = maxConsumables });
		}).CallDeferred();
	}

	private void OnPlayerDeath()
	{
		// Game Over Logic
		GD.Print("game over");
	}

	private void OnPlayerAteRewardableEntity()
	{
		Callable.From(() =>
		{
			int consumablesLeft = GetTree().GetNodeCountInGroup("Consumable");
			// I don't know why, but there is always one more entity in the group array
			// Maybe a race condition ? 
			UpdateGameDataEvent?.Invoke(this, new GameDataEventArgs { maxConsumables = maxConsumables, consumablesLeft = consumablesLeft - 1 });
			if (consumablesLeft - 1 <= 0)
			{
				// Win Condition
				GD.Print("You win !");
				player.StayAliveTimer.Stop();
			}
		}).CallDeferred();
	}

	public override void _Process(double delta)
	{
	}
}
