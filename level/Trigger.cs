using Godot;
using DialogueManagerRuntime;

public partial class Trigger : Node3D
{
	private Area3D area;
	private bool triggered = false;

	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");
		area.BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (triggered) return;
		if (!body.IsInGroup("player")) return;

		triggered = true;

		string eventName = Name;

		// Decide what type of trigger this is
		if (IsInGroup("houseEventTrigger"))
		{
			HandleHouseEvent(eventName);
		}
		else if (IsInGroup("selfDialogue"))
		{
			HandleDialogue(eventName);
		}
	}

	private void HandleDialogue(string eventName)
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

			// safety reset
			GlobalVariables.Instance.isTalking = false;
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

	private void HandleHouseEvent(string eventName)
	{
		GD.Print($"House Event Triggered: {eventName}");

		var house = GetTree().GetFirstNodeInGroup("house");

		if (house != null && house.HasMethod("PlayEvent"))
		{
			house.Call("PlayEvent", eventName);
		}
		else
		{
			GD.PrintErr("House node missing or missing PlayEvent()");
		}
	}

	private void OnDialogueEnded(Resource resource)
	{
		GlobalVariables.Instance.isTalking = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;

		DialogueManager.DialogueEnded -= OnDialogueEnded;
	}
}
