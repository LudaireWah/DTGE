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
    TextEdit dtgeSceneTextEntry;

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
            this.optionEditList.OnSceneUpdated = value;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.optionEditList = GetNode<OptionEditList>("OptionEditList");
        this.dtgeSceneIdEntry = GetNode<LineEdit>("VBoxContainer/PropertiesContainer/PropertyEntryContainer/IdLineEdit");
        this.dtgeSceneTextCopyAllButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextCopyAllButton");
        this.dtgeSceneTextPasteAllButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextPasteAllButton");
        this.dtgeSceneTextEntry = GetNode<TextEdit>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryTextEdit");

        this.dtgeScene = new DtgeCore.Scene();
        this.optionEditList.DtgeScene = this.dtgeScene;
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
        this.dtgeScene.SceneText = this.dtgeSceneTextEntry.Text;
        this.dtgeScene.Id = this.dtgeSceneIdEntry.Text;
        this.optionEditList.FlushChangesForSave();
    }

    public void UpdateUIFromScene()
    {
        this.dtgeSceneTextEntry.Text = this.dtgeScene.SceneText;
        this.dtgeSceneIdEntry.Text = this.dtgeScene.Id;
        this.optionEditList.DtgeScene = this.dtgeScene;
    }

    public void RestoreFromSerializedScene(string serializedScene)
    {
        this.dtgeScene = JsonSerializer.Deserialize<DtgeCore.Scene>(serializedScene);
        this.UpdateUIFromScene();
    }

    public void GiveIdEntryFocus()
    {
        this.dtgeSceneIdEntry.GrabFocus();
    }

    public void _on_scene_text_entry_text_edit_text_changed()
    {
        this.dtgeScene.SceneText = this.dtgeSceneTextEntry.Text;
        if (this.OnSceneUpdated != null)
        {
            this.OnSceneUpdated();
        }
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
        DisplayServer.ClipboardSet(this.dtgeScene.SceneText);
    }

    public void _on_scene_text_paste_all_button_pressed()
    {
        if (DisplayServer.ClipboardHas())
        {
            this.dtgeScene.SceneText = DisplayServer.ClipboardGet();
            this.UpdateUIFromScene();
        }
    }
}
