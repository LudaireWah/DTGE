using Godot;
using System;
using System.Text.Json;

namespace DtgeGame;

public partial class Game : Control
{
    MarginContainer marginContainer;
    RichTextLabel sceneTextDisplay;
    NavigationButtonGrid navigationButtonGrid;
    AcceptDialog errorAcceptDialog;

    DtgeCore.Scene currentDtgeScene;

    bool manualViewportSizeOverride;
    Vector2 manualViewportSize;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.marginContainer = GetNode<MarginContainer>("MarginContainer");
        this.sceneTextDisplay = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/SceneTextDisplay");
        this.navigationButtonGrid = GetNode<NavigationButtonGrid>("MarginContainer/VBoxContainer/NavigationButtonGridContainer");
        this.errorAcceptDialog = GetNode<AcceptDialog>("ErrorAcceptDialog");

        DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();

        this.loadScenesFromFiles();
        
        this.UpdateUIFromScene();

        this.GetTree().Root.SizeChanged += this.OnWindowSizeChanged;
        this.OnWindowSizeChanged();
        this.manualViewportSizeOverride = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void HandleOptionChosen(DtgeCore.Option option)
    {
        DtgeCore.SceneManager sceneManager = DtgeCore.SceneManager.GetSceneManager();
        DtgeCore.Scene dtgeScene = sceneManager.getSceneById(option.targetSceneId);

        if (dtgeScene == null)
        {
            this.OnGameError("Error code bigfoot: Target scene not found.");
        }
        else
        {
            this.currentDtgeScene = dtgeScene;
        }

        this.UpdateUIFromScene();
    }

    private void UpdateUIFromScene()
    {
        if (this.currentDtgeScene != null)
        {
            this.sceneTextDisplay.Text = currentDtgeScene.SceneText;
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
                    sceneManager.addScene(newScene);

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
            this.OnGameError("Eror code labyrinth: No start scene found.");
        }
    }
}
