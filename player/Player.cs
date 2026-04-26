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
	private Node3D currentItem;

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
		playerCollision = GetNode<CollisionShape3D>("playerCollision");
		playerMesh = GetNode<MeshInstance3D>("playerBody");
		playerRay = GetNode<RayCast3D>("head/Camera3D/playerRay");
		playerCamera = GetNode<Camera3D>("head/Camera3D");
		playerFlashlight = GetNode<SpotLight3D>("head/Camera3D/playerFlashlight");
		headBone = GetNode<Node3D>("head");
		interactBtn = GetNode<Button>("%interactButton");
		hand = GetNode<Node3D>("head/Camera3D/Hand");

		var g = GlobalVariables.Instance;
		
		g.playerFlashlight = playerFlashlight;

		if (playerCollision == null) GD.PrintErr("Player: Missing playerCollision node!");
		if (playerMesh == null) GD.PrintErr("Player: Missing playerBody node!");
		if (playerRay == null) GD.PrintErr("Player: Missing playerRay node!");
		if (playerCamera == null) GD.PrintErr("Player: Missing Camera3D node!");

		g.stamina = g.maxStamina;
		baseFlashlightPos = playerFlashlight?.Position ?? Vector3.Zero;
	}

	// ================= HOTBAR FUNCTION =================
private void EquipItem(int index)
{
	// my ass is too lazy to call it gvi mb
	var g = GlobalVariables.Instance;
	string itemName = g.GetItem(index);
	
	//remoive any item on hand (i think)
	if (currentItem != null) {
		// Turn off flashlight before deleting if it's a flashlight
		if (currentItem.Name.ToString().Contains("flashlight")) {
			currentItem.Call("shutOffFlashlight");
		}
		currentItem.QueueFree();
		currentItem = null;
	}
	g.equippedIndex = index;
	
	if (string.IsNullOrEmpty(itemName)) {
		GD.Print("Unequipped Item");
		return; //double check
	}
	
	// for loading the item and such
	// Shell/JM make sure the added "pickable item" is named the same as the .tscn file like
	// RigidBody3D = flashlight
	// Items folder = flashlight.tscn
	
	string itemPath = $"res://items/{itemName}.tscn";
	if(ResourceLoader.Exists(itemPath)) {
		var scene = GD.Load<PackedScene>(itemPath);
		currentItem = (Node3D)scene.Instantiate();
		
		//attached to the hand node 3d
		hand.AddChild(currentItem);
		currentItem.Visible = true;
		currentItem.TopLevel = false;
		currentItem.GlobalTransform = hand.GlobalTransform;
		
		if (currentItem is CollisionObject3D physicsItem) {
			physicsItem.CollisionLayer = 0;
			physicsItem.CollisionMask = 0;
		}
		// so rigidbody3d DOESN'T SPIN WHAT THE FUCK
		if (currentItem is RigidBody3D rb) {
			rb.Freeze = true;
			rb.LinearVelocity = Vector3.Zero;
			rb.AngularVelocity = Vector3.Zero;
		}
		currentItem.Position = Vector3.Zero;
		currentItem.Rotation = Vector3.Zero;
		currentItem.Scale = Vector3.One;
	}
	else {
		GD.PrintErr($"EquipItem: Could not find scene at {itemPath}");
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
			if(collider != null) {
				GlobalVariables.Instance.target = collider;
			} else {
				GlobalVariables.Instance.target = null;
			}
		} else {
			GlobalVariables.Instance.target = null;
		}

		if (GlobalVariables.Instance != null && GlobalVariables.Instance.target != null && (GlobalVariables.Instance.target.IsInGroup("interactables") || GlobalVariables.Instance.target.IsInGroup("pickables") || GlobalVariables.Instance.target.IsInGroup("batteries") || GlobalVariables.Instance.target.IsInGroup("Characters"))) {
			interactBtn.Visible = true;
			
			if(GlobalVariables.Instance.target.IsInGroup("batteries")) {
				interactBtn.Text = "Press E to Pick Up Battery";
			}
			else if(GlobalVariables.Instance.target.IsInGroup("Characters")) {
				interactBtn.Text = "Press E to Talk";
			}
			else if(GlobalVariables.Instance.target.IsInGroup("pickables")) {
				interactBtn.Text = "Press E to Pick Up";
			}
			else {
				interactBtn.Text = "Press E to Interact";
			}
			
			if (Input.IsActionJustPressed("interact")) {
				if(GlobalVariables.Instance.target.IsInGroup("batteries")) {
					GlobalVariables.Instance.FlashlightBattery += 25.0f;
					GD.Print($"Picked up: {GlobalVariables.Instance.target.Name}");
					Node parent = GlobalVariables.Instance.target.GetParent();
					if (parent != null && parent is Node3D && parent.Name != "Interactables") {
						parent.QueueFree();
					} else {
						GlobalVariables.Instance.target.QueueFree();
					}
					GlobalVariables.Instance.target = null;
					interactBtn.Visible = false;
				}
				else if(GlobalVariables.Instance.target.IsInGroup("Characters")) {
					var dialogueResource = GD.Load<Resource>("res://Dialogues/mang jason.dialogue");
					DialogueManager.ShowDialogueBalloon(dialogueResource, "start");
					GlobalVariables.Instance.target = null;
					interactBtn.Visible = false;
				}
				else if (GlobalVariables.Instance.AddItem(GlobalVariables.Instance.target.Name)) {
					GD.Print($"Picked up: {GlobalVariables.Instance.target.Name}");
					Node parent = GlobalVariables.Instance.target.GetParent();
					if (parent != null && parent is Node3D && parent.Name != "Interactables") {
						parent.QueueFree();
					} else {
						GlobalVariables.Instance.target.QueueFree();
					}
					GlobalVariables.Instance.target = null;
					interactBtn.Visible = false;
				}
			}
		}
		else {
			interactBtn.Visible = false;
			interactBtn.Text = "Press E to Interact";
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

	// ================= INPUT (FIXED LOCATION) =================
	public override void _Input(InputEvent @event)
	{
		var g = GlobalVariables.Instance;

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
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
		}
	}
}
