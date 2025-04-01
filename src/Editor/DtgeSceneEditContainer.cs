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

	SnippetListContainer snippetListContainer;

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
		
		this.dtgeSceneIdEntry = GetNode<LineEdit>("DtgeSceneAndPreviewSplitContainer/DtgeSceneEditVBoxContainer/PropertiesContainer/PropertyEntryContainer/IdLineEdit");

		this.newSubsceneButton = GetNode<Button>("DtgeSceneAndPreviewSplitContainer/DtgeSceneEditVBoxContainer/SubsceneListHeaderHBoxContainer/NewSubsceneButton");
		this.allowNoSubsceneCheckButton = GetNode<CheckButton>("DtgeSceneAndPreviewSplitContainer/DtgeSceneEditVBoxContainer/SubsceneListHeaderHBoxContainer/AllowNoSubsceneCheckButton");
		this.subsceneListHBoxContainer = GetNode<HBoxContainer>("DtgeSceneAndPreviewSplitContainer/DtgeSceneEditVBoxContainer/SubsceneListHeaderHBoxContainer/SubsceneListScrollContainer/SubsceneListHBoxContainer");

		this.snippetListContainer = GetNode<SnippetListContainer>("DtgeSceneAndPreviewSplitContainer/DtgeSceneEditVBoxContainer/SceneTextEditContainer/SceneTextEntryContainer/SnippetListContainer");

		this.pasteSnippetAcceptDialog = GetNode<AcceptDialog>("PasteSnippetsFailedAcceptDialog");
		this.chooseImageFileDialog = GetNode<FileDialog>("ChooseImageFileDialog");

		this.optionEditList.OnOptionListUpdated = this.HandleOptionListUpdated;
		this.optionEditList.OnTryOpenScene = this.HandleTryOpenScene;

		this.snippetListContainer.OnSnippetListUpdated = this.HandleSnippetListUpdated;
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
			this.Visible = false;
		}
		else
		{
			this.Visible = true;
			this.dtgeSceneIdEntry.Text = this.dtgeScene.Id;
			this.optionEditList.DtgeScene = this.dtgeScene;
			this.snippetListContainer.DtgeScene = this.dtgeScene;
			this.updateSubsceneListFromDTGEScene();
		}
	}

	public void GiveIdEntryFocus()
	{
		this.dtgeSceneIdEntry.GrabFocus();
	}

	public void HandleOptionListUpdated()
	{
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
			}
		}

		this.snippetListContainer.HandleSubsceneUpdate();

		this.OnSceneUpdated();
	}

	private void HandleTryOpenScene(DtgeCore.Scene.SceneId sceneId)
	{
		this.OnTryOpenScene(sceneId);
	}

	public void HandleSnippetListUpdated()
	{
		this.OnSceneUpdated();
	}

	private void HandleDeleteSubscenePanelContainer(SubscenePanelContainer subscenePanelContainer)
	{
		this.dtgeScene.RemoveSubsceneByIndex(subscenePanelContainer.GetIndex());
		this.updateSubsceneListFromDTGEScene();
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

	public void _on_new_subscene_button_pressed()
	{
		this.dtgeScene.AddSubscene("");
		this.updateSubsceneListFromDTGEScene();
		this.HandleSubscenesUpdated();
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
