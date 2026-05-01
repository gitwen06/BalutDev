using Godot;
using System;

public partial class Balut : RigidBody3D
{
	private GlobalVariables g;
	private RichTextLabel displayAmount;

	public override void _Ready()
	{
		g = GlobalVariables.Instance;

		displayAmount = GetNodeOrNull<RichTextLabel>("%displayAmount");

		if (displayAmount == null)
			GD.PrintErr("displayAmount not found!");
	}

	public override void _PhysicsProcess(double delta)
	{
		Node3D parent = GetParent() as Node3D;

		bool inHand = parent != null && parent.Name == "Hand";

		if (displayAmount != null)
		{
			displayAmount.Visible = inHand;

			if (inHand)
			{
				displayAmount.Text = $"{g.balutAmount} / 5 Balut";
			}
		}
	}
}
