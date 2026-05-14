using Godot;
using System;

public partial class KuyaJames : Node3D
{
	private AnimationPlayer animPlayer;
	private bool triggered = false;

	public override void _Ready()
	{
		GlobalVariables.Instance.kuyaJames = this;
		GD.Print("[KUYA JAMES] Ready");
		
		animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (animPlayer == null)
			GD.PrintErr("[KUYA JAMES] AnimationPlayer not found!");
	}

	public void OnDialogueEnded()
	{
		if (triggered) return;
		triggered = true;

		GD.Print("[KUYA JAMES] OnDialogueEnded called");
		
		var fifthScenario = GetParent() as FifthScenario;
		if (fifthScenario != null)
		{
			fifthScenario.StartMonsterSequence();
		}
		else
		{
			GD.PrintErr("[KUYA JAMES] FifthScenario parent not found!");
		}
	}

	public void PlayStandAnimation()
	{
		if (animPlayer != null)
		{
			animPlayer.Play("stand");
			GD.Print("[KUYA JAMES] Playing stand animation");
		}
	}

	public void PlayIdleAnimation()
	{
		if (animPlayer != null)
			animPlayer.Play("idle");
	}

	public void PlayDeadAnimation()
	{
		if (animPlayer != null)
		{
			animPlayer.Play("dead");
			GD.Print("[KUYA JAMES] Playing dead animation");
		}
	}
}
