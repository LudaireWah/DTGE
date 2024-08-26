using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeGame;

/**
 * The root Control node for the DTGE game. It directly manages the following:
 *  - Basic viewport stuff like window size changes
 *  - Loading of DTGE scenes from .dscn files into a DTGECore.SceneManager
 *  - Rendering scene text
 *  - Displaying game errors
 *  
 *  It's also the parent for the NavigationButtonGrid.
 */
public partial class Game : Control
{
	private const string SETTINGS_PATH = "dtge.config";

	private MarginContainer marginContainer;
	private RichTextLabel sceneTextDisplay;
	private NavigationButtonGrid navigationButtonGrid;
	private AcceptDialog errorAcceptDialog;

	private PopupMenu filePopupMenu;

	private enum PopupMenuIds
	{
		FileSettings
	}

	private GameSettingsWindow gameSettingsWindow;

	private GameSettings gameSettings;

	private DtgeCore.Scene currentDtgeScene;

	private bool manualViewportSizeOverride;
	private Vector2 manualViewportSize;

	public override void _Ready()
	{
		this.marginContainer = GetNode<MarginContainer>("MarginContainer");
		this.sceneTextDisplay = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/SceneTextPanelContainer/SceneTextMarginContainer/SceneTextDisplay");
		this.navigationButtonGrid = GetNode<NavigationButtonGrid>("MarginContainer/VBoxContainer/NavigationButtonGridContainer");
		this.errorAcceptDialog = GetNode<AcceptDialog>("ErrorAcceptDialog");
		this.filePopupMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/File");
		this.gameSettingsWindow = GetNode<GameSettingsWindow>("GameSettingsWindow");

		Game.initializeGameDataFromFile();
		DtgeCore.GameData gameData = DtgeCore.GameData.GetGameData();

		this.navigationButtonGrid.OnOptionSelected = this.handleOptionSelected;
		this.navigationButtonGrid.NavigationGridShortcutMode = gameData.ActiveNavigationGridShortcutMode;
		this.navigationButtonGrid.ChangeGridDimensions(gameData.NavigationGridColumns, gameData.NavigationGridRows);
		this.navigationButtonGrid.SizeFlagsStretchRatio = ((float)gameData.NavigationGridRows) / 10.0f;

		this.filePopupMenu.AddItem("Settings", (int)PopupMenuIds.FileSettings);

		this.gameSettingsWindow.OnSaveSettingsAction = this.onSaveSettings;
		bool settingsLoadingSuccessful = this.tryLoadSettingsFromFile(SETTINGS_PATH);
		if (!settingsLoadingSuccessful)
		{
			this.gameSettings = new GameSettings();
			this.saveSettingsToFile(SETTINGS_PATH);
		}

		this.updateUIFromSettings();
		this.loadScenesFromFiles();
		this.updateUIFromScene();

		this.GetTree().Root.SizeChanged += this.HandleWindowSizeChanged;
		this.HandleWindowSizeChanged();
		this.manualViewportSizeOverride = false;
	}

	private void handleOptionSelected(DtgeCore.Option option)
	{
		DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();
		DtgeCore.Scene.SceneId sceneId = new DtgeCore.Scene.SceneId(option.TargetSceneId);
		DtgeCore.Scene dtgeScene;
		DtgeCore.SceneManager.GetSceneSuccessValue successValue = sceneManager.GetSceneAndSubsceneById(sceneId, out dtgeScene);

		switch(successValue)
		{
		case DtgeCore.SceneManager.GetSceneSuccessValue.Success:
			this.currentDtgeScene = dtgeScene;
			break;
		case DtgeCore.SceneManager.GetSceneSuccessValue.SceneNotFound:
			this.PopupErrorDialog("Error code DEAD_END: Option [" + option.Id + "] attempted to open Scene [" + sceneId.scene + "], which was not found");
			break;
		case DtgeCore.SceneManager.GetSceneSuccessValue.SubsceneNotFound:
			if (sceneId.subscene == null)
			{
				this.PopupErrorDialog("Error code DEAD_END: Option [" + option.Id + "] attempted to open Scene [" + sceneId.scene + "] without a subscene, which is not supported by that Scene");
			}
			else
			{
				this.PopupErrorDialog("Error code DEAD_END: Option [" + option.Id + "] attempted to open Subscene [" + sceneId.subscene + "], which was not found in Scene [" + sceneId.scene + "]");
			}
			break;
		default:
			throw new NotImplementedException();
		}

		this.updateUIFromScene();
	}

	private void updateUIFromScene()
	{
		if (this.currentDtgeScene != null)
		{
			this.sceneTextDisplay.Text = currentDtgeScene.CalculateSceneText();
			this.sceneTextDisplay.ScrollToLine(0);
			this.navigationButtonGrid.BindSceneOptionsToButtons(this.currentDtgeScene);
		}
	}

	public void HandleWindowSizeChanged()
	{
		Vector2 newViewport;
		if (this.manualViewportSizeOverride)
		{
			newViewport = this.manualViewportSize;
		}
		else
		{
			newViewport = this.GetTree().Root.GetViewport().GetVisibleRect().Size;
		}
		this.marginContainer.SetSize(newViewport);
	}

	public void SetManualViewportSize(Vector2 manualSize)
	{
		this.manualViewportSizeOverride = true;
		this.manualViewportSize = manualSize;
		HandleWindowSizeChanged();
	}

	public void PopupErrorDialog(string errorText)
	{
		this.errorAcceptDialog.DialogText = errorText;
		this.errorAcceptDialog.Popup();
	}

	public void _on_popup_menu_index_pressed(int index)
	{
		switch ((PopupMenuIds)index)
		{
		case PopupMenuIds.FileSettings:
			this.gameSettingsWindow.GameSettings = new GameSettings(this.gameSettings);
			this.gameSettingsWindow.Popup();
			break;
		}
	}

	private static void initializeGameDataFromFile()
	{
		FileAccess gameDataFile = FileAccess.Open(DtgeCore.GameData.GAME_DATA_FILE_PATH, FileAccess.ModeFlags.Read);

		if (gameDataFile != null)
		{
			string gameDataJson = gameDataFile.GetAsText();
			DtgeCore.GameData.PopulateFromJson(gameDataJson);
			gameDataFile.Close();
		}
		else
		{
			FileAccess newGameDataFile = FileAccess.Open(DtgeCore.GameData.GAME_DATA_FILE_PATH, FileAccess.ModeFlags.Write);
			if (newGameDataFile != null)
			{
				string gameDataString = JsonSerializer.Serialize<DtgeCore.GameData>(DtgeCore.GameData.GetGameData());
				newGameDataFile.StoreString(gameDataString);
				newGameDataFile.Close();
			}
		}
	}

	private void loadScenesFromFiles()
	{
		DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();
		DtgeCore.GameData gameData = DtgeCore.GameData.GetGameData();

		DirAccess sceneDirectory = DirAccess.Open(gameData.SceneDirectoryPath);
		if (sceneDirectory == null)
		{
			this.PopupErrorDialog("Error Code LABYRINTH: No scene directory found.");
			return;
		}

		DtgeCore.Scene startScene = null;

		string[] sceneFileNames = sceneDirectory.GetFiles();
		
		for (int sceneFileIndex = 0; sceneFileIndex < sceneFileNames.Length; sceneFileIndex++)
		{
			string sceneFileName = sceneFileNames[sceneFileIndex];
			string sceneFilePath = gameData.SceneDirectoryPath + "/" + sceneFileName;
			FileAccess sceneFile = FileAccess.Open(sceneFilePath, FileAccess.ModeFlags.Read);

			if (sceneFile != null)
			{
				string sceneJson = sceneFile.GetAsText();
				DtgeCore.Scene newScene = DtgeCore.Scene.Deserialize(sceneJson);
				if (newScene != null)
				{
					sceneManager.AddScene(newScene);

					if (newScene.Id == gameData.StartSceneName)
					{
						if (startScene != null)
						{
							this.PopupErrorDialog("Error code GEMINI: Two start scenes found.");
						}
						startScene = newScene;
					}
				}

				sceneFile.Close();
			}
		}

		if (startScene == null)
		{
			this.PopupErrorDialog("Eror code NONSTARTER: No start scene found.");
		}
		else
		{
			this.currentDtgeScene = startScene;
		}
	}

	public void LoadScene(DtgeCore.Scene scene)
	{
		this.currentDtgeScene = scene;
		this.updateUIFromScene();
	}

	private void onSaveSettings(GameSettings settings)
	{
		this.gameSettings = settings;
		this.updateUIFromSettings();
		this.saveSettingsToFile(SETTINGS_PATH);
	}

	private void updateUIFromSettings()
	{
		this.sceneTextDisplay.AddThemeFontSizeOverride("normal_font_size", this.gameSettings.SceneTextSize);
	}

	private bool tryLoadSettingsFromFile(string filePath)
	{
		bool success = false;
		FileAccess settingsFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

		if (settingsFile != null)
		{
			string settingsJson = settingsFile.GetAsText();
			GameSettings loadedGameSettings = JsonSerializer.Deserialize<GameSettings>(settingsJson);
			if (loadedGameSettings != null)
			{
				this.gameSettings = loadedGameSettings;
			}

			settingsFile.Close();
			success = true;
		}

		return success;
	}

	private void saveSettingsToFile(string filePath)
	{
		FileAccess settingsFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);

		if (settingsFile != null)
		{
			string settingsJson = JsonSerializer.Serialize(this.gameSettings);
			settingsFile.StoreString(settingsJson);

			settingsFile.Close();
		}
	}
}
