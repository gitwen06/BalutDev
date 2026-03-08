using Godot;
using System;

public partial class Player : CharacterBody3D
{
	// ============= MOVEMENT =============
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float RunSpeed = 8.0f;
	public const float CrouchSpeed = 2.5f; 

	// ============= STAMINA SYSTEM =============
	public float maxStamina = 100.0f;
	public float stamina = 100.0f;
	private float regenDelayTimer = 0f;
	private const float RegenDelay = 3f; 
	private const float StaminaDrainRate = 15f; 
	private const float StaminaRegenRate = 12f; 

	// ============= PLAYER STATE =============
	public bool isRunning = false;
	public bool isCrouching = false;
	public bool isFlashlightOn = true;
	public bool canMove = true; // For cutscenes/scares

	// ============= CACHE REFERENCES =============
	private CollisionShape3D playerCollision;
	private MeshInstance3D playerMesh;
	private RayCast3D playerRay;
	private SpotLight3D playerFlashlight;
	private Camera3D playerCamera;
	private Node3D headBone; 

	// ============= CAMERA BOB & EFFECTS =============
	private float bobTime = 0f;
	private float bobAmount = 0.05f;
	private float bobSpeed = 8f;
	private float crouchBobSpeed = 5f; 

	// ============= FLASHLIGHT =============
	private float flashlightSwayAmount = 0.02f;
	private float flashlightSwaySpeed = 0.002f;
	private Vector3 baseFlashlightPos = Vector3.Zero;

	// ============= PERFORMANCE OPTIMIZATION =============
	private bool needsCollisionUpdate = false;
	private Vector3 cachedVelocity = Vector3.Zero;
	private double lastFrameDelta = 0f;

	public override void _Ready()
	{
		// Cache node references - validate they exist
		playerCollision = GetNode<CollisionShape3D>("playerCollision");
		playerMesh = GetNode<MeshInstance3D>("playerBody");
		playerRay = GetNode<RayCast3D>("head/Camera3D/playerRay");
		playerFlashlight = GetNode<SpotLight3D>("head/Camera3D/playerFlashlight");
		playerCamera = GetNode<Camera3D>("head/Camera3D");
		headBone = GetNode<Node3D>("head");

		if (playerCollision == null) GD.PrintErr("Player: Missing playerCollision node!");
		if (playerMesh == null) GD.PrintErr("Player: Missing playerBody node!");
		if (playerRay == null) GD.PrintErr("Player: Missing playerRay node!");
		if (playerFlashlight == null) GD.PrintErr("Player: Missing playerFlashlight node!");
		if (playerCamera == null) GD.PrintErr("Player: Missing Camera3D node!");

		stamina = maxStamina;
		baseFlashlightPos = playerFlashlight?.Position ?? Vector3.Zero;
	}

	public override void _PhysicsProcess(double delta)
	{
		lastFrameDelta = delta;
		float deltaF = (float)delta;

		if (!canMove)
		{
			// Stop movement during cutscenes/scares
			Velocity = Vector3.Zero;
			MoveAndSlide();
			return;
		}

		Vector3 velocity = Velocity;

		// ============= FLASHLIGHT TOGGLE =============
		if (Input.IsActionJustPressed("toggleFlashlight"))
		{
			isFlashlightOn = !isFlashlightOn;
			playerFlashlight.Visible = isFlashlightOn;
			playerFlashlight.LightEnergy = isFlashlightOn ? 1f : 0f;
		}

		// ============= FLASHLIGHT SWAY (Optimized) =============
		// Only update if flashlight is on (cheap check)
		if (isFlashlightOn && playerFlashlight != null)
		{
			float time = (float)Time.GetTicksMsec() * flashlightSwaySpeed;
			Vector3 sway = new Vector3(
				Mathf.Sin(time) * flashlightSwayAmount,
				Mathf.Cos(time * 0.7f) * flashlightSwayAmount * 0.5f, // Varied frequency
				0
			);
			playerFlashlight.Position = baseFlashlightPos + sway;
		}

		// ============= INTERACTION RAYCAST (Simple) =============
		if (playerRay != null && playerRay.IsColliding())
		{
			var target = playerRay.GetCollider();
			// TODO: Handle interaction logic
			// Emit signal or set variable for UI to show prompt
		}

		// ============= CROUCH TOGGLE =============
		if (Input.IsActionJustPressed("crouch"))
		{
			isCrouching = !isCrouching;
			needsCollisionUpdate = true;

			if (isCrouching)
			{
				playerCollision.Scale = new Vector3(1f, 0.5f, 1f);
				playerMesh.Scale = new Vector3(playerMesh.Scale.X, 0.5f, playerMesh.Scale.Z);
				playerCamera.Position = playerCamera.Position with { Y = playerCamera.Position.Y * 0.5f };
			}
			else
			{
				playerCollision.Scale = new Vector3(1f, 1f, 1f);
				playerMesh.Scale = new Vector3(playerMesh.Scale.X, 1f, playerMesh.Scale.Z);
				playerCamera.Position = playerCamera.Position with { Y = playerCamera.Position.Y * 2f };
			}
		}

		// ============= GRAVITY =============
		if (!IsOnFloor())
			velocity += GetGravity() * deltaF;

		// ============= JUMP (can't jump while crouching) =============
		if (Input.IsActionJustPressed("jump") && IsOnFloor() && !isCrouching)
		{
			velocity.Y = JumpVelocity;
		}

		// ============= MOVEMENT INPUT =============
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		float currentSpeed = isCrouching ? CrouchSpeed : Speed;

		// ============= RUNNING + STAMINA SYSTEM =============
		bool isInputMoving = direction != Vector3.Zero;
		bool tryingToRun = Input.IsActionPressed("run") && !isCrouching;

		if (tryingToRun && stamina > 0f && isInputMoving)
		{
			isRunning = true;
			currentSpeed = RunSpeed;

			stamina -= StaminaDrainRate * deltaF;
			regenDelayTimer = RegenDelay;
		}
		else
		{
			isRunning = false;

			if (regenDelayTimer > 0f)
				regenDelayTimer -= deltaF;
			else if (stamina < maxStamina)
				stamina += StaminaRegenRate * deltaF;
		}

		stamina = Mathf.Clamp(stamina, 0f, maxStamina);

		// ============= CAMERA BOB (Conditional for optimization) =============
		if (playerCamera != null && IsOnFloor())
		{
			if (isInputMoving)
			{
				float activeBobSpeed = isCrouching ? crouchBobSpeed : (isRunning ? bobSpeed * 1.3f : bobSpeed);
				bobTime += deltaF * activeBobSpeed;

				Vector3 camPos = playerCamera.Position;
				camPos.Y = Mathf.Sin(bobTime) * bobAmount;

				playerCamera.Position = camPos;
			}
			else
			{
				// Smooth return to center when idle
				Vector3 camPos = playerCamera.Position;
				camPos.Y = Mathf.Lerp(camPos.Y, 0, 10f * deltaF);
				camPos.X = Mathf.Lerp(camPos.X, 0, 10f * deltaF);
				playerCamera.Position = camPos;
				bobTime = 0f; // Reset bob
			}
		}

		// ============= APPLY MOVEMENT =============
		if (isInputMoving)
		{
			velocity.X = direction.X * currentSpeed;
			velocity.Z = direction.Z * currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, currentSpeed);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0, currentSpeed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	// ============= PLAYER API FUNCTIONS =============

	/// <summary>
	/// Freeze player during a cutscene/event
	/// </summary>
	public void FreezePlayer(bool freeze = true)
	{
		canMove = !freeze;
		isRunning = false;
	}

	/// <summary>
	/// Get current stamina percentage (0-1)
	/// </summary>
	public float GetStaminaPercent()
	{
		return stamina / maxStamina;
	}
}
