using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : Node3D
{
	// --- Main Stuff ---
	private List<Button> _menuButtons = new();
	private Color highlightColor = new Color("ffffffff");
	private Tween cursorTween;
	private ColorRect _cursorBox;
	private Control optionTab;
	private Control creditTab;
	private string currTab = "main";
	
	private Control loadingOverlay;
	private TextureProgressBar loadingBar;
	private float loadVal = 0f;
	private bool isLoading = false;
	
	// --- Audio Stuff ---
	private MarginContainer mainMenuUI;
	private AudioStreamPlayer menuMove;
	private AudioStreamPlayer menuSelect;
	
	
	public override void _Ready() {
		_cursorBox = GetNode<ColorRect>("%cursorBox");
		menuMove = GetNode<AudioStreamPlayer>("%buttonMoveSFX");
		menuSelect = GetNode<AudioStreamPlayer>("%buttonSelectSFX");
		
		optionTab = GetNode<Control>("%optionsTab");
		creditTab = GetNode<Control>("%creditsTab");
		mainMenuUI = GetNode<MarginContainer>("%mainMenuUI");
		loadingOverlay = GetNode<Control>("%loadingScreen");
		loadingBar = GetNode<TextureProgressBar>("%loadingPlay");
		loadingOverlay.Hide();
		
		string[] buttonIdentity = {"%play", "%options", "%credit", "%exit"};
		
		// arrayed so less lines
		foreach (string menuBtn in buttonIdentity) {
			Button btn = GetNode<Button>(menuBtn);
			_menuButtons.Add(btn);
			
			btn.MouseEntered += () => {
				if (!btn.HasFocus()) {
					btn.GrabFocus();
					}
			};
			btn.FocusEntered += () => UpdateHighlight(btn);
			
			// checkers so it doesn't randomly break
			if (menuBtn == "%play") btn.Pressed += OnPlayPressed;
			if (menuBtn == "%options") btn.Pressed += OnOptionPressed;
			if (menuBtn == "%credit") btn.Pressed += OnCreditPressed;
			if (menuBtn == "%exit") btn.Pressed += LeaveGamePressed;
		}
		GetNode<Button>("%play").GrabFocus();
	}

	private void OnPlayPressed() {
		menuSelect.Play();
		mainMenuUI.Hide();
		_cursorBox.Hide();
		loadingOverlay.Show();
		
		var loadingAnim = GetNode<AnimationPlayer>("%loadingScreenAnim");
		loadingAnim.Play("loadingIN");
		isLoading = true;
	}
	private void OnOptionPressed() {
		menuSelect.Play();
		mainMenuUI.Hide();
		optionTab.Show();
		currTab = "options";
		
		var optionAnim = GetNode<AnimationPlayer>("%optionsOpenClose");
		optionAnim.Play("optionsTab");
		
		GetNode<Button>("%muteButton").GrabFocus();
	}
	private void OnCreditPressed() {
		menuSelect.Play();
		mainMenuUI.Hide();
		creditTab.Show();
		currTab = "credits";
		// i hate this
		var creditAnim = GetNode<AnimationPlayer>("%creditsOpenClose");
		creditAnim.Play("creditsOpenClose");
	}
	// Covers function for exiting a menuTab
	private async void OnBackPressed() {
		menuSelect.Play();
		if (currTab == "options") {
			optionTab.Hide();
			GetNode<Button>("%options").GrabFocus();
		} else if (currTab == "credits") {
			creditTab.Hide();
			GetNode<Button>("%credit").GrabFocus();
		}
		currTab = "main";
		mainMenuUI.Show();
	}
	private void LeaveGamePressed() => GetTree().Quit();
	
	// --- HIghlight/Cursor Function ---
	private void UpdateHighlight(Button selectedBtn) {
		if (menuMove != null && !menuMove.Playing) {
			menuMove.Play();
		} if (cursorTween != null && cursorTween.IsValid()) {
			cursorTween.Kill();
		}
		cursorTween = CreateTween().SetParallel(true);
		// (Button Y) + (Half of Button Height) - (Half of Cursor Height)
		float centerY = selectedBtn.GlobalPosition.Y + (selectedBtn.Size.Y / 2) - (_cursorBox.Size.Y / 2); 
		//formula to center
		cursorTween.TweenProperty(_cursorBox, "global_position:y", centerY, 0.1f)
		.SetTrans(Tween.TransitionType.Sine);
			
		foreach (Button b in _menuButtons) {
			if (b == selectedBtn) {
				cursorTween.TweenProperty(b, "modulate", highlightColor, 0.1f).SetTrans(Tween.TransitionType.Sine);
				cursorTween.TweenProperty(b, "position:x", 30.0f, 0.1f).SetTrans(Tween.TransitionType.Sine);
				cursorTween.TweenProperty(b, "scale", new Vector2(1.1f, 1.1f), 0.1f).SetTrans(Tween.TransitionType.Sine);
			} else {
				cursorTween.TweenProperty(b, "modulate", Colors.White, 0.1f).SetTrans(Tween.TransitionType.Sine);
				cursorTween.TweenProperty(b, "position:x", 0.0f, 0.1f).SetTrans(Tween.TransitionType.Sine);
				cursorTween.TweenProperty(b, "scale", new Vector2(1.0f, 1.0f), 0.1f).SetTrans(Tween.TransitionType.Sine);
			}
		}
	}
	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel") && currTab != "main") {
			OnBackPressed();
		}
	}
	public override void _Process(double delta) {
		if (!isLoading) return;
		loadVal += (float)delta * 30f;
		loadingBar.Value = loadVal;
		if (loadVal >= 100) {
			isLoading = false;
			GetTree().ChangeSceneToFile("res://level/level.scn");
		}
	}
}
