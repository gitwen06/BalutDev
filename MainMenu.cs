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
	
	public override void _Ready() {
		string[] buttonIdentity = {"%play", "%options", "%credit", "%exit"};
		
		// arrayed so less lines
		foreach (string menuBtn in buttonIdentity) {
			Button btn = GetNode<Button>(menuBtn);
			btn.MouseEntered += () => btn.GrabFocus();
			// checkers so it doesn't randomly break
			if (menuBtn == "%play") btn.Pressed += OnPlayPressed;
			if (menuBtn == "%options") btn.Pressed += OnSettingsPressed;
			if (menuBtn == "%credit") btn.Pressed += OnCreditPressed;
			if (menuBtn == "%exit") btn.Pressed += LeaveGamePressed;
		}
		GetNode<Button>("%play").GrabFocus();
		
		//refernece
		//foreach (var name in buttonIdentity) {
			//var menuBtn = GetNode<Button>(name);
			//if (menuBtn != null) {
				//_menuButtons.Add(menuBtn);
				//menuBtn.MouseEntered += () => menuBtn.GrabFocus();
			//}
		//}
		//
		//GetNode<Button>("%play").Pressed += OnPlayPressed();
		//GetNode<Button>("%options").Pressed += OnSettingsPressed();
		//GetNode<Button>("%credit").Pressed += OnCreditPressed();
		//GetNode<Button>("%exit").Pressed += LeaveGamePressed();
		//_menuButtons[0].GrabFocus();
	}

	private void OnPlayPressed() => GetTree().ChangeSceneToFile("res://Plugins and Scenes/loading_screen.scn");
	private void OnSettingsPressed() => GetTree().ChangeSceneToFile("res://scenes/Settings.scn");
	private void OnCreditPressed() => GD.Print("it doesnt exist yet shell!");
	private void LeaveGamePressed() => GetTree().Quit();
}
