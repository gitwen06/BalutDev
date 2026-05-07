using Godot;
using DialogueManagerRuntime;

public partial class Trigger : Node3D
{
	private Area3D area;

	private bool triggered = false;

	private bool isHouseEventTrigger = false;
	private bool isSelfDialogueTrigger = false;
	private bool isJumpscareTrigger = false;
	

	// ================= SCREAM AREA =================
	private bool playerInsideScreamArea = false;
	private bool screamTriggered = false;

	private Camera3D playerCamera;
	private Node3D alingShoneng;

	private Transform3D originalCameraTransform;
	private bool cameraSaved = false;

	private Tween cameraTween;

	private string eventName = "";
	private Node houseNode = null;

	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");

		area.BodyEntered += OnBodyEntered;
		area.BodyExited += OnBodyExited;

		eventName = Name;

		isHouseEventTrigger = IsInGroup("houseEventTrigger");
		isSelfDialogueTrigger = IsInGroup("selfDialogue");
		isJumpscareTrigger = IsInGroup("jumpscare");

		// ================= HOUSE CACHE =================
		if (isHouseEventTrigger)
		{
			houseNode = GetTree().GetFirstNodeInGroup("house");

			if (houseNode == null)
				GD.PrintErr("House node missing or not in 'house' group");
		}

		// ================= DIALOGUEABOUTITEMS =================
		if (eventName == "DialogueAboutItems")
		{
			area.Monitoring = false;
			GD.Print("[TRIGGER] DialogueAboutItems DISABLED at start");
		}

		// ================= SCREAM AREA CACHE =================
		if (eventName == "ScreamArea")
		{
			playerCamera = GetTree().Root.FindChild("Camera3D", true, false) as Camera3D;

			alingShoneng = GetTree().Root.FindChild("Aling Shoneng", true, false) as Node3D;

			if (playerCamera == null)
				GD.PrintErr("[SCREAM AREA] Camera not found!");

			if (alingShoneng == null)
				GD.PrintErr("[SCREAM AREA] Aling Shoneng not found!");
		}
	}

	public override void _Process(double delta)
	{
		if (
			eventName == "ScreamArea" &&
			playerInsideScreamArea &&
			!screamTriggered &&
			Input.IsActionJustPressed("Scream")
		)
		{
			screamTriggered = true;

			GD.Print("[SCREAM AREA] Scream triggered!");

		StartScreamDialogue();
		}
	}
	// ================= START DIALOGUE =================
	private void StartScreamDialogue()
	{
		GlobalVariables.Instance.isTalking = true;

		Input.MouseMode = Input.MouseModeEnum.Visible;

		var dialoguePath = "res://Dialogues/aling shoneng.dialogue";
		var dialogue = GD.Load<Resource>(dialoguePath);

		DialogueManager.ShowDialogueBalloon(dialogue, "call");

		DialogueManager.DialogueEnded += OnDialogueEnded;
	}

	// ================= RESTORE CAMERA =================
	private void OnDialogueEnded(Resource resource)
	{
		ResetDialogueState();
		DialogueManager.DialogueEnded -= OnDialogueEnded;

	}

	// ================= BODY ENTER =================
	private void OnBodyEntered(Node3D body)
	{
		if (!body.IsInGroup("player"))
			return;

		if (eventName == "ScreamArea")
		{
			playerInsideScreamArea = true;
			GD.Print("[SCREAM AREA] Player entered");
			return;
		}

		if (triggered)
			return;

		triggered = true;

		if (isHouseEventTrigger)
			HandleHouseEvent();
		else if (isSelfDialogueTrigger)
			HandleDialogue();
		else if (isJumpscareTrigger)
			JumpscareManager.Instance.PlayJumpscare(eventName);
	}

	private void OnBodyExited(Node3D body)
	{
		if (!body.IsInGroup("player"))
			return;

		if (eventName == "ScreamArea")
		{
			playerInsideScreamArea = false;
			GD.Print("[SCREAM AREA] Player exited");
		}
	}

	public void EnableTrigger()
	{
		triggered = false;
		area.SetDeferred("monitoring", true);
		GD.Print($"[TRIGGER] {eventName} ENABLED");
	}

	// ================= HOUSE EVENTS =================
	private void HandleHouseEvent()
	{
		if (houseNode != null && houseNode.HasMethod("PlayEvent"))
			houseNode.Call("PlayEvent", eventName);
	}

	// ================= NORMAL DIALOGUE =================
	private void HandleDialogue()
	{
		GlobalVariables.Instance.isTalking = true;

		Input.MouseMode = Input.MouseModeEnum.Visible;

		string dialoguePath = $"res://Dialogues/{eventName}.dialogue";

		var dialogue = GD.Load<Resource>(dialoguePath);

		if (dialogue != null)
		{
			DialogueManager.ShowDialogueBalloon(dialogue, "start");
			DialogueManager.DialogueEnded += OnDialogueEnded;
		}
	}

	// ================= RESET =================
	private void ResetDialogueState()
	{
		GlobalVariables.Instance.isTalking = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
}
