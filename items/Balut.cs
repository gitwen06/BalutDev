using Godot;
using System;

public partial class Balut : StaticBody3D
{
	private GlobalVariables g;
	private RichTextLabel displayAmount;
	private AudioStreamPlayer screamPlayer;

	private Node3D cachedParent;
	private bool wasInHand = false;
	private int lastBalutAmount = -1;

	private const string HAND_NODE_NAME = "Hand";
	private const string DISPLAY_AMOUNT_PATH = "%displayAmount";
	private const string SOUND_PATH = "res://Sounds/balut.mp3";

	public override void _Ready()
	{
		g = GlobalVariables.Instance;

		displayAmount = GetNodeOrNull<RichTextLabel>(DISPLAY_AMOUNT_PATH);

		if (displayAmount != null)
			displayAmount.Visible = false;
		else
			GD.PrintErr("[BALUT] displayAmount not found!");

		// ================= AUDIO =================
		screamPlayer = new AudioStreamPlayer();
		AddChild(screamPlayer);

		screamPlayer.Stream = GD.Load<AudioStream>(SOUND_PATH);
		screamPlayer.VolumeDb = 0f;
		screamPlayer.Bus = "Master";
	}

	public override void _PhysicsProcess(double delta)
	{
		cachedParent = GetParent() as Node3D;

		bool inHand = cachedParent?.Name == HAND_NODE_NAME;

		UpdateDisplay(inHand);
	}

	public override void _Input(InputEvent @event)
	{
		cachedParent = GetParent() as Node3D;

		bool inHand = cachedParent?.Name == HAND_NODE_NAME;

		if (!inHand)
			return;

		// ================= PLAY SCREAM ON F =================
		if (@event.IsActionPressed("Scream"))
		{
			// IMPORTANT:
			// don't restart if already playing
			if (!screamPlayer.Playing)
			{
				screamPlayer.Play();

				GD.Print("[BALUT] scream played");
			}
		}
	}

	private void UpdateDisplay(bool inHand)
	{
		if (displayAmount == null)
			return;

		// Update visibility only when state changes
		if (wasInHand != inHand)
		{
			displayAmount.Visible = inHand;
			wasInHand = inHand;
		}

		// Update text only when amount changes
		if (inHand && lastBalutAmount != g.balutAmount)
		{
			displayAmount.Text = $"{g.balutAmount} / 6 : F to Scream Balut";
			lastBalutAmount = g.balutAmount;
		}
	}
}
