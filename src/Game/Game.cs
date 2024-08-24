using Godot;
using System;
using System.Text.Json;

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
    private const string SETTINGS_PATH = "dtge.config";

    private DtgeCore.Scene currentDtgeScene;

    private bool manualViewportSizeOverride;
    private Vector2 manualViewportSize;

    public override void _Ready()
    {
        this.marginContainer = GetNode<MarginContainer>("MarginContainer");
        this.sceneTextDisplay = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/SceneTextDisplay");
        this.navigationButtonGrid = GetNode<NavigationButtonGrid>("MarginContainer/VBoxContainer/NavigationButtonGridContainer");
        this.errorAcceptDialog = GetNode<AcceptDialog>("ErrorAcceptDialog");
        this.filePopupMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/File");
        this.gameSettingsWindow = GetNode<GameSettingsWindow>("GameSettingsWindow");

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

    public void HandleOptionChosen(DtgeCore.Option option)
    {
        DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();
        DtgeCore.Scene.SceneId sceneId = new DtgeCore.Scene.SceneId(option.TargetSceneId);
        DtgeCore.Scene dtgeScene = sceneManager.GetSceneAndSubsceneById(sceneId);

        if (dtgeScene == null)
        {
            this.PopupErrorDialog("Error code DEAD_END: Option [" + option.Id + "] attempted to open Scene [" + option.TargetSceneId + "], which was not found");
        }
        else
        {
            this.currentDtgeScene = dtgeScene;
        }

        this.updateUIFromScene();
    }

    private void updateUIFromScene()
    {
        if (this.currentDtgeScene != null)
        {
            this.sceneTextDisplay.Text = currentDtgeScene.CalculateSceneText(true);
            this.sceneTextDisplay.ScrollToLine(0);
            this.navigationButtonGrid.BindSceneOptionsToButtons(this.currentDtgeScene, this.HandleOptionChosen);
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

    private void loadScenesFromFiles()
    {
        DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();
        
        DirAccess sceneDirectory = DirAccess.Open(DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH);
        if (sceneDirectory == null)
        {
            this.PopupErrorDialog("Error Code LABYRINTH: No scene directory found.");
            return;
        }

        string[] sceneFileNames = sceneDirectory.GetFiles();
        
        for (int sceneFileIndex = 0; sceneFileIndex < sceneFileNames.Length; sceneFileIndex++)
        {
            string sceneFileName = sceneFileNames[sceneFileIndex];
            string sceneFilePath = DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH + "/" + sceneFileName;
            FileAccess sceneFile = FileAccess.Open(sceneFilePath, FileAccess.ModeFlags.Read);

            if (sceneFile != null)
            {
                string sceneJson = sceneFile.GetAsText();
                DtgeCore.Scene newScene = DtgeCore.Scene.Deserialize(sceneJson);
                if (newScene != null)
                {
                    sceneManager.AddScene(newScene);

                    if (sceneFileName == DtgeCore.Constants.DTGE_DEFAULT_START_SCENE_NAME && this.currentDtgeScene == null)
                    {
                        this.currentDtgeScene = newScene;
                    }
                }

                sceneFile.Close();
            }
        }

        if (this.currentDtgeScene == null)
        {
            this.PopupErrorDialog("Eror code LABYRINTH: No start scene found.");
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
