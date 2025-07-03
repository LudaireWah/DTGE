using System;

using Godot;

using DtgeGodotCommon;

namespace DtgeEditor;

/**
 * The root node for the Godot scene responsible for authoring DTGE Snippets.
 */
public partial class SnippetPanelContainer : PanelContainer
{
	private static readonly DtgeCore.Snippet.Mode[] SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING =
	{
		DtgeCore.Snippet.Mode.Simple,
		DtgeCore.Snippet.Mode.Subscene,
		DtgeCore.Snippet.Mode.Random
	};

	OptionButton conditionalModeOptionButton;
	TextEdit snippetTextEdit;
	HBoxContainer snippetTabsHBoxContainer;
	TabBar snippetTabBar;
	Button newTabButton;

	public DtgeCore.Editing.SnippetEditable SnippetEditable { get; set; }

	public Action<SnippetPanelContainer> OnSnippetMovedUp;
	public Action<SnippetPanelContainer> OnSnippetMovedDown;
	public Action<SnippetPanelContainer> OnSnippetDeleted;

	public override void _Ready()
	{
		this.conditionalModeOptionButton = this.GetNode<OptionButton>("SnippetMarginContainer/SnippetVBoxContainer/SnippetHeaderContainer/ConditionalModeOptionButton");
		this.snippetTextEdit = this.GetNode<TextEdit>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTextEdit");
		this.snippetTabsHBoxContainer = this.GetNode<HBoxContainer>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer");
		this.snippetTabBar = this.GetNode<TabBar>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer/SnippetTabBar");
		this.newTabButton = this.GetNode<Button>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer/NewTabButton");

		this.snippetTextEdit.FocusMode = FocusModeEnum.Click;
	}

	public void UpdateUIFromEditables()
	{
		if (this.SnippetEditable == null)
		{
			// error handling?
		}
		else
		{
			this.conditionalModeOptionButton.Selected = (int)(this.SnippetEditable.CurrentMode);

			this.setCanChangeToModeState(DtgeCore.Snippet.Mode.Simple);
			this.setCanChangeToModeState(DtgeCore.Snippet.Mode.Subscene);
			this.setCanChangeToModeState(DtgeCore.Snippet.Mode.Random);

			this.conditionalModeOptionButton.SetItemDisabled(this.snippetTabBar.CurrentTab, true);
			this.conditionalModeOptionButton.SetItemTooltip(this.snippetTabBar.CurrentTab, null);

			this.updateVariationTabsFromSnippet();

			GodotUtilities.UpdateNodeText(
				this.snippetTextEdit,
				this.SnippetEditable.GetVariationText(this.snippetTabBar.CurrentTab));
		}
	}

	public void _on_snippet_text_edit_text_changed()
	{
		this.SnippetEditable.SetVariationText(
			this.snippetTabBar.CurrentTab,
			this.snippetTextEdit.Text);
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
		this.SnippetEditable.ChangeModeTo(SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING[modeIndex]);
	}

	public void _on_new_tab_button_pressed()
	{
		this.createNewSnippetVariation();
	}

	public void _on_snippet_tab_bar_tab_close_pressed(int tabIndex)
	{
		this.snippetTabBar.RemoveTab(tabIndex);
		this.SnippetEditable.RemoveVariationEditable(tabIndex);
	}

	public void _on_snippet_tab_bar_tab_selected(int tabIndex)
	{
		GodotUtilities.UpdateNodeText(
			this.snippetTextEdit,
			this.SnippetEditable.GetVariationText(tabIndex));
		this.SnippetEditable.SetCurrentVariationIndex(tabIndex);
		this.snippetTextEdit.GrabFocus();
	}

	private void setCanChangeToModeState(DtgeCore.Snippet.Mode mode)
	{
		int optionButtonIndexFromMode = (int)mode;
		string cannotChangeMessage = string.Empty;
		bool canChange = this.SnippetEditable.CanChangeToMode(
				mode,
				out cannotChangeMessage);

		this.conditionalModeOptionButton.SetItemDisabled(
			optionButtonIndexFromMode,
			!canChange);
		this.conditionalModeOptionButton.SetItemTooltip(
			optionButtonIndexFromMode,
			canChange ? null : cannotChangeMessage);
	}

	private void updateVariationTabsFromSnippet()
	{
		this.snippetTabsHBoxContainer.Visible =
			!this.SnippetEditable.AlwaysHasOneVariation();

		for (int variationIndex = 0;
			variationIndex < this.SnippetEditable.GetVariationCount();
			variationIndex++)
		{
			if (this.snippetTabBar.TabCount > variationIndex)
			{
				this.snippetTabBar.SetTabTitle(
					variationIndex,
					this.SnippetEditable.GetVariationName(variationIndex));
			}
			else
			{
				this.snippetTabBar.AddTab(
					this.SnippetEditable.GetVariationName(variationIndex));
			}
		}

		while (this.snippetTabBar.TabCount > this.SnippetEditable.GetVariationCount())
		{
			this.snippetTabBar.RemoveTab(this.snippetTabBar.TabCount - 1);
		}

		if (this.SnippetEditable.CanEditVariationCount())
		{
			this.newTabButton.Visible = true;
			if (this.snippetTabBar.TabCount > 1)
			{
				this.snippetTabBar.TabCloseDisplayPolicy =
					TabBar.CloseButtonDisplayPolicy.ShowActiveOnly;
			}
			else
			{
				this.snippetTabBar.TabCloseDisplayPolicy =
					TabBar.CloseButtonDisplayPolicy.ShowNever;
			}
		}
		else
		{
			this.newTabButton.Visible = false;
			this.snippetTabBar.TabCloseDisplayPolicy = TabBar.CloseButtonDisplayPolicy.ShowNever;
		}

		if (this.snippetTabBar.CurrentTab != this.SnippetEditable.GetCurrentVariationIndex())
		{
			this.snippetTabBar.CurrentTab = this.SnippetEditable.GetCurrentVariationIndex();
		}
	}

	private void createNewSnippetVariation()
	{
		this.SnippetEditable.AddVariation();
		this.UpdateUIFromEditables();
		this.snippetTabBar.CurrentTab = this.snippetTabBar.TabCount - 1;
	}
}
