using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeEditor;

public partial class DtgeSceneEditContainer : Control
{
	TextEdit dtgeSceneTextEntry;
	RichTextLabel dtgeSceneTextPreview;
	LineEdit dtgeSceneIdEntry;
	OptionEditList optionEditList;

	private bool uiNeedUpdate;

    private DtgeCore.Scene dtgeScene;
	public DtgeCore.Scene DtgeScene
	{
		get {  return dtgeScene; }
		set
		{
			this.dtgeScene = value;
			this.uiNeedUpdate = true;
		}
	}

	public string CachedFilePath {  get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.dtgeSceneTextEntry = GetNode<TextEdit>("ScenePropertiesAndTextContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryTextEdit");
		this.dtgeSceneTextPreview = GetNode<RichTextLabel>("ScenePropertiesAndTextContainer/SceneTextEditContainer/SceneTextPreviewContainer/SceneTextPreviewRichTextLabel");
		this.dtgeSceneIdEntry = GetNode<LineEdit>("ScenePropertiesAndTextContainer/PropertiesContainer/PropertyEntryContainer/IdLineEdit");
		this.optionEditList = GetNode<OptionEditList>("OptionEditList");

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

	private void UpdateUIFromScene()
	{
		this.dtgeSceneTextEntry.Text = this.dtgeScene.SceneText;
		this.UpdateSceneTextPreview();
		this.dtgeSceneIdEntry.Text = this.dtgeScene.Id;
		this.optionEditList.DtgeScene = this.dtgeScene;
	}

	private void UpdateSceneTextPreview()
	{
		dtgeSceneTextPreview.Text = this.dtgeScene.SceneText;
	}

	public string GetSerializedScene()
	{
		return this.dtgeScene.Serialize();
	}

	public bool RestoreFromSerializedScene(string serializedScene)
	{
		this.dtgeScene = JsonSerializer.Deserialize<DtgeCore.Scene>(serializedScene);
		this.UpdateUIFromScene();
		return true;
	}

	public void _on_scene_text_entry_text_edit_text_changed()
	{
		this.dtgeScene.SceneText = this.dtgeSceneTextEntry.Text;
		this.UpdateSceneTextPreview();
	}

	public void _on_id_line_edit_text_changed(string new_text)
	{
		this.dtgeScene.Id = new_text;
    }
}
