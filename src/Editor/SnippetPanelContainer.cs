using Godot;
using System;

namespace DtgeEditor;

/**
 * The root node for the Godot scene responsible for authoring
 * DTGE Snippets.
 */
public partial class SnippetPanelContainer : PanelContainer
{
    private static readonly DtgeCore.Snippet.ConditionalMode[] SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING =
    {
        DtgeCore.Snippet.ConditionalMode.Simple,
        DtgeCore.Snippet.ConditionalMode.Subscene,
        DtgeCore.Snippet.ConditionalMode.Random
    };

    OptionButton conditionalModeOptionButton;
    TextEdit snippetTextEdit;
    HBoxContainer snippetTabsHBoxContainer;
    TabBar snippetTabBar;

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

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.conditionalModeOptionButton = GetNode<OptionButton>("SnippetMarginContainer/SnippetVBoxContainer/SnippetHeaderContainer/ConditionalModeOptionButton");
        this.snippetTextEdit = GetNode<TextEdit>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTextEdit");
        this.snippetTabsHBoxContainer = GetNode<HBoxContainer>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer");
        this.snippetTabBar = GetNode<TabBar>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer/SnippetTabBar");

        snippetTextEdit.FocusMode = FocusModeEnum.Click;

        this.UpdateUIFromSnippet();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
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
            this.conditionalModeOptionButton.Selected = (int)(this.boundSnippet.CurrentConditionalMode);

            this.conditionalModeOptionButton.SetItemDisabled((int)DtgeCore.Snippet.ConditionalMode.Simple, this.boundSnippet.IsSimpleModeDisabled());
            if (this.boundSnippet.IsSimpleModeDisabled())
            {
                this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.ConditionalMode.Simple, "Returning to simple mode requires that you have only one variation");
            }
            else
            {
                this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.ConditionalMode.Simple, null);
            }

            this.conditionalModeOptionButton.SetItemDisabled((int)DtgeCore.Snippet.ConditionalMode.Subscene, this.boundSnippet.IsSubsceneModeDisabled());
            if (this.boundSnippet.IsSubsceneModeDisabled())
            {
                this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.ConditionalMode.Subscene, "Subscene mode requires that the scene has implemented subscenes");
            }
            else
            {
                this.conditionalModeOptionButton.SetItemTooltip((int)DtgeCore.Snippet.ConditionalMode.Subscene, null);
            }

            this.updateVariationTabsFromSnippet();

            this.snippetTextEdit.Text = this.BoundSnippet.GetVariationTextByIndex(this.snippetTabBar.CurrentTab);
        }
    }

    private void updateVariationTabsFromSnippet()
    {

        switch (this.BoundSnippet.CurrentConditionalMode)
        {
        case DtgeCore.Snippet.ConditionalMode.Simple:
            this.snippetTabsHBoxContainer.Visible = false;
            break;
        case DtgeCore.Snippet.ConditionalMode.Subscene:
            this.snippetTabsHBoxContainer.Visible = true;
            this.snippetTabBar.TabCloseDisplayPolicy = TabBar.CloseButtonDisplayPolicy.ShowNever;

            break;
        case DtgeCore.Snippet.ConditionalMode.Random:
            this.snippetTabsHBoxContainer.Visible = true;
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
    }

    private void createNewSnippetVariation()
    {
        this.BoundSnippet.AddVariation();
        this.snippetTabBar.CurrentTab = this.snippetTabBar.TabCount - 1;
        this.UpdateUIFromSnippet();
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
        this.BoundSnippet.CurrentConditionalMode = SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING[modeIndex];
        this.UpdateUIFromSnippet();
        this.OnSnippetUpdated(false);
    }

    public void _on_new_tab_button_pressed()
    {
        this.createNewSnippetVariation();
    }

    public void _on_snippet_tab_bar_tab_close_pressed(int tabIndex)
    {
        this.snippetTabBar.RemoveTab(tabIndex);
        this.boundSnippet.RemoveVariation(tabIndex);
        this.UpdateUIFromSnippet();
    }

    public void _on_snippet_tab_bar_tab_selected(int tabIndex)
    {
        //if (System.Diagnostics.Debugger.IsAttached)
        //    System.Diagnostics.Debugger.Break();
        this.snippetTextEdit.Text = this.boundSnippet.GetVariationTextByIndex(tabIndex);
        this.boundSnippet.SetRandomizedIndex(tabIndex);
        this.OnSnippetUpdated(false);
        this.snippetTextEdit.GrabFocus();
    }
}
