using Godot;
using System;

public partial class GameOver : Control
{
	public Button retryButton;
	
	public override void _Ready()
	{
		retryButton = GetNode<Button>("Button");
		retryButton.Pressed += OnRetryPressed;
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	private void OnRetryPressed()
	{
		GD.Print("PRESSED BUTTON AYAW LANG GUMANA");
		GetTree().Paused = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().ReloadCurrentScene();
	}
}
