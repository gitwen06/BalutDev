using Godot;
using System;
using System.Collections.Generic;

public partial class QuestSystem : Node
{
	public static QuestSystem Instance;

	private Label displayQuest;

	private bool uiBound = false;

	public int currentQuest = 0;

	private bool gotBattery = false;
	private bool gotDrink = false;

	private bool canAdvance = true;
	private SceneTreeTimer advanceTimer;
	private SceneTreeTimer uiBindTimer;

	// Arrow caching
	private Dictionary<string, Sprite3D> arrowCache = new Dictionary<string, Sprite3D>();

	public override void _Ready()
	{
		if (Instance != null && Instance != this)
		{
			GD.PrintErr("Duplicate QuestSystem detected. Removing extra instance.");
			QueueFree();
			return;
		}

		Instance = this;

		ResetState();

		GD.Print($"[QUEST SYSTEM READY] ID: {GetInstanceId()}");

		CallDeferred(nameof(TryAutoBindUI));
		CallDeferred(nameof(CacheQuestArrows));
	}

	// ================= ARROW CACHING =================
	private void CacheQuestArrows()
	{
		// Get the main scene (not root)
		var mainScene = GetTree().CurrentScene;

		// Cache all quest item arrows using unique names
		CacheArrow("flashlight", mainScene);
		CacheArrow("balut", mainScene);
		CacheArrow("energydrink", mainScene);

		GD.Print($"[QUEST] Cached {arrowCache.Count} arrows");
	}

	private void CacheArrow(string itemName, Node root)
	{
		// Use GetNodeOrNull with % prefix for unique names
		var item = root.GetNodeOrNull<Node>($"%{itemName}");

		if (item != null)
		{
			var arrow = item.FindChild("arrow", false, false) as Sprite3D;

			if (arrow != null)
			{
				arrowCache[itemName] = arrow;
				// Make arrow visible through walls
				arrow.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
				arrow.Transparency = 0.7f;
				arrow.Visible = false;
				GD.Print($"[QUEST] Arrow cached for: {itemName}");
			}
		}
	}

	public override void _Process(double delta)
	{
		// Ensure current quest arrow stays visible
		UpdateQuestArrows();
	}

	// ================= AUTO UI BIND =================
	private void TryAutoBindUI()
	{
		if (uiBound) return;

		// try find UI anywhere in scene tree
		var root = GetTree().Root;

		displayQuest = root.FindChild("questDisplay", true, false) as Label;

		if (displayQuest != null)
		{
			uiBound = true;
			GD.Print("[QUEST] UI auto-bound successfully");
			UpdateUI();
		}
		else
		{
			// retry next frame until UI exists
			if (uiBindTimer != null)
				uiBindTimer.TimeLeft = 0;
			uiBindTimer = GetTree().CreateTimer(0.2);
			uiBindTimer.Timeout += TryAutoBindUI;
		}
	}

	// ================= OPTIONAL MANUAL BIND =================
	public void BindUI(Label label)
	{
		displayQuest = label;
		uiBound = true;

		GD.Print("[QUEST] UI manually bound");
		UpdateUI();
	}

	// ================= RESET =================
	private void ResetState()
	{
		gotBattery = false;
		gotDrink = false;
		canAdvance = true;
	}

	// ================= CORE =================
	public void ProceedQuest(string reason = "")
	{
		if (!canAdvance) return;

		canAdvance = false;

		currentQuest++;

		GD.Print($"[QUEST] Advanced → {GetCurrentObjective()} ({reason})");
		GD.Print($"[QUEST STATE] Now at: {currentQuest}");

		OnQuestChanged(currentQuest);

		if (advanceTimer != null)
			advanceTimer.TimeLeft = 0;
		advanceTimer = GetTree().CreateTimer(0.15);
		advanceTimer.Timeout += () =>
		{
			canAdvance = true;
		};
	}

	// ================= TRIGGERS =================
	public void TriggerQuestAdvance(string trigger)
	{
		if (string.IsNullOrEmpty(trigger)) return;

		switch (trigger)
		{
			case "mang_jason_talk":
				if (currentQuest == 0)
					ProceedQuest("dialogue:mang_jason");
				break;

			case "aling_neneng_has_balut":
				if (currentQuest == 3)
					ProceedQuest("dialogue:aling_neneng");
				break;
		}
	}

	// ================= ITEMS =================
	public void OnItemPicked(string itemName)
	{
		if (string.IsNullOrEmpty(itemName)) return;

		itemName = itemName.ToLower();

		GD.Print($"[QUEST] Item detected: {itemName} | Quest: {currentQuest}");

		switch (currentQuest)
		{
			case 1:
				if (itemName.Contains("flashlight") || itemName.Contains("balut") || itemName.Contains("battery") || itemName.Contains("drink"))
					ProceedQuest("item:picked");
				break;
			case 2:
				if (itemName.Contains("balut") || itemName.Contains("flashlight") || itemName.Contains("battery") || itemName.Contains("drink"))
					ProceedQuest("item:picked");
				break;
			case 4:
				if (itemName.Contains("battery"))
				{
					gotBattery = true;
					if (gotDrink)
						ProceedQuest("store:complete");
				}
				else if (itemName.Contains("drink"))
				{
					gotDrink = true;
					if (gotBattery)
						ProceedQuest("store:complete");
				}
				break;
		}
	}

	// ================= QUEST UPDATE =================
	private void OnQuestChanged(int quest)
	{
		ResetStageFlagsIfNeeded();
		UpdateUI();
		UpdateQuestArrows();

		GD.Print($"[QUEST UPDATED] → {GetCurrentObjective()}");
	}

	private void ResetStageFlagsIfNeeded()
	{
		if (currentQuest != 4)
		{
			gotBattery = false;
			gotDrink = false;
		}
	}

	// ================= QUEST ARROWS =================
	private void UpdateQuestArrows()
	{
		// Hide all arrows first
		foreach (var arrow in arrowCache.Values)
		{
			if (arrow != null && IsInstanceValid(arrow))
				arrow.Visible = false;
		}

		// Show arrow for current quest
		switch (currentQuest)
		{
			case 1:
				ShowArrow("flashlight");
				break;
			case 2:
				ShowArrow("balut");
				break;
			case 4:
				ShowArrow("energydrink");
				break;
		}
	}

	private void ShowArrow(string itemName)
	{
		if (arrowCache.TryGetValue(itemName, out var arrow) && arrow != null && IsInstanceValid(arrow))
		{
			arrow.Visible = true;
		}
	}

	// ================= UI =================
	private void UpdateUI()
	{
		if (!uiBound || displayQuest == null) return;

		displayQuest.Text = GetCurrentObjective();
	}

	// ================= OBJECTIVES =================
	public string GetCurrentObjective()
	{
		return currentQuest switch
		{
			0 => "Talk to Tatay",
			1 => "Get the flashlight",
			2 => "Get the balut",
			3 => "Give balut to Aling Neneng",
			4 => "Get battery and drink from store",
			_ => "Objective complete"
		};
	}
}
