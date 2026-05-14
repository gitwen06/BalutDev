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
	
	public const float Speed = 20.0f;
	public const float JumpVelocity = 4.5f;
	public const float RunSpeed = 8.0f;
	public const float CrouchSpeed = 2.5f; 
	
	// ============= STAMINA SYSTEM =============
	
	public float maxStamina = 100.0f;
	public float stamina = 100.0f;
	public float regenDelayTimer = 0f;
	public const float RegenDelay = 2.25f; 
	public const float StaminaDrainRate = 8f; 
	public const float StaminaRegenRate = 10f; 
	
	// ============= PLAYER STATE =============
	
	public bool isRunning = false;
	public bool isCrouching = false;
	public bool isFlashlightOn = true;
	public bool canMove = true;
	public bool isTalking = false;
	
	// ============= PLAYER INVENTORY =============
	public Node3D currentItem = null;
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
	
	public bool isFinalChase = false;
	
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
	
	// ============= FLASHLIGHT =============
	public Node playerFlashlight = null;
	public float FlashlightBattery = 100.0f;
	
	// NPC DESPAWN
	public Node3D spawnedNPC = null; // Track spawned NPC
	public void RemoveNPC()
	{
		if (spawnedNPC != null && IsInstanceValid(spawnedNPC))
		{
			spawnedNPC.QueueFree();
			spawnedNPC = null;
		}
	}
	
	// ================= ITEM TRACKING ================= (for item tracking wow why did i explain this shit)
	public bool hasFlashlight = false;
	public bool hasBalut = false;
	public bool hasBattery = false;
	public bool hasDrink = false;
	public bool hasKey = false;

	public void MarkItemPickedUp(string itemName)
	{
		itemName = itemName.ToLower();
		
		if (itemName.Contains("flashlight"))
			hasFlashlight = true;
		else if (itemName.Contains("balut"))
			hasBalut = true;
		else if (itemName.Contains("battery"))
			hasBattery = true;
		else if (itemName.Contains("drink") || itemName.Contains("energydrink"))
			hasDrink = true;
		else if (itemName.Contains("key"))
			hasKey = true;
		
		GD.Print($"[GLOBAL] Item marked: {itemName}");
	}
	// ============= BALUT VARIABLES =============
	public int balutAmount = 6;
	public Node balutModel = null;
	public void GiveBalut(int amount)
	{
		if (balutAmount >= amount)
		{
			balutAmount -= amount;
			GD.Print($"Gave {amount} balut. Remaining: {balutAmount}");
		}
		else
		{
			GD.Print("Not enough balut!");
		}
	}
	
	// ==== NPC VARIABLES ==== my ehad feels like its going to explode any moment. ive been coding for 9 hours 9 hours 9 hours 0h ours  hpisr= ssigpj
	public bool gaveAlingNeneng = false;
	public bool GaveAlingMarin = false;
	public bool gaveAlingMarites = false;
	public bool gaveKuyaJames = false;
	public bool gaveManongRafael = false;
	public bool gaveAlingShoneng = false;
	
	public AlingMarites alingMarites;
	public AlingShoneng alingShoneng;
	public OldLadyKnockDoor alingMarin;
	public KuyaJames kuyaJames;
	
	// ==== Event controllersererser ====
	public MonsterPeekTrigger monsterPeekTrigger;
	public void EnableMonsterPeek()
	{
		monsterPeekTrigger?.EnableTrigger();
	}
	
	// ==== ANIMATION CONTROLLERS ==== my head faking hurts
	public AnimationForCharacters animationController;
	// ==== ANIMATION WRAPPERS ====
	public void OpenAnim()
	{
		if (animationController == null)
		{
			GD.Print("AnimationController is NULL!");
			return;
		}

		animationController.OpenThenWaitThenClose();
	}
	
	public void EnableDialogueAboutItems()
	{
		var trigger = GetTree().Root.FindChild("DialogueAboutItems", true, false) as Trigger;

		if (trigger != null)
		{
			trigger.EnableTrigger();
			GD.Print("[GLOBAL] DialogueAboutItems re-enabled via GlobalVariables");
		}
		else
		{
			GD.PrintErr("[GLOBAL] DialogueAboutItems trigger not found in scene");
		}
	}

	public void CloseAnim()
	{
		if (animationController == null)
		{
			GD.Print("AnimationController is NULL!");
			return;
		}

		animationController.AllowClose();
	}
}
