using Godot;
using System;
using System.Collections.Generic;

namespace DtgeEditor;

/**
 * The root node for the Godot scene that manages the set of
 * SnipetPanels used to author DTGE snippets. It maintains
 * the list and coordinates updates to and from the snippets.
 */
public partial class SnippetListContainer : VBoxContainer
{
    VBoxContainer snippetListVBoxContainer;
    bool firstSceneHasBeenSet;
    bool uiNeedUpdate;

    private DtgeCore.Scene dtgeScene;

    public DtgeCore.Scene DtgeScene
    {
        get { return dtgeScene; }
        set
        {
            this.dtgeScene = value;
            this.uiNeedUpdate = true;
        }
    }

    public Action OnSnippetListUpdated;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.snippetListVBoxContainer = this.GetNode<VBoxContainer>("SnippetListScrollContainer/SnippetListVBoxContainer");

        this.firstSceneHasBeenSet = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (this.uiNeedUpdate)
        {
            this.updateSnippetPanelContainersFromSnippets();
            this.uiNeedUpdate = false;
        }
    }

    public void FlushChangesForSave()
    {
        for (int snippetPanelIndex = 0; snippetPanelIndex < this.snippetListVBoxContainer.GetChildCount(); snippetPanelIndex++)
        {
            this.snippetListVBoxContainer.GetChild<SnippetPanelContainer>(snippetPanelIndex).FlushChangesForSave();
        }
    }

    public void OnSnippetUpdated(bool countChanged)
    {
        if (countChanged)
        {
            this.dtgeScene.ClearAllSnippets();
            for (int snippetPanelIndex = 0; snippetPanelIndex < this.snippetListVBoxContainer.GetChildCount(); snippetPanelIndex++)
            {
                SnippetPanelContainer currentSnippetPanelContainer = this.snippetListVBoxContainer.GetChild<SnippetPanelContainer>(snippetPanelIndex);
                this.dtgeScene.AddSnippet(currentSnippetPanelContainer.BoundSnippet);
            }
        }

        if (this.OnSnippetListUpdated != null)
        {
            this.OnSnippetListUpdated();
        }
    }

    public void MoveSnippetUp(SnippetPanelContainer targetSnippet)
    {
        SnippetPanelContainer currentSnippetPanelContainer = null;
        SnippetPanelContainer aboveSnippetPanelContainer = null;

        for (int snippetPanelChildIndex = 0; snippetPanelChildIndex < this.snippetListVBoxContainer.GetChildCount(); snippetPanelChildIndex++)
        {
            currentSnippetPanelContainer = this.snippetListVBoxContainer.GetChildOrNull<SnippetPanelContainer>(snippetPanelChildIndex);
            if (currentSnippetPanelContainer != null &&
                aboveSnippetPanelContainer != null &&
                currentSnippetPanelContainer == targetSnippet)
            {
                DtgeCore.Snippet aboveSnippetCopy = new DtgeCore.Snippet();
                aboveSnippetCopy.CopyFrom(aboveSnippetPanelContainer.BoundSnippet);
                aboveSnippetPanelContainer.BoundSnippet.CopyFrom(currentSnippetPanelContainer.BoundSnippet);
                currentSnippetPanelContainer.BoundSnippet.CopyFrom(aboveSnippetCopy);
                break;
            }
            aboveSnippetPanelContainer = currentSnippetPanelContainer;
        }

        if (currentSnippetPanelContainer != null)
        {
            currentSnippetPanelContainer.UpdateUIFromSnippet();
        }

        if (aboveSnippetPanelContainer != null)
        {
            aboveSnippetPanelContainer.UpdateUIFromSnippet();
        }

        this.OnSnippetListUpdated();
    }

    public void MoveSnippetDown(SnippetPanelContainer targetSnippet)
    {
        SnippetPanelContainer currentSnippetPanelContainer = null;
        SnippetPanelContainer belowSnippetPanelContainer = null;
        for (int snippetPanelChildIndex = 0; snippetPanelChildIndex < this.snippetListVBoxContainer.GetChildCount(); snippetPanelChildIndex++)
        {
            currentSnippetPanelContainer = this.snippetListVBoxContainer.GetChildOrNull<SnippetPanelContainer>(snippetPanelChildIndex);
            belowSnippetPanelContainer = this.snippetListVBoxContainer.GetChildOrNull<SnippetPanelContainer>(snippetPanelChildIndex + 1);
            if (currentSnippetPanelContainer != null &&
                belowSnippetPanelContainer != null &&
                currentSnippetPanelContainer == targetSnippet)
            {
                DtgeCore.Snippet belowSnippetCopy = new DtgeCore.Snippet();
                belowSnippetCopy.CopyFrom(belowSnippetPanelContainer.BoundSnippet);
                belowSnippetPanelContainer.BoundSnippet.CopyFrom(currentSnippetPanelContainer.BoundSnippet);
                currentSnippetPanelContainer.BoundSnippet.CopyFrom(belowSnippetCopy);

                break;
            }
        }

        if (currentSnippetPanelContainer != null)
        {
            currentSnippetPanelContainer.UpdateUIFromSnippet();
        }

        if (belowSnippetPanelContainer != null)
        {
            belowSnippetPanelContainer.UpdateUIFromSnippet();
        }

        this.OnSnippetListUpdated();
    }

    public void OnSnippetDeleted(SnippetPanelContainer toRemove)
    {
        this.snippetListVBoxContainer.RemoveChild(toRemove);
        this.OnSnippetUpdated(true);
    }

    public void _on_add_snippet_button_pressed()
    {
        DtgeCore.Snippet newSnippet = new DtgeCore.Snippet();
        this.DtgeScene.AddSnippet(newSnippet);
        this.addNewSnippetPanelContainer(newSnippet);
        this.OnSnippetListUpdated();
    }

    private void updateSnippetPanelContainersFromSnippets()
    {
        List<DtgeCore.Snippet> updatedSnippets = this.dtgeScene.SnippetList;
        int nonNullSnippetCount = 0;

        for (int snippetIndex = 0; snippetIndex < updatedSnippets.Count; snippetIndex++)
        {
            DtgeCore.Snippet currentSnippet = updatedSnippets[snippetIndex];
            if (currentSnippet == null)
            {
                break;
            }
            else
            {
                nonNullSnippetCount++;
                SnippetPanelContainer currentSnippetPanelContainer = this.snippetListVBoxContainer.GetChildOrNull<SnippetPanelContainer>(snippetIndex);
                if (currentSnippetPanelContainer != null)
                {
                    currentSnippetPanelContainer.BoundSnippet = updatedSnippets[snippetIndex];
                }
                else
                {
                    this.addNewSnippetPanelContainer(updatedSnippets[snippetIndex]);
                }
            }
        }

        while (nonNullSnippetCount < this.snippetListVBoxContainer.GetChildCount())
        {
            SnippetPanelContainer excessSnippetPanelContainer
                = this.snippetListVBoxContainer.GetChildOrNull<SnippetPanelContainer>(
                    this.snippetListVBoxContainer.GetChildCount() - 1);
            this.snippetListVBoxContainer.RemoveChild(excessSnippetPanelContainer);
        }
    }

    private void addNewSnippetPanelContainer(DtgeCore.Snippet snippet)
    {
        SnippetPanelContainer newSnippetPanelContainer =
            ((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.SNIPPET_PANEL_CONTAINER_PATH)).Instantiate<SnippetPanelContainer>();

        if (newSnippetPanelContainer != null)
        {
            newSnippetPanelContainer.BoundSnippet = snippet;
            newSnippetPanelContainer.OnSnippetUpdated = this.OnSnippetUpdated;
            newSnippetPanelContainer.SnippetMovedUpAction = this.MoveSnippetUp;
            newSnippetPanelContainer.SnippetMovedDownAction = this.MoveSnippetDown;
            newSnippetPanelContainer.SnippetDeletedAction = this.OnSnippetDeleted;
            this.snippetListVBoxContainer.AddChild(newSnippetPanelContainer);
        }
    }
}
