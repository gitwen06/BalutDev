using Godot;
using System;

public partial class Balut : RigidBody3D
{
	private GlobalVariables g;
	private RichTextLabel displayAmount;
	private Node3D cachedParent;
	private bool wasInHand = false;
	private int lastBalutAmount = -1;

	public override void _Ready()
	{
		g = GlobalVariables.Instance;
		displayAmount = GetNodeOrNull<RichTextLabel>("%displayAmount");
		if (displayAmount == null)
			GD.PrintErr("displayAmount not found!");
		else
			displayAmount.Visible = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		cachedParent = GetParent() as Node3D;
		bool inHand = cachedParent != null && cachedParent.Name == "Hand";

		if (displayAmount != null)
		{
			// Only update if state changed
			if (wasInHand != inHand)
			{
				displayAmount.Visible = inHand;
				wasInHand = inHand;
			}

			// Only update text if amount changed and visible
			if (inHand && lastBalutAmount != g.balutAmount)
			{
				displayAmount.Text = $"{g.balutAmount} / 6 Balut";
				lastBalutAmount = g.balutAmount;
			}
		}
	}
}
