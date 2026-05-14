using Godot;
using System;

public partial class Credits : Node3D
{
	private Area3D area;
	private bool triggered = false;

	public override void _Ready()
	{
		area = GetNodeOrNull<Area3D>("Area3D");

		if (area != null)
		{
			area.BodyEntered += OnBodyEntered;
			GD.Print("[CREDITS] Area connected");
		}
		else
		{
			GD.PrintErr("[CREDITS] Area3D NOT FOUND");
		}

		GD.Print("[CREDITS] Ready");
	}

	private void OnBodyEntered(Node3D body)
	{
		GD.Print("[CREDITS] Body entered: " + body.Name);

		if (triggered) return;
		if (body == null) return;
		if (!body.IsInGroup("player")) return;

		if (!IsFinalChaseActive())
		{
			GD.Print("[CREDITS] Final chase NOT active");
			return;
		}

		triggered = true;
		StartCredits();
	}

	private bool IsFinalChaseActive()
	{
		return GlobalVariables.Instance != null &&
			   GlobalVariables.Instance.isFinalChase;
	}

	private async void StartCredits()
	{
		GD.Print("[CREDITS] Starting credits...");

		GlobalVariables.Instance.isTalking = true;

		await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

		GlobalVariables.Instance.isTalking = false;
		GlobalVariables.Instance.isFinalChase = false;

		// IMPORTANT: unpause before scene switch
		GetTree().Paused = false;

		GD.Print("[CREDITS] Switching scene...");

		GetTree().ChangeSceneToFile("res://Plugins and Scenes/endCredits.scn");

		GD.Print("[CREDITS] Switched to credits scene");
	}
}
