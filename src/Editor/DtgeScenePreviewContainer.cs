using Godot;
using System.Collections.Generic;

namespace DtgeEditor;

public partial class DtgeScenePreviewContainer : VBoxContainer
{
	CheckButton entityDisplayCheckButton;
	OptionButton subsceneSelectionOptionButton;
	Button randomizeButton;

	Button addEntitySetterButton;
	VBoxContainer entitySetterListContainer;

	RichTextLabel dtgeSceneTextRichTextLabel;

	Label entityResultsLabel;
	VBoxContainer entityResultsContainer;

	private DtgeCore.Scene dtgeScene;
	private bool uiNeedsUpdate = false;
	public DtgeCore.Scene DtgeScene
	{
		get
		{
			return this.dtgeScene;
		}
		set
		{
			this.dtgeScene = value;
			this.uiNeedsUpdate = true;
		}
	}

	private DtgeCore.Scene.SubsceneId lastSelectedSubsceneIdForTextPreviewSubsceneSelector;

	public override void _Ready()
	{
		this.entityDisplayCheckButton = GetNode<CheckButton>("ScenePreviewControlsHBoxContainer/PreviewEntityInfoCheckButton");
		this.subsceneSelectionOptionButton = GetNode<OptionButton>("ScenePreviewControlsHBoxContainer/PreviewSubsceneSelectionOptionButton");
		this.randomizeButton = GetNode<Button>("ScenePreviewControlsHBoxContainer/PreviewRandomizeButton");
		this.addEntitySetterButton = GetNode<Button>("PreviewEntityVBoxContainer/AddPreviewEntitySetterButton");
		this.entitySetterListContainer = GetNode<VBoxContainer>("PreviewEntityVBoxContainer/PreviewEntitySetterListVBoxContainer");
		this.dtgeSceneTextRichTextLabel = GetNode<RichTextLabel>("SceneTextPreviewPanelContainer/SceneTextPreviewMarginContainer/SceneTextPreviewRichTextLabel");
		this.entityResultsLabel = GetNode<Label>("PreviewEntityResultsLabel");
		this.entityResultsContainer = GetNode<VBoxContainer>("PreviewEntityResultsVBoxContainer");


	}

	public override void _Process(double delta)
	{
		if (this.uiNeedsUpdate)
		{
			this.UpdateUI();
		}
	}

	public void UpdateUI()
	{
		if (!this.IsNodeReady())
		{
			this.uiNeedsUpdate = true;
		}
		else
		{
			this.updateControls();
			this.updateEntitySetters();
			this.updateSceneText(false);
			this.updateEntityResults();

			this.uiNeedsUpdate = false;
		}
	}

	private void updateControls()
	{
		int subsceneCount = this.DtgeScene.GetSubsceneCount();
		if (subsceneCount == 0)
		{
			this.subsceneSelectionOptionButton.Visible = false;
		}
		else
		{
			this.subsceneSelectionOptionButton.Visible = true;

			for (int subsceneIndex = 0; subsceneIndex < this.dtgeScene.GetSubsceneCount(); subsceneIndex++)
			{
				DtgeCore.Scene.SubsceneId subsceneId = this.dtgeScene.GetSubsceneId(subsceneIndex);
				if (subsceneId != null)
				{
					if (subsceneIndex >= subsceneSelectionOptionButton.ItemCount)
					{
						this.subsceneSelectionOptionButton.AddItem(subsceneId.Name);
					}
					else
					{
						this.subsceneSelectionOptionButton.SetItemText(subsceneIndex, subsceneId.Name);
					}
				}
			}

			if (this.lastSelectedSubsceneIdForTextPreviewSubsceneSelector == null)
			{
				this.lastSelectedSubsceneIdForTextPreviewSubsceneSelector = this.dtgeScene.GetCurrentSubsceneId();
				this.subsceneSelectionOptionButton.Selected = this.dtgeScene.GetCurrentSubsceneIndex();
			}
			else
			{
				bool previousActiveSubsceneReselected = this.dtgeScene.SetCurrentSubscene(this.lastSelectedSubsceneIdForTextPreviewSubsceneSelector);
				this.subsceneSelectionOptionButton.Selected = this.dtgeScene.GetCurrentSubsceneIndex();
			}

			while (this.subsceneSelectionOptionButton.ItemCount > subsceneCount)
			{
				this.subsceneSelectionOptionButton.RemoveItem(this.subsceneSelectionOptionButton.ItemCount - 1);
			}
		}

		bool randomModeSnippetFound = false;
		for (int snippetIndex = 0; snippetIndex < this.dtgeScene.GetSnippetCount(); snippetIndex++)
		{
			if (this.dtgeScene.SnippetList[snippetIndex].CurrentMode == DtgeCore.Snippet.Mode.Random)
			{
				randomModeSnippetFound = true;
			}
		}

		this.randomizeButton.Visible = randomModeSnippetFound;
	}

	private void updateEntitySetters()
	{
		bool entityDisplayEnabled = this.entityDisplayCheckButton.ButtonPressed;
		this.addEntitySetterButton.Visible = entityDisplayEnabled;
		this.entitySetterListContainer.Visible = entityDisplayEnabled;
	}

	private void updateSceneText(bool randomize)
	{
		DtgeCore.SimpleEntityManager simpleEntityManager = DtgeCore.SimpleEntityManager.GetSimpleEntityManager();
		simpleEntityManager.Clear();

		for (int entitySetterIndex = 0; entitySetterIndex < this.entitySetterListContainer.GetChildCount(); entitySetterIndex++)
		{
			EntitySetterHBoxContainer entitySetterContainer = this.entitySetterListContainer.GetChildOrNull<EntitySetterHBoxContainer>(entitySetterIndex);
			
			if (entitySetterContainer != null)
			{
				DtgeCore.Snippet.EntitySetter entitySetter = entitySetterContainer.BoundEntitySetter;
				simpleEntityManager.SetEntityValue(entitySetter.Name, entitySetter.Value);
			}
		}

		this.dtgeSceneTextRichTextLabel.Text = this.dtgeScene.CalculateDebugSceneText(randomize);
	}

	private void updateEntityResults()
	{
		bool entityDisplayEnabled = this.entityDisplayCheckButton.ButtonPressed;
		this.entityResultsContainer.Visible = entityDisplayEnabled;
		this.entityResultsLabel.Visible = entityDisplayEnabled;

		if (entityDisplayEnabled)
		{
			DtgeCore.SimpleEntityManager simpleEntityManager = DtgeCore.SimpleEntityManager.GetSimpleEntityManager();
			Dictionary<string, bool>.KeyCollection entityKeys = simpleEntityManager.GetEntityKeys();

			int entityResultIndex = 0;
			foreach (string key in entityKeys)
			{
				if (entityResultIndex < this.entityResultsContainer.GetChildCount())
				{
					Label entityResultLabel = this.entityResultsContainer.GetChildOrNull<Label>(entityResultIndex);
					if (entityResultLabel != null)
					{
						entityResultLabel.Text = key + ": " + simpleEntityManager.GetEntityValue(key).ToString();
					}
				}
				else
				{
					Label newEntityResultLabel = new Label();
					newEntityResultLabel.Text = key + ": " + simpleEntityManager.GetEntityValue(key).ToString();
					this.entityResultsContainer.AddChild(newEntityResultLabel);
				}
				entityResultIndex++;
			}

			while (this.entityResultsContainer.GetChildCount() > entityKeys.Count)
			{
				this.entityResultsContainer.RemoveChild(this.entityResultsContainer.GetChild(this.entityResultsContainer.GetChildCount() - 1));
			}
		}
	}

	private void handleEntitySetterUpdated()
	{
		this.UpdateUI();
	}

	private void handleEntitySetterDeleted(EntitySetterHBoxContainer entitySetterContainer)
	{
		this.entitySetterListContainer.RemoveChild(entitySetterContainer);
	}

	public void _on_preview_entity_info_check_button_toggled(bool toggledOn)
	{
		this.UpdateUI();
	}

	public void _on_preview_subscene_selection_option_button_item_selected(int selected)
	{
		this.dtgeScene.CurrentSubsceneIndex = selected;
		this.lastSelectedSubsceneIdForTextPreviewSubsceneSelector = this.dtgeScene.GetCurrentSubsceneId();
		this.UpdateUI();
	}
	
	public void _on_preview_randomize_button_pressed()
	{
		this.updateSceneText(true);
	}

	public void _on_add_preview_entity_setter_button_pressed()
	{
		EntitySetterHBoxContainer newEntitySetterHBoxContaner =
			((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.ENTITY_SETTER_CONTAINER_PATH)).Instantiate<EntitySetterHBoxContainer>();

		if (newEntitySetterHBoxContaner != null)
		{
			newEntitySetterHBoxContaner.BoundEntitySetter = new DtgeCore.Snippet.EntitySetter();
			newEntitySetterHBoxContaner.OnEntitySetterUpdated = this.handleEntitySetterUpdated;
			newEntitySetterHBoxContaner.OnEntitySetterDeleted = this.handleEntitySetterDeleted;
			this.entitySetterListContainer.AddChild(newEntitySetterHBoxContaner);
		}
	}
}
