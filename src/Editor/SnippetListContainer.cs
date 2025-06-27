using Godot;

namespace DtgeEditor;

/**
 * The root node for the Godot scene that manages the set of SnipetPanels used to author DTGE
 * snippets. It maintains the list and coordinates updates to and from the snippets.
 */
public partial class SnippetListContainer : VBoxContainer
{
	VBoxContainer snippetListVBoxContainer;

	public bool UiNeedsUpdate;
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.snippetListVBoxContainer = this.GetNode<VBoxContainer>("SnippetListScrollContainer/SnippetListVBoxContainer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.UiNeedsUpdate)
		{
			this.updateUIFromEditables();
			this.UiNeedsUpdate = false;
		}
	}

	private void updateEditablesFromUI()
	{

	}

	private void updateUIFromEditables()
	{
		int nonNullSnippetCount = 0;

		for (int snippetIndex = 0;
			snippetIndex < this.dtgeSceneEditable.GetSnippetCount();
			snippetIndex++)
		{
			DtgeCore.Editing.SnippetEditable currentSnippet =
				this.dtgeSceneEditable.GetSnippetByIndex(snippetIndex);
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

	public void FlushChangesForSave()
	{
		for (int snippetPanelIndex = 0;
			snippetPanelIndex < this.snippetListVBoxContainer.GetChildCount();
			snippetPanelIndex++)
		{
			this.snippetListVBoxContainer.GetChild<SnippetPanelContainer>(snippetPanelIndex)
				.FlushChangesForSave();
		}
	}

	public void HandleSnippetDeleted(SnippetPanelContainer toRemove)
	{
		this.snippetListVBoxContainer.RemoveChild(toRemove);
		this.dtgeSceneEditable.RemoveSnippet(toRemove.SnippetEditable);
	}

	public void _on_add_snippet_button_pressed()
	{
		DtgeCore.Editing.SnippetEditable newSnippetEditable =
			this.dtgeSceneEditable.AllocateNewSnippet();
		this.addNewSnippetPanelContainer(newSnippetEditable);
	}

	private void addNewSnippetPanelContainer(DtgeCore.Editing.SnippetEditable snippetEditable)
	{
		SnippetPanelContainer newSnippetPanelContainer =
			((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.SNIPPET_PANEL_CONTAINER_PATH))
			.Instantiate<SnippetPanelContainer>();

		if (newSnippetPanelContainer != null)
		{
			newSnippetPanelContainer.SnippetEditable = snippetEditable;
			newSnippetPanelContainer.OnSnippetMovedUp = this.HandleMoveSnippetUp;
			newSnippetPanelContainer.OnSnippetMovedDown = this.HandleMoveSnippetDown;
			newSnippetPanelContainer.OnSnippetDeleted = this.HandleSnippetDeleted;
			this.snippetListVBoxContainer.AddChild(newSnippetPanelContainer);
		}
	}

	public void HandleMoveSnippetUp(SnippetPanelContainer targetSnippet)
	{
		//SnippetPanelContainer currentSnippetPanelContainer = null;
		//SnippetPanelContainer aboveSnippetPanelContainer = null;

		//for (int snippetPanelChildIndex = 0; snippetPanelChildIndex < this.snippetListVBoxContainer.GetChildCount(); snippetPanelChildIndex++)
		//{
		//	currentSnippetPanelContainer = this.snippetListVBoxContainer.GetChildOrNull<SnippetPanelContainer>(snippetPanelChildIndex);
		//	if (currentSnippetPanelContainer != null &&
		//		aboveSnippetPanelContainer != null &&
		//		currentSnippetPanelContainer == targetSnippet)
		//	{
		//		DtgeCore.Snippet aboveSnippetCopy = new DtgeCore.Snippet(this.dtgeScene.GetSubsceneContextProvider());
		//		aboveSnippetCopy.CopyFrom(aboveSnippetPanelContainer.BoundSnippet);
		//		aboveSnippetPanelContainer.BoundSnippet.CopyFrom(currentSnippetPanelContainer.BoundSnippet);
		//		currentSnippetPanelContainer.BoundSnippet.CopyFrom(aboveSnippetCopy);
		//		break;
		//	}
		//	aboveSnippetPanelContainer = currentSnippetPanelContainer;
		//}

		//if (currentSnippetPanelContainer != null)
		//{
		//	currentSnippetPanelContainer.UpdateUIFromSnippet();
		//}

		//if (aboveSnippetPanelContainer != null)
		//{
		//	aboveSnippetPanelContainer.UpdateUIFromSnippet();
		//}

		//this.OnSnippetListUpdated();
	}

	public void HandleMoveSnippetDown(SnippetPanelContainer targetSnippet)
	{
		//SnippetPanelContainer currentSnippetPanelContainer = null;
		//SnippetPanelContainer belowSnippetPanelContainer = null;
		//for (int snippetPanelChildIndex = 0; snippetPanelChildIndex < this.snippetListVBoxContainer.GetChildCount(); snippetPanelChildIndex++)
		//{
		//	currentSnippetPanelContainer = this.snippetListVBoxContainer.GetChildOrNull<SnippetPanelContainer>(snippetPanelChildIndex);
		//	belowSnippetPanelContainer = this.snippetListVBoxContainer.GetChildOrNull<SnippetPanelContainer>(snippetPanelChildIndex + 1);
		//	if (currentSnippetPanelContainer != null &&
		//		belowSnippetPanelContainer != null &&
		//		currentSnippetPanelContainer == targetSnippet)
		//	{
		//		DtgeCore.Snippet belowSnippetCopy = new DtgeCore.Snippet(this.dtgeScene.GetSubsceneContextProvider());
		//		belowSnippetCopy.CopyFrom(belowSnippetPanelContainer.BoundSnippet);
		//		belowSnippetPanelContainer.BoundSnippet.CopyFrom(currentSnippetPanelContainer.BoundSnippet);
		//		currentSnippetPanelContainer.BoundSnippet.CopyFrom(belowSnippetCopy);

		//		break;
		//	}
		//}

		//if (currentSnippetPanelContainer != null)
		//{
		//	currentSnippetPanelContainer.UpdateUIFromSnippet();
		//}

		//if (belowSnippetPanelContainer != null)
		//{
		//	belowSnippetPanelContainer.UpdateUIFromSnippet();
		//}

		//this.OnSnippetListUpdated();
	}
}
