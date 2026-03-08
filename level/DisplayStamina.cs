using Godot;
using System;

public partial class DisplayStamina : Label {
	private Player player;
	public override void _Ready() {
		player = GetNode<Player>("/root/playerGlobals");
	}
	public override void _Process(double delta) {
		if (player != null) {
			Text = $"Stamina: {player.stamina}";
		}
	}
}
