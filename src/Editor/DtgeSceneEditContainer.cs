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
    Button dtgeSceneTextCopyAllButton;
    Button dtgeSceneTextPasteAllButton;
    SnippetListContainer snippetListContainer;
    VBoxContainer dtgeSceneTextPreviewContainer;
    Button dtgeSceneTextPreviewRandomizeButton;
    RichTextLabel dtgeSceneTextPreviewRichTextLabel;

    private bool uiNeedUpdate;

    private DtgeCore.Scene dtgeScene;
    public DtgeCore.Scene DtgeScene
    {
        get { return dtgeScene; }
        set
        {
            this.dtgeScene = value;
            this.uiNeedUpdate = true;
        }
    }

	private Action<string> tryOpenSceneAction;
	public Action<string> TryOpenSceneAction
	{
		get
		{
			return this.tryOpenSceneAction;
		}
		set
		{
			this.tryOpenSceneAction = value;
            this.optionEditList.TryOpenSceneAction = value;
		}
	}

    private Action onSceneUpdated;
    public Action OnSceneUpdated
	{
        get
        {
            return this.onSceneUpdated;
        }
        set
        {
            this.onSceneUpdated = value;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.optionEditList = GetNode<OptionEditList>("OptionEditList");
        this.dtgeSceneIdEntry = GetNode<LineEdit>("VBoxContainer/PropertiesContainer/PropertyEntryContainer/IdLineEdit");
        this.dtgeSceneTextCopyAllButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextCopyAllButton");
        this.dtgeSceneTextPasteAllButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextPasteAllButton");
        this.snippetListContainer = GetNode<SnippetListContainer>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SnippetListContainer");
        this.dtgeSceneTextPreviewContainer = GetNode<VBoxContainer>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer");
        this.dtgeSceneTextPreviewRandomizeButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/HBoxContainer/SceneTextPreviewRandomizeButton");
        this.dtgeSceneTextPreviewRichTextLabel = GetNode<RichTextLabel>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/SceneTextPreviewRichTextLabel");

        this.optionEditList.OnOptionListUpdated = this.OnOptionListUpdated;
        this.snippetListContainer.OnSnippetListUpdated = this.OnSnippetListUpdated;

        this.DtgeScene = new DtgeCore.Scene();
        this.optionEditList.DtgeScene = this.DtgeScene;
        this.snippetListContainer.DtgeScene = this.DtgeScene;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (this.uiNeedUpdate)
        {
            this.UpdateUIFromScene();
            this.uiNeedUpdate = false;
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

    public void OnOptionListUpdated()
    {
        this.onSceneUpdated();
    }

    public void OnSnippetListUpdated()
    {
        this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeScene.CalculateSceneText(true);
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
        //DisplayServer.ClipboardSet(this.dtgeScene.SceneText);
    }

    public void _on_scene_text_paste_all_button_pressed()
    {
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
}
