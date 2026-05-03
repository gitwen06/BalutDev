using Godot;
using System;
public partial class PintoOpen : MeshInstance3D
{
	private AnimationPlayer animPlayer;
	private bool isOpen = false;
	private string requiredKey = "";
	
	[Export]
	public string openAnimation = "open_door";
	[Export]
	public string closeAnimation = "close_door";
	[Export]
	public string npcToSpawn = ""; // Set NPC name in Inspector (e.g., "aling_neneng")
	[Export]
	public float spawnDistance = 1.0f;
	[Export]
	public float spawnHeight = 0.633f;
	
	public bool IsOpen => isOpen;
	
	public override void _Ready()
	{
		animPlayer = FindChild("AnimationPlayer") as AnimationPlayer;
		if (animPlayer == null)
		{
			GD.PrintErr($"PintoOpen ({Name}): AnimationPlayer not found!");
			return;
		}
		GD.Print($"PintoOpen ({Name}): AnimationPlayer found!");
	}
	public void OpenDoor()
	{
		if (animPlayer == null)
		{
			GD.PrintErr($"PintoOpen ({Name}): AnimationPlayer is null!");
			return;
		}
		
		if (isOpen)
		{
			GD.Print("Door already open");
			return;
		}
		
		isOpen = true;
		animPlayer.Play(openAnimation);
		GD.Print($"Opening door with animation: {openAnimation}");
		
	}
	public void CloseDoor()
	{
		if (animPlayer == null)
		{
			GD.PrintErr($"PintoOpen ({Name}): AnimationPlayer is null!");
			return;
		}
		
		if (!isOpen)
			return;
		
		isOpen = false;
		animPlayer.Play(closeAnimation);
		GD.Print($"Closing door with animation: {closeAnimation}");
		
		// Remove NPC when door closes
		GlobalVariables.Instance.RemoveNPC();
	}
	public bool RequiresKey()
	{
		return !string.IsNullOrEmpty(requiredKey);
	}
	public bool HasKey()
	{
		return GlobalVariables.Instance.inventory.Contains(requiredKey);
	}
}
