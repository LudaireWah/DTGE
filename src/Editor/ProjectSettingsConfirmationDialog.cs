using System;

using Godot;

namespace DtgeEditor;

/**
 * This is the script attached to the root node for changing project settings.
 */
public partial class ProjectSettingsConfirmationDialog : ConfirmationDialog
{
	private LineEdit startSceneNameLineEdit;
	private OptionButton navigationButtonGridShortcutModeOptionButton;
	private SpinBox navigationButtonGridColumnCountSpinBox;
	private SpinBox navigationButtonGridRowCountSpinBox;

	public Action OnProjectSettingsSaved;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.startSceneNameLineEdit = this.GetNode<LineEdit>("ProjectSettingsHBoxContainer2/SettingsEntryVBoxContainer/StartSceneNameLineEdit");
		this.navigationButtonGridShortcutModeOptionButton = this.GetNode<OptionButton>("ProjectSettingsHBoxContainer2/SettingsEntryVBoxContainer/NavigationButtonGridShortcutModeOptionButton");
		this.navigationButtonGridColumnCountSpinBox = this.GetNode<SpinBox>("ProjectSettingsHBoxContainer2/SettingsEntryVBoxContainer/NavigationButtonGridColumnCountSpinBox");
		this.navigationButtonGridRowCountSpinBox = this.GetNode<SpinBox>("ProjectSettingsHBoxContainer2/SettingsEntryVBoxContainer/NavigationButtonGridRowCountSpinBox");

		this.navigationButtonGridShortcutModeOptionButton.SetItemTooltip((int)DtgeCore.GameData.NavigationGridShortcutMode.Keyboard, "The navigation grid will be mapped to the keyboard grid with 1 in the top left corner. Limits columns to 10 and rows to 4.");
		this.navigationButtonGridShortcutModeOptionButton.SetItemTooltip((int)DtgeCore.GameData.NavigationGridShortcutMode.Numeric, "The first 10 buttons in the navigation grid will be mapped to 1-0 on the keyboard. Limits columns and rows to 10.");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PopulateFromGameData()
	{
		DtgeCore.GameData gameData = DtgeCore.GameData.GetGameData();
		this.startSceneNameLineEdit.Text = gameData.StartSceneName;
		this.navigationButtonGridShortcutModeOptionButton.Selected =
			(int)gameData.ActiveNavigationGridShortcutMode;
		this.navigationButtonGridColumnCountSpinBox.Value = gameData.NavigationGridColumns;
		this.navigationButtonGridRowCountSpinBox.Value = gameData.NavigationGridRows;
	}

	public void _on_confirmed()
	{
		if(this.OnProjectSettingsSaved != null)
		{
			DtgeCore.GameData gameData = DtgeCore.GameData.GetGameData();

			gameData.StartSceneName = this.startSceneNameLineEdit.Text;
			gameData.ActiveNavigationGridShortcutMode =
				(DtgeCore.GameData.NavigationGridShortcutMode)
				this.navigationButtonGridShortcutModeOptionButton.Selected;
			gameData.NavigationGridColumns = (int)this.navigationButtonGridColumnCountSpinBox.Value;
			gameData.NavigationGridRows = (int)this.navigationButtonGridRowCountSpinBox.Value;

			this.OnProjectSettingsSaved();
		}
	}
}
