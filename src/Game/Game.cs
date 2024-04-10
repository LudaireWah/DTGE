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

    private DtgeCore.Scene currentDtgeScene;

    private bool manualViewportSizeOverride;
    private Vector2 manualViewportSize;

    public override void _Ready()
    {
        this.marginContainer = GetNode<MarginContainer>("MarginContainer");
        this.sceneTextDisplay = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/SceneTextDisplay");
        this.navigationButtonGrid = GetNode<NavigationButtonGrid>("MarginContainer/VBoxContainer/NavigationButtonGridContainer");
        this.errorAcceptDialog = GetNode<AcceptDialog>("ErrorAcceptDialog");

        DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();

        this.loadScenesFromFiles();
        this.updateUIFromScene();

        this.GetTree().Root.SizeChanged += this.OnWindowSizeChanged;
        this.OnWindowSizeChanged();
        this.manualViewportSizeOverride = false;
    }

    public void HandleOptionChosen(DtgeCore.Option option)
    {
        DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();
        DtgeCore.Scene dtgeScene = sceneManager.GetSceneById(option.TargetSceneId);

        if (dtgeScene == null)
        {
            this.OnGameError("Error code DEAD_END: Option [" + option.Id + "] attempted to open Scene [" + option.TargetSceneId + "], which was not found");
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
            this.sceneTextDisplay.Text = currentDtgeScene.SceneText;
            this.sceneTextDisplay.ScrollToLine(0);
            this.navigationButtonGrid.BindSceneOptionsToButtons(this.currentDtgeScene, this.HandleOptionChosen);
        }
    }

    public void OnWindowSizeChanged()
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
        OnWindowSizeChanged();
    }

    public void OnGameError(string errorText)
    {
        this.errorAcceptDialog.DialogText = errorText;
        this.errorAcceptDialog.Popup();
    }

    private void loadScenesFromFiles()
    {
        DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();
        
        DirAccess sceneDirectory = DirAccess.Open(DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH);
        if (sceneDirectory == null)
        {
            this.OnGameError("Error Code LABYRINTH: No scene directory found.");
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
                DtgeCore.Scene newScene = JsonSerializer.Deserialize<DtgeCore.Scene>(sceneJson);
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
            this.OnGameError("Eror code LABYRINTH: No start scene found.");
        }
    }

    public void LoadScene(DtgeCore.Scene scene)
    {
        this.currentDtgeScene = scene;
        this.updateUIFromScene();
    }
}
