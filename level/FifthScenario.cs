using Godot;
using System;

public partial class FifthScenario : Node3D
{
	private CharacterMonster monster;
	private KuyaJames kuyaJames;
	private Node3D player;
	private bool monsterSequenceStarted = false;

	public override void _Ready()
	{
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

		GD.Print("[FIFTH SCENARIO] Monster attack started");

		// ================= WAIT 5 SECONDS =================
		await ToSignal(GetTree().CreateTimer(5.0f), "timeout");

		// ================= KUYA JAMES DIES =================
		if (kuyaJames != null)
		{
			kuyaJames.PlayDeadAnimation();
			GD.Print("[FIFTH SCENARIO] Kuya James dead animation");
		}

		// ================= WAIT ANOTHER 5 SECONDS =================
		await ToSignal(GetTree().CreateTimer(5.0f), "timeout");

		// ================= ENABLE CHASE =================
		monster.EnableChaseAI();
		GlobalVariables.Instance.isFinalChase = true;

		GD.Print("[FIFTH SCENARIO] Monster AI enabled");
	}
}
