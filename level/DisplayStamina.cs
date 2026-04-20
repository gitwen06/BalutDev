using Godot;
using System;

public partial class DisplayStamina : Label
{
	public override void _Process(double delta)
	{
		var g = GlobalVariables.Instance;

		if (g != null)
		{
			Text = $"Stamina: {g.stamina:0}";
		}
	}
}
