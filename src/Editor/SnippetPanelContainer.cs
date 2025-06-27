using DtgeGodotCommon;
using Godot;
using System;

namespace DtgeEditor;

/**
 * The root node for the Godot scene responsible for authoring
 * DTGE Snippets.
 */
public partial class SnippetPanelContainer : PanelContainer
{
	private static readonly DtgeCore.Snippet.SnippetMode[] SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING =
	{
		DtgeCore.Snippet.SnippetMode.Simple,
		DtgeCore.Snippet.SnippetMode.Subscene,
		DtgeCore.Snippet.SnippetMode.Random
	};

	OptionButton conditionalModeOptionButton;
	TextEdit snippetTextEdit;
	HBoxContainer snippetTabsHBoxContainer;
	TabBar snippetTabBar;
	Button newTabButton;

	private bool uiNeedsUpdate;
	private DtgeCore.Editing.SnippetEditable snippetEditable;
	public DtgeCore.Editing.SnippetEditable SnippetEditable
	{
		get
		{
			return this.snippetEditable;
		}
		set
		{
			this.snippetEditable = value;
			this.uiNeedsUpdate = true;
		}
	}

	public Action<SnippetPanelContainer> OnSnippetMovedUp;
	public Action<SnippetPanelContainer> OnSnippetMovedDown;
	public Action<SnippetPanelContainer> OnSnippetDeleted;

	private DtgeCore.Editing.VariationEditable lastVariationSelectedByTab;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.conditionalModeOptionButton = this.GetNode<OptionButton>("SnippetMarginContainer/SnippetVBoxContainer/SnippetHeaderContainer/ConditionalModeOptionButton");
		this.snippetTextEdit = this.GetNode<TextEdit>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTextEdit");
		this.snippetTabsHBoxContainer = this.GetNode<HBoxContainer>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer");
		this.snippetTabBar = this.GetNode<TabBar>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer/SnippetTabBar");
		this.newTabButton = this.GetNode<Button>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer/NewTabButton");

		this.snippetTextEdit.FocusMode = FocusModeEnum.Click;

		this.UpdateUIFromEditables();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.uiNeedsUpdate)
		{
			this.UpdateUIFromEditables();
			this.uiNeedsUpdate = false;
		}
	}

	private void updateEditablesFromUI()
	{
		this.SnippetEditable.SetVariationText(this.snippetTabBar.CurrentTab, this.snippetTextEdit.Text);
	}

	private void UpdateUIFromEditables()
	{
		if (this.snippetEditable == null)
		{
			// error handling?
		}
		else
		{
			this.conditionalModeOptionButton.Selected = (int)(this.snippetEditable.Mode);

			this.conditionalModeOptionButton.SetItemDisabled((int)DtgeCore.Snippet.SnippetMode.Simple, this.snippetEditable.IsSimpleModeDisabled());
			if (this.snippetEditable.IsSimpleModeDisabled())
			{
				this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.SnippetMode.Simple, "Returning to simple mode requires that you have only one variation");
			}
			else
			{
				this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.SnippetMode.Simple, null);
			}

			this.conditionalModeOptionButton.SetItemDisabled((int)DtgeCore.Snippet.SnippetMode.Subscene, this.SnippetEditable.IsSubsceneModeDisabled());
			if (this.SnippetEditable.IsSubsceneModeDisabled())
			{
				this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.SnippetMode.Subscene, "Subscene mode requires that the scene has implemented subscenes");
			}
			else
			{
				this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.SnippetMode.Subscene, null);
			}

			this.updateVariationTabsFromSnippet();

			GodotUtilities.UpdateNodeText(this.snippetTextEdit, this.SnippetEditable.GetVariationTextByIndex(this.snippetTabBar.CurrentTab));
		}
	}

	public void FlushChangesForSave()
	{
		this.updateEditablesFromUI();
	}

	public void _on_snippet_text_edit_text_changed()
	{
		this.updateEditablesFromUI();
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
		this.SnippetEditable.ChangeMode(SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING[modeIndex]);
		this.UpdateUIFromEditables();
	}

	public void _on_new_tab_button_pressed()
	{
		this.createNewSnippetVariation();
	}

	public void _on_snippet_tab_bar_tab_close_pressed(int tabIndex)
	{
		this.snippetTabBar.RemoveTab(tabIndex);
		this.SnippetEditable.RemoveVariationByIndex(tabIndex);
	}

	public void _on_snippet_tab_bar_tab_selected(int tabIndex)
	{
		GodotUtilities.UpdateNodeText(this.snippetTextEdit, this.SnippetEditable.GetVariationTextByIndex(tabIndex));
		this.lastVariationSelectedByTab = this.SnippetEditable.GetVariationEditable(tabIndex);
		this.snippetTextEdit.GrabFocus();
	}

	private void updateVariationTabsFromSnippet()
	{
		switch (this.SnippetEditable.Mode)
		{
		case DtgeCore.Snippet.SnippetMode.Simple:
			this.snippetTabsHBoxContainer.Visible = false;
			this.newTabButton.Visible = false;
			break;
		case DtgeCore.Snippet.SnippetMode.Subscene:
			this.snippetTabsHBoxContainer.Visible = true;
			this.newTabButton.Visible = false;
			this.snippetTabBar.TabCloseDisplayPolicy = TabBar.CloseButtonDisplayPolicy.ShowNever;
			break;
		case DtgeCore.Snippet.SnippetMode.Random:
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

		for (int variationIndex = 0; variationIndex < this.SnippetEditable.GetVariationCount(); variationIndex++)
		{
			if (this.snippetTabBar.TabCount > variationIndex)
			{
				this.snippetTabBar.SetTabTitle(variationIndex, this.SnippetEditable.GetVariationName(variationIndex));
			}
			else
			{
				this.snippetTabBar.AddTab(this.SnippetEditable.GetVariationName(variationIndex));
			}
		}

		while (this.snippetTabBar.TabCount > this.SnippetEditable.GetVariationCount())
		{
			this.snippetTabBar.RemoveTab(this.snippetTabBar.TabCount - 1);
		}

		if (this.lastVariationSelectedByTab != null)
		{
			for (int variationIndex = 0; variationIndex < this.SnippetEditable.GetVariationCount(); variationIndex++)
			{
				DtgeCore.Editing.VariationEditable currentVariation = this.SnippetEditable.GetVariationEditable(variationIndex);
				if (currentVariation.Id == this.lastVariationSelectedByTab.Id &&
					variationIndex != this.snippetTabBar.CurrentTab)
				{
					this.snippetTabBar.CurrentTab = variationIndex;
					break;
				}
			}
		}

		this.lastVariationSelectedByTab = this.SnippetEditable.GetVariationEditable(this.snippetTabBar.CurrentTab);
	}

	private void createNewSnippetVariation()
	{
		this.SnippetEditable.AddVariation();
		this.snippetTabBar.CurrentTab = this.snippetTabBar.TabCount - 1;
		this.UpdateUIFromEditables();
	}
}
