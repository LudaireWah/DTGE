using Godot;
using DtgeGodotCommon;
using System;
using System.Collections.Generic;

namespace DtgeEditor;

/**
 * This is the root node for the entire DTGE editor. It's chiefly
 * responsible for coordinating between instantiated Godot scenes
 * that manage specific pieces of the engine. It also manages the
 * top menu bar.
 */
public partial class Editor : Control
{
    MarginContainer marginContainer;
    TabBar dtgeSceneTabBar;
    List<DtgeSceneTabInfo> dtgeSceneTabInfoList;
    DtgeSceneEditContainer dtgeSceneEditContainer;
    Button addNewDtgeSceneButton;

    private struct DtgeSceneTabInfo
    {
        public DtgeCore.Scene dtgeScene;
        public string path;
        public bool saved;
    }

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
        GameRunCurrentScene,
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

    const float SMALL_ACCEPT_DIALOG_SIZE_RATIO = 0.25f;
    const float LARGE_ACCEPT_DIALOG_SIZE_RATIO_X = 0.5f;
    const float LARGE_ACCEPT_DIALOG_SIZE_RATIO_Y = 0.75f;
    const int GAME_PREVIEW_CLOSE_BUTTON_HEIGHT = 50;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.marginContainer = GetNode<MarginContainer>("MarginContainer");
        this.dtgeSceneTabBar = GetNode<TabBar>("MarginContainer/VBoxContainer/HBoxContainer/DtgeScenesTabBar");
        this.addNewDtgeSceneButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/AddNewDtgeSceneButton");
        this.dtgeSceneEditContainer = GetNode<DtgeSceneEditContainer>("MarginContainer/VBoxContainer/DtgeSceneEditContainer");
        this.filePopMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/File");
        this.gamePopupMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/Game");
        this.helpPopupMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/Help");
        this.openFileDialog = GetNode<FileDialog>("OpenFileDialog");
        this.saveAsFileDialog = GetNode<FileDialog>("SaveAsFileDialog");
        this.aboutAcceptDialog = GetNode<AcceptDialog>("AboutAcceptDialog");
        this.tutorialAcceptDialog = GetNode<AcceptDialog>("TutorialAcceptDialog");
        this.licenseAcceptDialog = GetNode<AcceptDialog>("LicenseAcceptDialog");
        this.gamePreviewAcceptDialog = GetNode<AcceptDialog>("GamePreviewAcceptDialog");

        this.dtgeSceneTabInfoList = new List<DtgeSceneTabInfo>();
        //this.createNewSceneTab();

        this.dtgeSceneEditContainer.TryOpenSceneAction = this.TryOpenScene;
        this.dtgeSceneEditContainer.OnSceneUpdated = this.OnSceneUpdated;

        this.filePopMenu.AddItem("New", (int)PopupMenuIds.FileNew, GodotConstants.KEY_CTRL_N);
        this.filePopMenu.AddItem("Open...", (int)PopupMenuIds.FileOpen, GodotConstants.KEY_CTRL_O);
        this.filePopMenu.AddItem("Save", (int)PopupMenuIds.FileSave, GodotConstants.KEY_CTRL_S);
        this.filePopMenu.AddItem("Save As...", (int)PopupMenuIds.FileSaveAs, GodotConstants.KEY_CTRL_SHIFT_S);
        this.gamePopupMenu.AddItem("Run", (int)PopupMenuIds.GameRun, Key.F5);
        this.gamePopupMenu.AddItem("Run", (int)PopupMenuIds.GameRunCurrentScene, GodotConstants.KEY_CTRL_F5);
        this.helpPopupMenu.AddItem("About", (int)PopupMenuIds.HelpAbout);
        this.helpPopupMenu.AddItem("Tutorial", (int)PopupMenuIds.HelpTutorial, Key.F1);
        this.helpPopupMenu.AddItem("License", (int)PopupMenuIds.HelpLicense);

        this.openFileDialog.AddFilter("*.dscn", "DTGE Scene");
        this.openFileDialog.RootSubfolder = DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH;
        this.saveAsFileDialog.AddFilter("*.dscn", "DTGE Scene");
        this.saveAsFileDialog.RootSubfolder = DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH;

        this.GetTree().Root.SizeChanged += this.OnWindowSizeChanged;
        OnWindowSizeChanged();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void OnWindowSizeChanged()
    {
        Rect2 newViewport = this.GetTree().Root.GetViewport().GetVisibleRect();
        this.marginContainer.SetSize(newViewport.Size);
        this.aboutAndLicenseInitialSize.X = (int)(newViewport.Size.X * SMALL_ACCEPT_DIALOG_SIZE_RATIO);
        this.aboutAndLicenseInitialSize.Y = (int)(newViewport.Size.Y * SMALL_ACCEPT_DIALOG_SIZE_RATIO);
        this.tutorialInitialSize.X = (int)(newViewport.Size.X * LARGE_ACCEPT_DIALOG_SIZE_RATIO_X);
        this.tutorialInitialSize.Y = (int)(newViewport.Size.Y * LARGE_ACCEPT_DIALOG_SIZE_RATIO_Y);
    }

    public void TryOpenScene(string sceneId)
    {
        bool sceneFoundInTabs = false;
        for (int dtgeSceneTabInfoIndex = 0; dtgeSceneTabInfoIndex < this.dtgeSceneTabInfoList.Count; dtgeSceneTabInfoIndex++)
        {
            if (this.dtgeSceneTabInfoList[dtgeSceneTabInfoIndex].dtgeScene.Id.Equals(sceneId))
            {
                this.dtgeSceneTabBar.CurrentTab = dtgeSceneTabInfoIndex;
                this._on_dtge_scenes_tab_bar_tab_selected(dtgeSceneTabInfoIndex);
                sceneFoundInTabs = true;
                break;
            }
        }

        if (!sceneFoundInTabs)
        {
            this.createNewSceneTab();
            this.dtgeSceneEditContainer.DtgeScene.Id = sceneId;
            this.updateCurrentTabTitle(false);
        }
    }

    public void OnSceneUpdated()
    {
        this.updateCurrentTabTitle(false);
    }

    public void _on_popup_menu_index_pressed(int index)
    {
        switch ((PopupMenuIds)index)
        {
        case PopupMenuIds.FileNew:
        {
            this.createNewSceneTab();
            break;
        }
        case PopupMenuIds.FileOpen:
        {
            this.openFileDialog.Popup();
            break;
        }
        case PopupMenuIds.FileSave:
        {
            // TODO: Is there anything to handle here or should we just crash with the index out of bounds? I feel like this would
            //    be an assert if we had them.
            //if (index >= this.dtgeSceneTabInfoList.Count)
            //{
            //}
            //else
            {
                DtgeSceneTabInfo currentDtgeSceneTabInfo = this.dtgeSceneTabInfoList[this.dtgeSceneTabBar.CurrentTab];

                if (currentDtgeSceneTabInfo.path != null)
                {
                    this._on_save_as_file_dialog_file_selected(currentDtgeSceneTabInfo.path);
                }
                else
                {
                    this.saveAsFileDialog.CurrentFile = this.dtgeSceneEditContainer.DtgeScene.Id;
                    this.saveAsFileDialog.Popup();
                }
            }
            break;
        }
        case PopupMenuIds.FileSaveAs:
        {
            this.saveAsFileDialog.CurrentFile = this.dtgeSceneEditContainer.DtgeScene.Id;
            this.saveAsFileDialog.Popup();
            break;
        }
        case PopupMenuIds.GameRun:
        {
            this.runDebugGame();
            break;
        }
        case PopupMenuIds.GameRunCurrentScene:
        {
            this.runDebugGame();
            this.gamePreviewScene.LoadScene(this.dtgeSceneEditContainer.DtgeScene);
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

    public void _on_open_file_dialog_file_selected(string path)
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
            this.createOpenedSceneTab(this.dtgeSceneEditContainer.DtgeScene, path);
            this.updateCurrentTabTitle(true);

            sceneFile.Close();
        }
    }

    public void _on_save_as_file_dialog_file_selected(string path)
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

            sceneFile.Close();

            setCurrentSceneTabInfoPath(path);
            this.updateCurrentTabTitle(true);
        }
    }

    public void _on_game_preview_popup_popup_hide()
    {
        this.gamePreviewAcceptDialog.RemoveChild(this.gamePreviewScene);
    }

    public void _on_game_preview_accept_dialog_size_changed()
    {
        if (this.gamePreviewScene != null)
        {
            Vector2 previewAcceptDialogSize = this.gamePreviewAcceptDialog.GetViewport().GetVisibleRect().Size;
            previewAcceptDialogSize.Y -= GAME_PREVIEW_CLOSE_BUTTON_HEIGHT;
            this.gamePreviewScene.SetManualViewportSize(previewAcceptDialogSize);
        }
    }

    public void _on_add_new_dtge_scene_button_pressed()
    {
        this.createNewSceneTab();
    }

    public void _on_dtge_scenes_tab_bar_tab_selected(int tabIndex)
    {
        DtgeSceneTabInfo selectedTabInfo = this.dtgeSceneTabInfoList[tabIndex];
        this.dtgeSceneEditContainer.DtgeScene = selectedTabInfo.dtgeScene;
        this.dtgeSceneEditContainer.UpdateUIFromScene();
    }

    public void _on_dtge_scenes_tab_bar_tab_close_pressed(int tabIndex)
    {
        this.dtgeSceneTabInfoList.RemoveAt(tabIndex);
        this.dtgeSceneTabBar.RemoveTab(tabIndex);

        if (this.dtgeSceneTabBar.TabCount > 0)
        {
            DtgeSceneTabInfo currentDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();
            this.dtgeSceneEditContainer.DtgeScene = currentDtgeSceneTabInfo.dtgeScene;
            this.dtgeSceneEditContainer.UpdateUIFromScene();
        }
        else
        {
            this.dtgeSceneEditContainer.DtgeScene = new DtgeCore.Scene();
            this.dtgeSceneEditContainer.UpdateUIFromScene();
        }
    }

    private void runDebugGame()
    {
        this.gamePreviewAcceptDialog.RemoveChild(this.gamePreviewScene);
        this.gamePreviewScene = ((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.GAME_SCENE_PATH)).Instantiate<DtgeGame.Game>();

        this.gamePreviewAcceptDialog.AddChild(gamePreviewScene);
        this.gamePreviewAcceptDialog.Popup();
        this._on_game_preview_accept_dialog_size_changed();
    }

    private void createNewSceneTab()
    {
        this.dtgeSceneTabBar.AddTab("(new scene)*");
        this.dtgeSceneTabBar.CurrentTab = this.dtgeSceneTabBar.TabCount - 1;

        DtgeCore.Scene newDtgeScene = new DtgeCore.Scene();
        DtgeSceneTabInfo newDtgeSceneTabInfo;
        newDtgeSceneTabInfo.dtgeScene = newDtgeScene;
        newDtgeSceneTabInfo.path = null;
        newDtgeSceneTabInfo.saved = false;

        this.dtgeSceneTabInfoList.Add(newDtgeSceneTabInfo);
        this.dtgeSceneEditContainer.DtgeScene = newDtgeSceneTabInfo.dtgeScene;
        this.dtgeSceneEditContainer.UpdateUIFromScene();
    }

    private void createOpenedSceneTab(DtgeCore.Scene scene, string path)
    {
        DtgeSceneTabInfo openedSceneTabInfo;
        openedSceneTabInfo.dtgeScene = scene;
        openedSceneTabInfo.path = path;
        openedSceneTabInfo.saved = true;

        this.dtgeSceneTabBar.AddTab(scene.Id);
        this.dtgeSceneTabBar.CurrentTab = this.dtgeSceneTabBar.TabCount - 1;

        this.dtgeSceneTabInfoList.Add(openedSceneTabInfo);
    }

    private DtgeSceneTabInfo getCurrentDtgeSceneTabInfo()
    {
        return this.dtgeSceneTabInfoList[this.dtgeSceneTabBar.CurrentTab];
    }

    private void setCurrentSceneTabInfoScene(DtgeCore.Scene scene)
    {
        DtgeSceneTabInfo currentDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();
        currentDtgeSceneTabInfo.dtgeScene = scene;
        this.dtgeSceneTabInfoList[this.dtgeSceneTabBar.CurrentTab] = currentDtgeSceneTabInfo;
    }

    private void setCurrentSceneTabInfoPath(string path)
    {
        DtgeSceneTabInfo currentDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();
        currentDtgeSceneTabInfo.path = path;
        this.dtgeSceneTabInfoList[this.dtgeSceneTabBar.CurrentTab] = currentDtgeSceneTabInfo;
    }

    private void setCurrentSceneTabInfoSaved(bool saved)
    {
        DtgeSceneTabInfo currentDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();
        currentDtgeSceneTabInfo.saved = saved;
        this.dtgeSceneTabInfoList[this.dtgeSceneTabBar.CurrentTab] = currentDtgeSceneTabInfo;
    }

    private void updateCurrentTabTitle(bool saved)
    {
        if (this.dtgeSceneTabBar.TabCount == 0)
        {
            this.createNewSceneTab();
        }

        this.setCurrentSceneTabInfoSaved(saved);

        DtgeSceneTabInfo currentDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();

        string updatedTabTitle = currentDtgeSceneTabInfo.dtgeScene.Id;
        if (!saved)
        {
            updatedTabTitle += " *";
        }

        this.dtgeSceneTabBar.SetTabTitle(this.dtgeSceneTabBar.CurrentTab, updatedTabTitle);
    }
}
