using Godot;
using System;
using System.Collections.Generic;

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

	public const float RegenDelay = 2.25f; 
	public const float StaminaDrainRate = 10f; 
	public const float StaminaRegenRate = 10f; 

	// ============= PLAYER STATE =============
	public bool isRunning = false;
	public bool isCrouching = false;
	public bool isFlashlightOn = true;
	public bool canMove = true;
	
	// ============= PLAYER INVENTORY =============
	public Node target = null;
	public const int MaxSlots = 5;
	public List<string> inventory = new List<string>();
	public int equippedIndex = -1;
	public bool AddItem(string itemId) {
		if (inventory.Count >= MaxSlots)
			return false;

		inventory.Add(itemId);
		return true;
	}
	
	public void RemoveItem(int index) {
		if (index < 0 || index >= inventory.Count)
			return;

		inventory.RemoveAt(index);
	}
	
	public string GetItem(int index) {
		if (index < 0 || index >= inventory.Count)
			return null;

		return inventory[index];
	}

	// ============= PERFORMANCE / TEMP STATE =============
	public Vector3 cachedVelocity = Vector3.Zero;
	public double lastFrameDelta = 0f;

	// (IMPORTANT NOTE BELOW)
	
	// ============= FLASHLIGHT =============
	public Node playerFlashlight = null;
	public float FlashlightBattery = 100.0f;
}
