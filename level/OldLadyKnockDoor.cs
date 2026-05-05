using Godot;
using System;
using DialogueManagerRuntime;

public partial class OldLadyKnockDoor : Node3D
{
	// ================= NODES =================
	private AnimationPlayer doorAnim;
	private AnimationPlayer ladyAnim;

	private CharacterBody3D oldLady;
	private Node3D door;
	private Area3D triggerArea;
	private Node3D player;
	private Camera3D camera;
	private GpuParticles3D dustBurst;

	// ================= AUDIO =================
	private AudioStreamPlayer doorBreakSfx;
	private AudioStreamPlayer chaseSfx;
	private AudioStreamPlayer impactSfx;

	// ================= STATE =================
	private bool triggered = false;
	private bool dialogueTriggered = false;
	private bool chaseStopped = false;

	// ================= MOVEMENT =================
	private float chaseSpeed = 7.5f;
	private float rotationSpeed = 12f;

	private RandomNumberGenerator rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		// AUDIO
		doorBreakSfx = new AudioStreamPlayer();
		chaseSfx = new AudioStreamPlayer();
		impactSfx = new AudioStreamPlayer();

		AddChild(doorBreakSfx);
		AddChild(chaseSfx);
		AddChild(impactSfx);

		// NODES
		oldLady = GetNode<CharacterBody3D>("OldLady");
		door = GetNode<Node3D>("Door");
		dustBurst = GetNodeOrNull<GpuParticles3D>("DustBurst");

		doorAnim = GetNodeOrNull<AnimationPlayer>("Door/AnimationPlayer");
		ladyAnim = oldLady.GetNodeOrNull<AnimationPlayer>("Pivot/AnimationPlayer");

		triggerArea = GetNode<Area3D>("Door/Area3D");
		triggerArea.BodyEntered += OnBodyEntered;

		player = GetTree().Root.FindChild("player", true, false) as Node3D;

		if (player != null)
			camera = player.GetNodeOrNull<Camera3D>("Camera3D");

		oldLady.Visible = false;
		SetPhysicsProcess(false);

		GD.Print("[OLDLADY EVENT] Ready");
	}

	// ================= TRIGGER =================
	private void OnBodyEntered(Node3D body)
	{
		if (triggered) return;
		if (!body.IsInGroup("player")) return;

		triggered = true;
		StartEvent();
	}

	// ================= EVENT =================
	private async void StartEvent()
	{
		GD.Print("[OLDLADY] Event started");

		Vector3 spawnPos = door != null && IsInstanceValid(door)
			? door.GlobalPosition
			: Vector3.Zero;

		// PLAY DOOR ANIMATION
		if (doorAnim != null)
			doorAnim.Play("doorThrow");

		doorBreakSfx.Stream = GD.Load<AudioStream>("res://Sounds/door break.ogg");
		PlayDoorBreakWithOffset();
		ShakeCamera(0.25f, 0.35f);

		if (dustBurst != null)
		{
			dustBurst.GlobalPosition = spawnPos;
			dustBurst.Restart();
			dustBurst.Emitting = true;
		}

		// WAIT FOR ANIMATION
		if (doorAnim != null)
			await ToSignal(doorAnim, "animation_finished");

		// REMOVE DOOR
		if (door != null && IsInstanceValid(door))
			door.QueueFree();

		// SMALL DELAY FOR TENSION
		await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

		SpawnOldLady();
	}

	// ================= SPAWN =================
	private void SpawnOldLady()
	{
		if (!IsInstanceValid(oldLady)) return;

		Vector3 fixedSpawn = new Vector3(69.997f, 0.247f, 83.4f);

		oldLady.GlobalPosition = fixedSpawn;
		oldLady.Visible = true;

		GD.Print("[OLDLADY] Spawned at FIXED position");

		PlayLadyAnimation("run");

		// 🔊 CHASE SOUND STARTS HERE
		chaseSfx.Stream = GD.Load<AudioStream>("res://Sounds/Chase.mp3");
		chaseSfx.Play();

		SetPhysicsProcess(true);
	}

	// ================= CHASE =================
	public override void _PhysicsProcess(double delta)
	{
		if (!triggered || player == null || !oldLady.Visible || chaseStopped)
			return;

		Vector3 toPlayer = player.GlobalPosition - oldLady.GlobalPosition;
		toPlayer.Y = 0;

		Vector3 dir = toPlayer.Normalized();

		RotateToward(dir, delta);

		oldLady.Velocity = dir * chaseSpeed;
		oldLady.MoveAndSlide();

		if (!dialogueTriggered && toPlayer.Length() < 1.8f)
		{
			TriggerDialogue();
		}
	}

	// ================= ROTATION =================
	private void RotateToward(Vector3 dir, double delta)
	{
		if (dir.LengthSquared() < 0.001f) return;

		float targetAngle = Mathf.Atan2(dir.X, dir.Z);

		Vector3 rot = oldLady.Rotation;
		rot.Y = Mathf.LerpAngle(rot.Y, targetAngle, rotationSpeed * (float)delta);

		oldLady.Rotation = rot;
	}

	// ================= DIALOGUE =================
	private void TriggerDialogue()
	{
		if (dialogueTriggered) return;

		dialogueTriggered = true;
		chaseStopped = true;
		
		if (chaseSfx != null && chaseSfx.Playing)
		{
			chaseSfx.Stop();
		}
		GD.Print("[OLDLADY] Dialogue triggered");

		SetPhysicsProcess(false);
		oldLady.Velocity = Vector3.Zero;

		impactSfx.Stream = GD.Load<AudioStream>("res://Sounds/monsterWalkByDialogue.mp3");
		impactSfx.Play();

		if (ladyAnim != null)
			ladyAnim.Stop();

		GlobalVariables.Instance.isTalking = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		var dialogue = GD.Load<Resource>("res://Dialogues/oldLady.dialogue");

		if (dialogue != null)
		{
			DialogueManagerRuntime.DialogueManager.ShowDialogueBalloon(dialogue, "start");
			DialogueManagerRuntime.DialogueManager.DialogueEnded += OnDialogueEnd;
		}
	}

	// ================= DIALOGUE END =================
	private void OnDialogueEnd(Resource res)
	{
		GlobalVariables.Instance.isTalking = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;

		DialogueManagerRuntime.DialogueManager.DialogueEnded -= OnDialogueEnd;

		GD.Print("[OLDLADY] Dialogue ended");

		// DESPAWN AFTER 30s
		GetTree().CreateTimer(30.0f).Timeout += () =>
		{
			if (IsInstanceValid(oldLady))
			{
				oldLady.QueueFree();
				GD.Print("[OLDLADY] Removed after 30s");
			}
		};
	}

	// ================= CAMERA SHAKE =================
	private async void ShakeCamera(float intensity, float duration)
	{
		float time = 0f;

		while (time < duration)
		{
			time += (float)GetProcessDeltaTime();

			Vector3 offset = new Vector3(
				(rng.Randf() - 0.5f) * intensity,
				(rng.Randf() - 0.5f) * intensity,
				0
			);

			if (camera != null)
				camera.Position = offset;

			await ToSignal(GetTree(), "process_frame");
		}

		if (camera != null)
			camera.Position = Vector3.Zero;
	}
	
	private async void PlayDoorBreakWithOffset()
	{
		await ToSignal(GetTree().CreateTimer(2.65f), "timeout");

		if (doorBreakSfx != null)
			doorBreakSfx.Play();
	}

	// ================= ANIMATION =================
	private void PlayLadyAnimation(string anim)
	{
		if (ladyAnim == null) return;

		if (ladyAnim.HasAnimation(anim))
			ladyAnim.Play(anim);
	}
}
