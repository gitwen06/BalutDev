using Godot;
using System.Threading.Tasks;

public partial class AnimationForCharacters : Node
{
	private AnimationPlayer anim;
	private bool canClose = false;

	public override void _Ready()
	{
		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		GlobalVariables.Instance.animationController = this;
		
		GD.Print("neger");
	}

	// THIS is what Dialogue Manager will call
	public async void OpenThenWaitThenClose()
	{
		// Play forward (open)
		canClose = false;
		
		anim.Play("new_animation");
		await ToSignal(anim, "animation_finished");

		// Wait until condition is triggered
		await WaitUntil(() => canClose);

		// Play backwards (close)
		anim.PlayBackwards("new_animation");
	}

	public void AllowClose()
	{
		canClose = true;
	}

	private async Task WaitUntil(System.Func<bool> condition)
	{
		while (!condition())
			await ToSignal(GetTree(), "process_frame");
	}
}
