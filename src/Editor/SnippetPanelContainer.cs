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
        DtgeCore.Snippet.ConditionalMode.Random,
        DtgeCore.Snippet.ConditionalMode.Subscene
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

    public Action<bool> OnSnippetUpdated;
    public Action<SnippetPanelContainer> SnippetMovedUpAction;
    public Action<SnippetPanelContainer> SnippetMovedDownAction;
    public Action<SnippetPanelContainer> SnippetDeletedAction;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.conditionalModeOptionButton = GetNode<OptionButton>("SnippetMarginContainer/SnippetVBoxContainer/SnippetHeaderContainer/ConditionalModeOptionButton");
        this.snippetTextEdit = GetNode<TextEdit>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTextEdit");
        this.snippetTabsHBoxContainer = GetNode<HBoxContainer>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer");
        this.snippetTabBar = GetNode<TabBar>("SnippetMarginContainer/SnippetVBoxContainer/SnippetTabsHBoxContainer/SnippetTabBar");

        snippetTextEdit.FocusMode = FocusModeEnum.Click;

        if (this.boundSnippet == null)
        {
            this.boundSnippet = new DtgeCore.Snippet();
        }
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
        //if (System.Diagnostics.Debugger.IsAttached)
        //    System.Diagnostics.Debugger.Break();
        this.BoundSnippet.SetSnippetVariationText(snippetTabBar.CurrentTab, this.snippetTextEdit.Text);
        this.OnSnippetUpdated(false);
    }

    public void UpdateUIFromSnippet()
    {
        this.conditionalModeOptionButton.Selected = (int)(this.boundSnippet.CurrentConditionalMode);

        switch (this.BoundSnippet.CurrentConditionalMode)
        {
        case DtgeCore.Snippet.ConditionalMode.Simple:
            this.snippetTabsHBoxContainer.Visible = false;
            break;
        case DtgeCore.Snippet.ConditionalMode.Random:
            this.snippetTabsHBoxContainer.Visible = true;
            break;
        case DtgeCore.Snippet.ConditionalMode.Subscene:
            this.snippetTabsHBoxContainer.Visible = true;
            break;
        }
        this.snippetTabBar.ClearTabs();
        for (int variationIndex = 0; variationIndex < boundSnippet.GetSnippetVariationCount(); variationIndex++)
        {
            this.snippetTabBar.AddTab("variation +" + variationIndex);
        }

        this.snippetTextEdit.Text = this.BoundSnippet.GetVariationTextByIndex(this.snippetTabBar.CurrentTab);

        this.OnSnippetUpdated(false);
    }

    private void createNewSnippetTab(string tabName = "snippet_tab")
    {
        this.snippetTabBar.AddTab(tabName);
        this.snippetTabBar.CurrentTab = this.snippetTabBar.TabCount - 1;

        this.BoundSnippet.AddSnippetVariation();
        this.UpdateUIFromSnippet();
    }

    public void _on_snippet_text_edit_text_changed()
    {
        //if (System.Diagnostics.Debugger.IsAttached)
        //    System.Diagnostics.Debugger.Break();
        this.updateSnippetFromUI();
    }

    public void _on_move_up_button_pressed()
    {
        this.SnippetMovedUpAction(this);
    }

    public void _on_move_down_button_pressed()
    {
        this.SnippetMovedDownAction(this);
    }

    public void _on_delete_button_pressed()
    {
        this.SnippetDeletedAction(this);
    }

    public void _on_conditional_mode_option_button_item_selected(int modeIndex)
    {
        DtgeCore.Snippet.ConditionalMode newMode = SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING[modeIndex];
        bool canUpdate = true;

        switch(newMode)
        {
        case DtgeCore.Snippet.ConditionalMode.Simple:
            if (this.snippetTabBar.TabCount > 1)
            {
                canUpdate = false;
            }
            this.conditionalModeOptionButton.Selected = (int)(this.boundSnippet.CurrentConditionalMode);
            break;
        }

        if (canUpdate)
        {
            this.BoundSnippet.CurrentConditionalMode = SNIPPET_CONDITIONAL_MODE_INDEX_MAPPING[modeIndex];
            this.UpdateUIFromSnippet();
        }
    }

    public void _on_new_tab_button_pressed()
    {
        this.createNewSnippetTab();
    }

    public void _on_snippet_tab_bar_tab_close_pressed(int tabIndex)
    {
        this.snippetTabBar.RemoveTab(tabIndex);
        this.boundSnippet.RemoveSnippetVariation(tabIndex);
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
