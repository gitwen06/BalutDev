using Godot;
using DialogueManagerRuntime;

public partial class Trigger : Node3D
{
	private Area3D area;
	private bool triggered = false;
	private bool isHouseEventTrigger = false;
	private bool isSelfDialogueTrigger = false;
	private bool isJumpscareTrigger = false;

	private string eventName = "";
	private Node houseNode = null;

	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");
		area.BodyEntered += OnBodyEntered;

		// Cache group checks and event name
		eventName = Name;
		isHouseEventTrigger = IsInGroup("houseEventTrigger");
		isSelfDialogueTrigger = IsInGroup("selfDialogue");
		isJumpscareTrigger = IsInGroup("jumpscare");

		// Cache house node once
		if (isHouseEventTrigger)
		{
			houseNode = GetTree().GetFirstNodeInGroup("house");
			if (houseNode == null)
				GD.PrintErr("House node missing or not in 'house' group");
		}
	}

	private void OnBodyEntered(Node3D body)
	{
		if (triggered || !body.IsInGroup("player"))
			return;

		triggered = true;

		if (isHouseEventTrigger)
		{
			HandleHouseEvent();
		}
		else if (isSelfDialogueTrigger)
		{
			HandleDialogue();
		}
		else if (isJumpscareTrigger)
		{
			GD.Print("triggered jumpscare");

			JumpscareManager.Instance.PlayJumpscare(eventName);
		}
	}

	// ================= HOUSE EVENTS =================
	private void HandleHouseEvent()
	{
		GD.Print($"House Event Triggered: {eventName}");

		if (eventName == "spawnMonster")
		{
			var monsterScene = GD.Load<PackedScene>("res://MonstersCharacters/Character_Monster.tscn");

			if (monsterScene == null)
			{
				GD.PrintErr("Monster scene not found!");
				return;
			}

			// ✅ SPAWN IN CURRENT SCENE (NOT ROOT)
			Node currentScene = GetTree().CurrentScene;

			if (currentScene == null)
			{
				GD.PrintErr("CurrentScene is null!");
				return;
			}

			Node3D monster = monsterScene.Instantiate<Node3D>();
			currentScene.AddChild(monster);

			// IMPORTANT: let monster handle its own spawn logic
			if (monster.HasMethod("PlayEvent"))
			{
				monster.Call("PlayEvent", "spawnMonster");
			}

			GD.Print("Monster spawned successfully");
			return;
		}

		// DEFAULT HOUSE EVENT BEHAVIOR
		if (houseNode != null && houseNode.HasMethod("PlayEvent"))
		{
			houseNode.Call("PlayEvent", eventName);
		}
		else
		{
			GD.PrintErr("House node missing or missing PlayEvent()");
		}
	}

	// ================= DIALOGUE =================
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
		else
		{
			GD.PrintErr($"Dialogue not found: {dialoguePath}");
			ResetDialogueState();
		}
	}

	// ================= DIALOGUE END =================
	private void OnDialogueEnded(Resource resource)
	{
		ResetDialogueState();
		DialogueManager.DialogueEnded -= OnDialogueEnded;
	}

	private void ResetDialogueState()
	{
		GlobalVariables.Instance.isTalking = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
}
