using System;
using System.Text.Json;

using Godot;

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

	private enum PopupMenuIds
	{
		FileSettings
	}

	private MarginContainer marginContainer;
	private PanelContainer sceneTextPanelContainer;
	private RichTextLabel sceneTextDisplay;
	private TextureRect leftTextureRect;
	private TextureRect rightTextureRect;
	private TextureRect topTextureRect;
	private TextureRect bottomTextureRect;
	private NavigationButtonGrid navigationButtonGrid;
	private AcceptDialog errorAcceptDialog;
	private PopupMenu filePopupMenu;
	private GameSettingsWindow gameSettingsWindow;

	private GameSettings gameSettings;

	private DtgeCore.Scene currentDtgeScene;

	private bool manualViewportSizeOverride;
	private Vector2 manualViewportSize;

	public override void _Ready()
	{
		this.marginContainer = this.GetNode<MarginContainer>("MarginContainer");
		this.sceneTextPanelContainer = this.GetNode<PanelContainer>("MarginContainer/VBoxContainer/SceneBodyVBoxContainer/SceneBodyHBoxContainer/SceneTextPanelContainer");
		this.sceneTextDisplay = this.GetNode<RichTextLabel>("MarginContainer/VBoxContainer/SceneBodyVBoxContainer/SceneBodyHBoxContainer/SceneTextPanelContainer/SceneTextMarginContainer/SceneTextDisplay");
		this.leftTextureRect = this.GetNode<TextureRect>("MarginContainer/VBoxContainer/SceneBodyVBoxContainer/SceneBodyHBoxContainer/LeftTextureRect");
		this.rightTextureRect = this.GetNode<TextureRect>("MarginContainer/VBoxContainer/SceneBodyVBoxContainer/SceneBodyHBoxContainer/RightTextureRect");
		this.topTextureRect = this.GetNode<TextureRect>("MarginContainer/VBoxContainer/SceneBodyVBoxContainer/TopTextureRect");
		this.bottomTextureRect = this.GetNode<TextureRect>("MarginContainer/VBoxContainer/SceneBodyVBoxContainer/BottomTextureRect");
		this.navigationButtonGrid = this.GetNode<NavigationButtonGrid>("MarginContainer/VBoxContainer/NavigationButtonGridContainer");
		this.errorAcceptDialog = this.GetNode<AcceptDialog>("ErrorAcceptDialog");
		this.filePopupMenu = this.GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/File");
		this.gameSettingsWindow = this.GetNode<GameSettingsWindow>("GameSettingsWindow");

		Game.initializeGameDataFromFile();
		DtgeCore.GameData gameData = DtgeCore.GameData.GetGameData();

		this.navigationButtonGrid.OnOptionSelected = this.handleOptionSelected;
		this.navigationButtonGrid.NavigationGridShortcutMode =
			gameData.ActiveNavigationGridShortcutMode;
		this.navigationButtonGrid.ChangeGridDimensions(
			gameData.NavigationGridColumns,
			gameData.NavigationGridRows);
		this.navigationButtonGrid.SizeFlagsStretchRatio =
			((float)gameData.NavigationGridRows) / 10.0f;

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

		this.GetTree().Root.SizeChanged += this._on_root_size_changed;
		this.updateWindowSize();
		this.manualViewportSizeOverride = false;
	}

	private void handleOptionSelected(DtgeCore.Option option)
	{
		DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();
		DtgeCore.SceneId sceneId = new DtgeCore.SceneId(option.TargetSceneId);
		DtgeCore.Scene dtgeScene;
		DtgeCore.SceneManager.GetSceneSuccessValue successValue =
			sceneManager.GetSceneAndSetSubsceneById(sceneId, out dtgeScene);

		switch (successValue)
		{
		case DtgeCore.SceneManager.GetSceneSuccessValue.Success:
			this.currentDtgeScene = dtgeScene;
			break;
		case DtgeCore.SceneManager.GetSceneSuccessValue.SceneNotFound:
			this.PopupErrorDialog("Error code DEAD_END: Option [" + option.Id + "] attempted to open Scene [" + sceneId.sceneName + "], which was not found");
			break;
		case DtgeCore.SceneManager.GetSceneSuccessValue.SubsceneNotFound:
			if (sceneId.subsceneName == null)
			{
				this.PopupErrorDialog("Error code DEAD_END: Option [" + option.Id + "] attempted to open Scene [" + sceneId.sceneName + "] without a subscene, which is not supported by that Scene");
			}
			else
			{
				this.PopupErrorDialog("Error code DEAD_END: Option [" + option.Id + "] attempted to open Subscene [" + sceneId.subsceneName + "], which was not found in Scene [" + sceneId.sceneName + "]");
			}
			break;
		default:
			throw new NotImplementedException();
		}

		this.updateUIFromScene();
	}

	public void LoadScene(DtgeCore.Scene scene)
	{
		this.currentDtgeScene = scene;
		this.updateUIFromScene();
	}

	public void SetManualViewportSize(Vector2 manualSize)
	{
		this.manualViewportSizeOverride = true;
		this.manualViewportSize = manualSize;
		this.updateWindowSize();
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

	private void _on_root_size_changed()
	{
		this.updateWindowSize();
	}

	private void updateUIFromScene()
	{
		if (this.currentDtgeScene != null)
		{
			this.sceneTextDisplay.Text = this.currentDtgeScene.CalculateSceneText();
			this.sceneTextDisplay.ScrollToLine(0);
			this.navigationButtonGrid.BindSceneOptionsToButtons(this.currentDtgeScene);
			this.updateNodeVisibilityFromScene(this.currentDtgeScene);
		}
	}

	private void updateWindowSize()
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

	private void PopupErrorDialog(string errorText)
	{
		this.errorAcceptDialog.DialogText = errorText;
		this.errorAcceptDialog.Popup();
	}

	private static void initializeGameDataFromFile()
	{
		FileAccess gameDataFile =
			FileAccess.Open(DtgeCore.GameData.GAME_DATA_FILE_PATH, FileAccess.ModeFlags.Read);

		if (gameDataFile != null)
		{
			string gameDataJson = gameDataFile.GetAsText();
			DtgeCore.GameData.PopulateFromJson(gameDataJson);
			gameDataFile.Close();
		}
		else
		{
			FileAccess newGameDataFile =
				FileAccess.Open(DtgeCore.GameData.GAME_DATA_FILE_PATH, FileAccess.ModeFlags.Write);
			if (newGameDataFile != null)
			{
				string gameDataString =
					JsonSerializer.Serialize<DtgeCore.GameData>(DtgeCore.GameData.GetGameData());
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
				DtgeCore.Scene newScene = DtgeCore.Scene.DeserializeFromJsonString(sceneJson);
				if (newScene == null || newScene.Name == null)
				{
					this.PopupErrorDialog("Error code VOID: A scene failed to load or had no id.");
				}
				else
				{
					sceneManager.AddScene(newScene);

					if (newScene.Name == gameData.StartSceneName)
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

	private void onSaveSettings(GameSettings settings)
	{
		this.gameSettings = settings;
		this.updateUIFromSettings();
		this.saveSettingsToFile(SETTINGS_PATH);
	}

	private void updateUIFromSettings()
	{
		this.sceneTextDisplay.AddThemeFontSizeOverride(
			"normal_font_size",
			this.gameSettings.SceneTextSize);
	}

	private bool tryLoadSettingsFromFile(string filePath)
	{
		bool success = false;
		FileAccess settingsFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

		if (settingsFile != null)
		{
			string settingsJson = settingsFile.GetAsText();
			GameSettings loadedGameSettings =
				JsonSerializer.Deserialize<GameSettings>(settingsJson);
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

	private void updateNodeVisibilityFromScene(DtgeCore.Scene scene)
	{
		if (scene == null || !scene.RenderImage)
		{
			this.sceneTextPanelContainer.Visible = true;
			this.leftTextureRect.Visible = false;
			this.rightTextureRect.Visible = false;
			this.topTextureRect.Visible = false;
			this.bottomTextureRect.Visible = false;
		}
		else
		{
			Image sceneImage = new Image();
			sceneImage.Load(scene.ImagePath);
			sceneImage.Rotate90(ClockDirection.Counterclockwise);
			ImageTexture sceneTexture = ImageTexture.CreateFromImage(sceneImage);

			switch (scene.ImagePosition)
			{
			case DtgeCore.Scene.SceneImagePosition.Left:
				this.sceneTextPanelContainer.Visible = true;
				this.leftTextureRect.Visible = true;
				this.leftTextureRect.Texture = sceneTexture;
				this.rightTextureRect.Visible = false;
				this.rightTextureRect.Texture = null;
				this.topTextureRect.Visible = false;
				this.topTextureRect.Texture = null;
				this.bottomTextureRect.Visible = false;
				this.bottomTextureRect.Texture = null;
				break;
			case DtgeCore.Scene.SceneImagePosition.Right:
				this.sceneTextPanelContainer.Visible = true;
				this.leftTextureRect.Visible = false;
				this.leftTextureRect.Texture = null;
				this.rightTextureRect.Visible = true;
				this.rightTextureRect.Texture = sceneTexture;
				this.topTextureRect.Visible = false;
				this.topTextureRect.Texture = null;
				this.bottomTextureRect.Visible = false;
				this.bottomTextureRect.Texture = null;
				break;
			case DtgeCore.Scene.SceneImagePosition.Top:
				this.sceneTextPanelContainer.Visible = true;
				this.leftTextureRect.Visible = false;
				this.leftTextureRect.Texture = null;
				this.rightTextureRect.Visible = false;
				this.rightTextureRect.Texture = null;
				this.topTextureRect.Visible = true;
				this.topTextureRect.Texture = sceneTexture;
				this.bottomTextureRect.Visible = false;
				this.bottomTextureRect.Texture = null;
				break;
			case DtgeCore.Scene.SceneImagePosition.Bottom:
				this.sceneTextPanelContainer.Visible = true;
				this.leftTextureRect.Visible = false;
				this.leftTextureRect.Texture = null;
				this.rightTextureRect.Visible = false;
				this.rightTextureRect.Texture = null;
				this.topTextureRect.Visible = false;
				this.topTextureRect.Texture = null;
				this.bottomTextureRect.Visible = true;
				this.bottomTextureRect.Texture = sceneTexture;
				break;
			case DtgeCore.Scene.SceneImagePosition.OnlyImage:
				this.sceneTextPanelContainer.Visible = false;
				this.leftTextureRect.Visible = true;
				this.leftTextureRect.Texture = sceneTexture;
				this.rightTextureRect.Visible = false;
				this.rightTextureRect.Texture = null;
				this.topTextureRect.Visible = false;
				this.topTextureRect.Texture = null;
				this.bottomTextureRect.Visible = false;
				this.bottomTextureRect.Texture = null;
				break;
			default:
				throw new NotImplementedException();
			}
		}
	}
}
