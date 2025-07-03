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
		this.snippetListVBoxContainer = this.GetNode<VBoxContainer>("SnippetListScrollContainer/SnippetListVBoxContainer");
	}

	public void UpdateFromEditables()
	{
		int nonNullSnippetCount = 0;

		for (int snippetIndex = 0;
			snippetIndex < this.DtgeSceneEditable.GetSnippetCount();
			snippetIndex++)
		{
			DtgeCore.Editing.SnippetEditable currentSnippet =
				this.DtgeSceneEditable.GetSnippetByIndex(snippetIndex);
			if (currentSnippet == null)
			{
				break;
			}
			else
			{
				nonNullSnippetCount++;
				SnippetPanelContainer currentSnippetPanelContainer =
					this.snippetListVBoxContainer
					.GetChildOrNull<SnippetPanelContainer>(snippetIndex);
				if (currentSnippetPanelContainer != null)
				{
					currentSnippetPanelContainer.SnippetEditable = currentSnippet;
					currentSnippetPanelContainer.UpdateUIFromEditables();
				}
				else
				{
					this.addNewSnippetPanelContainer(currentSnippet);
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

	public void _on_add_snippet_button_pressed()
	{
		DtgeCore.Editing.SnippetEditable newSnippetEditable =
			this.DtgeSceneEditable.AllocateNewSnippet();
		this.addNewSnippetPanelContainer(newSnippetEditable);
	}

	private void HandleSnippetMovedUp(DtgeCore.Editing.SnippetEditable targetSnippet)
	{
		this.DtgeSceneEditable.TryMoveSnippet(targetSnippet, -1);
	}

	private void HandleMoveSnippetDown(DtgeCore.Editing.SnippetEditable targetSnippet)
	{
		this.DtgeSceneEditable.TryMoveSnippet(targetSnippet, 1);
	}

	private void HandleSnippetDeleted(DtgeCore.Editing.SnippetEditable toRemove)
	{
		this.DtgeSceneEditable.RemoveSnippet(toRemove);
	}

	private void addNewSnippetPanelContainer(DtgeCore.Editing.SnippetEditable snippetEditable)
	{
		SnippetPanelContainer newSnippetPanelContainer =
			((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.SNIPPET_PANEL_CONTAINER_PATH))
			.Instantiate<SnippetPanelContainer>();

		if (newSnippetPanelContainer != null)
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
