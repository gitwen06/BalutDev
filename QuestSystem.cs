using Godot;
using System;
using System.Collections.Generic;

public partial class QuestSystem : Node
{
	public static QuestSystem Instance;

	private Label displayQuest;
	private Dictionary<string, Node3D> glowNodes = new Dictionary<string, Node3D>();

	private bool uiBound = false;
	private bool canAdvance = true;
	private bool levelReady = false;

	private SceneTreeTimer advanceTimer;
	private SceneTreeTimer uiBindTimer;
	private SceneTreeTimer levelWaitTimer;

	public int currentQuest = 0;
	public bool returnHorrorActive = false;

	private bool gotBattery = false;
	private bool gotDrink = false;

	private const float ADVANCE_COOLDOWN = 0.15f;
	private const float UI_BIND_RETRY_DELAY = 0.2f;
	private const float LEVEL_RETRY_DELAY = 0.5f;

	private static readonly Dictionary<int, string[]> QuestItems = new()
	{
		{ 0, new[] { "mang jason" } },
		{ 1, new[] { "flashlight", "balut" } },
		{ 2, new[] { "balut" } },
		{ 3, new[] { "aling neneng" } },
		{ 4, new[] { "energydrink", "battery" } },
		{ 5, new[] { "balut" } },
		{ 6, new[] { "aling shoneng" } },
		{ 7, new[] { "aling marites" } },
		{ 8, new[] { "aling marin" } },
	};

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

		WaitForLevelScene();
	}

	// ================= WAIT FOR REAL LEVEL =================
	private void WaitForLevelScene()
	{
		Node currentScene = GetTree().CurrentScene;

		if (currentScene == null)
		{
			RetryLevelWait();
			return;
		}

		string sceneName = currentScene.Name.ToString().ToLower();

		if (sceneName.Contains("mainmenu") || sceneName.Contains("loading"))
		{
			GD.Print($"[QUEST] Waiting for gameplay level... Current scene: {sceneName}");
			RetryLevelWait();
			return;
		}

		levelReady = true;
		GD.Print($"[QUEST] Gameplay level detected: {sceneName}");

		CacheGlowNodes();
		TryAutoBindUI();
		UpdateQuestGlow();
		UpdateUI();
	}

	private void RetryLevelWait()
	{
		if (levelWaitTimer != null)
			levelWaitTimer.TimeLeft = 0;

		levelWaitTimer = GetTree().CreateTimer(LEVEL_RETRY_DELAY);
		levelWaitTimer.Timeout += WaitForLevelScene;
	}

	// ================= GLOW CACHING =================
	private void CacheGlowNodes()
	{
		if (!levelReady)
			return;

		glowNodes.Clear();
		var mainScene = GetTree().CurrentScene;

		if (mainScene == null)
		{
			GD.PrintErr("[QUEST] CurrentScene is null!");
			return;
		}

		string[] questObjects =
		{
			"flashlight",
			"balut",
			"battery",
			"energydrink",
			"mang jason",
			"aling neneng",
			"aling shoneng",
			"aling marites",
			"aling marin"
		};

		foreach (string itemName in questObjects)
		{
			Node item = mainScene.FindChild(itemName, true, false);

			if (item == null)
			{
				GD.PrintErr($"[QUEST] Item not found: {itemName}");
				continue;
			}

			Node3D glow = item.FindChild("Glow", true, false) as Node3D;

			if (glow == null)
			{
				GD.PrintErr($"[QUEST] Glow node not found for: {itemName}");
				continue;
			}

			glowNodes[itemName] = glow;
			glow.Visible = false;
			GD.Print($"[QUEST] ✓ Cached glow for: {itemName}");
		}

		GD.Print($"[QUEST] Cached {glowNodes.Count} glow nodes");
	}

	// ================= AUTO UI BIND =================
	private void TryAutoBindUI()
	{
		if (!levelReady)
			return;

		if (uiBound)
			return;

		displayQuest = GetTree().Root.FindChild("questDisplay", true, false) as Label;

		if (displayQuest != null)
		{
			uiBound = true;
			GD.Print("[QUEST] UI auto-bound successfully");
			UpdateUI();
		}
		else
		{
			if (uiBindTimer != null)
				uiBindTimer.TimeLeft = 0;

			uiBindTimer = GetTree().CreateTimer(UI_BIND_RETRY_DELAY);
			uiBindTimer.Timeout += TryAutoBindUI;
		}
	}

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
		if (!levelReady)
			return;

		if (!canAdvance)
			return;

		canAdvance = false;
		currentQuest++;

		GD.Print($"[QUEST] Advanced → {GetCurrentObjective()} ({reason})");

		OnQuestChanged(currentQuest);

		if (advanceTimer != null)
			advanceTimer.TimeLeft = 0;

		advanceTimer = GetTree().CreateTimer(ADVANCE_COOLDOWN);
		advanceTimer.Timeout += () => { canAdvance = true; };
	}

	// ================= TRIGGERS =================
	public void TriggerQuestAdvance(string trigger)
	{
		if (!levelReady)
			return;

		if (string.IsNullOrEmpty(trigger))
			return;

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

			case "aling_shoneng_has_balut":
				if (currentQuest == 6)
					ProceedQuest("dialogue:aling_shoneng");
				break;

			case "aling_marites_has_balut":
				if (currentQuest == 7)
					ProceedQuest("dialogue:aling_marites");
				break;

			case "aling_marin_has_balut":
				if (currentQuest == 8)
					ProceedQuest("dialogue:aling_marin");
				break;
		}
	}

	// ================= ITEMS =================
	public void OnItemPicked(string itemName)
	{
		if (!levelReady)
			return;

		if (string.IsNullOrEmpty(itemName))
			return;

		itemName = itemName.ToLower();
		GD.Print($"[QUEST] Item detected: {itemName} | Quest: {currentQuest}");

		switch (currentQuest)
		{
			// Quest 1-2: Get flashlight & balut
			case 1:
			case 2:
				if (IsRelevantItem(itemName))
					ProceedQuest("item:picked");
				break;

			// Quest 4: Get battery & coke
			case 4:
				if (itemName.Contains("battery"))
				{
					gotBattery = true;
					if (gotDrink)
						ProceedQuest("store:complete");
				}
				else if (itemName.Contains("drink") || itemName.Contains("energydrink"))
				{
					gotDrink = true;
					if (gotBattery)
						ProceedQuest("store:complete");
				}
				break;
		}
	}

	private bool IsRelevantItem(string itemName)
	{
		return itemName.Contains("flashlight")
			|| itemName.Contains("balut")
			|| itemName.Contains("battery")
			|| itemName.Contains("drink")
			|| itemName.Contains("energydrink");
	}

	// ================= QUEST UPDATE =================
	private void OnQuestChanged(int quest)
	{
		ResetStageFlagsIfNeeded();
		UpdateUI();
		UpdateQuestGlow();

		if (quest == 5)
		{
			returnHorrorActive = true;
			GD.Print("[QUEST] HORROR MODE ACTIVATED (return path enabled)");
		}

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

	// ================= QUEST GLOW =================
	private void UpdateQuestGlow()
	{
		if (!levelReady)
			return;

		// Hide all glows
		foreach (var glow in glowNodes.Values)
		{
			if (glow != null && IsInstanceValid(glow))
				glow.Visible = false;
		}

		// No quest mapping
		if (!QuestItems.ContainsKey(currentQuest))
		{
			GD.Print($"[QUEST] No glow mapping for quest {currentQuest}");
			return;
		}

		// Show current quest glow
		foreach (string name in QuestItems[currentQuest])
		{
			if (glowNodes.TryGetValue(name, out var glow))
			{
				if (glow != null && IsInstanceValid(glow))
					glow.Visible = true;
			}
			else
			{
				GD.PrintErr($"[QUEST] Glow missing: {name}");
			}
		}
	}

	// ================= UI =================
	private void UpdateUI()
	{
		if (!levelReady)
			return;

		if (!uiBound)
			return;

		if (displayQuest == null)
			return;

		displayQuest.Text = GetCurrentObjective();
	}

	// ================= OBJECTIVES =================
	public string GetCurrentObjective()
	{
		return currentQuest switch
		{
			0 => "Kausapin si tatay",
			1 => "Kunin ang flashlight",
			2 => "Kunin ang balut",
			3 => "Bigyan ng balut si Aling Neneng",
			4 => "Kunin ang baterya at inumin sa tindahan ni Aling Neneng",
			5 => "Pumunta sa Baranggay Piti Piw Wiw Wiw at maglako ng balut",
			6 => "Bigyan ng balut si Aling Shoneng",
			7 => "Bigyan ng balut si Aling Marites",
			8 => "Bigyan ng balut si Aling Marin",
			_ => "Objective complete"
		};
	}

	public bool IsReturnHorrorActive()
	{
		return returnHorrorActive;
	}
}
