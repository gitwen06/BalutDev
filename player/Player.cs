using Godot;
using System;
using System.Threading;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float runSpeed = 8.0f;
	public float stamina = 100.0f;
	private float regenDelayTimer = 0f;
	private float regenDelay = 3f;
	public bool isRunning = false;	

	public CollisionShape3D playerCollision;
	public MeshInstance3D playerMesh;

	private bool isCrouching = false;

	public override void _Ready()
	{
		playerCollision = GetNode<CollisionShape3D>("CollisionShape3D");
		playerMesh = GetNode<MeshInstance3D>("MeshInstance3D");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Crouch
		if (Input.IsActionJustPressed("crouch"))
		{
			isCrouching = !isCrouching;

			if (isCrouching)
			{
				playerCollision.Scale = new Vector3(1, 0.5f, 1);
				playerMesh.Scale = new Vector3(
					playerMesh.Scale.X,
					0.5f,
					playerMesh.Scale.Z
				);
			}
			else
			{
				playerCollision.Scale = new Vector3(1, 1, 1);
				playerMesh.Scale = new Vector3(
					playerMesh.Scale.X,
					1f,
					playerMesh.Scale.Z
				);
			}
		}
		// Gravity
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		// Jump
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			// Force stand when jumping
			isCrouching = false;
			playerCollision.Scale = new Vector3(1, 1, 1);
			playerMesh.Scale = new Vector3(
				playerMesh.Scale.X,
				1f,
				playerMesh.Scale.Z
			);
		}
		// Movement
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		float currentSpeed = Speed;
		if (Input.IsActionPressed("run") && stamina > 0f)
		{
			isRunning = true;
			currentSpeed = runSpeed;
			stamina -= 10.5f * (float)delta;
			regenDelayTimer = regenDelay;
		}
		else {
			isRunning = false;

			// Countdown the delay
		if (regenDelayTimer > 0f)
		{
			regenDelayTimer -= (float)delta;
		}
		else {
		stamina += 10.5f * (float)delta;
	}
}

stamina = Mathf.Clamp(stamina, 0f, 100f);

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * currentSpeed;
			velocity.Z = direction.Z * currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, currentSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, currentSpeed);
		}
		Velocity = velocity;
		MoveAndSlide();
	}
}
