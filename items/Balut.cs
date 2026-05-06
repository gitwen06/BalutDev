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

	private float screamTimer = 0f;
	private float nextScreamTime = 0f;
	private Random random = new Random();

	private const float MIN_SCREAM_INTERVAL = 10f;
	private const float MAX_SCREAM_INTERVAL = 15f;
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

		// Setup audio
		screamPlayer = new AudioStreamPlayer();
		AddChild(screamPlayer);
		screamPlayer.Stream = GD.Load<AudioStream>(SOUND_PATH);
		screamPlayer.VolumeDb = 0f;

		SetNextScreamTime();
	}

	public override void _PhysicsProcess(double delta)
	{
		cachedParent = GetParent() as Node3D;
		bool inHand = cachedParent?.Name == HAND_NODE_NAME;

		UpdateDisplay(inHand);
		UpdateScream(inHand, (float)delta);
	}

	private void UpdateDisplay(bool inHand)
	{
		if (displayAmount == null) return;

		// Update visibility only when state changes
		if (wasInHand != inHand)
		{
			displayAmount.Visible = inHand;
			wasInHand = inHand;
		}

		// Update text only when amount changes
		if (inHand && lastBalutAmount != g.balutAmount)
		{
			displayAmount.Text = $"{g.balutAmount} / 6 Balut";
			lastBalutAmount = g.balutAmount;
		}
	}

	private void UpdateScream(bool inHand, float delta)
	{
		if (!inHand)
		{
			screamTimer = 0f;
			return;
		}

		screamTimer += delta;

		if (screamTimer >= nextScreamTime)
		{
			screamPlayer.Play();
			screamTimer = 0f;
			SetNextScreamTime();
		}
	}

	private void SetNextScreamTime()
	{
		nextScreamTime = random.Next((int)MIN_SCREAM_INTERVAL, (int)MAX_SCREAM_INTERVAL + 1);
	}
}
