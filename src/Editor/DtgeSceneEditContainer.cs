using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeEditor;

/**
 * This is the root control for the Godot scene that displays and
 * modifies DTGE scenes and their components.
 */
public partial class DtgeSceneEditContainer : Control
{
    OptionEditList optionEditList;

    LineEdit dtgeSceneIdEntry;
    HBoxContainer subsceneListHBoxContainer;
    Button newSubsceneButton;

    Button dtgeSceneTextCopyAllButton;
    Button dtgeSceneTextPasteAllButton;

    SnippetListContainer snippetListContainer;

    VBoxContainer dtgeSceneTextPreviewContainer;
    OptionButton dtgeSceneTextPreviewSubsceneSelectionOptionButton;
    Button dtgeSceneTextPreviewRandomizeButton;
    RichTextLabel dtgeSceneTextPreviewRichTextLabel;

    private bool uiNeedsUpdate;

    private DtgeCore.Scene dtgeScene;
    public DtgeCore.Scene DtgeScene
    {
        get { return dtgeScene; }
        set
        {
            this.dtgeScene = value;
            this.uiNeedsUpdate = true;
        }
    }

	public Action<DtgeCore.Scene.SceneId> OnTryOpenScene;
    public Action OnSceneUpdated;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.optionEditList = GetNode<OptionEditList>("OptionEditList");
        
        this.dtgeSceneIdEntry = GetNode<LineEdit>("VBoxContainer/PropertiesContainer/PropertyEntryContainer/IdLineEdit");

        this.newSubsceneButton = GetNode<Button>("VBoxContainer/SubsceneListHeaderHBoxContainer/NewSubsceneButton");
        this.subsceneListHBoxContainer = GetNode<HBoxContainer>("VBoxContainer/SubsceneListHeaderHBoxContainer/SubsceneListScrollContainer/SubsceneListHBoxContainer");

        this.dtgeSceneTextCopyAllButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextCopyAllButton");
        this.dtgeSceneTextPasteAllButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextPasteAllButton");
        
        this.snippetListContainer = GetNode<SnippetListContainer>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SnippetListContainer");

        this.dtgeSceneTextPreviewContainer = GetNode<VBoxContainer>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer");
        this.dtgeSceneTextPreviewSubsceneSelectionOptionButton = GetNode<OptionButton>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/HBoxContainer/SceneTextPreviewSubsceneSelectionOptionButton");
        this.dtgeSceneTextPreviewRandomizeButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/HBoxContainer/SceneTextPreviewRandomizeButton");
        this.dtgeSceneTextPreviewRichTextLabel = GetNode<RichTextLabel>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/SceneTextPreviewRichTextLabel");

        this.optionEditList.OnOptionListUpdated = this.HandleOptionListUpdated;
        this.optionEditList.OnTryOpenScene = this.HandleTryOpenScene;
        this.optionEditList.DtgeScene = this.DtgeScene;

        this.snippetListContainer.OnSnippetListUpdated = this.HandleSnippetListUpdated;

        this.DtgeScene = new DtgeCore.Scene();
        this.snippetListContainer.DtgeScene = this.DtgeScene;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (this.uiNeedsUpdate)
        {
            this.UpdateUIFromScene();
            this.uiNeedsUpdate = false;
        }
    }

    public void FlushChangesForSave()
    {
        //this.dtgeScene.SceneText = this.dtgeSceneTextEntry.Text;
        this.dtgeScene.Id = this.dtgeSceneIdEntry.Text;
        this.optionEditList.FlushChangesForSave();
    }

    public void UpdateUIFromScene()
    {
        //this.dtgeSceneTextEntry.Text = this.dtgeScene.SceneText;
        this.dtgeSceneIdEntry.Text = this.dtgeScene.Id;
        this.optionEditList.DtgeScene = this.dtgeScene;
        this.snippetListContainer.DtgeScene = this.dtgeScene;
        this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeScene.CalculateSceneText(true);
        this.updateSubsceneListFromDTGEScene();
        this.updateSceneTextPreview();
    }

    public void RestoreFromSerializedScene(string serializedScene)
    {
        this.dtgeScene = DtgeCore.Scene.Deserialize(serializedScene);
        this.UpdateUIFromScene();
    }

    public void GiveIdEntryFocus()
    {
        this.dtgeSceneIdEntry.GrabFocus();
    }

    public void HandleOptionListUpdated()
    {
        this.updateSceneTextPreview();
        this.OnSceneUpdated();
    }

    private void HandleSubscenesUpdated()
    {
        for (int subsceneIndex = 0; subsceneIndex < this.subsceneListHBoxContainer.GetChildCount();  subsceneIndex++)
        {
            SubscenePanelContainer currentSubscenePanelContainer = this.subsceneListHBoxContainer.GetChildOrNull<SubscenePanelContainer>(subsceneIndex);
            if(currentSubscenePanelContainer != null)
            {
                string currentSubsceneName = currentSubscenePanelContainer.GetSubsceneName();
                this.dtgeScene.SetSubsceneName(subsceneIndex, currentSubsceneName);
                if (dtgeSceneTextPreviewSubsceneSelectionOptionButton.ItemCount < subsceneIndex)
                {
                    this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.AddItem(currentSubsceneName);
                }
                else
                {
                    this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.SetItemText(subsceneIndex, currentSubsceneName);
                }
            }
        }

        this.snippetListContainer.HandleSubsceneUpdate();

        this.updateSceneTextPreview();
        this.OnSceneUpdated();
    }

    private void HandleTryOpenScene(DtgeCore.Scene.SceneId sceneId)
    {
        this.OnTryOpenScene(sceneId);
    }

    public void HandleSnippetListUpdated()
    {
        this.updateSceneTextPreview();
        this.OnSceneUpdated();
    }

    private void updateSubsceneListFromDTGEScene()
    {
        for (int subsceneIndex = 0; subsceneIndex < this.dtgeScene.GetSubsceneCount(); subsceneIndex++)
        {
            SubscenePanelContainer currentSubscenePanelContainer =
                this.subsceneListHBoxContainer.GetChildOrNull<SubscenePanelContainer>(subsceneIndex);
            if (currentSubscenePanelContainer != null)
            {
                currentSubscenePanelContainer.SetSubsceneName(this.dtgeScene.GetSubsceneName(subsceneIndex));
            }
            else
            {
                this.addNewSubscenePanelContainer(this.dtgeScene.GetSubsceneName(subsceneIndex));
            }

            if (dtgeSceneTextPreviewSubsceneSelectionOptionButton.ItemCount >= subsceneIndex)
            {
                this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.AddItem(this.dtgeScene.GetSubsceneName(subsceneIndex));
            }
            else
            {
                this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.SetItemText(subsceneIndex, this.dtgeScene.GetSubsceneName(subsceneIndex));
            }
        }

        while (this.subsceneListHBoxContainer.GetChildCount() > this.dtgeScene.GetSubsceneCount())
        {
            Node nodeToRemove = this.subsceneListHBoxContainer.GetChild(this.subsceneListHBoxContainer.GetChildCount() - 1);
            this.subsceneListHBoxContainer.RemoveChild(nodeToRemove);
        }

        this.updateSceneTextPreview();
    }

    private void updateSceneTextPreview()
    {
        for (int subsceneIndex = 0; subsceneIndex < this.dtgeScene.GetSubsceneCount(); subsceneIndex++)
        {
            if (this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.ItemCount >= subsceneIndex)
            {
                this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.AddItem(this.dtgeScene.GetSubsceneName(subsceneIndex));
            }
            else
            {
                this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.SetItemText(subsceneIndex, this.dtgeScene.GetSubsceneName(subsceneIndex));
            }
        }

        while (this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.ItemCount > this.dtgeScene.GetSubsceneCount())
        {
            this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.RemoveItem(this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.ItemCount - 1);
        }

        if (this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.ItemCount == 0)
        {
            this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.Visible = false;
        }
        else
        {
            this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.Visible = true;
        }
        this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.Selected = this.dtgeScene.GetCurrentSubsceneIndex();

        bool randomModeSnippetFound = false;
        for (int snippetIndex = 0; snippetIndex < this.dtgeScene.GetSnippetCount(); snippetIndex++)
        {
            if (this.dtgeScene.SnippetList[snippetIndex].CurrentConditionalMode == DtgeCore.Snippet.ConditionalMode.Random)
            {
                randomModeSnippetFound = true;
            }
        }

        this.dtgeSceneTextPreviewRandomizeButton.Visible = randomModeSnippetFound;

        this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeScene.CalculateSceneText(true);
    }

    private void addNewSubscenePanelContainer(string subsceneName)
    {
        SubscenePanelContainer newSubscenePanelContainer =
            ((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.SUBSCENE_PANEL_CONTAINER_PATH)).Instantiate<SubscenePanelContainer>();

        if (newSubscenePanelContainer != null)
        {
            newSubscenePanelContainer.OnSubsceneDeleted = this.deleteSubscenePanelContainer;
            newSubscenePanelContainer.OnSubsceneUpdated = this.HandleSubscenesUpdated;
            this.subsceneListHBoxContainer.AddChild(newSubscenePanelContainer);
        }
    }

    private void deleteSubscenePanelContainer(SubscenePanelContainer subscenePanelContainer)
    {
        this.dtgeScene.RemoveSubsceneByIndex(subscenePanelContainer.GetIndex());
        this.subsceneListHBoxContainer.RemoveChild(subscenePanelContainer);
        this.updateSceneTextPreview();
        this.OnSceneUpdated();
    }

    public void _on_id_line_edit_text_changed(string new_text)
    {
        this.dtgeScene.Id = new_text;
        if (this.OnSceneUpdated != null)
        {
            this.OnSceneUpdated();
        }
    }

    public void _on_scene_text_copy_all_button_pressed()
    {
        throw new NotImplementedException();
        //DisplayServer.ClipboardSet(this.dtgeScene.SceneText);
    }

    public void _on_scene_text_paste_all_button_pressed()
    {
        throw new NotImplementedException();
        //if (DisplayServer.ClipboardHas())
        //{
        //    this.dtgeScene.SceneText = DisplayServer.ClipboardGet();
        //    this.UpdateUIFromScene();
        //}
    }

    public void _on_scene_text_preview_randomize_button_pressed()
    {
        this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeScene.CalculateSceneText(false);
    }

    public void _on_show_preview_check_button_toggled(bool toggled_on)
    {
        if (toggled_on)
        {
            this.dtgeSceneTextPreviewContainer.Visible = true;
        }
        else
        {
            this.dtgeSceneTextPreviewContainer.Visible = false;
        }
    }

    public void _on_new_subscene_button_pressed()
    {
        this.dtgeScene.AddSubscene("");
        this.updateSubsceneListFromDTGEScene();
        this.HandleSubscenesUpdated();
    }

    public void _on_scene_text_preview_subscene_selection_option_button_item_selected(int selected)
    {
        this.dtgeScene.CurrentSubsceneIndex = selected;
        this.updateSceneTextPreview();
    }
}
