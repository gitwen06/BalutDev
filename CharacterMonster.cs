using Godot;
using System;

public partial class CharacterMonster : CharacterBody3D
{
	private Node3D player;
	private AnimationPlayer animPlayer;
	private Skeleton3D skeleton;
	private Area3D jumpscareArea;

	private float hearingRange = 0.000001f;
	private float chaseSpeed = 5.0f;
	private float patrolSpeed = 2.0f;
	private float gravity = 9.8f;

	private Vector3 velocity = Vector3.Zero;

	private bool isChasing = false;
	private bool jumpscareTriggered = false;

	// IMPORTANT: locks AI during scripted events
	private bool isEventPlaying = false;

	// Patrol system
	private Vector3[] patrolPoints = new Vector3[4];
	private int currentPatrolIndex = 0;
	private float patrolWaitTime = 2.0f;
	private float patrolWaitCounter = 0f;

	public override void _Ready()
	{
		player = GetTree().Root.FindChild("player", owned: false, recursive: true) as Node3D;

		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		skeleton = GetNode<Skeleton3D>("Skeleton3D");

		jumpscareArea = GetNodeOrNull<Area3D>("Area3D");
		if (jumpscareArea != null)
			jumpscareArea.AreaEntered += OnJumpscareAreaEntered;

		SetupPatrolPoints();

		// Monster starts hidden until event or spawn logic
		Hide();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (player == null || isEventPlaying)
			return;

		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);

		// JUMPSCARE
		if (distanceToPlayer < 1.5f && isChasing && !jumpscareTriggered)
		{
			TriggerJumpscare();
			return;
		}

		// CHASE
		if (distanceToPlayer <= hearingRange)
		{
			isChasing = true;

			Vector3 dir = (player.GlobalPosition - GlobalPosition).Normalized();

			velocity.X = dir.X * chaseSpeed;
			velocity.Z = dir.Z * chaseSpeed;

			RotateSkeletonToward(dir);

			if (!animPlayer.IsPlaying() || animPlayer.CurrentAnimation != "walk")
				animPlayer.Play("walk");
		}
		else
		{
			isChasing = false;
			Patrol(delta);
		}

		// GRAVITY
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;
		else
			velocity.Y = 0;

		Velocity = velocity;
		MoveAndSlide();
	}

	// ---------------- PATROL ----------------
	private void Patrol(double delta)
	{
		Vector3 target = patrolPoints[currentPatrolIndex];
		Vector3 dir = (target - GlobalPosition).Normalized();
		float dist = GlobalPosition.DistanceTo(target);

		if (dist < 1.0f)
		{
			patrolWaitCounter += (float)delta;

			if (patrolWaitCounter >= patrolWaitTime)
			{
				currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
				patrolWaitCounter = 0f;
			}

			velocity.X = 0;
			velocity.Z = 0;

			if (!animPlayer.IsPlaying() || animPlayer.CurrentAnimation != "idle")
				animPlayer.Play("idle");
		}
		else
		{
			velocity.X = dir.X * patrolSpeed;
			velocity.Z = dir.Z * patrolSpeed;

			RotateSkeletonToward(dir);

			if (!animPlayer.IsPlaying() || animPlayer.CurrentAnimation != "walk")
				animPlayer.Play("walk");
		}
	}

	private void SetupPatrolPoints()
	{
		Vector3 start = GlobalPosition;
		float distance = 10.0f;

		patrolPoints[0] = start + new Vector3(distance, 0, distance);
		patrolPoints[1] = start + new Vector3(-distance, 0, distance);
		patrolPoints[2] = start + new Vector3(-distance, 0, -distance);
		patrolPoints[3] = start + new Vector3(distance, 0, -distance);
	}

	// ---------------- ROTATION ----------------
	private void RotateSkeletonToward(Vector3 direction)
	{
		if (direction.Length() == 0)
			return;

		float angle = Mathf.Atan2(direction.X, direction.Z);
		skeleton.Rotation = new Vector3(skeleton.Rotation.X, angle, skeleton.Rotation.Z);
	}

	// ---------------- JUMPSCARE ----------------
	private void TriggerJumpscare()
	{
		GD.Print("JUMPSCARE!");

		jumpscareTriggered = true;

		var gameOverScene = GD.Load<PackedScene>("res://Plugins and Scenes/GameOver.tscn");

		if (gameOverScene != null)
		{
			var ui = gameOverScene.Instantiate();
			GetTree().Root.AddChild(ui);

			GetTree().Paused = true;
		}
	}

	private void OnJumpscareAreaEntered(Area3D area)
	{
		if (area.IsInGroup("player"))
			TriggerJumpscare();
	}

	// ---------------- EVENT SYSTEM ----------------
	public void PlayEvent(string eventName)
	{
		Show();

		isEventPlaying = true;
		velocity = Vector3.Zero;

		GD.Print($"Monster Event: {eventName}");

		switch (eventName)
		{
			case "MonsterWalkByDialogue":
				PlayWalkBy();
				break;

			case "HallwayPeek":
				PlayPeek();
				break;

			default:
				GD.PrintErr($"Unknown event: {eventName}");
				isEventPlaying = false;
				break;
		}
	}

	// ---------------- EVENTS ----------------
	private async void PlayWalkBy()
	{
		GlobalPosition = new Vector3(0, GlobalPosition.Y, 0);

		animPlayer.Play("walk");

		await ToSignal(GetTree().CreateTimer(3.0f), "timeout");

		isEventPlaying = false;
		Hide();
	}

	private async void PlayPeek()
	{
		animPlayer.Play("peek");

		await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

		isEventPlaying = false;
		Hide();
	}
}
