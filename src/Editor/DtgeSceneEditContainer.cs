using System;

using DtgeCore;

using Godot;

namespace DtgeEditor;

/**
 * This is the root control for the Godot scene that displays and modifies DTGE scenes and their
 * components.
 */
public partial class DtgeSceneEditContainer : Control
{
	OptionEditList optionEditList;

	LineEdit dtgeSceneIdEntry;
	Button addSceneImageButton;
	HBoxContainer sceneImageHboxContainer;
	Button removeSceneImageButton;
	OptionButton sceneImagePositionOptionButton;
	Button sceneImageChooseFileButton;
	Label sceneImagePathLabel;

	HBoxContainer subsceneListHBoxContainer;
	CheckButton allowNoSubsceneCheckButton;
	Button newSubsceneButton;

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

	public Action<DtgeCore.Scene.SceneId> OnTryOpenScene;
	public Action OnSceneUpdated;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionEditList = this.GetNode<OptionEditList>("OptionEditList");
		
		this.dtgeSceneIdEntry = this.GetNode<LineEdit>("VBoxContainer/PropertiesContainer/PropertyEntryContainer/SceneIdAndAddImageHBoxContainer/IdLineEdit");
		this.addSceneImageButton = this.GetNode<Button>("VBoxContainer/PropertiesContainer/PropertyEntryContainer/SceneIdAndAddImageHBoxContainer/AddSceneImageButton");
		this.sceneImageHboxContainer = this.GetNode<HBoxContainer>("VBoxContainer/SceneImageHBoxContainer");
		this.removeSceneImageButton = this.GetNode<Button>("VBoxContainer/SceneImageHBoxContainer/RemoveSceneImageButton");
		this.sceneImagePositionOptionButton = this.GetNode<OptionButton>("VBoxContainer/SceneImageHBoxContainer/ImagePositionOptionButton");
		this.sceneImageChooseFileButton = this.GetNode<Button>("VBoxContainer/SceneImageHBoxContainer/ChooseImageButton");
		this.sceneImagePathLabel = this.GetNode<Label>("VBoxContainer/SceneImageHBoxContainer/ImagePathLabel");

		this.newSubsceneButton = this.GetNode<Button>("VBoxContainer/SubsceneListHeaderHBoxContainer/NewSubsceneButton");
		this.allowNoSubsceneCheckButton = this.GetNode<CheckButton>("VBoxContainer/SubsceneListHeaderHBoxContainer/AllowNoSubsceneCheckButton");
		this.subsceneListHBoxContainer = this.GetNode<HBoxContainer>("VBoxContainer/SubsceneListHeaderHBoxContainer/SubsceneListScrollContainer/SubsceneListHBoxContainer");

		this.dtgeSceneTextCopySnippetsButton = this.GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextCopySnippetsButton");
		this.dtgeSceneTextPasteSnippetsButton = this.GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextPasteSnippetsButton");
		
		this.snippetListContainer = this.GetNode<SnippetListContainer>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SnippetListContainer");

		this.dtgeSceneTextPreviewContainer = this.GetNode<VBoxContainer>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer");
		this.dtgeSceneTextPreviewRandomizeButton = this.GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/HBoxContainer/SceneTextPreviewRandomizeButton");
		this.dtgeSceneTextPreviewRichTextLabel = this.GetNode<RichTextLabel>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/SceneTextPreviewRichTextLabel");

		this.pasteSnippetsFailedAcceptDialog = this.GetNode<AcceptDialog>("PasteSnippetsFailedAcceptDialog");
		this.chooseImageFileDialog = this.GetNode<FileDialog>("ChooseImageFileDialog");

		this.optionEditList.OnTryOpenScene = this.HandleTryOpenScene;
		this.optionEditList.DtgeSceneEditable = this.DtgeSceneEditable;
		this.snippetListContainer.DtgeSceneEditable = this.DtgeSceneEditable;

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

					this.optionEditList.DtgeSceneEditable = this.DtgeSceneEditable;
					this.optionEditList.UpdateFromEditables();

					this.updateSubsceneListFromDTGEScene();

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
					GlobalErrorHandler.InvokeError("An exception was hit while updating the editor UI. Exception: " + exception.Message);
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
		this.dtgeSceneIdEntry.GrabFocus();
	}

	private void HandleTryOpenScene(DtgeCore.Scene.SceneId sceneId)
	{
		this.OnTryOpenScene(sceneId);
	}

	public void AddSubscene(string subsceneName)
	{
		DtgeCore.Editing.SubsceneEditable newSubscene =
			this.DtgeSceneEditable.AllocateNewSubscene();
		newSubscene.Name = subsceneName;
		this.DtgeSceneEditable.SetCurrentSubscene(newSubscene);
		this.addNewSubscenePanelContainer(newSubscene);
	}

	public void _on_id_line_edit_text_changed(string new_text)
	{
		this.DtgeSceneEditable.Id = new_text;
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

	public void _on_new_subscene_button_pressed()
	{
		this.AddSubscene("");
	}

	public void _on_allow_no_subscene_check_button_toggled(bool on)
	{
		if (on)
		{
			this.DtgeSceneEditable.EnableNullSubscene();
		}
		else
		{
			this.DtgeSceneEditable.DisableNullSubscene();
		}
		this.updateSubsceneListFromDTGEScene();
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

	private void HandleSubsceneDeleted(SubscenePanelContainer subscenePanelContainer)
	{
		this.DtgeSceneEditable.RemoveSubsceneByIndex(subscenePanelContainer.GetIndex());
		this.subsceneListHBoxContainer.RemoveChild(subscenePanelContainer);
	}

	private void updateSceneHeader()
	{
		if (this.dtgeSceneIdEntry.Text != this.DtgeSceneEditable.Id)
		{
			this.dtgeSceneIdEntry.Text = this.DtgeSceneEditable.Id;
		}
		this.addSceneImageButton.Visible = !this.DtgeSceneEditable.RenderImage;
		this.sceneImageHboxContainer.Visible = this.DtgeSceneEditable.RenderImage;
		this.sceneImagePositionOptionButton.Selected = (int)this.DtgeSceneEditable.ImagePosition;
		this.sceneImagePathLabel.Text = this.DtgeSceneEditable.ImagePath;
	}

	private void updateSubsceneListFromDTGEScene()
	{
		this.allowNoSubsceneCheckButton.ButtonPressed =
			this.DtgeSceneEditable.NullSubsceneEnabled;
		for (int subsceneIndex = 0;
			subsceneIndex < this.DtgeSceneEditable.GetSubsceneCount();
			subsceneIndex++)
		{
			SubscenePanelContainer currentSubscenePanelContainer =
				this.subsceneListHBoxContainer.GetChildOrNull<SubscenePanelContainer>(
					subsceneIndex);
			if (currentSubscenePanelContainer != null)
			{
				currentSubscenePanelContainer.SubsceneEditable =
					this.DtgeSceneEditable.GetSubscene(subsceneIndex);
				currentSubscenePanelContainer.UpdateUIFromEditables();
			}
			else
			{
				this.addNewSubscenePanelContainer(
					this.DtgeSceneEditable.GetSubscene(subsceneIndex));
			}
		}

		while (this.subsceneListHBoxContainer.GetChildCount() >
			this.DtgeSceneEditable.GetSubsceneCount())
		{
			Node nodeToRemove =
				this.subsceneListHBoxContainer.GetChild(
					this.subsceneListHBoxContainer.GetChildCount() - 1);
			this.subsceneListHBoxContainer.RemoveChild(nodeToRemove);
		}

		if (this.DtgeSceneEditable.GetSubsceneCount() == 0)
		{
			this.allowNoSubsceneCheckButton.Visible = false;
		}
		else
		{
			this.allowNoSubsceneCheckButton.Visible = true;
		}

		this.updateSceneTextPreview();
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


	private void addNewSubscenePanelContainer(DtgeCore.Editing.SubsceneEditable subsceneEditable)
	{
		SubscenePanelContainer newSubscenePanelContainer =
			((PackedScene)GD.Load(
				DtgeGodotCommon.GodotConstants.SUBSCENE_PANEL_CONTAINER_PATH))
				.Instantiate<SubscenePanelContainer>();

		if (newSubscenePanelContainer != null)
		{
			newSubscenePanelContainer.SubsceneEditable = subsceneEditable;
			newSubscenePanelContainer.OnSubsceneDeleted = this.HandleSubsceneDeleted;
			this.subsceneListHBoxContainer.AddChild(newSubscenePanelContainer);
			newSubscenePanelContainer.UpdateUIFromEditables();
		}
	}

	private void setSceneTextPreviewText(bool preserveRandomization)
	{
		//this.dtgeSceneTextPreviewRichTextLabel.Text =
		//	this.DtgeSceneEditable.CalculateSceneText();
		this.dtgeSceneTextPreviewRichTextLabel.Text =
			this.DtgeSceneEditable.CalculateDebugSceneText(preserveRandomization);
		//this.dtgeSceneTextPreviewRichTextLabel.Text =
		//	this.DtgeSceneEditable.GetCopyableText();
	}
}
