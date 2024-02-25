using Godot;
using DtgeGodotCommon;
using System;

namespace DtgeEditor;

public partial class Editor : Control
{
    MarginContainer marginContainer;
    DtgeSceneEditContainerV1 dtgeSceneEditContainer;

    PopupMenu filePopMenu;
    PopupMenu gamePopupMenu;
    PopupMenu helpPopupMenu;

    private enum PopupMenuIds
    {
        FileNew,
        FileOpen,
        FileSave,
        FileSaveAs,
        GameRun,
        HelpAbout,
        HelpTutorial,
        HelpLicense,
    }

    FileDialog openFileDialog;
    FileDialog saveAsFileDialog;
    AcceptDialog aboutAcceptDialog;
    AcceptDialog tutorialAcceptDialog;
    AcceptDialog licenseAcceptDialog;
    Vector2I aboutAndLicenseInitialSize;
    Vector2I tutorialInitialSize;

    AcceptDialog gamePreviewAcceptDialog;
    DtgeGame.Game gamePreviewScene;

    const int GAME_PREVIEW_CLOSE_BUTTON_HEIGHT = 50;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.dtgeSceneEditContainer = GetNode<DtgeSceneEditContainerV1>("MarginContainer/VBoxContainer/DtgeSceneEditContainer");
        this.marginContainer = GetNode<MarginContainer>("MarginContainer");

        this.filePopMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/File");
        this.filePopMenu.AddItem("New", (int)PopupMenuIds.FileNew, GodotConstants.KEY_CTRL_N);
        this.filePopMenu.AddItem("Open...", (int)PopupMenuIds.FileOpen, GodotConstants.KEY_CTRL_O);
        this.filePopMenu.AddItem("Save", (int)PopupMenuIds.FileSave, GodotConstants.KEY_CTRL_S);
        this.filePopMenu.AddItem("Save As...", (int)PopupMenuIds.FileSaveAs, GodotConstants.KEY_CTRL_SHIFT_S);

        this.gamePopupMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/Game");
        this.gamePopupMenu.AddItem("Run", (int)PopupMenuIds.GameRun, Key.F5);

        this.helpPopupMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/Help");
        this.helpPopupMenu.AddItem("About", (int)PopupMenuIds.HelpAbout);
        this.helpPopupMenu.AddItem("Tutorial", (int)PopupMenuIds.HelpTutorial, Key.F1);
        this.helpPopupMenu.AddItem("License", (int)PopupMenuIds.HelpLicense);

        this.openFileDialog = GetNode<FileDialog>("OpenFileDialog");
        this.openFileDialog.AddFilter("*.dscn", "DTGE Scene");
        this.openFileDialog.RootSubfolder = DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH;

        this.saveAsFileDialog = GetNode<FileDialog>("SaveAsFileDialog");
        this.saveAsFileDialog.AddFilter("*.dscn", "DTGE Scene");
        this.saveAsFileDialog.RootSubfolder = DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH;

        this.aboutAcceptDialog = GetNode<AcceptDialog>("AboutAcceptDialog");
        this.tutorialAcceptDialog = GetNode<AcceptDialog>("TutorialAcceptDialog");
        this.licenseAcceptDialog = GetNode<AcceptDialog>("LicenseAcceptDialog");

        this.gamePreviewAcceptDialog = GetNode<AcceptDialog>("GamePreviewAcceptDialog");

        this.GetTree().Root.SizeChanged += this.OnWindowSizeChanged;
        OnWindowSizeChanged();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnWindowSizeChanged()
    {
        Rect2 newViewport = this.GetTree().Root.GetViewport().GetVisibleRect();
        this.marginContainer.SetSize(newViewport.Size);
        this.aboutAndLicenseInitialSize.X = (int)(newViewport.Size.X * 0.25f);
        this.aboutAndLicenseInitialSize.Y = (int)(newViewport.Size.Y * 0.25f);
        this.tutorialInitialSize.X = (int)(newViewport.Size.X * 0.5f);
        this.tutorialInitialSize.Y = (int)(newViewport.Size.Y * 0.75f);
    }

    public void _on_popup_menu_index_pressed(int index)
    {
        switch ((PopupMenuIds)index)
        {
        case PopupMenuIds.FileNew:
        {
            this.dtgeSceneEditContainer.DtgeScene = new DtgeCore.Scene();
            this.dtgeSceneEditContainer.CachedFilePath = null;
            this.dtgeSceneEditContainer.GiveIdEntryFocus();
            break;
        }
        case PopupMenuIds.FileOpen:
        {
            this.openFileDialog.CurrentPath = this.dtgeSceneEditContainer.CachedFilePath;
            this.openFileDialog.Popup();
            break;
        }
        case PopupMenuIds.FileSave:
        {
            if (this.dtgeSceneEditContainer.CachedFilePath != null)
            {
                this._on_save_as_file_dialog_file_selected(this.dtgeSceneEditContainer.CachedFilePath);
            }
            else
            {
                this.saveAsFileDialog.CurrentFile = this.dtgeSceneEditContainer.DtgeScene.Id;
                this.saveAsFileDialog.Popup();
            }
            break;
        }
        case PopupMenuIds.FileSaveAs:
        {
            if (this.dtgeSceneEditContainer.CachedFilePath != null)
            {
                this.saveAsFileDialog.CurrentPath = this.dtgeSceneEditContainer.CachedFilePath;
            }
            else
            {
                this.saveAsFileDialog.CurrentFile = this.dtgeSceneEditContainer.DtgeScene.Id;
            }
            this.saveAsFileDialog.Popup();
            break;
        }
        case PopupMenuIds.GameRun:
        {
            this.gamePreviewAcceptDialog.RemoveChild(this.gamePreviewScene);
            this.gamePreviewScene = ((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.GAME_SCENE_PATH)).Instantiate<DtgeGame.Game>();

            this.gamePreviewAcceptDialog.AddChild(gamePreviewScene);
            this.gamePreviewAcceptDialog.Popup();
            this._on_game_preview_accept_dialog_size_changed();
            break;
        }
        case PopupMenuIds.HelpAbout:
        {
            this.aboutAcceptDialog.Size = this.aboutAndLicenseInitialSize;
            this.aboutAcceptDialog.Popup();
            break;
        }
        case PopupMenuIds.HelpTutorial:
        {
            this.tutorialAcceptDialog.Size = this.tutorialInitialSize;
            this.tutorialAcceptDialog.Popup();
            break;
        }
        case PopupMenuIds.HelpLicense:
        {
            this.licenseAcceptDialog.Size = this.aboutAndLicenseInitialSize;
            this.licenseAcceptDialog.Popup();
            break;
        }
        }
    }

    void _on_open_file_dialog_file_selected(string path)
    {
        FileAccess sceneFile = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (sceneFile == null)
        {
            return; // TODO
        }
        else
        {
            string sceneJson = sceneFile.GetAsText();
            this.dtgeSceneEditContainer.RestoreFromSerializedScene(sceneJson);
            this.dtgeSceneEditContainer.CachedFilePath = path;

            sceneFile.Close();
        }
    }

    void _on_save_as_file_dialog_file_selected(string path)
    {
        FileAccess sceneFile= FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (sceneFile == null)
        {
            return; // TODO
        }
        else
        {
            dtgeSceneEditContainer.FlushChangesForSave();
            string serializedScene = this.dtgeSceneEditContainer.GetSerializedScene();
            sceneFile.StoreString(serializedScene);
            this.dtgeSceneEditContainer.CachedFilePath = path;

            sceneFile.Close();
        }
    }

    void _on_game_preview_popup_popup_hide()
    {
        this.gamePreviewAcceptDialog.RemoveChild(this.gamePreviewScene);
    }

    void _on_game_preview_accept_dialog_size_changed()
    {
        if (this.gamePreviewScene != null)
        {
            Vector2 previewAcceptDialogSize = this.gamePreviewAcceptDialog.GetViewport().GetVisibleRect().Size;
            previewAcceptDialogSize.Y -= GAME_PREVIEW_CLOSE_BUTTON_HEIGHT;
            this.gamePreviewScene.SetManualViewportSize(previewAcceptDialogSize);
        }
    }
}
