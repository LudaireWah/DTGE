using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeGame;

public partial class GameSettingsWindow : Window
{
	MarginContainer rootMarginContainer;
	LineEdit sceneTextSizeLineEdit;

	public GameSettings GameSettings;

	public Action<GameSettings> OnSaveSettingsAction;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.rootMarginContainer = GetNode<MarginContainer>("RootMarginContainer");
		this.sceneTextSizeLineEdit = GetNode<LineEdit>("RootMarginContainer/RootVBoxContainer/SettingsHBoxContainer/SettingValues/SceneTextSizeLineEdit");

		this.SizeChanged += this.HandleWindowSizeChanged;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void HandleWindowSizeChanged()
	{
		Rect2 newViewport = this.GetViewport().GetVisibleRect();
		this.rootMarginContainer.SetSize(newViewport.Size);
	}

	public void _on_about_to_popup()
	{
		if (this.GameSettings != null)
		{
			this.sceneTextSizeLineEdit.Text = this.GameSettings.SceneTextSize.ToString();
		}
	}

	public void _on_close_requested()
	{
		this.Hide();
	}

	public void _on_save_button_pressed()
	{
		if (this.OnSaveSettingsAction != null)
		{
			this.OnSaveSettingsAction(this.GameSettings);
		}
		this.Hide();
	}

	public void _on_cancel_button_pressed()
	{
		this.Hide();
	}

	public void _on_scene_text_size_line_edit_text_submitted()
	{
		if (!this.sceneTextSizeLineEdit.Text.IsValidInt())
		{
			this.sceneTextSizeLineEdit.Text = this.GameSettings.SceneTextSize.ToString();
		}
		else
		{
			this.GameSettings.SceneTextSize = this.sceneTextSizeLineEdit.Text.ToInt();
		}
	}
}
