using Godot;
using System;

public partial class AlingShoneng : Node3D
{
	private bool triggered = false;
	
	public override void _Ready()
	{
		GD.Print("[SHONENG] Ready");
		GlobalVariables.Instance.alingShoneng = this;
	}

	public void OnDialogueEnded()
	{
		if (triggered) return;
		triggered = true;
		GD.Print("[SHONENG] Dialogue ended");
		QuestSystem.Instance.TriggerQuestAdvance("aling_shoneng_has_balut");
	}
}
