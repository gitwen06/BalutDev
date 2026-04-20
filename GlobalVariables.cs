using Godot;
using System;

public partial class GlobalVariables : Node
{
	// ============= SINGLETON =============
	public static GlobalVariables Instance;

	public override void _Ready()
	{
		Instance = this;
	}

	// ============= MOVEMENT =============
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float RunSpeed = 8.0f;
	public const float CrouchSpeed = 2.5f; 

	// ============= STAMINA SYSTEM =============
	public float maxStamina = 100.0f;
	public float stamina = 100.0f;
	public float regenDelayTimer = 0f;

	public const float RegenDelay = 3f; 
	public const float StaminaDrainRate = 15f; 
	public const float StaminaRegenRate = 12f; 

	// ============= PLAYER STATE =============
	public bool isRunning = false;
	public bool isCrouching = false;
	public bool isFlashlightOn = true;
	public bool canMove = true;

	// ============= PERFORMANCE / TEMP STATE =============
	public Vector3 cachedVelocity = Vector3.Zero;
	public double lastFrameDelta = 0f;

	// (IMPORTANT NOTE BELOW)
}
