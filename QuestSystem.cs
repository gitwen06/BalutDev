using Godot;
using System;
using System.Collections.Generic;

public partial class QuestSystem : Node
{
	public static QuestSystem Instance;

	private CanvasLayer waypointCanvas;
	private Control waypointMarker;
	private Label waypointLabel;
	private Label questObjectiveLabel;

	private bool uiBound = false;
	private bool levelReady = false;

	public int currentQuest = 0;
	public bool returnHorrorActive = false;

	private bool gotBattery = false;
	private bool gotDrink = false;

	private const float ADVANCE_COOLDOWN = 0.15f;
	private const float UI_BIND_RETRY_DELAY = 0.2f;
	private const float LEVEL_RETRY_DELAY = 0.5f;

	private SceneTreeTimer advanceTimer;
	private SceneTreeTimer uiBindTimer;
	private SceneTreeTimer levelWaitTimer;

	private Camera3D playerCamera;
	private Viewport viewport;

	// ================= QUEST WAYPOINT COORDINATES =================
	private static readonly Dictionary<int, Vector3> QuestWaypoints = new()
	{
		{ 0, new Vector3(211.843f, 2.5f, 76.982f) },              // Talk to mang jason
		{ 1, new Vector3(209.42f, 5.185f, 77.739f) },              // Get flashlight (was -3.815, now +6.185)
		{ 2, new Vector3(209.651f, 4.988f, 77.418f) },             // Get balut (was -4.012, now +5.988)
		{ 3, new Vector3(212.993f, 11.196f, -25.127f) },           // Aling neneng
		{ 4, new Vector3(209.742f, 3.874f, -31.135f) },            // Get battery (was -4.126, now +5.874)
		{ 5, new Vector3(161.067f, 7.529f, 33.426f) },            // Go to baranggay
		{ 6, new Vector3(8.596f, 3.068f, -19.498f) },             // Aling shoneng
		{ 7, new Vector3(111.79f, 2.447f, 56.359f) },             // Aling marites
		{ 8, new Vector3(-15.05f, -4.389f,-5.383f) },              // Aling marin
		{ 9, new Vector3(27.304f, 7.718f, -15.224f) },            // Maglako ulit sa bahay ni manong rafael(RAFAEL?????????????)
		{ 10, new Vector3(12.021f, -3.792f, 53.391f) },             // Get key under rag (was -3.792, now +6.208)
		{ 11, new Vector3(40.903f, 10.0f, 155.785f) },             // Manong rafael
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

	public override void _Process(double delta)
	{
		if (!levelReady || waypointMarker == null || playerCamera == null)
			return;

		UpdateWaypointPosition();
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

		viewport = GetViewport();
		playerCamera = GetTree().Root.FindChild("Camera3D", true, false) as Camera3D;

		if (playerCamera == null)
		{
			GD.PrintErr("[QUEST] Camera3D not found!");
			return;
		}

		TryAutoBindUI();
		CreateWaypointMarker();
		UpdateQuestWaypoint();
		UpdateUI();
	}

	private void RetryLevelWait()
	{
		if (levelWaitTimer != null)
			levelWaitTimer.TimeLeft = 0;

		levelWaitTimer = GetTree().CreateTimer(LEVEL_RETRY_DELAY);
		levelWaitTimer.Timeout += WaitForLevelScene;
	}

	// ================= CREATE 2D WAYPOINT MARKER =================
	private void CreateWaypointMarker()
	{
		if (waypointMarker != null)
			return;

		// Create canvas layer for UI overlay
		waypointCanvas = new CanvasLayer();
		waypointCanvas.Name = "QuestWaypointCanvas";
		waypointCanvas.Layer = 100;
		GetTree().Root.AddChild(waypointCanvas);

		// Create marker container
		waypointMarker = new Control();
		waypointMarker.Name = "QuestMarker";
		waypointMarker.CustomMinimumSize = new Vector2(64, 64);
		waypointMarker.AnchorLeft = 0.5f;
		waypointMarker.AnchorTop = 0.5f;
		waypointMarker.OffsetLeft = -32;
		waypointMarker.OffsetTop = -32;
		waypointCanvas.AddChild(waypointMarker);

		// Create marker background
		var panelStyleBox = new StyleBoxFlat();
		panelStyleBox.BgColor = new Color(0, 1, 1, 0.9f);  // Cyan
		panelStyleBox.BorderColor = new Color(0, 0.7f, 0.7f, 1);
		panelStyleBox.BorderWidthLeft = 2;
		panelStyleBox.BorderWidthRight = 2;
		panelStyleBox.BorderWidthTop = 2;
		panelStyleBox.BorderWidthBottom = 2;

		var markerPanel = new Panel();
		markerPanel.AddThemeStyleboxOverride("panel", panelStyleBox);
		markerPanel.CustomMinimumSize = new Vector2(64, 64);
		waypointMarker.AddChild(markerPanel);

		// Create label for marker
		waypointLabel = new Label();
		waypointLabel.Text = "►";
		waypointLabel.AddThemeColorOverride("font_color", Colors.Black);
		waypointLabel.AddThemeFontSizeOverride("font_size", 32);
		waypointLabel.HorizontalAlignment = HorizontalAlignment.Center;
		waypointLabel.VerticalAlignment = VerticalAlignment.Center;
		waypointLabel.CustomMinimumSize = new Vector2(64, 64);
		waypointMarker.AddChild(waypointLabel);

		GD.Print("[QUEST] 2D Waypoint marker created");
	}

	// ================= UPDATE WAYPOINT POSITION =================
	private void UpdateWaypointPosition()
	{
		if (waypointMarker == null || playerCamera == null || viewport == null)
			return;

		if (!QuestWaypoints.TryGetValue(currentQuest, out var targetWorldPos))
		{
			waypointMarker.Visible = false;
			return;
		}

		waypointMarker.Visible = true;

		// Convert to camera space to check if behind camera
		Vector3 targetCameraPos = playerCamera.GlobalTransform.AffineInverse() * targetWorldPos;
		bool isBehindCamera = targetCameraPos.Z > 0;

		// Project target position to screen space
		Vector2 screenPos = playerCamera.UnprojectPosition(targetWorldPos);

		// Get viewport size
		Vector2 viewportSize = viewport.GetVisibleRect().Size;
		var screenPos2D = screenPos;

		// Add offset if behind camera
		if (isBehindCamera)
		{
			// Flip position to opposite side of screen
			screenPos2D.X = viewportSize.X - screenPos2D.X;
			screenPos2D.Y = viewportSize.Y - screenPos2D.Y;
		}

		// Clamp to screen edges with padding
		float padding = 80;
		screenPos2D.X = Mathf.Clamp(screenPos2D.X, padding, viewportSize.X - padding);
		screenPos2D.Y = Mathf.Clamp(screenPos2D.Y, padding, viewportSize.Y - padding);

		// Set position
		waypointMarker.Position = screenPos2D;

		// Rotate arrow to point to target
		if (!isBehindCamera && (targetCameraPos.Z > 0 && targetCameraPos.Z < 1000))
		{
			Vector3 dirToTarget = (targetWorldPos - playerCamera.GlobalPosition).Normalized();
			float angle = Mathf.Atan2(dirToTarget.X, dirToTarget.Z);
			waypointMarker.Rotation = angle;
		}
	}

	// ================= UPDATE QUEST WAYPOINT =================
	private void UpdateQuestWaypoint()
	{
		if (!levelReady || waypointMarker == null)
			return;

		if (!QuestWaypoints.ContainsKey(currentQuest))
		{
			waypointMarker.Visible = false;
			return;
		}

		waypointMarker.Visible = true;
		GD.Print($"[QUEST] Waypoint set to quest {currentQuest}");
	}

	// ================= AUTO UI BIND =================
	private void TryAutoBindUI()
	{
		if (!levelReady)
			return;

		if (uiBound)
			return;

		questObjectiveLabel = GetTree().Root.FindChild("questDisplay", true, false) as Label;

		if (questObjectiveLabel != null)
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
		questObjectiveLabel = label;
		uiBound = true;
		GD.Print("[QUEST] UI manually bound");
		UpdateUI();
	}

	// ================= RESET =================
	private void ResetState()
	{
		gotBattery = false;
		gotDrink = false;
	}

	// ================= CORE =================
	public void ProceedQuest(string reason = "")
	{
		if (!levelReady)
			return;

		currentQuest++;

		GD.Print($"[QUEST] Advanced → {GetCurrentObjective()} ({reason})");

		OnQuestChanged(currentQuest);

		if (advanceTimer != null)
			advanceTimer.TimeLeft = 0;

		advanceTimer = GetTree().CreateTimer(ADVANCE_COOLDOWN);
	}

	// ================= TRIGGERS =================
	public void TriggerQuestAdvance(string trigger)
	{
		GD.Print($"[QUEST] TriggerQuestAdvance called with trigger: {trigger}");
		GD.Print($"[QUEST] levelReady: {levelReady}, trigger is null: {string.IsNullOrEmpty(trigger)}");
		
		if (!levelReady)
		{
			GD.PrintErr($"[QUEST] levelReady is FALSE! Cannot advance quest!");
			return;
		}

		if (string.IsNullOrEmpty(trigger))
			return;

		GD.Print($"[QUEST] Processing trigger: {trigger}");

		switch (trigger)
		{
			case "mang_jason_talk":
				GD.Print("[QUEST] Matched: mang_jason_talk");
				if (currentQuest == 0)
					ProceedQuest("dialogue:mang_jason");
				break;

			case "aling_neneng_has_balut":
				GD.Print("[QUEST] Matched: aling_neneng_has_balut");
				if (currentQuest == 3)
					ProceedQuest("dialogue:aling_neneng");
				break;
			
			case "monster_walk_by_ended":
				GD.Print("[QUEST] Matched: monster_walk_by_ended");
				GD.Print($"[QUEST] Current quest: {currentQuest}");
				if (currentQuest == 5)
				{
					GD.Print("[QUEST] Quest is 4! Proceeding!");
					ProceedQuest("dialogue:monster_walkby");
				}
				else
				{
					GD.PrintErr($"[QUEST] Expected quest 4, got {currentQuest}!");
				}
				break;

			case "aling_shoneng_has_balut":
				GD.Print("[QUEST] Matched: aling_shoneng_has_balut");
				if (currentQuest == 6)
					ProceedQuest("dialogue:aling_shoneng");
				break;

			case "aling_marites_has_balut":
				GD.Print("[QUEST] Matched: aling_marites_has_balut");
				if (currentQuest == 7)
					ProceedQuest("dialogue:aling_marites");
				break;

			case "aling_marin_has_balut":
				GD.Print("[QUEST] Matched: aling_marin_has_balut");
				if (currentQuest == 8)
					ProceedQuest("dialogue:aling_marin");
				break;

			case "manong_rafael_call": // call for touching rear 3d ok
			GD.Print("[QUEST] Matched: manong_rafael_call");
			GD.Print($"[QUEST] Current quest: {currentQuest}");
			if (currentQuest == 9)
			{
				GD.Print("[QUEST] Quest is 9! Proceeding to quest 10 (get key)!");
				ProceedQuest("dialogue:manong_rafael_call");
			}
			else
			{
				GD.PrintErr($"[QUEST] Expected quest 9, got {currentQuest}!");
			}
			break;
			
			case "manong_rafael_has_balut": // rafael del rosario gets balut cool ig
			GD.Print("[QUEST] Matched: manong_rafael_has_balut");
			GD.Print($"[QUEST] Current quest: {currentQuest}");
			if (currentQuest == 11)
			{
				GD.Print("[QUEST] Quest is 11! Proceeding!");
				ProceedQuest("dialogue:manong_rafael_balut");
			}
			else
			{
				GD.PrintErr($"[QUEST] Expected quest 11, got {currentQuest}!");
			}
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

		// Mark item as picked in GlobalVariables
		GlobalVariables.Instance.MarkItemPickedUp(itemName);

		switch (currentQuest)
		{
			// Quest 1: Get flashlight
			case 1:
				if (GlobalVariables.Instance.hasFlashlight)
				{
					GD.Print("[QUEST] Flashlight obtained!");
					ProceedQuest("item:picked");
				}
				break;

			// Quest 2: Get balut
			case 2:
				if (GlobalVariables.Instance.hasBalut)
				{
					GD.Print("[QUEST] Balut obtained!");
					ProceedQuest("item:picked");
				}
				break;

			// Quest 4: Get battery & drink (both required)
			case 4:
				if (GlobalVariables.Instance.hasBattery && GlobalVariables.Instance.hasDrink)
				{
					GD.Print("[QUEST] Both battery and drink obtained!");
					ProceedQuest("store:complete");
				}
				break;

			// Quest 10: Get key
			case 10:
				if (GlobalVariables.Instance.hasKey)
				{
					GD.Print("[QUEST] Key obtained!");
					ProceedQuest("item:key_picked");
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
		UpdateQuestWaypoint();

		if (quest == 5)
		{
			returnHorrorActive = true;
		}
		
		CheckForAlreadyPickedItems(quest);
		GD.Print($"[QUEST UPDATED] → {GetCurrentObjective()}");
	}
	
	private void CheckForAlreadyPickedItems(int quest)
	{
		var g = GlobalVariables.Instance;
		switch (quest)
		{
			case 1:
				if (g.hasFlashlight)
				{
					GD.Print("[QUEST] Flashlight already picked! Auto-advancing...");
					currentQuest++;
					OnQuestChanged(currentQuest);
				}
				break;

			case 2:
				if (g.hasBalut)
				{
					GD.Print("[QUEST] Balut already picked! Auto-advancing...");
					currentQuest++;
					OnQuestChanged(currentQuest);
				}
				break;

			case 4:
				if (g.hasBattery && g.hasDrink)
				{
					GD.Print("[QUEST] Battery and drink already picked! Auto-advancing...");
					currentQuest++;
					OnQuestChanged(currentQuest);
				}
				break;

			case 10:
				if (g.hasKey)
				{
					GD.Print("[QUEST] Key already picked! Auto-advancing...");
					currentQuest++;
					OnQuestChanged(currentQuest);
				}
				break;
		}
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
		if (!levelReady)
			return;

		if (!uiBound)
			return;

		if (questObjectiveLabel == null)
			return;

		questObjectiveLabel.Text = GetCurrentObjective();
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
			9 => "Maglako ulit ng balut",
			10 => "Kunin ang susi sa ilalim ng basahan",
			11 => "Bigyan ng balut si Manong Rafael",
			_ => "Objective complete"
		};
	}

	public bool IsReturnHorrorActive()
	{
		return returnHorrorActive;
	}
}
