using Godot;
using System;

public partial class QuestSystem : Node
{
	public static QuestSystem Instance;

	private Node3D outlinedObject;
	private Label displayQuest;

	private bool uiBound = false;

	public int currentQuest = 0;

	private bool gotBattery = false;
	private bool gotDrink = false;

	private bool canAdvance = true;

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

		// 🔥 auto try bind UI after scene loads
		CallDeferred(nameof(TryAutoBindUI));
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
			GetTree().CreateTimer(0.2).Timeout += TryAutoBindUI;
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

		GetTree().CreateTimer(0.15).Timeout += () =>
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

		if (itemName.Contains("flashlight") && currentQuest == 1)
			ProceedQuest("item:flashlight");

		else if (itemName.Contains("balut") && currentQuest == 2)
			ProceedQuest("item:balut");

		else if (itemName.Contains("battery") && currentQuest == 4)
		{
			gotBattery = true;
			CheckStore();
		}
		else if (itemName.Contains("drink") && currentQuest == 4)
		{
			gotDrink = true;
			CheckStore();
		}
	}

	private void CheckStore()
	{
		if (gotBattery && gotDrink)
			ProceedQuest("store:complete");
	}

	// ================= QUEST UPDATE =================
	private void OnQuestChanged(int quest)
	{
		RemoveOutline();
		ResetStageFlagsIfNeeded();
		UpdateUI();

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

	// ================= OUTLINE =================
	public void ApplyOutline(Node3D obj)
	{
		outlinedObject = obj;
	}

	public void RemoveOutline()
	{
		outlinedObject = null;
	}
}
