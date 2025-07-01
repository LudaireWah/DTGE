using System;

using Godot;

using DtgeGodotCommon;
using DtgeCore.Editing;

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

	private bool UiNeedsUpdate;
	private DtgeCore.Editing.SceneEditable dtgeSceneEditable;
	public DtgeCore.Editing.SceneEditable DtgeSceneEditable
	{
		get { return this.dtgeSceneEditable; }
		set
		{
			this.dtgeSceneEditable = value;
			this.UiNeedsUpdate = true;
		}
	}
	private DtgeCore.Editing.ISnippetEditable snippetEditable;
	public DtgeCore.Editing.ISnippetEditable SnippetEditable
	{
		get
		{
			return this.snippetEditable;
		}
		set
		{
			this.snippetEditable = value;
			this.UiNeedsUpdate = true;
		}
	}

	public Action<SnippetPanelContainer, DtgeCore.Snippet.Mode> OnSnippetModeChanged;
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
		if (this.UiNeedsUpdate)
		{
			this.UpdateUIFromEditables();
			this.UiNeedsUpdate = false;
		}
	}

	private void updateEditablesFromUI()
	{
		this.SnippetEditable.GetVariationEditable(this.snippetTabBar.CurrentTab).Text
			= this.snippetTextEdit.Text;
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

			string simpleConvertFailedMessage = "";
			bool canConvertToSimple =
				SnippetSimpleEditable.CanConvertFrom(
					this.SnippetEditable,
					out simpleConvertFailedMessage);

			this.conditionalModeOptionButton.SetItemDisabled(
				(int)DtgeCore.Snippet.Mode.Simple,
				!canConvertToSimple);

			if (canConvertToSimple)
			{
				this.conditionalModeOptionButton.SetItemTooltip(
					(int)DtgeCore.Snippet.Mode.Simple,
					simpleConvertFailedMessage);
			}
			else
			{
				this.conditionalModeOptionButton.SetItemTooltip(
					(int)DtgeCore.Snippet.Mode.Simple,
					null);
			}

			string subsceneConvertFailedMessage = "";
			bool canConvertToSubscene =
				SnippetSubsceneEditable.CanConvertFrom(
					this.DtgeSceneEditable,
					this.snippetEditable,
					out subsceneConvertFailedMessage);

			this.conditionalModeOptionButton.SetItemDisabled(
				(int)DtgeCore.Snippet.Mode.Subscene,
				!canConvertToSubscene);

			if (canConvertToSubscene)
			{
				this.conditionalModeOptionButton.SetItemTooltip(
					(int)DtgeCore.Snippet.Mode.Subscene,
					null);
			}
			else
			{
				this.conditionalModeOptionButton.SetItemTooltip(
					(int)DtgeCore.Snippet.Mode.Subscene,
					subsceneConvertFailedMessage);
			}

			this.conditionalModeOptionButton.SetItemDisabled(this.snippetTabBar.CurrentTab, true);
			this.conditionalModeOptionButton.SetItemTooltip(this.snippetTabBar.CurrentTab, null);

			this.updateVariationTabsFromSnippet();

			GodotUtilities.UpdateNodeText(
				this.snippetTextEdit,
				this.SnippetEditable.GetVariationEditable(this.snippetTabBar.CurrentTab).Text);
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
		this.OnSnippetModeChanged(this, SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING[modeIndex]);
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
			this.SnippetEditable.GetVariationEditable(tabIndex).Text);
		this.lastVariationSelectedByTab = this.SnippetEditable.GetVariationEditable(tabIndex);
		this.snippetTextEdit.GrabFocus();
	}

	private void updateVariationTabsFromSnippet()
	{
		this.snippetTabsHBoxContainer.Visible =
			this.SnippetEditable.Mode != DtgeCore.Snippet.Mode.Simple;

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

		for (int variationIndex = 0;
			variationIndex < this.SnippetEditable.GetVariationCount();
			variationIndex++)
		{
			if (this.snippetTabBar.TabCount > variationIndex)
			{
				this.snippetTabBar.SetTabTitle(
					variationIndex,
					this.SnippetEditable.GetVariationEditable(variationIndex).Name);
			}
			else
			{
				this.snippetTabBar.AddTab(
					this.SnippetEditable.GetVariationEditable(variationIndex).Name);
			}
		}

		while (this.snippetTabBar.TabCount > this.SnippetEditable.GetVariationCount())
		{
			this.snippetTabBar.RemoveTab(this.snippetTabBar.TabCount - 1);
		}

		if (this.lastVariationSelectedByTab != null)
		{
			for (int variationIndex = 0;
				variationIndex < this.SnippetEditable.GetVariationCount();
				variationIndex++)
			{
				DtgeCore.Editing.VariationEditable currentVariation =
					this.SnippetEditable.GetVariationEditable(variationIndex);
				if (currentVariation.Id == this.lastVariationSelectedByTab.Id &&
					variationIndex != this.snippetTabBar.CurrentTab)
				{
					this.snippetTabBar.CurrentTab = variationIndex;
					break;
				}
			}
		}

		this.lastVariationSelectedByTab =
			this.SnippetEditable.GetVariationEditable(this.snippetTabBar.CurrentTab);
	}

	private void createNewSnippetVariation()
	{
		this.SnippetEditable.AddVariation();
		this.snippetTabBar.CurrentTab = this.snippetTabBar.TabCount - 1;
		this.UpdateUIFromEditables();
	}
}
