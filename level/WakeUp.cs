using Godot;
using System.Threading.Tasks;

public partial class WakeUp : Control
{
	private ColorRect fade;
	private ColorRect blur;
	private Control ui;

	public override void _Ready()
	{
		// UI references (direct children of this Control)
		fade = GetNodeOrNull<ColorRect>("WakeUpFade");
		blur = GetNodeOrNull<ColorRect>("BlurRect");
		ui = GetNodeOrNull<Control>("UserInterface");

		// Start state: UI hidden
		if (ui != null)
			ui.Visible = false;

		StartWakeUp();
	}

	private async void StartWakeUp()
	{
		// INITIAL STATE: black screen + blur
		if (fade != null)
			fade.Modulate = new Color(0, 0, 0, 1);

		if (blur != null)
			blur.Modulate = new Color(1, 1, 1, 0.6f);

		// BLINKING EFFECT (eyes adjusting)
		for (int i = 0; i < 3; i++)
		{
			await FadeBlack(1f, 0.85f, 0.2f);
			await FadeBlack(0.85f, 1f, 0.2f);
		}

		// GRADUAL VISION RETURN
		await FadeBlack(1f, 0f, 2.5f);

		// REMOVE BLUR
		if (blur != null)
			await FadeBlur(0.6f, 0f, 2.5f);

		// FINAL CLEANUP
		if (fade != null)
			fade.Modulate = new Color(0, 0, 0, 0);

		// SHOW UI AFTER WAKE-UP
		if (ui != null)
			ui.Visible = true;
	}

	private async Task FadeBlack(float from, float to, float duration)
	{
		if (fade == null) return;

		float t = 0;

		while (t < duration)
		{
			t += (float)GetProcessDeltaTime();
			float lerp = t / duration;

			float value = Mathf.Lerp(from, to, lerp);

			fade.Modulate = new Color(0, 0, 0, value);

			await ToSignal(GetTree(), "process_frame");
		}
	}

	private async Task FadeBlur(float from, float to, float duration)
	{
		if (blur == null) return;

		float t = 0;

		while (t < duration)
		{
			t += (float)GetProcessDeltaTime();
			float lerp = t / duration;

			float value = Mathf.Lerp(from, to, lerp);

			blur.Modulate = new Color(1, 1, 1, value);

			await ToSignal(GetTree(), "process_frame");
		}
	}
}
