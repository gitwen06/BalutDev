using Godot;
using System;

public partial class MonsterPeekTrigger : Area3D
{
	private bool enabledTrigger = false;
	private bool triggered = false;
	private CharacterMonster monster;
	private AudioStreamPlayer audioPlayer;
	private Node3D marites;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		monster = GetTree().Root.FindChild("Character_Monster", true, false) as CharacterMonster;
		marites = GetTree().Root.FindChild("Aling Marites", true, false) as Node3D;
		audioPlayer = new AudioStreamPlayer();
		AddChild(audioPlayer);
		audioPlayer.Bus = "Master";
		GlobalVariables.Instance.monsterPeekTrigger = this;

		Monitoring = false;
		GD.Print("[PEEK] Ready - waiting for EnableTrigger()");
		GD.Print($"[PEEK] Monster found: {monster != null}");
		GD.Print($"[PEEK] Marites found: {marites != null}");
		GD.Print($"[PEEK] Area3D exists: {this != null}");
	}

	public void EnableTrigger()
	{
		GD.Print("[PEEK] Trigger ENABLED - now monitoring");
		enabledTrigger = true;
		triggered = false;
		Monitoring = true;
		GD.Print($"[PEEK] Monitoring is now: {Monitoring}");
	}

	public void PlayPeekSFX()
	{
		var sound = GD.Load<AudioStream>("res://Sounds/monsterWalkByDialogue.mp3");
		if (sound == null)
		{
			GD.PrintErr("[PEEK] Sound not found!");
			return;
		}
		audioPlayer.Stream = sound;
		audioPlayer.Play();
		GD.Print("[PEEK] Peek sound played");
	}

	private void OnBodyEntered(Node3D body)
	{
		GD.Print($"[PEEK] OnBodyEntered fired! Body: {body.Name}");
		GD.Print($"[PEEK] enabledTrigger: {enabledTrigger}, triggered: {triggered}");
		GD.Print($"[PEEK] Monitoring: {Monitoring}");
		
		if (!enabledTrigger || triggered) 
		{
			GD.Print($"[PEEK] Ignoring body - enabled:{enabledTrigger}, triggered:{triggered}");
			return;
		}

		if (!body.IsInGroup("player") && !body.Name.ToString().ToLower().Contains("player"))
		{
			GD.Print($"[PEEK] Ignored body: {body.Name}");
			return;
		}

		triggered = true;
		GD.Print("[PEEK] Player entered trigger!");

		if (monster != null)
		{
			monster.PlayPeek(
			new Vector3(112.901f, 0.52f, 50.367f),
			new Vector3(72.9f, -9.5f, 76.1f)
		);
			if (monster.HasMethod("PlayAnimationSafe"))
				monster.Call("PlayAnimationSafe", "Idle");
			GD.Print("[PEEK] Monster positioned + idle");
		}
		else
		{
			GD.PrintErr("[PEEK] Monster not found!");
		}

		PlayPeekSFX();

		if (marites != null)
		{
			marites.QueueFree();
			GD.Print("[PEEK] Aling Marites disappeared");
		}

		SetDeferred("monitoring", false);
	}
}
