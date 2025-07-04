using System;

using Godot;

namespace DtgeEditor;

/**
 * The root node for the Godot scene that manages the set of OptionEditPanels used to author DTGE
 * Options. It maintains the list and coordinates between the options for things like which
 * navigation button the option will slot into.
 */
public partial class OptionEditList : VBoxContainer
{
	VBoxContainer optionEditListVBoxContainer;
	Label tooManyOptionsLabel;

	public DtgeCore.Editing.SceneEditable DtgeSceneEditable;

	public Action<DtgeCore.SceneId> OnTryOpenScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionEditListVBoxContainer = this.GetNode<VBoxContainer>("OptionEditListScrollContainer/OptionEditListVBoxContainer");
		this.tooManyOptionsLabel = this.GetNode<Label>("TooManyOptionsLabel");
	}

	public void UpdateFromEditables()
	{
		for (int optionIndex = 0;
			optionIndex < this.DtgeSceneEditable.GetOptionCount();
			optionIndex++)
		{
			DtgeCore.Editing.OptionEditable optionEditable =
				this.DtgeSceneEditable.GetOptionByIndex(optionIndex);
			OptionEditPanel optionEditPanel =
				this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(optionIndex);
			if (optionEditPanel != null)
			{
				optionEditPanel.OptionEditable = optionEditable;
				optionEditPanel.UpdateFromEditables();
			}
			else
			{
				OptionEditPanel newOptionEditPanel =
					((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.OPTION_EDIT_PANEL_PATH))
					.Instantiate<OptionEditPanel>();

				if (newOptionEditPanel != null)
				{
					newOptionEditPanel.OptionEditable = optionEditable;
					newOptionEditPanel.OnOptionMovedUp = this.HandleOptionMovedUp;
					newOptionEditPanel.OnOptionMovedDown = this.HandleOptionMovedDown;
					newOptionEditPanel.OnOptionDeleted = this.HandleOptionDeleted;
					newOptionEditPanel.OnTryOpenScene = this.HandleTryOpenScene;
					this.optionEditListVBoxContainer.AddChild(newOptionEditPanel);
					newOptionEditPanel.UpdateFromEditables();
				}
			}
		}

		while (this.DtgeSceneEditable.GetOptionCount() <
			this.optionEditListVBoxContainer.GetChildCount())
		{
			OptionEditPanel excessOptionEditPanel
				= this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(
					this.optionEditListVBoxContainer.GetChildCount() - 1);
			this.optionEditListVBoxContainer.RemoveChild(excessOptionEditPanel);
		}

		for (int optionPanelIndex = 0, optionlocationLabelIndex = 0;
			optionPanelIndex < this.optionEditListVBoxContainer.GetChildCount();
			optionPanelIndex++, optionlocationLabelIndex++)
		{
			OptionEditPanel currentOptionEditPanel =
				this.optionEditListVBoxContainer.GetChild<OptionEditPanel>(optionPanelIndex);
			currentOptionEditPanel.UpdateOptionLocationLabel(optionPanelIndex);
		}

		if (this.optionEditListVBoxContainer.GetChildCount() >
			DtgeCore.GameData.GetGameData().MaximumSupportedOptions)
		{
			this.tooManyOptionsLabel.Visible = true;
		}
		else
		{
			this.tooManyOptionsLabel.Visible = false;
		}
	}

	public void _on_add_option_button_pressed()
	{
		DtgeCore.Editing.OptionEditable newOption = this.DtgeSceneEditable.AllocateNewOption();
	}

	private void HandleOptionMovedUp(DtgeCore.Editing.OptionEditable targetOptionEditable)
	{
		this.DtgeSceneEditable.TryMoveOption(targetOptionEditable, -1);
	}

	private void HandleOptionMovedDown(DtgeCore.Editing.OptionEditable targetOptionEditable)
	{
		this.DtgeSceneEditable.TryMoveOption(targetOptionEditable, 1);
	}

	public void HandleOptionDeleted(DtgeCore.Editing.OptionEditable optionEditable)
	{
		this.DtgeSceneEditable.RemoveOption(optionEditable);
	}

	private void HandleTryOpenScene(DtgeCore.SceneId sceneId)
	{
		this.OnTryOpenScene(sceneId);
	}
}
