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

	private DtgeCore.Scene.SubsceneId lastSelectedSubsceneIdForTextPreviewSubsceneSelector;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionEditList = GetNode<OptionEditList>("OptionEditList");
		
		this.dtgeSceneIdEntry = GetNode<LineEdit>("VBoxContainer/PropertiesContainer/PropertyEntryContainer/SceneIdAndAddImageHBoxContainer/IdLineEdit");
		this.addSceneImageButton = GetNode<Button>("VBoxContainer/PropertiesContainer/PropertyEntryContainer/SceneIdAndAddImageHBoxContainer/AddSceneImageButton");
		this.sceneImageHboxContainer = GetNode<HBoxContainer>("VBoxContainer/SceneImageHBoxContainer");
		this.removeSceneImageButton = GetNode<Button>("VBoxContainer/SceneImageHBoxContainer/RemoveSceneImageButton");
		this.sceneImagePositionOptionButton = GetNode<OptionButton>("VBoxContainer/SceneImageHBoxContainer/ImagePositionOptionButton");
		this.sceneImageChooseFileButton = GetNode<Button>("VBoxContainer/SceneImageHBoxContainer/ChooseImageButton");
		this.sceneImagePathLabel = GetNode<Label>("VBoxContainer/SceneImageHBoxContainer/ImagePathLabel");

		this.newSubsceneButton = GetNode<Button>("VBoxContainer/SubsceneListHeaderHBoxContainer/NewSubsceneButton");
		this.allowNoSubsceneCheckButton = GetNode<CheckButton>("VBoxContainer/SubsceneListHeaderHBoxContainer/AllowNoSubsceneCheckButton");
		this.subsceneListHBoxContainer = GetNode<HBoxContainer>("VBoxContainer/SubsceneListHeaderHBoxContainer/SubsceneListScrollContainer/SubsceneListHBoxContainer");

		this.dtgeSceneTextCopySnippetsButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextCopySnippetsButton");
		this.dtgeSceneTextPasteSnippetsButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SceneTextEntryHeader/SceneTextPasteSnippetsButton");
		
		this.snippetListContainer = GetNode<SnippetListContainer>("VBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SnippetListContainer");

		this.dtgeSceneTextPreviewContainer = GetNode<VBoxContainer>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer");
		this.dtgeSceneTextPreviewSubsceneSelectionOptionButton = GetNode<OptionButton>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/HBoxContainer/SceneTextPreviewSubsceneSelectionOptionButton");
		this.dtgeSceneTextPreviewRandomizeButton = GetNode<Button>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/HBoxContainer/SceneTextPreviewRandomizeButton");
		this.dtgeSceneTextPreviewRichTextLabel = GetNode<RichTextLabel>("VBoxContainer/SceneTextEditContainer/SceneTextPreviewContainer/SceneTextPreviewRichTextLabel");

		this.pasteSnippetAcceptDialog = GetNode<AcceptDialog>("PasteSnippetsFailedAcceptDialog");
		this.chooseImageFileDialog = GetNode<FileDialog>("ChooseImageFileDialog");

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
		this.dtgeScene.Id = this.dtgeSceneIdEntry.Text;
		this.optionEditList.FlushChangesForSave();
	}

	public void UpdateUIFromScene()
	{
		if (this.IsNodeReady())
		{
			if (this.dtgeScene == null)
			{
				this.Visible = false;
			}
			else
			{
				this.Visible = true;
				this.optionEditList.DtgeScene = this.dtgeScene;
				this.snippetListContainer.DtgeScene = this.dtgeScene;
				this.updateSceneHeader();
				setSceneTextPreviewText(true);
				this.updateSubsceneListFromDTGEScene();
				this.updateSceneTextPreview();
			}
		}
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

	private void HandleDeleteSubscenePanelContainer(SubscenePanelContainer subscenePanelContainer)
	{
		this.dtgeScene.RemoveSubsceneByIndex(subscenePanelContainer.GetIndex());
		this.updateSubsceneListFromDTGEScene();
		this.updateSceneTextPreview();
		this.snippetListContainer.HandleSubsceneUpdate();
		this.OnSceneUpdated();
	}

	private void updateSceneHeader()
	{
		this.dtgeSceneIdEntry.Text = this.dtgeScene.Id;
		this.addSceneImageButton.Visible = !this.dtgeScene.RenderImage;
		this.sceneImageHboxContainer.Visible = this.dtgeScene.RenderImage;
		this.sceneImagePositionOptionButton.Selected = (int)this.dtgeScene.ImagePosition;
		this.sceneImagePathLabel.Text = this.dtgeScene.ImagePath;
	}

	private void updateSubsceneListFromDTGEScene()
	{
		this.allowNoSubsceneCheckButton.ButtonPressed = this.dtgeScene.AllowNullSubscene;
		for (int editableSubsceneIndex = 0; editableSubsceneIndex < this.dtgeScene.GetEditableSubsceneCount(); editableSubsceneIndex++)
		{
			SubscenePanelContainer currentSubscenePanelContainer =
				this.subsceneListHBoxContainer.GetChildOrNull<SubscenePanelContainer>(editableSubsceneIndex);
			if (currentSubscenePanelContainer != null)
			{
				currentSubscenePanelContainer.SetSubsceneName(this.dtgeScene.GetEditableSubsceneId(editableSubsceneIndex).Name);
			}
			else
			{
				this.addNewSubscenePanelContainer(this.dtgeScene.GetEditableSubsceneId(editableSubsceneIndex).Name);
			}
		}

		while (this.subsceneListHBoxContainer.GetChildCount() > this.dtgeScene.GetEditableSubsceneCount())
		{
			Node nodeToRemove = this.subsceneListHBoxContainer.GetChild(this.subsceneListHBoxContainer.GetChildCount() - 1);
			this.subsceneListHBoxContainer.RemoveChild(nodeToRemove);
		}

		if (this.dtgeScene.GetEditableSubsceneCount() == 0)
		{
			this.dtgeScene.DisableNullSubscene();
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
		if (this.dtgeScene.GetSubsceneCount() > 0)
		{
			for (int subsceneIndex = 0; subsceneIndex < this.dtgeScene.GetSubsceneCount(); subsceneIndex++)
			{
				DtgeCore.Scene.SubsceneId currentSubsceneId = this.dtgeScene.GetSubsceneId(subsceneIndex);
				if (dtgeSceneTextPreviewSubsceneSelectionOptionButton.ItemCount <= subsceneIndex)
				{
					this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.AddItem(currentSubsceneId.Name);
				}
				else
				{
					this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.SetItemText(subsceneIndex, currentSubsceneId.Name);
				}
			}
			if (this.lastSelectedSubsceneIdForTextPreviewSubsceneSelector != null)
			{
				bool previousActiveSubsceneReselected = this.dtgeScene.SetCurrentSubscene(this.lastSelectedSubsceneIdForTextPreviewSubsceneSelector);
				this.dtgeSceneTextPreviewSubsceneSelectionOptionButton.Selected = this.dtgeScene.GetCurrentSubsceneIndex();
			}

			this.lastSelectedSubsceneIdForTextPreviewSubsceneSelector = this.dtgeScene.GetCurrentSubsceneId();
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

		bool randomModeSnippetFound = false;
		for (int snippetIndex = 0; snippetIndex < this.dtgeScene.GetSnippetCount(); snippetIndex++)
		{
			if (this.dtgeScene.SnippetList[snippetIndex].CurrentMode == DtgeCore.Snippet.Mode.Random)
			{
				randomModeSnippetFound = true;
			}
		}

		this.dtgeSceneTextPreviewRandomizeButton.Visible = randomModeSnippetFound;

		setSceneTextPreviewText(true);
	}

	private void addNewSubscenePanelContainer(string subsceneName)
	{
		SubscenePanelContainer newSubscenePanelContainer =
			((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.SUBSCENE_PANEL_CONTAINER_PATH)).Instantiate<SubscenePanelContainer>();

		if (newSubscenePanelContainer != null)
		{
			newSubscenePanelContainer.SetSubsceneName(subsceneName);
			newSubscenePanelContainer.OnSubsceneDeleted = this.HandleDeleteSubscenePanelContainer;
			newSubscenePanelContainer.OnSubsceneUpdated = this.HandleSubscenesUpdated;
			this.subsceneListHBoxContainer.AddChild(newSubscenePanelContainer);
		}
	}

	private void setSceneTextPreviewText(bool preserveRandomization)
	{
		//this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeScene.CalculateSceneText();
		this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeScene.CalculateDebugSceneText(preserveRandomization);
		//this.dtgeSceneTextPreviewRichTextLabel.Text = this.dtgeScene.GetCopyableText();
	}

	public void _on_id_line_edit_text_changed(string new_text)
	{
		this.dtgeScene.Id = new_text;
		if (this.OnSceneUpdated != null)   
		{
			this.OnSceneUpdated();
		}
	}

	public void _on_scene_text_copy_snippets_button_pressed()
	{
		DisplayServer.ClipboardSet(this.dtgeScene.GetCopyableText());
	}

	public void _on_scene_text_paste_snippets_button_pressed()
	{
		if (DisplayServer.ClipboardHas())
		{
			bool success = this.dtgeScene.RestoreFromPastedText(DisplayServer.ClipboardGet());
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
		setSceneTextPreviewText(false);
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
		this.lastSelectedSubsceneIdForTextPreviewSubsceneSelector = this.dtgeScene.GetCurrentSubsceneId();
		this.updateSceneTextPreview();
	}

	public void _on_allow_no_subscene_check_button_toggled(bool on)
	{
		if (on)
		{
			this.DtgeScene.EnableNullSubscene();
		}
		else
		{
			this.DtgeScene.DisableNullSubscene();
		}
		this.updateSubsceneListFromDTGEScene();
		this.HandleSubscenesUpdated();
	}

	public void _on_add_scene_image_button_pressed()
	{
		this.dtgeScene.RenderImage = true;
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
		this.DtgeScene.ImagePosition = (DtgeCore.Scene.SceneImagePosition)indexSelected;
	}

	public void _on_remove_scene_image_button_pressed()
	{
		this.dtgeScene.RenderImage = false;
		this.dtgeScene.ImagePath = null;
		this.dtgeScene.ImagePosition = (DtgeCore.Scene.SceneImagePosition)0;
		this.UpdateUIFromScene();
	}

	public void _on_choose_image_file_dialog_file_selected(string filePathSelected)
	{
		this.dtgeScene.ImagePath = filePathSelected;
		this.UpdateUIFromScene();
	}
}
