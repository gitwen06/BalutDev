using Godot;
using System;

public partial class CharacterMonster : CharacterBody3D
{
	private Node3D player;
	private AnimationPlayer animPlayer;
	private Skeleton3D skeleton;
	private Area3D jumpscareArea;
	private float hearingRange = 15.0f;
	private float chaseSpeed = 5.0f;
	private float patrolSpeed = 2.0f;
	private Vector3 velocity = Vector3.Zero;
	private float gravity = 9.8f;
	private bool isChasing = false;
	private bool jumpscareTriggered = false;
	
	// Patrol waypoints
	private Vector3[] patrolPoints = new Vector3[4];
	private int currentPatrolIndex = 0;
	private float patrolWaitTime = 2.0f;
	private float patrolWaitCounter = 0f;

	public override void _Ready()
	{
		player = GetTree().Root.FindChild("player", owned: false, recursive: true) as Node3D;
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		skeleton = GetNode<Skeleton3D>("Skeleton3D");
		
		// Try to get jumpscare area
		jumpscareArea = GetNode<Area3D>("Area3D");
		if (jumpscareArea != null)
		{
			jumpscareArea.AreaEntered += OnJumpscareAreaEntered;
			GD.Print("Monster: Jumpscare area found");
		}
		
		if (player == null)
			GD.PrintErr("Monster: Player not found!");
		if (animPlayer == null)
			GD.PrintErr("Monster: AnimationPlayer not found!");
		
		// Set up patrol points around current position
		SetupPatrolPoints();
		GD.Print("Monster: Ready to patrol");
	}

	private void SetupPatrolPoints()
	{
		// Create 4 patrol points in a square around starting position
		Vector3 startPos = GlobalPosition;
		float distance = 10.0f;
		
		patrolPoints[0] = startPos + new Vector3(distance, 0, distance);
		patrolPoints[1] = startPos + new Vector3(-distance, 0, distance);
		patrolPoints[2] = startPos + new Vector3(-distance, 0, -distance);
		patrolPoints[3] = startPos + new Vector3(distance, 0, -distance);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (player == null)
			return;

		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);

		// JUMPSCARE TRIGGER - when player gets very close
		if (distanceToPlayer < 1.5f && isChasing && !jumpscareTriggered)
		{
			TriggerJumpscare();
			return;
		}

		// If player is within hearing range - CHASE
		if (distanceToPlayer <= hearingRange)
		{
			isChasing = true;
			
			// Move toward player
			Vector3 directionToPlayer = (player.GlobalPosition - GlobalPosition).Normalized();
			
			velocity.X = directionToPlayer.X * chaseSpeed;
			velocity.Z = directionToPlayer.Z * chaseSpeed;
			
			// Rotate skeleton to face player
			RotateSkeletonToward(directionToPlayer);
			
			// Play walk animation
			if (animPlayer != null && (!animPlayer.IsPlaying() || animPlayer.CurrentAnimation != "walk"))
				animPlayer.Play("walk");
			
			GD.Print($"Chasing player! Distance: {distanceToPlayer}m");
		}
		else
		{
			// PATROL
			isChasing = false;
			Patrol(delta);
		}

		// Apply gravity
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;
		else
			velocity.Y = 0;

		Velocity = velocity;
		MoveAndSlide();
	}

	private void Patrol(double delta)
	{
		Vector3 targetPoint = patrolPoints[currentPatrolIndex];
		Vector3 directionToWaypoint = (targetPoint - GlobalPosition).Normalized();
		float distanceToWaypoint = GlobalPosition.DistanceTo(targetPoint);

		// If reached waypoint, wait then move to next
		if (distanceToWaypoint < 1.0f)
		{
			patrolWaitCounter += (float)delta;
			
			if (patrolWaitCounter >= patrolWaitTime)
			{
				currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
				patrolWaitCounter = 0f;
			}
			
			// Stop moving
			velocity.X = 0;
			velocity.Z = 0;
			
			// Play idle animation
			if (animPlayer != null && (!animPlayer.IsPlaying() || animPlayer.CurrentAnimation != "idle"))
				animPlayer.Play("idle");
		}
		else
		{
			// Move to waypoint
			velocity.X = directionToWaypoint.X * patrolSpeed;
			velocity.Z = directionToWaypoint.Z * patrolSpeed;
			
			// Rotate skeleton to face waypoint
			RotateSkeletonToward(directionToWaypoint);
			
			// Play walk animation
			if (animPlayer != null && (!animPlayer.IsPlaying() || animPlayer.CurrentAnimation != "walk"))
				animPlayer.Play("walk");
		}
	}
	//ROTATE SKELETON TO PLAY WHEN SEEN
	private void RotateSkeletonToward(Vector3 direction)
	{
		if (direction.Length() == 0)
			return;
		
		float angle = Mathf.Atan2(direction.X, direction.Z);
		skeleton.Rotation = new Vector3(skeleton.Rotation.X, angle, skeleton.Rotation.Z);
	}
	//TRIGGER JUMPSCARE WHEN TOUCH PLAYER WOW
	private void TriggerJumpscare()
	{
		GD.Print("JUMPSCARE! Player touched monster!");
		jumpscareTriggered = true;
		
		// Load and show game over scene
		var gameOverScene = GD.Load<PackedScene>("res://Plugins and Scenes/GameOver.tscn");
		if (gameOverScene != null)
		{
			var gameOverUI = gameOverScene.Instantiate();
			GetTree().Root.AddChild(gameOverUI);
			
			// Pause the game
			GetTree().Paused = true;
			
		}
		else
		{
			GD.PrintErr("GameOver scene not found!");
		}
	}

	private void OnJumpscareAreaEntered(Area3D area)
	{
		GD.Print($"Area entered: {area.Name}");
		if (area.IsInGroup("player") || area.Name == "playerCollision")
		{
			TriggerJumpscare();
		}
	}
}
