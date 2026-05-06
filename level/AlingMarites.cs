using Godot;
using System;

public partial class AlingMarites : Node3D
{
	private bool triggered = false;
	
	public override void _Ready()
	{
		GlobalVariables.Instance.alingMarites = this;
		GD.Print("[MARITES] Ready and stored in GlobalVariables");
	}

	public void OnDialogueEnded()
	{
		if (triggered) return;
		triggered = true;

		GD.Print("[MARITES] Dialogue ended → enabling peek");
		var peek = GetTree().Root.FindChild("MonsterPeekTrigger", true, false) as MonsterPeekTrigger;

		if (peek != null)
			GD.Print("not found");
			peek.EnableTrigger();
	}
}
