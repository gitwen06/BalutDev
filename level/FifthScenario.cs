using Godot;
using System;

public partial class FifthScenario : Node3D
{
	private CharacterMonster monster;
	private KuyaJames kuyaJames;
	private Node3D player;
	private bool monsterSequenceStarted = false;
	private AudioStreamPlayer stabPlayer;

	public override void _Ready()
	{
		stabPlayer = new AudioStreamPlayer();
		AddChild(stabPlayer);
		stabPlayer.Bus = "Master";
		stabPlayer.Stream = GD.Load<AudioStream>("res://Sounds/Stab2.wav");
		GD.Print("[FIFTH SCENARIO] Ready");
		
		monster = GetTree().Root.FindChild("Character_Monster", true, false) as CharacterMonster;
		kuyaJames = GetNode<KuyaJames>("KuyaJames");
		player = GetTree().Root.FindChild("player", true, false) as Node3D;
		
		if (monster == null)
			GD.PrintErr("[FIFTH SCENARIO] Monster not found!");
		if (kuyaJames == null)
			GD.PrintErr("[FIFTH SCENARIO] Kuya James not found!");
		if (player == null)
			GD.PrintErr("[FIFTH SCENARIO] Player not found!");
	}

	public async void StartMonsterSequence()
	{
		GD.Print("[FIFTH SCENARIO] Starting monster sequence");
		if (monster == null)
		{
			GD.PrintErr("[FIFTH SCENARIO] Monster is NULL!");
			return;
		}
		monsterSequenceStarted = true;
		
		// ================= POSITION MONSTER =================
		Vector3 attackPos = new Vector3(-50.328f, 0.827f, 5.951f);
		monster.GlobalPosition = attackPos;
		monster.Velocity = Vector3.Zero;
		
		// ================= ROTATION =================
		monster.RotationDegrees = new Vector3(0, 105.3f, 0);
		
		// ================= DISABLE AI + PLAY ATTACK =================
		monster.StartScriptedAttack();
		await ToSignal(GetTree().CreateTimer(1.4f), "timeout");
		if (stabPlayer != null)
		{
			stabPlayer.Play();
			GD.Print("[FIFTH SCENARIO] Stab sound played");
		}
		GD.Print("[FIFTH SCENARIO] Monster attack started");
		
		
		// ================= WAIT 3.5 SECONDS =================
		await ToSignal(GetTree().CreateTimer(2.1f), "timeout");
		
		// ================= KUYA JAMES DIES =================
		if (kuyaJames != null)
		{
			kuyaJames.PlayDeadAnimation();
			GD.Print("[FIFTH SCENARIO] Kuya James dead animation");
		}
		
		// ================= ENABLE FIRE MESHES =================
		EnableFireMeshes();
		GD.Print("[FIFTH SCENARIO] Fire meshes enabled");
		
		// ================= WAIT ANOTHER 5 SECONDS =================
		await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
		
		// ================= ENABLE CHASE =================
		monster.EnableChaseAI();
		GlobalVariables.Instance.isFinalChase = true;
		GD.Print("[FIFTH SCENARIO] Monster AI enabled - FINAL CHASE START");
		QuestSystem.Instance.TriggerQuestAdvance("gaveKuyaJames");
	}

	private void EnableFireMeshes()
	{
		// Enable tripsFifthScenario
		var tripsNode = GetTree().Root.FindChild("tripsFifthScenario", true, false) as Node3D;
		if (tripsNode != null)
		{
			RecursivelyEnableNode(tripsNode);
			GD.Print("[FIFTH SCENARIO] tripsFifthScenario enabled recursively");
		}
		else
		{
			GD.PrintErr("[FIFTH SCENARIO] tripsFifthScenario not found!");
		}

		// Enable fireMeshes
		var fireNode = GetTree().Root.FindChild("fireMeshes", true, false) as Node3D;
		if (fireNode != null)
		{
			RecursivelyEnableNode(fireNode);
			GD.Print("[FIFTH SCENARIO] fireMeshes enabled recursively");
		}
		else
		{
			GD.PrintErr("[FIFTH SCENARIO] fireMeshes not found!");
		}
	}

	private void RecursivelyEnableNode(Node node)
	{
		// Show this node
		if (node is Node3D node3d)
		{
			node3d.Show();
			node3d.Visible = true;
		}
		
		if(node is MeshInstance3D mesh) {
			mesh.Layers = 1;
		}

		// Enable collision if it's a collision object
		if (node is CollisionObject3D collision)
		{
			collision.CollisionLayer = 1;
			collision.CollisionMask = 1;
			GD.Print($"[FIRE MESH] Enabled collision for {node.Name}");
		}

		// Recursively enable all children
		foreach (Node child in node.GetChildren())
		{
			RecursivelyEnableNode(child);
		}
	}
}
