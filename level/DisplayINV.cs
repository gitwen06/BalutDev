using Godot;
using System;
using System.Collections.Generic;

public partial class DisplayINV : Control
{
	private TextureRect[] slots = new TextureRect[5];
	private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
	private Color equippedColor = new Color(1, 1, 1, 1);
	private Color unequippedColor = new Color(0.6f, 0.6f, 0.6f, 1);
	private int lastEquippedIndex = -1;
	private int lastInventoryCount = 0;

	public override void _Ready() {
		slots[0] = GetNode<TextureRect>("hotbarIcon-1/TextureRect");
		slots[1] = GetNode<TextureRect>("hotbarIcon-2/TextureRect");
		slots[2] = GetNode<TextureRect>("hotbarIcon-3/TextureRect");
		slots[3] = GetNode<TextureRect>("hotbarIcon-4/TextureRect");
		slots[4] = GetNode<TextureRect>("hotbarIcon-5/TextureRect");
	}

	public override void _Process(double delta) {
		var g = GlobalVariables.Instance;
		int inventoryCount = g.inventory.Count;
		int equippedIndex = g.equippedIndex;

		// Only update if inventory or equipped index changed
		if (lastInventoryCount != inventoryCount || lastEquippedIndex != equippedIndex) {
			UpdateInventoryDisplay(g, inventoryCount, equippedIndex);
			lastInventoryCount = inventoryCount;
			lastEquippedIndex = equippedIndex;
		}
	}

	private void UpdateInventoryDisplay(GlobalVariables g, int inventoryCount, int equippedIndex) {
		for (int i = 0; i < slots.Length; i++) {
			if (i < inventoryCount) {
				slots[i].Visible = true;
				slots[i].Texture = GetCachedIcon(g.inventory[i]);
			}
			else {
				slots[i].Visible = false;
				slots[i].Texture = null;
			}

			// Only update modulate if equipped state changed
			slots[i].Modulate = (i == equippedIndex) ? equippedColor : unequippedColor;
		}
	}

	private Texture2D GetCachedIcon(string itemId) {
		if (itemId.Contains("batteries")) {
			return null;
		}

		if (textureCache.TryGetValue(itemId, out var cached)) {
			return cached;
		}

		string path = $"res://Images/{itemId}.png";
		Texture2D texture = GD.Load<Texture2D>(path);
		textureCache[itemId] = texture;
		return texture;
	}
}
