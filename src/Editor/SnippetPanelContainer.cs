using Godot;
using System;
using System.Collections.Generic;

namespace DtgeEditor;

/**
 * The root node for the Godot scene responsible for authoring
 * DTGE Snippets.
 */
public partial class SnippetPanelContainer : PanelContainer
{
	private static readonly DtgeCore.Snippet.Mode[] SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING =
	{
		DtgeCore.Snippet.Mode.Simple,
		DtgeCore.Snippet.Mode.Subscene,
		DtgeCore.Snippet.Mode.If,
		DtgeCore.Snippet.Mode.IfElse,
		DtgeCore.Snippet.Mode.Random
	};

	OptionButton conditionalModeOptionButton;
	HBoxContainer snippetTabsHBoxContainer;
	Button newTabButton;
	TabBar snippetTabBar;
	HBoxContainer conditionalHBoxContainer;
	Label conditionalPrefixLabel;
	LineEdit conditionalLineEdit;
	Label conditionalSuffixLabel;
	TextEdit snippetTextEdit;
	VBoxContainer entitySetterListVBoxContainer;
	Button addEntitySetterButton;

	private bool uiNeedsUpdate;
	private DtgeCore.Snippet boundSnippet;
	public DtgeCore.Snippet BoundSnippet
	{
		get
		{
			return this.boundSnippet;
		}
		set
		{
			this.boundSnippet = value;
			this.uiNeedsUpdate = true;
		}
	}

	public Action<bool> OnSnippetUpdated; // (bool snippetCountChanged)
	public Action<SnippetPanelContainer> OnSnippetMovedUp;
	public Action<SnippetPanelContainer> OnSnippetMovedDown;
	public Action<SnippetPanelContainer> OnSnippetDeleted;

	private DtgeCore.Snippet.Variation lastVariationSelectedByTab;

	public override void _Ready()
	{
		this.conditionalModeOptionButton = GetNode<OptionButton>("SnippetMarginContainer/SnippetVBoxContainer/SnippetHeaderContainer/ConditionalModeOptionButton");
		this.snippetTabsHBoxContainer = GetNode<HBoxContainer>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer");
		this.newTabButton = GetNode<Button>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer/NewTabButton");
		this.snippetTabBar = GetNode<TabBar>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer/SnippetTabBar");
		this.conditionalHBoxContainer = GetNode<HBoxContainer>("SnippetMarginContainer/SnippetVBoxContainer/ConditionalHBoxContainer");
		this.conditionalPrefixLabel = GetNode<Label>("SnippetMarginContainer/SnippetVBoxContainer/ConditionalHBoxContainer/ConditionalPrefixLabel");
		this.conditionalLineEdit = GetNode<LineEdit>("SnippetMarginContainer/SnippetVBoxContainer/ConditionalHBoxContainer/ConditionalLineEdit");
		this.conditionalSuffixLabel = GetNode<Label>("SnippetMarginContainer/SnippetVBoxContainer/ConditionalHBoxContainer/ConditionalSuffixLabel");
		this.snippetTextEdit = GetNode<TextEdit>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTextEdit");
		this.entitySetterListVBoxContainer = GetNode<VBoxContainer>("SnippetMarginContainer/SnippetVBoxContainer/EntitySetterListVBoxContainer");

		snippetTextEdit.FocusMode = FocusModeEnum.Click;

		this.UpdateUIFromSnippet();
	}

	public override void _Process(double delta)
	{
		if (this.uiNeedsUpdate)
		{
			this.UpdateUIFromSnippet();
			this.uiNeedsUpdate = false;
		}
	}

	public void FlushChangesForSave()
	{
		this.updateSnippetFromUI();
	}

	private void updateSnippetFromUI()
	{
		this.BoundSnippet.SetVariationText(snippetTabBar.CurrentTab, this.snippetTextEdit.Text);
		this.OnSnippetUpdated(false);
	}

	public void UpdateUIFromSnippet()
	{
		if (this.boundSnippet == null)
		{
			// error handling?
		}
		else
		{
			this.conditionalModeOptionButton.Selected = (int)(this.boundSnippet.CurrentMode);

			this.conditionalModeOptionButton.SetItemDisabled((int)DtgeCore.Snippet.Mode.Subscene, this.boundSnippet.IsSubsceneModeDisabled());
			if (this.boundSnippet.IsSubsceneModeDisabled())
			{
				this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.Mode.Subscene, "Subscene mode requires that the scene has subscenes");
			}
			else
			{
				this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.Mode.Subscene, null);
			}

			this.updateVariationTabsFromSnippet();

			this.updateConditionalUI();

			DtgeCore.Snippet.Variation currentVariation = this.boundSnippet.GetVariation(this.snippetTabBar.CurrentTab);

			this.snippetTextEdit.Text = currentVariation.Text;

			this.updateUIForEntitySetters(currentVariation.EntitySetters);
		}
	}

	private void updateVariationTabsFromSnippet()
	{		
		for (int variationIndex = 0; variationIndex < this.boundSnippet.GetVariationCount(); variationIndex++)
		{
			if (this.snippetTabBar.TabCount > variationIndex)
			{
				this.snippetTabBar.SetTabTitle(variationIndex, this.boundSnippet.GetVariationName(variationIndex));
			}
			else
			{
				this.snippetTabBar.AddTab(this.boundSnippet.GetVariationName(variationIndex));
			}
		}

		while (this.snippetTabBar.TabCount > this.boundSnippet.GetVariationCount())
		{
			this.snippetTabBar.RemoveTab(this.snippetTabBar.TabCount - 1);
		}

		switch (this.BoundSnippet.CurrentMode)
		{
		case DtgeCore.Snippet.Mode.Simple:
			this.snippetTabsHBoxContainer.Visible = false;
			this.newTabButton.Visible = false;
			break;
		case DtgeCore.Snippet.Mode.Subscene:
			this.snippetTabsHBoxContainer.Visible = true;
			this.newTabButton.Visible = false;
			this.snippetTabBar.TabCloseDisplayPolicy = TabBar.CloseButtonDisplayPolicy.ShowNever;
			break;
		case DtgeCore.Snippet.Mode.If:
			this.snippetTabsHBoxContainer.Visible = false;
			this.newTabButton.Visible = false;
			break;
		case DtgeCore.Snippet.Mode.IfElse:
			this.snippetTabsHBoxContainer.Visible = true;
			this.newTabButton.Visible = true;
			if (this.snippetTabBar.TabCount > 2)
			{
				this.snippetTabBar.TabCloseDisplayPolicy = TabBar.CloseButtonDisplayPolicy.ShowActiveOnly;
			}
			else
			{
				this.snippetTabBar.TabCloseDisplayPolicy = TabBar.CloseButtonDisplayPolicy.ShowNever;
			}
			break;
		case DtgeCore.Snippet.Mode.Random:
			this.snippetTabsHBoxContainer.Visible = true;
			this.newTabButton.Visible = true;
			if (this.snippetTabBar.TabCount > 1)
			{
				this.snippetTabBar.TabCloseDisplayPolicy = TabBar.CloseButtonDisplayPolicy.ShowActiveOnly;
			}
			else
			{
				this.snippetTabBar.TabCloseDisplayPolicy = TabBar.CloseButtonDisplayPolicy.ShowNever;
			}
			break;
		default:
			throw new NotImplementedException();
		}

		if (this.lastVariationSelectedByTab != null)
		{
			for (int variationIndex = 0; variationIndex < this.boundSnippet.GetVariationCount(); variationIndex++)
			{
				DtgeCore.Snippet.Variation currentVariationInfo = this.boundSnippet.GetVariation(variationIndex);
				if (currentVariationInfo.Id == this.lastVariationSelectedByTab.Id &&
					variationIndex != this.snippetTabBar.CurrentTab)
				{
					this.snippetTabBar.CurrentTab = variationIndex;
					break;
				}
			}
		}

		this.lastVariationSelectedByTab = this.boundSnippet.GetVariation(this.snippetTabBar.CurrentTab);
	}

	private void createNewSnippetVariation()
	{
		this.BoundSnippet.AddVariation();
		this.UpdateUIFromSnippet();
		this.snippetTabBar.CurrentTab = this.snippetTabBar.TabCount - 1;
		this.OnSnippetUpdated(false);
	}

	private void updateConditionalUI()
	{
		switch(this.boundSnippet.CurrentMode)
		{
		case DtgeCore.Snippet.Mode.Simple:
			this.conditionalHBoxContainer.Visible = false;
			break;
		case DtgeCore.Snippet.Mode.Subscene:
			this.conditionalHBoxContainer.Visible = false;
			break;
		case DtgeCore.Snippet.Mode.If:
			this.conditionalHBoxContainer.Visible = true;
			this.conditionalPrefixLabel.Text = "if (";
			this.conditionalLineEdit.Visible = true;
			this.conditionalLineEdit.Text = this.boundSnippet.GetVariation(this.snippetTabBar.CurrentTab).ConditionalEntityName;
			this.conditionalSuffixLabel.Visible = true;
			break;
		case DtgeCore.Snippet.Mode.IfElse:
			this.conditionalHBoxContainer.Visible = true;
			if (this.snippetTabBar.CurrentTab == 0)
			{
				this.conditionalPrefixLabel.Text = "if (";
				this.conditionalLineEdit.Visible = true;
				this.conditionalLineEdit.Text = this.boundSnippet.GetVariation(this.snippetTabBar.CurrentTab).ConditionalEntityName;
				this.conditionalSuffixLabel.Visible = true;
			}
			else if (this.snippetTabBar.CurrentTab == this.snippetTabBar.TabCount - 1)
			{
				this.conditionalPrefixLabel.Text = "else";
				this.conditionalLineEdit.Visible = false;
				this.conditionalSuffixLabel.Visible = false;
			}
			else
			{
				this.conditionalPrefixLabel.Text = "else if (";
				this.conditionalLineEdit.Visible = true;
				this.conditionalLineEdit.Text = this.boundSnippet.GetVariation(this.snippetTabBar.CurrentTab).ConditionalEntityName;
				this.conditionalSuffixLabel.Visible = true;
			}
			break;
		case DtgeCore.Snippet.Mode.Random:
			this.conditionalHBoxContainer.Visible = false;
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void updateUIForEntitySetters(Dictionary<uint, DtgeCore.Snippet.EntitySetter> entitySetters)
	{
		Godot.Collections.Array<Node> entitySetterHBoxContainers = this.entitySetterListVBoxContainer.GetChildren();

		int currentEntitySetterIndex = 0;
		foreach (uint id in entitySetters.Keys)
		{
			DtgeCore.Snippet.EntitySetter currentEntitySetter = entitySetters[id];
			EntitySetterHBoxContainer currentEntitySetterHBoxContainer;

			if (currentEntitySetterIndex >= entitySetterHBoxContainers.Count)
			{
				currentEntitySetterHBoxContainer = this.addNewEntitySetterHBoxContainer(currentEntitySetter);
			}
			else
			{
				currentEntitySetterHBoxContainer = (EntitySetterHBoxContainer)entitySetterHBoxContainers[currentEntitySetterIndex];
			}
			currentEntitySetterHBoxContainer.BoundEntitySetter = currentEntitySetter;
			currentEntitySetterHBoxContainer.UpdateUI();
			currentEntitySetterIndex++;
		}

		while (entitySetters.Count < this.entitySetterListVBoxContainer.GetChildCount())
		{
			this.entitySetterListVBoxContainer.RemoveChild(this.entitySetterListVBoxContainer.GetChild(this.entitySetterListVBoxContainer.GetChildCount() - 1));
		}
	}

	private EntitySetterHBoxContainer addNewEntitySetterHBoxContainer(DtgeCore.Snippet.EntitySetter newEntitySetter)
	{
		EntitySetterHBoxContainer newEntitySetterHBoxContaner =
			((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.ENTITY_SETTER_CONTAINER_PATH)).Instantiate<EntitySetterHBoxContainer>();

		if (newEntitySetterHBoxContaner != null)
		{
			newEntitySetterHBoxContaner.BoundEntitySetter = newEntitySetter;
			newEntitySetterHBoxContaner.OnEntitySetterUpdated = this.handleEntitySetterUpdated;
			newEntitySetterHBoxContaner.OnEntitySetterDeleted = this.handleEntitySetterDeleted;
			this.entitySetterListVBoxContainer.AddChild(newEntitySetterHBoxContaner);
		}

		this.OnSnippetUpdated(false);

		return newEntitySetterHBoxContaner;
	}

	private void handleEntitySetterUpdated()
	{
		this.OnSnippetUpdated(false);
	}

	private void handleEntitySetterDeleted(EntitySetterHBoxContainer entitySetterHBoxContainer)
	{
		DtgeCore.Snippet.Variation variation = this.boundSnippet.GetVariation(this.snippetTabBar.CurrentTab);
		variation.EntitySetters.Remove(entitySetterHBoxContainer.BoundEntitySetter.Id);
		this.entitySetterListVBoxContainer.RemoveChild(entitySetterHBoxContainer);
		this.OnSnippetUpdated(false);
	}

	public void _on_snippet_text_edit_text_changed()
	{
		this.updateSnippetFromUI();
	}

	public void _on_move_up_button_pressed()
	{
		this.OnSnippetMovedUp(this);
	}

	public void _on_move_down_button_pressed()
	{
		this.OnSnippetMovedDown(this);
	}

	public void _on_delete_button_pressed()
	{
		this.OnSnippetDeleted(this);
	}

	public void _on_conditional_mode_option_button_item_selected(int modeIndex)
	{
		this.BoundSnippet.ChangeMode(SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING[modeIndex]);
		this.UpdateUIFromSnippet();
		this.OnSnippetUpdated(false);
	}

	public void _on_new_tab_button_pressed()
	{
		this.createNewSnippetVariation();
		this.updateConditionalUI();
	}

	public void _on_snippet_tab_bar_tab_close_pressed(int tabIndex)
	{
		this.snippetTabBar.RemoveTab(tabIndex);
		this.boundSnippet.RemoveVariation(tabIndex);
		this.UpdateUIFromSnippet();
	}

	public void _on_snippet_tab_bar_tab_selected(int tabIndex)
	{
		this.snippetTextEdit.Text = this.boundSnippet.GetVariationTextByIndex(tabIndex);
		this.updateUIForEntitySetters(this.boundSnippet.GetVariation(tabIndex).EntitySetters);
		this.lastVariationSelectedByTab = this.boundSnippet.GetVariation(tabIndex);
		this.updateConditionalUI();
		this.snippetTextEdit.GrabFocus();
	}

	public void _on_add_entity_setter_button_pressed()
	{
		DtgeCore.Snippet.EntitySetter newEntitySetter = new DtgeCore.Snippet.EntitySetter();

		DtgeCore.Snippet.Variation variation = this.boundSnippet.GetVariation(this.snippetTabBar.CurrentTab);
		variation.EntitySetters.Add(newEntitySetter.Id, newEntitySetter);

		this.addNewEntitySetterHBoxContainer(newEntitySetter);
	}

	public void _on_conditional_line_edit_text_changed(string newText)
	{
		this.boundSnippet.GetVariation(this.snippetTabBar.CurrentTab).ConditionalEntityName = newText;
		if (this.OnSnippetUpdated != null)
		{
			this.OnSnippetUpdated(false);
		}
	}
}
