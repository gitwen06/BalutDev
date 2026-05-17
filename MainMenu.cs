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
	private TextureRect gameLogo;
	private Control optionTab;
	private Control creditTab;
	private string currTab = "main";
	
	//Monster model (for tweaking purposes)
	private Skeleton3D monster;
	private AnimationPlayer tweakingAnim;
	
	//private Control loadingOverlay;
	//private TextureProgressBar loadingBar;
	
	// --- Opening Loading Bar  ---
	private CanvasLayer openGameLoadingScreen;
	private Control loadingBackground;
	private ProgressBar openingLoadingBar;
	private bool isOpening = true;
	private bool isLoading = false;
	private float loadVal = 0f;
	private AnimationPlayer newGameAnimPlayer;
	
	// --- Audio Stuff ---
	private MarginContainer mainMenuUI;
	private AudioStreamPlayer menuMove;
	private AudioStreamPlayer menuSelect;
	private AudioStreamPlayer menuMusic;
	
	public override void _Ready() {
		_cursorBox = GetNode<ColorRect>("%cursorBox");
		gameLogo = GetNode<TextureRect>("%companyLogo");
		menuMove = GetNode<AudioStreamPlayer>("%buttonMoveSFX");
		menuSelect = GetNode<AudioStreamPlayer>("%buttonSelectSFX");
		menuMusic = GetNode<AudioStreamPlayer>("%mainMenuMusic");
		
		optionTab = GetNode<Control>("%optionsTab");
		creditTab = GetNode<Control>("%creditsTab");
		mainMenuUI = GetNode<MarginContainer>("%mainMenuUI");
		
		//loadingOverlay = GetNode<Control>("%loadingScreen");
		//loadingBar = GetNode<TextureProgressBar>("%loadingPlay");
		//loadingOverlay.Hide();
		
		openGameLoadingScreen = GetNode<CanvasLayer>("%openGameLoadingScreen");
		loadingBackground = GetNode<Control>("%loadingBackground");
		openingLoadingBar = GetNode<ProgressBar>("%loadingProgressGame");
		newGameAnimPlayer = GetNode<AnimationPlayer>("%newGameAnim");
		
		menuMusic.Play();
		
		//openGameLoadingScreen.Visible = true;
		openGameLoadingScreen.Show();
		loadingBackground.Modulate = new Color(1, 1, 1, 1); // Fully visible
		mainMenuUI.Modulate = new Color(1, 1, 1, 0);
		// mainMenuUI.Hide();
		isOpening = true;
		
		openingLoadingBar.Value = 0;
		_cursorBox.Hide();
		gameLogo.Hide();
		openGameLoadingScreen.Call("update_tip");
		if (newGameAnimPlayer != null) {
			newGameAnimPlayer.Play("newGame");
		}
		
		monster = GetNode<Skeleton3D>("Skeleton3D");
		PlayMonsterAnimation();
		
		string[] buttonIdentity = {"%play", "%options", "%credit", "%exit"};
		
		mainMenuUI.Modulate = new Color(1, 1, 1, 0);
		if (_cursorBox != null) _cursorBox.Hide();
		if (gameLogo != null) gameLogo.Hide();
		if (newGameAnimPlayer != null) {
			newGameAnimPlayer.Play("newGame");
		}
		openGameLoadingScreen.Show();
		isOpening = true;
		
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
	private async void PlayMonsterAnimation() {
		var animPlayer = monster.GetNode<AnimationPlayer>("AnimationPlayer");
		while (true) {
			// Play the animation
			animPlayer.Play("tweaking");
			
			// Wait for animation to finish
			await ToSignal(animPlayer, "animation_finished");
			
			// Random delay between 1-5 seconds
			float randomDelay = (float)GD.Randf() * 4.0f + 1.0f; // 1.0 to 5.0
			await ToSignal(GetTree().CreateTimer(randomDelay), "timeout");
		}
	}
	private async void OnPlayPressed() { 
		if (isLoading || isOpening) return;
		if (menuSelect != null) menuSelect.Play();
		if (newGameAnimPlayer != null) {
			newGameAnimPlayer.Play("newGame");
			await ToSignal(newGameAnimPlayer, "animation_finished");
		}
		loadVal = 0;
		openingLoadingBar.Value = 0;
		loadingBackground.Modulate = new Color(1, 1, 1, 0); 
		openGameLoadingScreen.Show();
		Tween fadeInTween = CreateTween();
		fadeInTween.TweenProperty(loadingBackground, "modulate:a", 1.0f, 0.5f);
		// mainMenuUI.Hide();

		if (openGameLoadingScreen.HasMethod("update_tip")) {
			openGameLoadingScreen.Call("update_tip");
		}
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
		// The "Button's Y Axis" + "Half of Button Height" - "Half of The Cursor Height".
		// To equal the spacing for the cursor.
		float centerY = selectedBtn.GlobalPosition.Y + (selectedBtn.Size.Y / 2) - (_cursorBox.Size.Y / 2); 
		//formula to center
		cursorTween.TweenProperty(_cursorBox, "global_position:y", centerY, 0.1f)
		.SetTrans(Tween.TransitionType.Sine);
			
		foreach (Button b in _menuButtons) {
			if (b == selectedBtn) {
				cursorTween.TweenProperty(b, "modulate", highlightColor, 0.1f).SetTrans(Tween.TransitionType.Sine);
				cursorTween.TweenProperty(b, "position:x", 30.0f, 0.1f).SetTrans(Tween.TransitionType.Sine);
				//cursorTween.TweenProperty(b, "scale", new Vector2(1.1f, 1.1f), 0.1f).SetTrans(Tween.TransitionType.Sine);
			} else {
				cursorTween.TweenProperty(b, "modulate", Colors.White, 0.1f).SetTrans(Tween.TransitionType.Sine);
				cursorTween.TweenProperty(b, "position:x", 0.0f, 0.1f).SetTrans(Tween.TransitionType.Sine);
				//cursorTween.TweenProperty(b, "scale", new Vector2(1.0f, 1.0f), 0.1f).SetTrans(Tween.TransitionType.Sine);
			}
		}
	}
	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel") && currTab != "main") {
			OnBackPressed();
		}
	}
	
	public override void _Process(double delta) {
		if (isOpening) {
			openingLoadingBar.Value += delta * 50f;
			if (openingLoadingBar.Value >= 100) {
				isOpening = false; // Stop the loop immediately
				FinishOpeningSequence();
			}
			return;
		}
		if (isLoading) {
			loadVal += (float)delta * 30f;
			openingLoadingBar.Value = loadVal;
			if (loadVal >= 100) {
				isLoading = false;
				GetTree().ChangeSceneToFile("res://level/level.scn");
			}
		}
	}
	private void FinishOpeningSequence() {
		isOpening = false;
		mainMenuUI.Show(); 
		Tween fadeTween = CreateTween().SetParallel(true);
		fadeTween.TweenProperty(loadingBackground, "modulate:a", 0.0f, 0.5f);
		fadeTween.TweenProperty(mainMenuUI, "modulate:a", 1.0f, 0.5f);

		fadeTween.Finished += () => {
			openGameLoadingScreen.Hide(); // Hide the whole layer when done
			if (_cursorBox != null) _cursorBox.Show();
			if (gameLogo != null) gameLogo.Show();
			GetNode<Button>("%play").GrabFocus();
		};
	}
}
