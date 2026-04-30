using Godot;
using System;

public partial class LoadingScreen : Control
{
	private ProgressBar progressBar;
	private Label loadingLabel;
	private float loadingProgress = 0f;

	public override void _Ready()
	{
		progressBar = GetNode<ProgressBar>("ProgressBar");
		
		progressBar.Value = 0;
	}

	public override void _Process(double delta)
	{
		// Simulate loading progress
		loadingProgress += (float)delta * 30; // Adjust speed
		progressBar.Value = loadingProgress;

		// Once "loaded", switch to game scene
		if(loadingProgress >= 100)
		{
			GetTree().ChangeSceneToFile("res://level/level.tscn"); 
		}
	}
}
