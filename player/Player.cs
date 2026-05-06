using Godot;
using System;
using DialogueManagerRuntime;

public partial class Player : CharacterBody3D
{
	// ============= CACHE REFERENCES =============
	private CollisionShape3D playerCollision;
	private MeshInstance3D playerMesh;
	private RayCast3D playerRay;
	private SpotLight3D playerFlashlight;
	private Camera3D playerCamera;
	private Node3D headBone;
	private Button interactBtn;
	private Node3D hand;
	private Area3D collisionDetector;
	private AudioStreamPlayer footstepPlayer;
	private Node lastCharacterNode = null;

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
	private bool lastInteractButtonState = false;
	private string lastCharacterNameCache = "";
	private string lastDialoguePathCache = "";

	public override void _Ready()
	{
		playerCollision = GetNode<CollisionShape3D>("playerCollision");
		playerMesh = GetNode<MeshInstance3D>("playerBody");
		playerRay = GetNode<RayCast3D>("head/Camera3D/playerRay");
		playerCamera = GetNode<Camera3D>("head/Camera3D");
		playerFlashlight = GetNode<SpotLight3D>("head/Camera3D/playerFlashlight");
		headBone = GetNode<Node3D>("head");
		interactBtn = GetNode<Button>("%interactButton");
		hand = GetNode<Node3D>("head/Camera3D/Hand");
		collisionDetector = GetNode<Area3D>("Area3D");

		var g = GlobalVariables.Instance;
		
		g.playerFlashlight = playerFlashlight;

		if (playerCollision == null) GD.PrintErr("Player: Missing playerCollision node!");
		if (playerMesh == null) GD.PrintErr("Player: Missing playerBody node!");
		if (playerRay == null) GD.PrintErr("Player: Missing playerRay node!");
		if (playerCamera == null) GD.PrintErr("Player: Missing Camera3D node!");

		g.stamina = g.maxStamina;
		baseFlashlightPos = playerFlashlight?.Position ?? Vector3.Zero;
		interactBtn.Visible = false;
		lastInteractButtonState = false;
		
		footstepPlayer = new AudioStreamPlayer();
		AddChild(footstepPlayer);
		footstepPlayer.Bus = "Master";

		footstepPlayer.Stream = GD.Load<AudioStream>("res://Sounds/grass_walk.mp3");
		footstepPlayer.VolumeDb = -10f;
		footstepPlayer.Autoplay = false;
	}
	//imgonnacrashoutonthismotherfuckingsystemohmygodthisshitissoirritatingimlitteralyabouttocrashout
	// ================= HOTBAR FUNCTION =================
	private void EquipItem(int index)
	{
		// hi im currently coding the logic for this balut system and its been 2 hours and its been 2 hours and its been 2 hours and its been 2 hours and its been 2 hours
		var g = GlobalVariables.Instance;
		if (index < 0)
		{
			if (g.currentItem != null)
			{
				if (g.currentItem.Name.ToString().Contains("flashlight"))
				{
					g.currentItem.Call("shutOffFlashlight");
				}
				//dont ever modify this this single line will crash the whole game if removed
				g.currentItem.QueueFree();
				g.currentItem = null;
			}
			GD.Print("Unequipped Item");
			return;
		}

		string itemName = g.GetItem(index);
		GD.Print($"Equipping item: {itemName}");
		
		if (g.currentItem != null) {
			if (g.currentItem.Name.ToString().Contains("flashlight")) {
				g.currentItem.Call("shutOffFlashlight");
			}
			g.currentItem.QueueFree();
			g.currentItem = null;
		}

		g.equippedIndex = index;
		
		if (string.IsNullOrEmpty(itemName)) {
			GD.Print("Unequipped Item");
			return;
		}
		// for loading the item and such
		// Shell/JM make sure the added "pickable item" is named the same as the .tscn file like
		// RigidBody3D = flashlight
		// Items folder = flashlight.tscn

		string itemPath = $"res://items/{itemName}.tscn";
		if(ResourceLoader.Exists(itemPath)) {
			var scene = GD.Load<PackedScene>(itemPath);
			g.currentItem = (Node3D)scene.Instantiate();
			GD.Print($"Instantiated item: {g.currentItem.Name}");
			
			hand.AddChild(g.currentItem);
			GD.Print($"Added to hand. Current parent: {g.currentItem.GetParent().Name}");
			
			g.currentItem.Visible = true;
			g.currentItem.TopLevel = false;
			g.currentItem.GlobalTransform = hand.GlobalTransform;
			
			if (g.currentItem is CollisionObject3D physicsItem) {
				physicsItem.CollisionLayer = 0;
				physicsItem.CollisionMask = 0;
			}
			if (g.currentItem is RigidBody3D rb) {
				rb.Freeze = true;
				rb.LinearVelocity = Vector3.Zero;
				rb.AngularVelocity = Vector3.Zero;
			}
			g.currentItem.Position = Vector3.Zero;
			g.currentItem.Rotation = Vector3.Zero;
			g.currentItem.Scale = Vector3.One;
			
			if(g.currentItem.Name.ToString().Contains("balut")) {
				g.balutModel = g.currentItem;
				GD.Print($"✓ Balut assigned to GlobalVariables");
				GD.Print($"  Item name: {g.currentItem.Name}");
				GD.Print($"  Parent: {g.currentItem.GetParent().Name}");
			}
		}
		else {
			GD.PrintErr($"EquipItem: Could not find scene at {itemPath}");
		}
	}

	//REMOVE TARGET SO YOU DONT HAVE TO REPEAT THIS LINE EVERY TIME!!!!!!!!!!!!!!!!!!!!!!
	private void RemoveTarget(Node target)
	{
		Node parent = target.GetParent();
		if (parent != null && parent is Node3D && parent.Name != "Interactables") {
			parent.QueueFree();
		} else {
			target.QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		
		var g = GlobalVariables.Instance;

		lastFrameDelta = delta;
		float deltaF = (float)delta;
		
		if (!g.canMove)
		{
			Velocity = Vector3.Zero;
			MoveAndSlide();
			return;
		}
		Vector3 velocity = Velocity;
		
		//Interact and Pick-up
		if(playerRay != null && playerRay.IsColliding()) {
			Node collider = playerRay.GetCollider() as Node;
			GlobalVariables.Instance.target = collider;
		} else {
			GlobalVariables.Instance.target = null;
		}

		Node target = GlobalVariables.Instance.target;
		bool isTalking = GlobalVariables.Instance.isTalking;
		bool shouldShowButton = false;

		if (target != null && (target.IsInGroup("interactables") || target.IsInGroup("pickables") || target.IsInGroup("batteries") || target.IsInGroup("Characters") || target.IsInGroup("drinks") || target.IsInGroup("doors"))) {
			if(!isTalking) {
				shouldShowButton = true;
				
				if (Input.IsActionJustPressed("interact")) {
					//Interact system for batteries
					if(target.IsInGroup("batteries")) {
						if (GlobalVariables.Instance.AddItem(target.Name)) {
							GD.Print($"Picked up: {target.Name}");

							QuestSystem.Instance.OnItemPicked(target.Name);

							RemoveTarget(target);
							GlobalVariables.Instance.target = null;
							shouldShowButton = false;
						}
					}
					//Interact system for characters
					else if(target.IsInGroup("Characters")) {
					GlobalVariables.Instance.isTalking = true;
					Input.MouseMode = Input.MouseModeEnum.Visible;
					shouldShowButton = false;
					
					Node characterNode = target;
					if (target is Area3D && target.GetParent() is Node3D parent)
					{
						characterNode = parent;
					}
					
					string characterName = characterNode.Name.ToString().ToLower();
					lastCharacterNode = characterNode;  // Store the actual character node
					
					bool hasBalut = g.equippedIndex >= 0 && g.currentItem != null && g.currentItem.Name.ToString().Contains("balut");
					
					string startDialogue = "start";
					if (characterName != "mang jason" && !hasBalut)
					{
						startDialogue = "no_balut";
					}
					
					string dialoguePath = $"res://Dialogues/{characterName}.dialogue";
					var dialogueResource = GD.Load<Resource>(dialoguePath);
					if(dialogueResource != null) {
						DialogueManager.ShowDialogueBalloon(dialogueResource, startDialogue);
						DialogueManager.DialogueEnded += OnDialogueEnded;
					} else {
						GD.PrintErr($"Dialogue file not found: {dialoguePath}");
						GlobalVariables.Instance.isTalking = false;
					}
					
					GlobalVariables.Instance.target = null;
				}
					//Interact system for drinks
					else if(target.IsInGroup("drinks")) {
						if (GlobalVariables.Instance.AddItem(target.Name)) {
							GD.Print($"Picked up: {target.Name}");

							QuestSystem.Instance.OnItemPicked(target.Name);

							RemoveTarget(target);
							GlobalVariables.Instance.target = null;
							shouldShowButton = false;
						}
					}
					//Interact system for doors
					else if(target.IsInGroup("doors")) {
						Node doorNode = target.GetParent();
						
						if (doorNode == null)
						{
							GD.PrintErr("Could not find door mesh!");
							GlobalVariables.Instance.target = null;
							return;
						}
						
						GD.Print($"Door detected: {doorNode.Name}");
						GD.Print($"Door node type: {doorNode.GetType().Name}");
						GD.Print($"Door script: {doorNode.GetScript()}");
						
						var pintoOpen = doorNode as PintoOpen;
						if (pintoOpen != null)
						{
							GD.Print("PintoOpen cast successful!");
							if (pintoOpen.IsOpen)
							{
								pintoOpen.CloseDoor();
							}
							else
							{
								if (pintoOpen.RequiresKey())
								{
									if (pintoOpen.HasKey())
									{
										pintoOpen.OpenDoor();
										GD.Print("Opened door with key");
									}
									else
									{
										GD.Print("Need key to open this door!");
									}
								}
								else
								{
									pintoOpen.OpenDoor();
									GD.Print("Opened door");
								}
							}
						}
						else
						{
							GD.PrintErr($"Failed to cast {doorNode.Name} to PintoOpen! Type: {doorNode.GetType().Name}");
						}
						GlobalVariables.Instance.target = null;
						shouldShowButton = false;
					}
					//Interact system for pickups
					else if (GlobalVariables.Instance.AddItem(target.Name)) {
						GD.Print($"Picked up: {target.Name}");

						QuestSystem.Instance.OnItemPicked(target.Name);

						RemoveTarget(target);
						GlobalVariables.Instance.target = null;
						shouldShowButton = false;
					}
				}
			}
		} else if(!isTalking) {
			shouldShowButton = false;
		}

		// Only update UI if state changed
		if (lastInteractButtonState != shouldShowButton) {
			interactBtn.Visible = shouldShowButton;
			lastInteractButtonState = shouldShowButton;
		}

		// CROUCH
		if (Input.IsActionJustPressed("crouch"))
		{
			g.isCrouching = !g.isCrouching;
			needsCollisionUpdate = true;

			if (g.isCrouching)
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
		// GRAVITY
		if (!IsOnFloor())
			velocity += GetGravity() * deltaF;
		// JUMP
		if (Input.IsActionJustPressed("jump") && IsOnFloor() && !g.isCrouching)
			velocity.Y = GlobalVariables.JumpVelocity;
		// MOVEMENT
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		float currentSpeed = g.isCrouching ? GlobalVariables.CrouchSpeed : GlobalVariables.Speed;

		bool isInputMoving = direction != Vector3.Zero;
		
		if (IsOnFloor() && isInputMoving && !g.isCrouching)
		{
			if (!footstepPlayer.Playing)
			{
				footstepPlayer.Play();
			}
		}
		else
		{
			if (footstepPlayer.Playing)
			{
				footstepPlayer.Stop();
				footstepPlayer.Seek(0); // reset sound
			}
		}	
		bool tryingToRun = Input.IsActionPressed("run") && !g.isCrouching;
		// RUN + STAMINA
		if (tryingToRun && g.stamina > 0f && isInputMoving)
		{
			g.isRunning = true;
			currentSpeed = GlobalVariables.RunSpeed;

			g.stamina -= GlobalVariables.StaminaDrainRate * deltaF;
			g.regenDelayTimer = GlobalVariables.RegenDelay;
		}
		else
		{
			g.isRunning = false;

			if (g.regenDelayTimer > 0f)
				g.regenDelayTimer -= deltaF;
			else if (g.stamina < g.maxStamina)
				g.stamina += GlobalVariables.StaminaRegenRate * deltaF;
		}

		g.stamina = Mathf.Clamp(g.stamina, 0f, g.maxStamina);
		// CAMERA BOB
		if (playerCamera != null && IsOnFloor())
		{
			if (isInputMoving)
			{
				float activeBobSpeed =
					g.isCrouching ? crouchBobSpeed :
					(g.isRunning ? bobSpeed * 1.3f : bobSpeed);

				bobTime += deltaF * activeBobSpeed;

				Vector3 camPos = playerCamera.Position;
				camPos.Y = Mathf.Sin(bobTime) * bobAmount;

				playerCamera.Position = camPos;
			}
			else
			{
				Vector3 camPos = playerCamera.Position;
				camPos.Y = Mathf.Lerp(camPos.Y, 0, 10f * deltaF);
				camPos.X = Mathf.Lerp(camPos.X, 0, 10f * deltaF);
				playerCamera.Position = camPos;
				bobTime = 0f;
			}
		}
		// APPLY MOVEMENT
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
	// ================= UNLOCK MOUSE WHEN DIALOGUE ENDS ================
	private void OnDialogueEnded(Resource resource) {
	GlobalVariables.Instance.isTalking = false;
	Input.MouseMode = Input.MouseModeEnum.Captured;

	DialogueManager.DialogueEnded -= OnDialogueEnded;

	// ================= CALL CHARACTER'S DIALOGUE END METHOD =================
	if (lastCharacterNode != null)
	{
		string characterName = lastCharacterNode.Name.ToString().ToLower();
		
		if (lastCharacterNode.HasMethod("OnDialogueEnded"))
		{
			lastCharacterNode.Call("OnDialogueEnded");
			GD.Print($"[PLAYER] Called OnDialogueEnded on {characterName}");
		}
		else
		{
			GD.PrintErr($"[PLAYER] {characterName} has no OnDialogueEnded method!");
		}
	}
	else
	{
		GD.Print("[PLAYER] No character node to call");
	}
	
	lastCharacterNode = null;
}
	// ================= INPUT =================
	public override void _Input(InputEvent @event) {
		var g = GlobalVariables.Instance;

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			// Hotbar selection (1-5)
			for (int i = 0; i < 5; i++)
			{
				if (keyEvent.Keycode == Key.Key1 + i)
				{
					if (g.equippedIndex == i)
					{
						EquipItem(-1);
						g.equippedIndex = -1;
					}
					else
					{
						EquipItem(i);
						g.equippedIndex = i;
					}
				}
			}
			// Consume equipped item (R key)
			if (keyEvent.Keycode == Key.R && g.equippedIndex >= 0 && g.currentItem != null)
			{
				string itemName = g.currentItem.Name.ToString().ToLower();
				int consumedIndex = g.equippedIndex; // Save the index before changing it
				
				if (itemName.Contains("battery"))
				{
					g.FlashlightBattery += 25.0f;
					GD.Print($"Consumed battery! Battery now at: {g.FlashlightBattery}");
					EquipItem(-1); // Unequip and remove from hand
					g.RemoveItem(consumedIndex); // Remove from inventory
					g.equippedIndex = -1;
				}
				else if (itemName.Contains("energydrink"))
				{
					g.stamina += 50.0f;
					GD.Print($"Consumed energy drink! Stamina now at: {g.stamina}");
					EquipItem(-1); // Unequip and remove from hand
					g.RemoveItem(consumedIndex); // Remove from inventory
					g.equippedIndex = -1;
				}
			}
		}
	}
}
