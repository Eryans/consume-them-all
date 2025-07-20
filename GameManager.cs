using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameManager : Node
{
	private int maxConsumables;
	public static GameManager Instance { get; private set; }
	private Player player;
	public override void _Ready()
	{
		if (Instance != null && Instance != this)
		{
			GD.Print("WARNING : GameManager Instance alreadyExist ! overwriting current instance");
			Instance = this;
		}
		player = (Player)GetTree().GetFirstNodeInGroup("Player");
		maxConsumables = GetTree().GetNodeCountInGroup("Consumable");
		player.OnAteRewardableEntity += OnPlayerAteRewardableEntity;
		player.OnPlayerDeath += OnPlayerDeath;
	}

	private void OnPlayerDeath()
	{
		// Game Over Logic
		GD.Print("game over");
	}

	private void OnPlayerAteRewardableEntity()
	{
		GD.Print("Nom nom +1 Score !");
		if (GetTree().GetNodeCountInGroup("Consumable") <= 0)
		{
			// Win Condition
			GD.Print("You win !");
		}
	}

	public override void _Process(double delta)
	{
	}
}
