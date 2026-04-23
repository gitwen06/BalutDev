using Godot;
using System;

public partial class DisplayINV : Control
{
	private TextureRect[] slots = new TextureRect[5];

	public override void _Ready() {
		slots[0] = GetNode<TextureRect>("hotbarIcon-1/TextureRect");
		slots[1] = GetNode<TextureRect>("hotbarIcon-2/TextureRect");
		slots[2] = GetNode<TextureRect>("hotbarIcon-3/TextureRect");
		slots[3] = GetNode<TextureRect>("hotbarIcon-4/TextureRect");
		slots[4] = GetNode<TextureRect>("hotbarIcon-5/TextureRect");
	}

	public override void _Process(double delta) {
		var g = GlobalVariables.Instance;

		for (int i = 0; i < slots.Length; i++) {
			if (i < g.inventory.Count) {
				slots[i].Visible = true;
				slots[i].Texture = LoadIcon(g.inventory[i]);
			}
			else {
				slots[i].Texture = null;
			}
			if (i == g.equippedIndex) {
				slots[i].Modulate = new Color(1, 1, 1);
			}
			else {
				slots[i].Modulate = new Color(0.6f, 0.6f, 0.6f);
			}
		}
	}

	private Texture2D LoadIcon(string itemId) {
		string path = "res://Images/" + itemId + ".jpg";
		return GD.Load<Texture2D>(path);
	}
}
