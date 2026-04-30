using Godot;
using System;

public partial class MainMenu : Node3D
{
	private Button playButton;
	private Button settingsButton;

	public override void _Ready()
	{
		playButton = GetNode<Button>("VBoxContainer/PlayButton");
		settingsButton = GetNode<Button>("VBoxContainer/SettingsButton");
		
		playButton.Pressed += OnPlayPressed;
		settingsButton.Pressed += OnSettingsPressed;
	}

	private void OnPlayPressed()
	{
		GetTree().ChangeSceneToFile("res://Plugins and Scenes/loading_screen.tscn");
	}

	private void OnSettingsPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/Settings.tscn");
	}
}
