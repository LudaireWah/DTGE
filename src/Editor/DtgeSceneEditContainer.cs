using System;

using Godot;

using DtgeGodotCommon;

namespace DtgeEditor;

/**
 * This is the root control for the Godot scene that displays and modifies DTGE scenes and their
 * components.
 */
public partial class DtgeSceneEditContainer : Control
{
	OptionListContainer optionListContainer;

	LineEdit dtgeSceneNameEntry;
	Button addSceneImageButton;
	HBoxContainer sceneImageHboxContainer;
	Button removeSceneImageButton;
	OptionButton sceneImagePositionOptionButton;
	Button sceneImageChooseFileButton;
	Label sceneImagePathLabel;

	SubsceneListContainer subsceneListContainer;

	Button dtgeSceneTextCopySnippetsButton;
	Button dtgeSceneTextPasteSnippetsButton;

	SnippetListContainer snippetListContainer;

	VBoxContainer dtgeSceneTextPreviewContainer;
	Button dtgeSceneTextPreviewRandomizeButton;
	RichTextLabel dtgeSceneTextPreviewRichTextLabel;

	AcceptDialog pasteSnippetsFailedAcceptDialog;
	FileDialog chooseImageFileDialog;

	private bool sceneChangedSinceLastUpdate;
	private DtgeCore.Editing.SceneEditable _dtgeSceneEditable;
	public DtgeCore.Editing.SceneEditable DtgeSceneEditable
	{
		get { return this._dtgeSceneEditable; }
		set
		{
			this._dtgeSceneEditable = value;
			this.sceneChangedSinceLastUpdate = true;
		}
	}

	public Action<DtgeCore.SceneId> OnTryOpenScene;
	public Action OnSceneUpdated;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionListContainer = GodotUtilities.GetNodeSmart<OptionListContainer>(this, "OptionListContainer", GodotEditorErrorHandler.InvokeError);
		
		this.dtgeSceneNameEntry = GodotUtilities.GetNodeSmart<LineEdit>(this, "VBoxContainer/PropertiesContainer/PropertyEntryContainer/SceneIdAndAddImageHBoxContainer/SceneNameLineEdit", GodotEditorErrorHandler.InvokeError);
		this.addSceneImageButton = GodotUtilities.GetNodeSmart<Button>(this, "VBoxContainer/PropertiesContainer/PropertyEntryContainer/SceneIdAndAddImageHBoxContainer/AddSceneImageButton", GodotEditorErrorHandler.InvokeError);
		this.sceneImageHboxContainer = GodotUtilities.GetNodeSmart<HBoxContainer>(this, "VBoxContainer/SceneImageHBoxContainer", GodotEditorErrorHandler.InvokeError);
		this.removeSceneImageButton = GodotUtilities.GetNodeSmart<Button>(this, "VBoxContainer/SceneImageHBoxContainer/RemoveSceneImageButton", GodotEditorErrorHandler.InvokeError);
		this.sceneImagePositionOptionButton = GodotUtilities.GetNodeSmart<OptionButton>(this, "VBoxContainer/SceneImageHBoxContainer/ImagePositionOptionButton", GodotEditorErrorHandler.InvokeError);
		this.sceneImageChooseFileButton = GodotUtilities.GetNodeSmart<Button>(this, "VBoxContainer/SceneImageHBoxContainer/ChooseImageButton", GodotEditorErrorHandler.InvokeError);
		this.sceneImagePathLabel = GodotUtilities.GetNodeSmart<Label>(this, "VBoxContainer/SceneImageHBoxContainer/ImagePathLabel", GodotEditorErrorHandler.InvokeError);

		this.subsceneListContainer = GodotUtilities.GetNodeSmart<SubsceneListContainer>(this, "VBoxContainer/SubsceneListContainer", GodotEditorErrorHandler.InvokeError);

		this.dtgeSceneTextCopySnippetsButton = GodotUtilities.GetNodeSmart<Button>(this, "VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextCopySnippetsButton", GodotEditorErrorHandler.InvokeError);
		this.dtgeSceneTextPasteSnippetsButton = GodotUtilities.GetNodeSmart<Button>(this, "VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextPasteSnippetsButton", GodotEditorErrorHandler.InvokeError);
		
		this.snippetListContainer = GodotUtilities.GetNodeSmart<SnippetListContainer>(this, "VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SnippetListContainer", GodotEditorErrorHandler.InvokeError);

		this.dtgeSceneTextPreviewContainer = GodotUtilities.GetNodeSmart<VBoxContainer>(this, "VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer", GodotEditorErrorHandler.InvokeError);
		this.dtgeSceneTextPreviewRandomizeButton = GodotUtilities.GetNodeSmart<Button>(this, "VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/HBoxContainer/SceneTextPreviewRandomizeButton", GodotEditorErrorHandler.InvokeError);
		this.dtgeSceneTextPreviewRichTextLabel = GodotUtilities.GetNodeSmart<RichTextLabel>(this, "VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/SceneTextPreviewRichTextLabel", GodotEditorErrorHandler.InvokeError);

		this.pasteSnippetsFailedAcceptDialog = GodotUtilities.GetNodeSmart<AcceptDialog>(this, "PasteSnippetsFailedAcceptDialog", GodotEditorErrorHandler.InvokeError);
		this.chooseImageFileDialog = GodotUtilities.GetNodeSmart<FileDialog>(this, "ChooseImageFileDialog", GodotEditorErrorHandler.InvokeError);

		this.optionListContainer.OnTryOpenScene = this.HandleTryOpenScene;
		this.optionListContainer.DtgeSceneEditable = this.DtgeSceneEditable;
		this.snippetListContainer.DtgeSceneEditable = this.DtgeSceneEditable;
		this.subsceneListContainer.DtgeSceneEditable = this.DtgeSceneEditable;

		this.UpdateFromEditables();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if ((this.DtgeSceneEditable != null && this.DtgeSceneEditable.NeedsUIUpdate) ||
			this.sceneChangedSinceLastUpdate)
		{
			this.UpdateFromEditables();
		}
	}

	public void UpdateFromEditables()
	{
		if (this.IsNodeReady())
		{
			if (this.DtgeSceneEditable == null)
			{
				this.Visible = false;
			}
			else
			{
				try
				{
					this.Visible = true;
					this.updateSceneHeader();
					this.setSceneTextPreviewText(true);

					this.updateSceneTextPreview();

					this.optionListContainer.DtgeSceneEditable = this.DtgeSceneEditable;
					this.optionListContainer.UpdateFromEditables();

					this.subsceneListContainer.DtgeSceneEditable = this.DtgeSceneEditable;
					this.subsceneListContainer.UpdateFromEditables();

					this.snippetListContainer.DtgeSceneEditable = this.DtgeSceneEditable;
					this.snippetListContainer.UpdateFromEditables();
					
					if (!this.sceneChangedSinceLastUpdate)
					{
						this.OnSceneUpdated();
					}

					this.DtgeSceneEditable.NotifyUIUpdateDone();
					this.sceneChangedSinceLastUpdate = false;
				}
				catch (Exception exception)
				{
					GodotEditorErrorHandler.InvokeError("An unknown error while updating the editor UI.\r\nException: " + exception.Message);
				}
			}
		}
	}

	public void NotifyGameDataChanged()
	{
		this.UpdateFromEditables();
	}

	public void RestoreFromSerializedSceneJson(string serializedSceneJson)
	{
		this.DtgeSceneEditable =
			DtgeCore.Editing.SceneEditable.DeserializeFromJsonString(serializedSceneJson);
	}

	public void GiveIdEntryFocus()
	{
		this.dtgeSceneNameEntry.GrabFocus();
	}

	private void HandleTryOpenScene(DtgeCore.SceneId sceneId)
	{
		this.OnTryOpenScene(sceneId);
	}

	public void AddSubscene(string subsceneName)
	{
		DtgeCore.Editing.SubsceneEditable newSubscene =
			this.DtgeSceneEditable.AllocateNewSubscene();
		newSubscene.Name = subsceneName;
		this.DtgeSceneEditable.TrySetCurrentSubscene(newSubscene.Name);
	}

	public void _on_id_line_edit_text_changed(string newText)
	{
		if (!DtgeCore.Editing.UserDefinedNameValidator.IsValidName(newText))
		{
			GodotUtilities.PlayTextEntryErrorSound();
			string correctedName = DtgeCore.Editing.UserDefinedNameValidator.CorrectName(newText);
			this.DtgeSceneEditable.Name = correctedName;
			int oldCaretColumn = this.dtgeSceneNameEntry.CaretColumn;
			this.dtgeSceneNameEntry.Text = correctedName;
			this.dtgeSceneNameEntry.CaretColumn = oldCaretColumn;
		}
		else
		{
			this.DtgeSceneEditable.Name = newText;
		}

		if (this.OnSceneUpdated != null)
		{
			this.OnSceneUpdated();
		}
	}

	public void _on_scene_text_copy_snippets_button_pressed()
	{
		DisplayServer.ClipboardSet(this.DtgeSceneEditable.GetCopyableText());
	}

	public void _on_scene_text_paste_snippets_button_pressed()
	{
		if (DisplayServer.ClipboardHas())
		{
			bool success =
				this.DtgeSceneEditable.RestoreFromPastedText(DisplayServer.ClipboardGet());
			if (success)
			{
				this.snippetListContainer.UpdateFromEditables();
			}
			else
			{
				this.pasteSnippetsFailedAcceptDialog.Popup();
			}
		}
	}

	public void _on_scene_text_preview_randomize_button_pressed()
	{
		this.setSceneTextPreviewText(false);
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

	public void _on_add_scene_image_button_pressed()
	{
		this.DtgeSceneEditable.RenderImage = true;
		this.UpdateFromEditables();
	}

	public void _on_choose_image_button_pressed()
	{
		DtgeCore.GameData gameData = DtgeCore.GameData.GetGameData();
		this.chooseImageFileDialog.RootSubfolder = gameData.SceneImageDirectoryPath;
		this.chooseImageFileDialog.Popup();
	}

	public void _on_image_position_option_button_item_selected(int indexSelected)
	{
		this.DtgeSceneEditable.ImagePosition = (DtgeCore.Scene.SceneImagePosition)indexSelected;
	}

	public void _on_remove_scene_image_button_pressed()
	{
		this.DtgeSceneEditable.RenderImage = false;
		this.DtgeSceneEditable.ImagePath = null;
		this.DtgeSceneEditable.ImagePosition = (DtgeCore.Scene.SceneImagePosition)0;
		this.UpdateFromEditables();
	}

	public void _on_choose_image_file_dialog_file_selected(string filePathSelected)
	{
		this.DtgeSceneEditable.ImagePath = filePathSelected;
		this.UpdateFromEditables();
	}

	private void updateSceneHeader()
	{
		if (this.dtgeSceneNameEntry.Text != this.DtgeSceneEditable.Name)
		{
			this.dtgeSceneNameEntry.Text = this.DtgeSceneEditable.Name;
		}
		this.addSceneImageButton.Visible = !this.DtgeSceneEditable.RenderImage;
		this.sceneImageHboxContainer.Visible = this.DtgeSceneEditable.RenderImage;
		this.sceneImagePositionOptionButton.Selected = (int)this.DtgeSceneEditable.ImagePosition;
		this.sceneImagePathLabel.Text = this.DtgeSceneEditable.ImagePath;
	}

	private void updateSceneTextPreview()
	{
		bool randomModeSnippetFound = false;
		for (int snippetIndex = 0;
			snippetIndex < this.DtgeSceneEditable.GetSnippetCount();
			snippetIndex++)
		{
			DtgeCore.Editing.SnippetEditable snippetEditable =
				this.DtgeSceneEditable.GetSnippetByIndex(snippetIndex);
			if (snippetEditable.CurrentMode == DtgeCore.Snippet.Mode.Random)
			{
				randomModeSnippetFound = true;
			}
		}

		this.dtgeSceneTextPreviewRandomizeButton.Visible = randomModeSnippetFound;

		this.setSceneTextPreviewText(true);
	}

	private void setSceneTextPreviewText(bool preserveRandomization)
	{
		this.dtgeSceneTextPreviewRichTextLabel.Text =
			this.DtgeSceneEditable.CalculateDebugSceneText(preserveRandomization);
	}
}
