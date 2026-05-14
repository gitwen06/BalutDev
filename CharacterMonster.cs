using Godot;
using System;
using DialogueManagerRuntime;
//todo kill myself

public partial class CharacterMonster : CharacterBody3D
{
	private Node3D player;
	private AnimationPlayer animPlayer;
	private Skeleton3D skeleton;
	private Area3D jumpscareArea;

	private AudioStreamPlayer audioPlayer;   
	private AudioStreamPlayer chasePlayer;  

	private bool eventLocked = false;

	private float hearingRange = 9999.0f;
	private float chaseSpeed = 6.0f;
	private float gravity = 9.8f;

	private Vector3 velocity = Vector3.Zero;

	private bool isChasing = false;
	private bool jumpscareTriggered = false;
	private bool isEventPlaying = false;
	private bool isVisible = false;

	private bool dialogueStarted = false;
	private string currentAnimation = "";

	private bool isDestroyed = false;

	private void Debug(string msg)
	{
		GD.Print($"[MONSTER DEBUG] {msg}");
	}

	public override void _Ready()
	{
		audioPlayer = new AudioStreamPlayer();
		AddChild(audioPlayer);
		audioPlayer.Bus = "Master";

		chasePlayer = new AudioStreamPlayer();
		AddChild(chasePlayer);
		chasePlayer.Bus = "Master";
		chasePlayer.Stream = GD.Load<AudioStream>("res://Sounds/Chase.mp3");
		var stream = GD.Load<AudioStream>("res://Sounds/Chase.mp3");
		if (stream is AudioStreamMP3 mp3)
			mp3.Loop = true;
		else if (stream is AudioStreamOggVorbis ogg)
			ogg.Loop = true;

		chasePlayer.Stream = stream;

		player = GetTree().Root.FindChild("player", true, false) as Node3D;

		animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		skeleton = GetNodeOrNull<Skeleton3D>("Skeleton3D");

		jumpscareArea = GetNodeOrNull<Area3D>("Area3D");
		if (jumpscareArea != null)
			jumpscareArea.AreaEntered += OnJumpscareAreaEntered;

		GlobalPosition = GetPointA();

		DisableMonster();

		Debug("READY (START DISABLED)");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isDestroyed || !isVisible || player == null)
			return;

		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);

		// ================= JUMPSCARE =================
		if (!jumpscareTriggered && distance <= 2.0f)
		{
			Debug($"JUMPSCARE DIST HIT: {distance}");
			TriggerJumpscare();
			return;
		}

		// ================= CHASE =================
		if (!isEventPlaying && distance <= hearingRange)
		{
			if (!isChasing)
			{
				Debug("CHASE START");
				isChasing = true;

				// 🔊 START CHASE MUSIC
				if (!chasePlayer.Playing)
					chasePlayer.Play();
			}

			Vector3 dir = (player.GlobalPosition - GlobalPosition).Normalized();

			velocity.X = dir.X * chaseSpeed;
			velocity.Z = dir.Z * chaseSpeed;

			FaceDirection(dir);
			if (currentAnimation != "walk") {
				PlayAnimationSafe("walk");
			}
		}
		else
		{
			if (isChasing)
			{
				Debug("CHASE END");
				isChasing = false;

				if (chasePlayer.Playing)
					chasePlayer.Stop();
			}

			velocity.X = 0;
			velocity.Z = 0;
		}

		// ================= GRAVITY =================
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;
		else
			velocity.Y = 0;

		Velocity = velocity;
		MoveAndSlide();
	}

	// ================= ENABLE / DISABLE =================
	private void EnableMonster()
	{
		isVisible = true;
		Show();
		Visible = true;

		Debug("ENABLED");
	}

	private void DisableMonster()
	{
		isVisible = false;
		isEventPlaying = false;
		isChasing = false;

		velocity = Vector3.Zero;
		Velocity = Vector3.Zero;

		audioPlayer.Stop();
		chasePlayer.Stop(); 

		Hide();
		Visible = false;

		Debug("DISABLED");
	}

	private void ResetState()
	{
		isEventPlaying = false;
		isChasing = false;
		jumpscareTriggered = false;
		dialogueStarted = false;

		velocity = Vector3.Zero;
		Velocity = Vector3.Zero;

		audioPlayer.Stop();
		chasePlayer.Stop(); 
	}

	// ================= ROTATION =================
	private void FaceDirection(Vector3 dir)
	{
		if (dir.LengthSquared() < 0.001f) return;

		float angle = Mathf.Atan2(dir.X, dir.Z);
		Rotation = new Vector3(0, angle, 0);
	}

	// ================= ANIMATION =================
	private void PlayAnimationSafe(string animName)
	{
		if (animPlayer == null) return;

		if (!animPlayer.HasAnimation(animName))
		{
			GD.PrintErr($"[MONSTER] Missing animation: {animName}");
			return;
		}

		// FORCE STOP CURRENT ANIMATION FIRST
		animPlayer.Stop();

		animPlayer.Play(animName);

		currentAnimation = animName;
	}

	// ================= JUMPSCARE =================
	private void TriggerJumpscare()
	{
		if (jumpscareTriggered || isDestroyed)
			return;

		jumpscareTriggered = true;
		isDestroyed = true;

		Debug("JUMPSCARE TRIGGERED");

		isEventPlaying = true;
		isChasing = false;

		velocity = Vector3.Zero;
		Velocity = Vector3.Zero;

		if (chasePlayer.Playing)
			chasePlayer.Stop();

		// 🔊 PLAY JUMPSCARE SOUND
		audioPlayer.Stream = GD.Load<AudioStream>("res://Sounds/jumpscare1.mp3");
		audioPlayer.VolumeDb = 6f;
		audioPlayer.Play();

		GetTree().CreateTimer(2.0).Timeout += () =>
		{
			ShowGameOver();
		};
	}

	private void ShowGameOver()
	{
		var scene = GD.Load<PackedScene>("res://Plugins and Scenes/GameOver.tscn");

		if (scene != null)
		{
			GetTree().Root.AddChild(scene.Instantiate());
			GetTree().Paused = true;
		}

		DisableMonster();
	}

	private void OnJumpscareAreaEntered(Area3D area)
	{
		if (area.IsInGroup("player"))
			TriggerJumpscare();
	}

	// ================= EVENT SYSTEM =================
	public void PlayEvent(string eventName)
	{
		if (eventLocked || isDestroyed)
			return;

		eventLocked = true;

		Debug($"EVENT: {eventName}");

		ResetState();
		EnableMonster();

		isEventPlaying = true;

		switch (eventName)
		{
			case "spawnMonster":
				SpawnMonster();
				break;

			case "monsterWalkByDialogue":
			case "spawnChase":
				PlayWalkBy();
				break;

			default:
				Debug("UNKNOWN EVENT");
				eventLocked = false;
				return;
		}
	}

	// ================= SPAWN =================
	private void SpawnMonster()
	{
		Debug("SPAWN MODE");

		GlobalPosition = new Vector3(-50.328f, 0.827f, 5.951f);

		isEventPlaying = false; // somehow this was the problem im so fucking diumb im gonna gkill myself
		eventLocked = false;

		EnableMonster();

		Debug("Monster spawned successfully");
	}

	// ================= WALK =================
	private async void PlayWalkBy()
	{
		if (isDestroyed) return;

		Debug("WALK START");

		Vector3 startPos = GetPointA();
		Vector3 endPos = GetPointB();

		GlobalPosition = startPos;

		Vector3 dir = (endPos - startPos).Normalized();
		FaceDirection(dir);

		PlayAnimationSafe("walk");

		float duration = 1.0f;
		float elapsed = 0f;

		while (elapsed < duration && !isDestroyed)
		{
			elapsed += (float)GetProcessDeltaTime();

			float t = elapsed / duration;
			GlobalPosition = startPos.Lerp(endPos, t);

			await ToSignal(GetTree(), "process_frame");
		}

		Debug("WALK END");

		isEventPlaying = false;
		eventLocked = false;

		DisableMonster();

		StartWalkByDialogue();
	}
	
	public async void PlayPeek(Vector3 pos, Vector3 rotDeg)
	{
		if (isDestroyed) return;

		PlayAnimationSafe("idle");
		Debug("PEEK START");

		// Save state
		bool prevVisible = isVisible;
		bool prevEvent = isEventPlaying;
		bool prevChase = isChasing;

		Vector3 prevPos = GlobalPosition;
		Vector3 prevRot = Rotation;

		// Stop everything
		isEventPlaying = true;
		isChasing = false;
		velocity = Vector3.Zero;
		Velocity = Vector3.Zero;

		// Force show
		Show();
		Visible = true;
		isVisible = true;

		// Move to peek spot
		GlobalPosition = pos;
		RotationDegrees = rotDeg;

		await ToSignal(GetTree().CreateTimer(0.8f), "timeout");

		// Restore previous state
		GlobalPosition = prevPos;
		Rotation = prevRot;

		isEventPlaying = prevEvent;
		isChasing = prevChase;
		isVisible = prevVisible;

		if (!prevVisible)
		{
			Hide();
			Visible = false;
		}

		Debug("PEEK END");
	}
	
	public void StartScriptedAttack()
	{
		isEventPlaying = true;
		isChasing = false;

		velocity = Vector3.Zero;
		Velocity = Vector3.Zero;

		EnableMonster();

		// STOP ANY CURRENT ANIMATION COMPLETELY
		if (animPlayer != null)
		{
			animPlayer.Stop();
			animPlayer.Play("attack");
			currentAnimation = "attack";
		}

		Debug("SCRIPTED ATTACK START");
	}

	public async void EnableChaseAI()
	{
		// RESET CURRENT ANIMATION STATE
		currentAnimation = "";

		// FORCE STOP ATTACK COMPLETELY
		if (animPlayer != null)
		{
			animPlayer.Stop();

			await ToSignal(GetTree(), "process_frame");

			animPlayer.Play("walk");
			currentAnimation = "walk";
		}

		// ENABLE AI AFTER ANIMATION SWITCH
		isEventPlaying = false;
		isChasing = true;

		Debug("CHASE AI ENABLED");
	}

	// ================= POSITIONS =================
	private Vector3 GetPointA() => new Vector3(153.2f, 0.222f, 45.85f);
	private Vector3 GetPointB() => new Vector3(153.2f, 0.222f, 4.605f);

	// ================= DIALOGUE =================
	private void StartWalkByDialogue()
	{
		if (dialogueStarted || isDestroyed) return;

		dialogueStarted = true;

		var dialogue = GD.Load<Resource>("res://Dialogues/monsterWalkByDialogue.dialogue");

		if (dialogue != null)
		{
			GlobalVariables.Instance.isTalking = true;
			Input.MouseMode = Input.MouseModeEnum.Visible;

			DialogueManagerRuntime.DialogueManager.DialogueEnded += OnDialogueEnded;
			DialogueManagerRuntime.DialogueManager.ShowDialogueBalloon(dialogue, "start");

			Debug("DIALOGUE STARTED");
		}
	}

	private void OnDialogueEnded(Resource res)
	{
	GlobalVariables.Instance.isTalking = false;
	Input.MouseMode = Input.MouseModeEnum.Captured;

	DialogueManagerRuntime.DialogueManager.DialogueEnded -= OnDialogueEnded;

	Debug("DIALOGUE ENDED");
	
	if (QuestSystem.Instance != null)
	{
		GD.Print("[MONSTER] Calling quest advance for monster_walk_by_ended");
		QuestSystem.Instance.TriggerQuestAdvance("monster_walk_by_ended");
		GD.Print("[MONSTER] Quest advance called!");
	}
	else
	{
		GD.PrintErr("[MONSTER] QuestSystem.Instance is NULL!");
	}
}
}
