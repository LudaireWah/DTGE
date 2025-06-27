using Godot;
using System;

namespace DtgeEditor;

/**
 * This is the root control for the Godot scene that displays and
 * modifies DTGE scenes and their components.
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
	OptionButton dtgeSceneTextPreviewSubsceneSelectionOptionButton;
	Button dtgeSceneTextPreviewRandomizeButton;
	RichTextLabel dtgeSceneTextPreviewRichTextLabel;

	AcceptDialog pasteSnippetAcceptDialog;
	FileDialog chooseImageFileDialog;

	private bool uiNeedsUpdate;

	private DtgeCore.Editing.SceneEditable dtgeSceneEditable;
	public DtgeCore.Editing.SceneEditable DtgeSceneEditable
	{
		get { return this.dtgeSceneEditable; }
		set
		{
			this.dtgeSceneEditable = value;
			this.uiNeedsUpdate = true;
		}
	}

	public Action<DtgeCore.SceneId> OnTryOpenScene;
	public Action OnSceneUpdated;

	private DtgeCore.Editing.SubsceneEditable lastSelectedSubsceneForTextPreviewSubsceneSelector;

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
		this.dtgeSceneTextPreviewSubsceneSelectionOptionButton = this.GetNode<OptionButton>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/HBoxContainer/SceneTextPreviewSubsceneSelectionOptionButton");
		this.dtgeSceneTextPreviewRandomizeButton = this.GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/HBoxContainer/SceneTextPreviewRandomizeButton");
		this.dtgeSceneTextPreviewRichTextLabel = this.GetNode<RichTextLabel>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/SceneTextPreviewRichTextLabel");

		this.pasteSnippetAcceptDialog = this.GetNode<AcceptDialog>("PasteSnippetsFailedAcceptDialog");
		this.chooseImageFileDialog = this.GetNode<FileDialog>("ChooseImageFileDialog");

		this.DtgeSceneEditable = new DtgeCore.Editing.SceneEditable();

		this.optionEditList.OnTryOpenScene = this.HandleTryOpenScene;
		this.optionEditList.DtgeSceneEditable = this.DtgeSceneEditable;
		this.snippetListContainer.DtgeSceneEditable = this.DtgeSceneEditable;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.DtgeSceneEditable != null && this.dtgeSceneEditable.NeedsUIUpdate)
		{
			this.UpdateUIFromScene();
			this.dtgeSceneEditable.NotifyUIUpdateDone();
		}
	}
	public void FlushChangesForSave()
	{
		this.dtgeSceneEditable.Id = this.dtgeSceneIdEntry.Text;
		this.optionEditList.FlushChangesForSave();
	}

	public void UpdateUIFromScene()
	{
		if (this.IsNodeReady())
		{
			if (this.dtgeSceneEditable == null)
			{
				this.Visible = false;
			}
			else
			{
				this.Visible = true;
				this.optionEditList.DtgeSceneEditable = this.dtgeSceneEditable;
				this.snippetListContainer.DtgeSceneEditable = this.dtgeSceneEditable;
				this.updateSceneHeader();
				this.setSceneTextPreviewText(true);
				this.updateSubsceneListFromDTGEScene();
				this.updateSceneTextPreview();
			}
		}
	}

	public void RestoreFromSerializedSceneJson(string serializedSceneJson)
	{
		this.dtgeSceneEditable = DtgeCore.Editing.SceneEditable.DeserializeFromJsonString(serializedSceneJson);
		this.UpdateUIFromScene();
	}

	public void GiveIdEntryFocus()
	{
		this.dtgeSceneIdEntry.GrabFocus();
	}

	private void HandleTryOpenScene(DtgeCore.SceneId sceneId)
	{
		this.OnTryOpenScene(sceneId);
	}

	public void AddSubscene(string subsceneName)
	{
		DtgeCore.Editing.SubsceneEditable newSubscene = this.dtgeSceneEditable.AllocateNewSubscene();
		newSubscene.Name = subsceneName;
		this.addNewSubscenePanelContainer(newSubscene);
	}

	private void HandleSubsceneDeleted(SubscenePanelContainer subscenePanelContainer)
	{
		this.dtgeSceneEditable.RemoveSubsceneByIndex(subscenePanelContainer.GetIndex());
		this.subsceneListHBoxContainer.RemoveChild(subscenePanelContainer);
	}

	private void updateSceneHeader()
	{
		if (this.dtgeSceneIdEntry.Text != this.dtgeSceneEditable.Id)
		{
			this.dtgeSceneIdEntry.Text = this.dtgeSceneEditable.Id;
		}
		this.addSceneImageButton.Visible = !this.dtgeSceneEditable.RenderImage;
		this.sceneImageHboxContainer.Visible = this.dtgeSceneEditable.RenderImage;
		this.sceneImagePositionOptionButton.Selected = (int)this.dtgeSceneEditable.ImagePosition;
		this.sceneImagePathLabel.Text = this.dtgeSceneEditable.ImagePath;
	}

	private void updateSubsceneListFromDTGEScene()
	{
		this.allowNoSubsceneCheckButton.ButtonPressed = this.dtgeSceneEditable.NullSubsceneEnabled;
		for (int editableSubsceneIndex = 0; editableSubsceneIndex < this.dtgeSceneEditable.GetSubsceneCount(); editableSubsceneIndex++)
		{
			SubscenePanelContainer currentSubscenePanelContainer =
				this.subsceneListHBoxContainer.GetChildOrNull<SubscenePanelContainer>(editableSubsceneIndex);
			if (currentSubscenePanelContainer != null)
			{
				currentSubscenePanelContainer.SubsceneEditable = this.dtgeSceneEditable.GetSubscene(editableSubsceneIndex);
			}
			else
			{
				this.addNewSubscenePanelContainer(this.dtgeSceneEditable.GetSubscene(editableSubsceneIndex));
			}
		}

		while (this.subsceneListHBoxContainer.GetChildCount() > this.dtgeSceneEditable.GetSubsceneCount())
		{
			Node nodeToRemove = this.subsceneListHBoxContainer.GetChild(this.subsceneListHBoxContainer.GetChildCount() - 1);
			this.subsceneListHBoxContainer.RemoveChild(nodeToRemove);
		}

		if (this.dtgeSceneEditable.GetSubsceneCount() == 0)
		{
			this.dtgeSceneEditable.DisableNullSubscene();
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
		if (this.dtgeSceneEditable.GetSubsceneCount() > 0)
		{
			for (int subsceneIndex = 0; subsceneIndex < this.dtgeSceneEditable.GetSubsceneCount(); subsceneIndex++)
			{
				DtgeCore.Editing.SubsceneEditable subsceneEditable = this.dtgeSceneEditable.GetSubscene(subsceneIndex);
				if (this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.ItemCount <= subsceneIndex)
				{
					this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.AddItem(subsceneEditable.Name);
				}
				else
				{
					this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.SetItemText(subsceneIndex, subsceneEditable.Name);
				}
			}
			if (this.lastSelectedSubsceneForTextPreviewSubsceneSelector != null)
			{
				bool previousActiveSubsceneReselected = this.dtgeSceneEditable.SetCurrentSubscene(this.lastSelectedSubsceneForTextPreviewSubsceneSelector);
				this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.Selected = this.dtgeSceneEditable.CurrentSubsceneIndex;
			}

			this.lastSelectedSubsceneForTextPreviewSubsceneSelector = this.dtgeSceneEditable.CurrentSubscene;
		}

		while (this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.ItemCount > this.dtgeSceneEditable.GetSubsceneCount())
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

		bool randomModeSnippetFound = false;
		for (int snippetIndex = 0; snippetIndex < this.dtgeSceneEditable.GetSnippetCount(); snippetIndex++)
		{
			DtgeCore.Editing.SnippetEditable snippetEditable = this.dtgeSceneEditable.GetSnippetByIndex(snippetIndex);
			if (snippetEditable.Mode == DtgeCore.Snippet.SnippetMode.Random)
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
			((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.SUBSCENE_PANEL_CONTAINER_PATH)).Instantiate<SubscenePanelContainer>();

		if (newSubscenePanelContainer != null)
		{
			newSubscenePanelContainer.SubsceneEditable = subsceneEditable;
			newSubscenePanelContainer.OnSubsceneDeleted = this.HandleSubsceneDeleted;
			this.subsceneListHBoxContainer.AddChild(newSubscenePanelContainer);
		}
	}

	private void setSceneTextPreviewText(bool preserveRandomization)
	{
		//this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeSceneEditable.CalculateSceneText();
		this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeSceneEditable.CalculateDebugSceneText(preserveRandomization);
		//this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeSceneEditable.GetCopyableText();
	}

	public void _on_id_line_edit_text_changed(string new_text)
	{
		this.dtgeSceneEditable.Id = new_text;
		if (this.OnSceneUpdated != null)
		{
			this.OnSceneUpdated();
		}
	}

	public void _on_scene_text_copy_snippets_button_pressed()
	{
		DisplayServer.ClipboardSet(this.dtgeSceneEditable.GetCopyableText());
	}

	public void _on_scene_text_paste_snippets_button_pressed()
	{
		if (DisplayServer.ClipboardHas())
		{
			bool success = this.dtgeSceneEditable.RestoreFromPastedText(DisplayServer.ClipboardGet());
			if (success)
			{
				this.UpdateUIFromScene();
			}
			else
			{
				this.pasteSnippetAcceptDialog.Popup();
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

	public void _on_scene_text_preview_subscene_selection_option_button_item_selected(int selected)
	{
		this.dtgeSceneEditable.CurrentSubsceneIndex = selected;
		this.lastSelectedSubsceneForTextPreviewSubsceneSelector = this.dtgeSceneEditable.CurrentSubscene;
		this.updateSceneTextPreview();
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
		this.dtgeSceneEditable.RenderImage = true;
		this.UpdateUIFromScene();
	}

	public void _on_choose_image_button_pressed()
	{
		DtgeCore.GameData gameData = DtgeCore.GameData.GetGameData();
		this.chooseImageFileDialog.RootSubfolder = gameData.SceneImageDirectoryPath;
		this.chooseImageFileDialog.Popup();
	}

	public void _on_image_position_option_button_item_selected(int indexSelected)
	{
		this.DtgeSceneEditable.ImagePosition = (DtgeCore.SceneImagePosition)indexSelected;
	}

	public void _on_remove_scene_image_button_pressed()
	{
		this.dtgeSceneEditable.RenderImage = false;
		this.dtgeSceneEditable.ImagePath = null;
		this.dtgeSceneEditable.ImagePosition = (DtgeCore.SceneImagePosition)0;
		this.UpdateUIFromScene();
	}

	public void _on_choose_image_file_dialog_file_selected(string filePathSelected)
	{
		this.dtgeSceneEditable.ImagePath = filePathSelected;
		this.UpdateUIFromScene();
	}
}
