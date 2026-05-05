using Godot;
using System;

public partial class JumpscareManager : Node
{
	private AudioStreamPlayer audioPlayer;
	public static JumpscareManager Instance;

	public override void _Ready()
	{
		// Create audio player for jumpscare sounds
		Instance = this;
		audioPlayer = new AudioStreamPlayer();
		AddChild(audioPlayer);
		audioPlayer.Bus = "Master";

		GD.Print("[JUMPSCARE] Manager initialized");
	}

	public void PlayJumpscare(string jumpscareType)
	{
		GD.Print($"[JUMPSCARE] Playing: {jumpscareType}");

		// Play sound based on node name
		PlaySound($"res://Sounds/{jumpscareType}.mp3");
		// Screen shake
		ShakeScreen(0.8f, 12.0f);

		// Call monster event
		CallMonsterEvent(jumpscareType);
	}

	private void PlaySound(string soundPath)
	{
		if (ResourceLoader.Exists(soundPath))
		{
			var sound = GD.Load<AudioStream>(soundPath);
			audioPlayer.Stream = sound;
			audioPlayer.Play();
		}
		else
		{
			GD.PrintErr($"Sound not found: {soundPath}");
		}
	}

	private void ShakeScreen(float duration, float intensity)
	{
		var player = GetTree().Root.FindChild("player", owned: false, recursive: true) as Node3D;
		if (player != null && player.HasMethod("ShakeCamera"))
		{
			player.Call("ShakeCamera", duration, intensity);
		}
	}

	private void CallMonsterEvent(string eventName)
	{
		// Get the monster via unique name
		var monster = GetTree().CurrentScene.GetNodeOrNull<Node>("%Character_Monster");

		if (monster != null && monster.HasMethod("PlayEvent"))
		{
			monster.Call("PlayEvent", eventName);
			GD.Print($"[JUMPSCARE] Called monster event: {eventName}");
		}
		else
		{
			GD.PrintErr("Monster not found or missing PlayEvent method");
		}
	}
}
