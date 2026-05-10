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
	public string npcToSpawn = "";

	[Export]
	public float spawnDistance = 1.0f;

	[Export]
	public float spawnHeight = 0.633f;

	private bool needKey = false;

	public bool IsOpen => isOpen;

	public override void _Ready()
	{
		animPlayer = FindChild("AnimationPlayer") as AnimationPlayer;

		if (animPlayer == null)
		{
			GD.PrintErr($"PintoOpen ({Name}): AnimationPlayer not found!");
			return;
		}

		// ================= AUTO LOCK BY NAME =================
		if (Name.ToString().Contains("DoorNeedKey"))
		{
			needKey = true;
		}
		else
		{
			needKey = false;
		}

		GD.Print($"PintoOpen ({Name}) ready");
		GD.Print($"needKey = {needKey}");
	}

	public void OpenDoor()
	{
		
		GD.Print($"needKey: {needKey}");
		if (animPlayer == null)
		{
			GD.PrintErr($"PintoOpen ({Name}): AnimationPlayer is null!");
			return;
		}

		// LOCKED → SHAKE
		if (needKey && !HasKey())
		{
			animPlayer.Play("shake_door");
			GD.Print("Door is locked. Need key.");
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

		GlobalVariables.Instance.RemoveNPC();
	}

	public bool RequiresKey()
	{
		return needKey;
	}

	public bool HasKey()
	{
		if (!needKey)
			return true;

		var g = GlobalVariables.Instance;

		if (g == null)
			return false;

		if (g.currentItem == null)
			return false;

		return g.currentItem.Name.ToString().ToLower().Contains("key");
	}
}
