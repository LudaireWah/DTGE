using Godot;
using DtgeGodotCommon;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DtgeEditor;

/**
 * This is the root node for the entire DTGE editor. It's chiefly
 * responsible for coordinating between instantiated Godot scenes
 * that manage specific pieces of the engine. It also manages the
 * top menu bar.
 */
public partial class Editor : Control
{
    private const string CONFIRMATIONDIALOG_METADATA_TAG_SCENE_NAME = "sceneName";
    private const string CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_IS_NON_NULL = "subsceneIsNonNull";
    private const string CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_NAME = "subsceneName";

    MarginContainer marginContainer;
    TabBar dtgeSceneTabBar;
    DtgeSceneEditContainer dtgeSceneEditContainer;
    Button addNewDtgeSceneButton;

    PopupMenu filePopupMenu;
    PopupMenu gamePopupMenu;
    PopupMenu helpPopupMenu;

    private enum PopupMenuIds
    {
        FileNew,
        FileOpen,
        FileSave,
        FileSaveAs,
        FileSaveAll,
        GameRun,
        GameRunCurrentScene,
        HelpAbout,
        HelpTutorial,
        HelpLicense,
    }

    FileDialog openFileDialog;
    FileDialog saveAsFileDialog;
    FileDialog saveAsAndCloseFileDialog;
    AcceptDialog aboutAcceptDialog;
    AcceptDialog tutorialAcceptDialog;
    AcceptDialog licenseAcceptDialog;
    YesNoCancelDialog saveYesNoCancelDialog;
    ConfirmationDialog createNewSceneFromOptionConfirmationDialog;
    ConfirmationDialog createNewSubsceneFromOptionConfirmationDialog;
    ConfirmationDialog enableNullSubsceneOptionConfirmationDialog;
    AcceptDialog gamePreviewAcceptDialog;
    DtgeGame.Game gamePreviewScene;

    Vector2I smallDialogInitialSize;
    Vector2I largeDialogInitialSize;
    const float SMALL_ACCEPT_DIALOG_SIZE_RATIO = 0.25f;
    const float LARGE_ACCEPT_DIALOG_SIZE_RATIO_X = 0.5f;
    const float LARGE_ACCEPT_DIALOG_SIZE_RATIO_Y = 0.75f;
    const int GAME_PREVIEW_CLOSE_BUTTON_HEIGHT = 50;

    private enum EditorState
    {
        Active,
        Closing
    }

    private EditorState currentState;
    private int tabToProcessBeforeClosing;

    // A dictionary using creation order as a unique key, as we can't safely assume that scene ids are unique during
    // authorship. This key will be stored with the tabs as metadata to associate them with the entry in the dictionary
    Dictionary<int, DtgeSceneTabInfo> openDtgeSceneDictionary;
    int nextKeyforOpenDtgeSceneDictionary;

    private string pendingSceneIdForCreateNewSceneFromOptionConfirmationDialog;

    private struct DtgeSceneTabInfo
    {
        public DtgeCore.Scene dtgeScene;
        public string path;
        public bool saved;
    }

    bool anyEditsMade = false;

    public Editor()
    {

    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        this.marginContainer = GetNode<MarginContainer>("MarginContainer");
        this.dtgeSceneTabBar = GetNode<TabBar>("MarginContainer/VBoxContainer/SceneTabsHBoxContainer/DtgeScenesTabBar");
        this.addNewDtgeSceneButton = GetNode<Button>("MarginContainer/VBoxContainer/SceneTabsHBoxContainer/AddNewDtgeSceneButton");
        this.dtgeSceneEditContainer = GetNode<DtgeSceneEditContainer>("MarginContainer/VBoxContainer/DtgeSceneEditContainer");
        this.filePopupMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/File");
        this.gamePopupMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/Game");
        this.helpPopupMenu = GetNode<PopupMenu>("MarginContainer/VBoxContainer/MenuBar/Help");
        this.openFileDialog = GetNode<FileDialog>("OpenFileDialog");
        this.saveAsFileDialog = GetNode<FileDialog>("SaveAsFileDialog");
        this.saveAsAndCloseFileDialog = GetNode<FileDialog>("SaveAsAndCloseFileDialog");
        this.aboutAcceptDialog = GetNode<AcceptDialog>("AboutAcceptDialog");
        this.tutorialAcceptDialog = GetNode<AcceptDialog>("TutorialAcceptDialog");
        this.licenseAcceptDialog = GetNode<AcceptDialog>("LicenseAcceptDialog");
        this.gamePreviewAcceptDialog = GetNode<AcceptDialog>("GamePreviewAcceptDialog");
        this.saveYesNoCancelDialog = GetNode<YesNoCancelDialog>("SaveYesNoCancelDialog");
        this.createNewSceneFromOptionConfirmationDialog = GetNode<ConfirmationDialog>("CreateNewSceneFromOptionConfirmationDialog");
        this.createNewSubsceneFromOptionConfirmationDialog = GetNode<ConfirmationDialog>("CreateNewSubsceneFromOptionConfirmationDialog");
        this.enableNullSubsceneOptionConfirmationDialog = GetNode<ConfirmationDialog>("EnableNullSubsceneConfirmationDialog");

        this.openDtgeSceneDictionary = new Dictionary<int, DtgeSceneTabInfo>();
        this.nextKeyforOpenDtgeSceneDictionary = 0;

        this.dtgeSceneEditContainer.OnTryOpenScene = this.HandleTryOpenScene;
        this.dtgeSceneEditContainer.OnSceneUpdated = this.HandleSceneUpdated;

        this.filePopupMenu.AddItem("New", (int)PopupMenuIds.FileNew, GodotConstants.KEY_CTRL_N);
        this.filePopupMenu.AddItem("Open...", (int)PopupMenuIds.FileOpen, GodotConstants.KEY_CTRL_O);
        this.filePopupMenu.AddItem("Save", (int)PopupMenuIds.FileSave, GodotConstants.KEY_CTRL_S);
        this.filePopupMenu.AddItem("Save As...", (int)PopupMenuIds.FileSaveAs, GodotConstants.KEY_CTRL_SHIFT_S);
        this.filePopupMenu.AddItem("Save All", (int)PopupMenuIds.FileSaveAll, GodotConstants.KEY_CTRL_SHIFT_ALT_S);
        this.gamePopupMenu.AddItem("Run Game", (int)PopupMenuIds.GameRun, Key.F5);
        this.gamePopupMenu.AddItem("Run Current Scene", (int)PopupMenuIds.GameRunCurrentScene, GodotConstants.KEY_CTRL_F5);
        this.helpPopupMenu.AddItem("About", (int)PopupMenuIds.HelpAbout);
        this.helpPopupMenu.AddItem("Tutorial", (int)PopupMenuIds.HelpTutorial, Key.F1);
        this.helpPopupMenu.AddItem("License", (int)PopupMenuIds.HelpLicense);

        this.openFileDialog.AddFilter("*.dscn", "DTGE Scene");
        this.openFileDialog.RootSubfolder = DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH;
        this.saveAsFileDialog.AddFilter("*.dscn", "DTGE Scene");
        this.saveAsFileDialog.RootSubfolder = DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH;
        this.saveAsFileDialog.Canceled += () =>
        {
            if (this.currentState == EditorState.Closing)
            {
                this.currentState = EditorState.Active;
            }
        };
        this.saveAsAndCloseFileDialog.AddFilter("*.dscn", "DTGE Scene");
        this.saveAsAndCloseFileDialog.RootSubfolder = DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH;
        this.saveAsAndCloseFileDialog.Canceled += () =>
        {
            if (this.currentState == EditorState.Closing)
            {
                this.currentState = EditorState.Active;
            }
        };

        this.createNewSceneTab();

        this.GetTree().Root.SizeChanged += this.HandleWindowSizeChanged;
        HandleWindowSizeChanged();

        this.currentState = EditorState.Active;
        GetTree().AutoAcceptQuit = false;

        this.anyEditsMade = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);

        switch (this.currentState)
        {
        case EditorState.Active:
        {
            break;
        }
        case EditorState.Closing:
        {
            if (!this.saveAsFileDialog.Visible &&
                !this.saveYesNoCancelDialog.Visible)
            {
                if (this.tabToProcessBeforeClosing == this.dtgeSceneTabBar.TabCount)
                {
                    this.GetTree().Quit();
                }

                for (; this.tabToProcessBeforeClosing < this.dtgeSceneTabBar.TabCount; this.tabToProcessBeforeClosing++)
                {
                    this.dtgeSceneTabBar.CurrentTab = this.tabToProcessBeforeClosing;
                    DtgeSceneTabInfo currentDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();

                    if (!currentDtgeSceneTabInfo.saved)
                    {
                        this.popSaveYesNoCancelDialog(currentDtgeSceneTabInfo, this.tabToProcessBeforeClosing, false);
                        this.tabToProcessBeforeClosing++;
                        break;
                    }
                }

            }
            break;
        }
        }
    }

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationWMCloseRequest)
        {
            this.currentState = EditorState.Closing;
            this.tabToProcessBeforeClosing = 0;
        }
    }

    public void HandleWindowSizeChanged()
    {
        Rect2 newViewport = this.GetTree().Root.GetViewport().GetVisibleRect();
        this.marginContainer.SetSize(newViewport.Size);
        this.smallDialogInitialSize.X = (int)(newViewport.Size.X * SMALL_ACCEPT_DIALOG_SIZE_RATIO);
        this.smallDialogInitialSize.Y = (int)(newViewport.Size.Y * SMALL_ACCEPT_DIALOG_SIZE_RATIO);
        this.largeDialogInitialSize.X = (int)(newViewport.Size.X * LARGE_ACCEPT_DIALOG_SIZE_RATIO_X);
        this.largeDialogInitialSize.Y = (int)(newViewport.Size.Y * LARGE_ACCEPT_DIALOG_SIZE_RATIO_Y);
    }

    public void HandleTryOpenScene(DtgeCore.Scene.SceneId sceneId)
    {
        bool sceneFoundInTabs = false;

        for (int dtgeSceneTabIndex = 0; dtgeSceneTabIndex < this.dtgeSceneTabBar.TabCount; dtgeSceneTabIndex++)
        {
            DtgeSceneTabInfo dtgeSceneTabInfo = this.openDtgeSceneDictionary[this.getKeyFromTabIndex(dtgeSceneTabIndex)];
            if (dtgeSceneTabInfo.dtgeScene.Id == sceneId.scene)
            {
                this.dtgeSceneTabBar.CurrentTab = dtgeSceneTabIndex;
                sceneFoundInTabs = true;
                this._on_dtge_scenes_tab_bar_tab_selected(dtgeSceneTabIndex);
                break;
            }
        }

        bool sceneFoundInFiles = false;
        if (!sceneFoundInTabs)
        {
            DirAccess sceneDirectory = DirAccess.Open(DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH);
            if (sceneDirectory == null)
            {
                return;
            }

            string[] sceneFileNames = sceneDirectory.GetFiles();
            string targetSceneFileName = sceneId.scene + ".dscn";

            for (int sceneFileIndex = 0; sceneFileIndex < sceneFileNames.Length; sceneFileIndex++)
            {
                string sceneFileName = sceneFileNames[sceneFileIndex];

                if (sceneFileName == targetSceneFileName)
                {
                    string sceneFilePath = DtgeCore.Constants.DTGE_DEFAULT_SCENE_DIRECTORY_PATH + "/" + sceneFileName;
                    FileAccess sceneFile = FileAccess.Open(sceneFilePath, FileAccess.ModeFlags.Read);

                    if (sceneFile != null)
                    {
                        string sceneJson = sceneFile.GetAsText();
                        DtgeCore.Scene newScene = DtgeCore.Scene.Deserialize(sceneJson);
                        if (newScene != null)
                        {
                            this.createOpenedSceneTab(newScene, sceneFilePath);
                            sceneFoundInFiles = true;
                        }

                        sceneFile.Close();
                    }
                }
            }
        }

        if (sceneFoundInTabs || sceneFoundInFiles)
        {
            bool subsceneSuccessfullySet = this.dtgeSceneEditContainer.DtgeScene.SetCurrentSubscene(sceneId.subscene);
            if (!subsceneSuccessfullySet)
            {
                if (sceneId.subscene != null)
                {
                    this.createNewSubsceneFromOptionConfirmationDialog.DialogText =
                        "Scene [" + sceneId.scene + "] does not contain subscene [" + sceneId.subscene + "]. Do you want to create this subscene?";
                    this.createNewSubsceneFromOptionConfirmationDialog.RemoveMeta(CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_NAME);
                    this.createNewSubsceneFromOptionConfirmationDialog.SetMeta(CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_NAME, sceneId.subscene);
                    this.createNewSubsceneFromOptionConfirmationDialog.Popup();
                }
                else
                {
                    this.enableNullSubsceneOptionConfirmationDialog.DialogText = "Scene [" + sceneId.scene + "] does not support (None) subscenes. Do you want to allow them?";
                    this.enableNullSubsceneOptionConfirmationDialog.Popup();
                }
            }
        }

        if (!sceneFoundInTabs && !sceneFoundInFiles)
        {
            this.createNewSceneFromOptionConfirmationDialog.DialogText = "No scene [" + sceneId.scene + "] was found. Do you want to create one?";
            this.createNewSceneFromOptionConfirmationDialog.RemoveMeta(CONFIRMATIONDIALOG_METADATA_TAG_SCENE_NAME);
            this.createNewSceneFromOptionConfirmationDialog.RemoveMeta(CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_IS_NON_NULL);
            this.createNewSceneFromOptionConfirmationDialog.RemoveMeta(CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_NAME);
            this.createNewSceneFromOptionConfirmationDialog.SetMeta(CONFIRMATIONDIALOG_METADATA_TAG_SCENE_NAME, sceneId.scene);
            if (sceneId.subscene != null)
            {
                this.createNewSceneFromOptionConfirmationDialog.SetMeta(CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_IS_NON_NULL, sceneId.subscene != null);
                this.createNewSceneFromOptionConfirmationDialog.SetMeta(CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_NAME, sceneId.subscene);
            }
            this.createNewSceneFromOptionConfirmationDialog.Popup();
        }
    }

    public void HandleSceneUpdated()
    {
        this.setCurrentSceneTabInfoSaved(false);
        this.updateTabTitle(this.dtgeSceneTabBar.CurrentTab);
        this.anyEditsMade = true;
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
            DtgeSceneTabInfo currentDtgeSceneTabInfo = this.openDtgeSceneDictionary[this.getKeyFromTabIndex(this.dtgeSceneTabBar.CurrentTab)];
            if (currentDtgeSceneTabInfo.path != null)
            {
                this._on_save_as_file_dialog_file_selected(currentDtgeSceneTabInfo.path);
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
            this.saveAsFileDialog.CurrentFile = this.dtgeSceneEditContainer.DtgeScene.Id;
            this.saveAsFileDialog.Popup();
            break;
        }
        case PopupMenuIds.FileSaveAll:
        {
            foreach (int dtgeSceneTabInfoKey in this.openDtgeSceneDictionary.Keys)
            {
                DtgeSceneTabInfo dtgeSceneTabInfo = this.openDtgeSceneDictionary[dtgeSceneTabInfoKey];
                if (dtgeSceneTabInfo.path != null)
                {
                    Editor.saveSceneToPath(dtgeSceneTabInfo.dtgeScene, dtgeSceneTabInfo.path);
                    dtgeSceneTabInfo.saved = true;
                    this.openDtgeSceneDictionary[dtgeSceneTabInfoKey] = dtgeSceneTabInfo;
                }
            }
            for (int tabIndex = 0; tabIndex < this.dtgeSceneTabBar.TabCount; tabIndex++)
            {
                this.updateTabTitle(tabIndex);
            }
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
            this.aboutAcceptDialog.Size = this.smallDialogInitialSize;
            this.aboutAcceptDialog.Popup();
            break;
        }
        case PopupMenuIds.HelpTutorial:
        {
            this.tutorialAcceptDialog.Size = this.largeDialogInitialSize;
            this.tutorialAcceptDialog.Popup();
            break;
        }
        case PopupMenuIds.HelpLicense:
        {
            this.licenseAcceptDialog.Size = this.smallDialogInitialSize;
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
            this.updateTabTitle(this.dtgeSceneTabBar.CurrentTab);

            sceneFile.Close();
        }
    }

    public void _on_save_as_file_dialog_file_selected(string path)
    {
        this.saveCurrentSceneToPath(path);
    }

    public void _on_save_as_and_close_file_dialog_file_selected(string path)
    {
        this.saveCurrentSceneToPath(path);
        this.removeDtgeSceneTab(this.dtgeSceneTabBar.CurrentTab);
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
        if (this.openDtgeSceneDictionary.Count > 0)
        {
            DtgeSceneTabInfo selectedTabInfo = this.openDtgeSceneDictionary[this.getKeyFromTabIndex(tabIndex)];
            this.dtgeSceneEditContainer.DtgeScene = selectedTabInfo.dtgeScene;
            this.dtgeSceneEditContainer.UpdateUIFromScene();
        }
    }

    public void _on_dtge_scenes_tab_bar_tab_close_pressed(int tabIndex)
    {
        if (this.dtgeSceneTabBar.TabCount == 0)
        {
        }
        else if (this.dtgeSceneTabBar.CurrentTab != tabIndex)
        {
            throw new Exception("The editor isn't supposed to support closing any tabs except the active one");
        }
        else
        {
            DtgeSceneTabInfo targetDtgeSceneTabInfo = this.openDtgeSceneDictionary[this.getKeyFromTabIndex(tabIndex)];
            if (targetDtgeSceneTabInfo.saved)
            {
                this.removeDtgeSceneTab(tabIndex);
            }
            else
            {
                this.popSaveYesNoCancelDialog(targetDtgeSceneTabInfo, tabIndex, true);
            }
        }
    }

    public void _on_new_tab_button_pressed()
    {
        this.createNewSceneTab();
    }

    public void _on_dtge_scenes_tab_bar_active_tab_rearranged(int indexTo)
    {
        return;
    }

    public void _on_create_new_scene_from_option_confirmation_dialog_confirmed()
    {
        if (this.anyEditsMade)
        {
            this.createNewSceneTab();
        }
        this.dtgeSceneEditContainer.DtgeScene.Id = (string)this.createNewSceneFromOptionConfirmationDialog.GetMeta(CONFIRMATIONDIALOG_METADATA_TAG_SCENE_NAME);
        bool subsceneIsNonNull = (bool)this.createNewSceneFromOptionConfirmationDialog.GetMeta(CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_IS_NON_NULL);
        if (subsceneIsNonNull)
        {
            string subsceneName = (string)this.createNewSceneFromOptionConfirmationDialog.GetMeta(CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_NAME);
            this.dtgeSceneEditContainer.DtgeScene.AddSubscene(subsceneName);
        }
        this.setCurrentSceneTabInfoSaved(false);
        this.updateTabTitle(this.dtgeSceneTabBar.CurrentTab);

        this.pendingSceneIdForCreateNewSceneFromOptionConfirmationDialog = null;
    }

    public void _on_create_new_subscene_from_option_confirmation_dialog_confirmed()
    {
        this.dtgeSceneEditContainer.DtgeScene.AddSubscene((string)this.createNewSubsceneFromOptionConfirmationDialog.GetMeta(CONFIRMATIONDIALOG_METADATA_TAG_SUBSCENE_NAME));
        this.dtgeSceneEditContainer.UpdateUIFromScene();
        this.HandleSceneUpdated();
    }

    public void _on_enable_null_subscene_confirmation_dialog_confirmed()
    {
        this.dtgeSceneEditContainer.DtgeScene.EnableNullSubscene();
        this.dtgeSceneEditContainer.UpdateUIFromScene();
        this.HandleSceneUpdated();
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
        DtgeCore.Scene newDtgeScene = new DtgeCore.Scene();
        DtgeSceneTabInfo newDtgeSceneTabInfo;
        newDtgeSceneTabInfo.dtgeScene = newDtgeScene;
        newDtgeSceneTabInfo.path = null;
        newDtgeSceneTabInfo.saved = false;

        this.dtgeSceneTabBar.AddTab("(new scene)*");
        int newTabIndex = this.dtgeSceneTabBar.TabCount - 1;
        this.dtgeSceneTabBar.SetTabMetadata(newTabIndex, this.nextKeyforOpenDtgeSceneDictionary);
        this.openDtgeSceneDictionary.Add(this.nextKeyforOpenDtgeSceneDictionary, newDtgeSceneTabInfo);
        this.nextKeyforOpenDtgeSceneDictionary++;
        this.dtgeSceneTabBar.CurrentTab = newTabIndex;

        this.dtgeSceneEditContainer.DtgeScene = newDtgeSceneTabInfo.dtgeScene;
        this.dtgeSceneEditContainer.UpdateUIFromScene();
        this.anyEditsMade = true;
    }

    private void createOpenedSceneTab(DtgeCore.Scene scene, string path)
    {
        if (!this.anyEditsMade)
        {
            this.dtgeSceneTabBar.RemoveTab(0);
        }
        DtgeSceneTabInfo openedDtgeSceneTabInfo;
        openedDtgeSceneTabInfo.dtgeScene = scene;
        openedDtgeSceneTabInfo.path = path;
        openedDtgeSceneTabInfo.saved = true;

        this.dtgeSceneTabBar.AddTab(scene.Id);
        int newTabIndex = this.dtgeSceneTabBar.TabCount - 1;
        this.dtgeSceneTabBar.SetTabMetadata(newTabIndex, this.nextKeyforOpenDtgeSceneDictionary);
        this.openDtgeSceneDictionary.Add(this.nextKeyforOpenDtgeSceneDictionary, openedDtgeSceneTabInfo);
        this.nextKeyforOpenDtgeSceneDictionary++;
        this.dtgeSceneTabBar.CurrentTab = newTabIndex;

        this.dtgeSceneEditContainer.DtgeScene = openedDtgeSceneTabInfo.dtgeScene;
        this.dtgeSceneEditContainer.UpdateUIFromScene();
        this.anyEditsMade = true;
    }

    private DtgeSceneTabInfo getCurrentDtgeSceneTabInfo()
    {
        int tabKey = this.getKeyFromTabIndex(this.dtgeSceneTabBar.CurrentTab);
        return this.openDtgeSceneDictionary[tabKey];
    }

    private void setCurrentSceneTabInfoScene(DtgeCore.Scene scene)
    {
        DtgeSceneTabInfo currentDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();
        currentDtgeSceneTabInfo.dtgeScene = scene;
        this.openDtgeSceneDictionary[this.getKeyFromTabIndex(this.dtgeSceneTabBar.CurrentTab)] = currentDtgeSceneTabInfo;
    }

    private void setCurrentSceneTabInfoPath(string path)
    {
        DtgeSceneTabInfo currentDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();
        currentDtgeSceneTabInfo.path = path;
        this.openDtgeSceneDictionary[this.getKeyFromTabIndex(this.dtgeSceneTabBar.CurrentTab)] = currentDtgeSceneTabInfo;
    }

    private void setCurrentSceneTabInfoSaved(bool saved)
    {
        DtgeSceneTabInfo currentDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();
        currentDtgeSceneTabInfo.saved = saved;
        this.openDtgeSceneDictionary[this.getKeyFromTabIndex(this.dtgeSceneTabBar.CurrentTab)] = currentDtgeSceneTabInfo;
    }

    private void updateTabTitle(int tabIndex)
    {
        if (this.dtgeSceneTabBar.TabCount == 0)
        {
            this.createNewSceneTab();
        }

        DtgeSceneTabInfo dtgeSceneTabInfo = this.openDtgeSceneDictionary[this.getKeyFromTabIndex(tabIndex)];

        string updatedTabTitle;

        if (dtgeSceneTabInfo.dtgeScene.Id.Equals(""))
        {
            updatedTabTitle = "(empty id)";
        }
        else
        {
            updatedTabTitle = dtgeSceneTabInfo.dtgeScene.Id;
        }

        if (!dtgeSceneTabInfo.saved)
        {
            updatedTabTitle += " *";
        }

        this.dtgeSceneTabBar.SetTabTitle(tabIndex, updatedTabTitle);
    }

    private void removeDtgeSceneTab(int tabIndex)
    {
        this.openDtgeSceneDictionary.Remove(this.getKeyFromTabIndex(tabIndex));
        this.dtgeSceneTabBar.RemoveTab(tabIndex);

        if (this.dtgeSceneTabBar.TabCount > 0)
        {
            DtgeSceneTabInfo newActiveDtgeSceneTabInfo = this.getCurrentDtgeSceneTabInfo();
            this.dtgeSceneEditContainer.DtgeScene = newActiveDtgeSceneTabInfo.dtgeScene;
            this.dtgeSceneEditContainer.UpdateUIFromScene();
        }
        else
        {
            this.dtgeSceneEditContainer.DtgeScene = new DtgeCore.Scene();
            this.dtgeSceneEditContainer.UpdateUIFromScene();
        }
    }

    private void saveCurrentSceneToPath(string path)
    {
        this.dtgeSceneEditContainer.FlushChangesForSave();
        Editor.saveSceneToPath(this.dtgeSceneEditContainer.DtgeScene, path);
        setCurrentSceneTabInfoPath(path);
        setCurrentSceneTabInfoSaved(true);
        this.updateTabTitle(this.dtgeSceneTabBar.CurrentTab);
    }

    private static void saveSceneToPath(DtgeCore.Scene scene, string path)
    {
        FileAccess sceneFile = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (sceneFile == null)
        {
            return; // TODO
        }
        else
        {
            string serializedScene = scene.Serialize();
            sceneFile.StoreString(serializedScene);

            sceneFile.Close();
        }
    }

    private void popSaveYesNoCancelDialog(DtgeSceneTabInfo dtgeSceneTabInfo, int tabIndex, bool closeTab)
    {
        if (this.anyEditsMade)
        {
            this.saveYesNoCancelDialog.Size = this.smallDialogInitialSize;
            string sceneDisplayName = "";
            if (dtgeSceneTabInfo.dtgeScene.Id.Length > 0)
            {
                sceneDisplayName = "Scene [" + dtgeSceneTabInfo.dtgeScene.Id + "]";
            }
            else
            {
                sceneDisplayName = "unnamed scene";
            }
            this.saveYesNoCancelDialog.SetDialogText("Would you like to save " + sceneDisplayName+ " before closing it?");
            this.saveYesNoCancelDialog.OnYesSelected = () =>
            {
                if (dtgeSceneTabInfo.path != null)
                {
                    Editor.saveSceneToPath(dtgeSceneTabInfo.dtgeScene, dtgeSceneTabInfo.path);
                    if (closeTab)
                    {
                        this.removeDtgeSceneTab(tabIndex);
                    }
                }
                else
                {
                    if (closeTab)
                    {
                        this.saveAsAndCloseFileDialog.Popup();
                    }
                    else
                    {
                        this.saveAsFileDialog.Popup();
                    }
                }
            };
            this.saveYesNoCancelDialog.OnNoSelected = () =>
            {
                if (closeTab)
                {
                    this.removeDtgeSceneTab(tabIndex);
                }
            };
            this.saveYesNoCancelDialog.OnCancelSelected = () =>
            {
                if (this.currentState == EditorState.Closing)
                {
                    this.currentState = EditorState.Active;
                }
            };
            this.saveYesNoCancelDialog.Popup();
        }
    }

    private int getKeyFromTabIndex(int tabIndex)
    {
        return (int)this.dtgeSceneTabBar.GetTabMetadata(tabIndex);
    }
}
