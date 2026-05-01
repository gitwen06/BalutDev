using Godot;
using System;

public partial class PintoInteractionKnock : MeshInstance3D
{
	private string npcToSpawn = "aling_neneng"; // Name of NPC scene without .tscn
	private float spawnDistance = 1.0f; // Distance in front of door
	private Node3D spawnedNPC;
	
	public override void _Ready()
	{
	}

	public void KnockOnDoor()
	{
		GD.Print("Knocking on door...");
		
		if (string.IsNullOrEmpty(npcToSpawn))
		{
			GD.PrintErr("PintoKnock: No NPC to spawn!");
			return;
		}
		
		SpawnNPC();
	}

	private void SpawnNPC()
	{
		string npcPath = $"res://MonstersCharacters/{npcToSpawn}.tscn";
		
		if (ResourceLoader.Exists(npcPath))
		{
			var npcScene = GD.Load<PackedScene>(npcPath);
			spawnedNPC = (Node3D)npcScene.Instantiate();
			
			GetTree().Root.AddChild(spawnedNPC);
			
			Vector3 spawnPos = GlobalPosition + (GlobalTransform.Basis.X * spawnDistance);
			spawnPos.Y = 0.633f;
			spawnedNPC.GlobalPosition = spawnPos;
			
			spawnedNPC.Rotation = new Vector3(0, 70, 0);
			
			// Track in GlobalVariables
			GlobalVariables.Instance.spawnedNPC = spawnedNPC;
			
			GD.Print($"Spawned {npcToSpawn} at door facing towards it");
		}
		else
		{
			GD.PrintErr($"PintoKnock: NPC scene not found at {npcPath}");
		}
	}

	public void RemoveNPC()
	{
		if (spawnedNPC != null)
		{
			spawnedNPC.QueueFree();
			spawnedNPC = null;
		}
	}
}
