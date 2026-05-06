using Godot;
using System;

public partial class LoadingScreen : Control
{
	private TextureProgressBar progressBar;
	private Label loadingLabel;
	private float loadingProgress = 0f;

	public override void _Ready()
	{
		progressBar = GetNode<TextureProgressBar>("CanvasLayer/ColorRect/MarginContainer/VBoxContainer/HBoxContainer/TextureProgressBar");
		
		progressBar.Value = 0;
	}

	public override void _Process(double delta)
	{
		// Simulate loading progress
		if(progressBar == null) return;
		loadingProgress += (float)delta * 30; // Adjust speed
		progressBar.Value = loadingProgress;

		// Once "loaded", switch to game scene
		if(loadingProgress >= 100) {
			GetTree().ChangeSceneToFile("res://level/level.scn"); 
		}
	}
}
