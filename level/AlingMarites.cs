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
		
		if (GlobalVariables.Instance.monsterPeekTrigger != null)
		{
			GlobalVariables.Instance.monsterPeekTrigger.EnableTrigger();
			GD.Print("[MARITES] Peek trigger enabled");
		}
		else
		{
			GD.PrintErr("[MARITES] monsterPeekTrigger is NULL!");
		}

		QuestSystem.Instance.TriggerQuestAdvance("aling_marites_has_balut");
	}
}
