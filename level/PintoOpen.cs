using Godot;
using System;

public partial class PintoOpen : MeshInstance3D
{
	private AnimationPlayer animPlayer;
	private bool isOpen = false;
	private string requiredKey = ""; // Leave empty if no key needed
	
	public bool IsOpen => isOpen; // Add this line
	
	public override void _Ready()
	{
		animPlayer = FindChild("AnimationPlayer") as AnimationPlayer;
		if (animPlayer == null)
			GD.PrintErr("PintoOpen: AnimationPlayer not found!");
	}

	public void OpenDoor()
	{
		if (isOpen)
		{
			GD.Print("Door already open");
			return;
		}
		
		isOpen = true;
		if (animPlayer != null)
		{
			animPlayer.Play("open_door");
			GD.Print("Opening door");
		}
	}

	public void ResetDoor()
	{
		isOpen = false;
		GD.Print("Door reset");
	}

	public void CloseDoor()
	{
		if (!isOpen)
			return;
		
		isOpen = false;
		if (animPlayer != null)
		{
			animPlayer.Play("close_door");
			GD.Print("Closing door");
		}
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
