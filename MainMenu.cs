using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : Node3D
{
	private Button playButton;
	private Button settingsButton;
	private Button creditButton;
	private Button exitButton;
	private List<Button> _menuButtons = new();
	private Color highlightColor = new Color("ffffffff");
	private ColorRect _cursorBox;

	public override void _Ready() {
		_cursorBox = GetNode<ColorRect>("%cursorBox");
		string[] buttonIdentity = {"%play", "%options", "%credit", "%exit"};
		
		// arrayed so less lines
		foreach (string menuBtn in buttonIdentity) {
			Button btn = GetNode<Button>(menuBtn);
			_menuButtons.Add(btn);
			
			btn.MouseEntered += () => btn.GrabFocus();
			btn.FocusEntered += () => UpdateHighlight(btn);
			
			// checkers so it doesn't randomly break
			if (menuBtn == "%play") btn.Pressed += OnPlayPressed;
			if (menuBtn == "%options") btn.Pressed += OnSettingsPressed;
			if (menuBtn == "%credit") btn.Pressed += OnCreditPressed;
			if (menuBtn == "%exit") btn.Pressed += LeaveGamePressed;
		}
		GetNode<Button>("%play").GrabFocus();
	}

	private void OnPlayPressed() => GetTree().ChangeSceneToFile("res://Plugins and Scenes/loading_screen.scn");
	private void OnSettingsPressed() => GetTree().ChangeSceneToFile("res://scenes/Settings.scn");
	private void OnCreditPressed() => GD.Print("it doesnt exist yet shell!");
	private void LeaveGamePressed() => GetTree().Quit();
	
	private void UpdateHighlight(Button selectedBtn) {
		Tween cursorTween = CreateTween().SetParallel(true);
		
		cursorTween.TweenProperty(_cursorBox, "global_position:y", selectedBtn.GlobalPosition.Y, 0.1f)
			 .SetTrans(Tween.TransitionType.Sine);
			
		foreach (Button b in _menuButtons) {
			if (b == selectedBtn) {
				cursorTween.TweenProperty(b, "modulate", highlightColor, 0.1f).SetTrans(Tween.TransitionType.Sine);
				cursorTween.TweenProperty(b, "position:x", 30.0f, 0.1f).SetTrans(Tween.TransitionType.Sine);
				// cursorTween.TweenProperty(b, "scale", new Vector2(1.1f, 1.1f), 0.1f).SetTrans(Tween.TransitionType.Sine);
			} else {
				cursorTween.TweenProperty(b, "modulate", Colors.White, 0.1f).SetTrans(Tween.TransitionType.Sine);
				cursorTween.TweenProperty(b, "position:x", 0.0f, 0.1f).SetTrans(Tween.TransitionType.Sine);
				// cursorTween.TweenProperty(b, "scale", new Vector2(1.0f, 1.0f), 0.1f).SetTrans(Tween.TransitionType.Sine);
			}
		}
	}
}
