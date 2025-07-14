using DtgeGodotCommon;

using Godot;

namespace DtgeEditor;

/**
 * The root node for the Godot scene that manages the set of SnipetPanels used to author DTGE
 * snippets. It maintains the list and coordinates updates to and from the snippets.
 */
public partial class SnippetListContainer : VBoxContainer
{
	VBoxContainer snippetListVBoxContainer;

	public DtgeCore.Editing.SceneEditable DtgeSceneEditable { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.snippetListVBoxContainer = GodotUtilities.GetNodeSmart<VBoxContainer>(this, "SnippetListScrollContainer/SnippetListVBoxContainer", GodotEditorErrorHandler.InvokeError);
	}

	public void UpdateFromEditables()
	{
		for (int snippetIndex = 0;
			snippetIndex < this.DtgeSceneEditable.GetSnippetCount();
			snippetIndex++)
		{
			DtgeCore.Editing.SnippetEditable snippetEditable =
				this.DtgeSceneEditable.GetSnippetByIndex(snippetIndex);
			SnippetPanelContainer currentSnippetPanelContainer =
				this.snippetListVBoxContainer
				.GetChildOrNull<SnippetPanelContainer>(snippetIndex);
			if (currentSnippetPanelContainer != null)
			{
				currentSnippetPanelContainer.SnippetEditable = snippetEditable;
				currentSnippetPanelContainer.UpdateUIFromEditables();
			}
			else
			{
				SnippetPanelContainer newSnippetPanelContainer =
					((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.SNIPPET_PANEL_CONTAINER_PATH))
					.Instantiate<SnippetPanelContainer>();

				if (newSnippetPanelContainer == null)
				{
					GodotEditorErrorHandler.InvokeError("Failed to find the Godot Scene for initializing a Snippet Panel Container.");
				}
				else
				{
					newSnippetPanelContainer.SnippetEditable = snippetEditable;
					newSnippetPanelContainer.OnSnippetMovedUp = this.HandleSnippetMovedUp;
					newSnippetPanelContainer.OnSnippetMovedDown = this.HandleMoveSnippetDown;
					newSnippetPanelContainer.OnSnippetDeleted = this.HandleSnippetDeleted;
					this.snippetListVBoxContainer.AddChild(newSnippetPanelContainer);
					newSnippetPanelContainer.UpdateUIFromEditables();
				}
			}
		}

		while (this.DtgeSceneEditable.GetSnippetCount() <
			this.snippetListVBoxContainer.GetChildCount())
		{
			SnippetPanelContainer excessSnippetPanelContainer
				= this.snippetListVBoxContainer.GetChildOrNull<SnippetPanelContainer>(
					this.snippetListVBoxContainer.GetChildCount() - 1);
			this.snippetListVBoxContainer.RemoveChild(excessSnippetPanelContainer);
		}
	}

	public void _on_add_snippet_button_pressed()
	{
		DtgeCore.Editing.SnippetEditable newSnippetEditable =
			this.DtgeSceneEditable.AllocateNewSnippet();
	}

	private void HandleSnippetMovedUp(DtgeCore.Editing.SnippetEditable targetSnippet)
	{
		this.DtgeSceneEditable.MoveSnippet(targetSnippet, -1);
	}

	private void HandleMoveSnippetDown(DtgeCore.Editing.SnippetEditable targetSnippet)
	{
		this.DtgeSceneEditable.MoveSnippet(targetSnippet, 1);
	}

	private void HandleSnippetDeleted(DtgeCore.Editing.SnippetEditable toRemove)
	{
		this.DtgeSceneEditable.RemoveSnippet(toRemove);
	}
}
