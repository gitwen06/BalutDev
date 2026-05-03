using Godot;
using DialogueManagerRuntime;

public partial class Trigger : Node3D
{
	[Export] public string dialoguePath = "res://Dialogues/selfDialogue.dialogue";
	[Export] public string startNode = "start";

	private Area3D area;
	private bool triggered = false;

	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");
		area.BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (triggered)
			return;

		if (!body.IsInGroup("player"))
			return;

		triggered = true;

		// Unlock mouse
		Input.MouseMode = Input.MouseModeEnum.Visible;

		var dialogue = GD.Load<Resource>(dialoguePath);

		if (dialogue != null)
		{
			DialogueManager.ShowDialogueBalloon(dialogue, startNode);

			// optional: also ensure you stop re-triggering logic elsewhere
			DialogueManager.DialogueEnded += OnDialogueEnded;
		}
		else
		{
			GD.PrintErr($"Dialogue not found: {dialoguePath}");
		}
	}

	private void OnDialogueEnded(Resource resource)
	{
		// Lock mouse back
		Input.MouseMode = Input.MouseModeEnum.Captured;

		DialogueManager.DialogueEnded -= OnDialogueEnded;
	}
}
