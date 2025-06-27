using Godot;
using System;

namespace DtgeEditor;

/**
 * The root node for the Godot scene that manages the set of
 * OptionEditPanels used to author DTGE Options. It maintains
 * the list and coordinates between the options for things like
 * which navigation button the option will slot into.
 */
public partial class OptionEditList : VBoxContainer
{
	VBoxContainer optionEditListVBoxContainer;
	Label tooManyOptionsLabel;

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

	public int MaximumSupportedOptions;
	public Action<DtgeCore.SceneId> OnTryOpenScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionEditListVBoxContainer = this.GetNode<VBoxContainer>("OptionEditListScrollContainer/OptionEditListVBoxContainer");
		this.tooManyOptionsLabel = this.GetNode<Label>("TooManyOptionsLabel");

		this.updateOptionLabelsAndTooManyWarning();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.UiNeedsUpdate)
		{
			this.updateUiFromEditables();
			this.UiNeedsUpdate = false;
		}
	}

	private void updateEditablesFromUi()
	{

	}

	private void updateUiFromEditables()
	{
		for (int optionIndex = 0; optionIndex < this.dtgeSceneEditable.GetOptionCount(); optionIndex++)
		{
			DtgeCore.Editing.OptionEditable currentOptionEditable = this.dtgeSceneEditable.GetOptionByIndex(optionIndex);
			OptionEditPanel currentOptionEditPanel = this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(optionIndex);
			if (currentOptionEditPanel != null)
			{
				currentOptionEditPanel.OptionEditable = currentOptionEditable;
			}
			else
			{
				this.addNewOptionEditPanel(currentOptionEditable);
			}
		}

		while (this.dtgeSceneEditable.GetOptionCount() < this.optionEditListVBoxContainer.GetChildCount())
		{
			OptionEditPanel excessOptionEditPanel
				= this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(
					this.optionEditListVBoxContainer.GetChildCount() - 1);
			this.optionEditListVBoxContainer.RemoveChild(excessOptionEditPanel);
		}

		this.updateOptionLabelsAndTooManyWarning();
	}

	public void FlushChangesForSave()
	{
		for (int optionPanelIndex = 0; optionPanelIndex < this.optionEditListVBoxContainer.GetChildCount(); optionPanelIndex++)
		{
			this.optionEditListVBoxContainer.GetChild<OptionEditPanel>(optionPanelIndex).FlushChangesForSave();
		}
	}

	public void HandleOptionDeleted(OptionEditPanel toRemove)
	{
		this.optionEditListVBoxContainer.RemoveChild(toRemove);
		this.dtgeSceneEditable.RemoveOption(toRemove.OptionEditable);
	}

	public void HandleTryOpenScene(DtgeCore.SceneId sceneId)
	{
		this.OnTryOpenScene(sceneId);
	}

	public void _on_add_option_button_pressed()
	{
		DtgeCore.Editing.OptionEditable newOption = this.dtgeSceneEditable.AllocateNewOption();
		this.addNewOptionEditPanel(newOption);
	}

	private void updateOptionLabelsAndTooManyWarning()
	{
		for (int optionPanelIndex = 0, optionlocationLabelIndex = 0;
			optionPanelIndex < this.optionEditListVBoxContainer.GetChildCount();
			optionPanelIndex++, optionlocationLabelIndex++)
		{
			OptionEditPanel currentOptionEditPanel = this.optionEditListVBoxContainer.GetChild<OptionEditPanel>(optionPanelIndex);
			currentOptionEditPanel.UpdateOptionLocationLabel(optionPanelIndex);
		}

		if (this.optionEditListVBoxContainer.GetChildCount() > DtgeCore.GameData.GetGameData().MaximumSupportedOptions)
		{
			this.tooManyOptionsLabel.Visible = true;
		}
		else
		{
			this.tooManyOptionsLabel.Visible = false;
		}
	}

	private void updateOptionEditPanelsFromScene()
	{
	}

	private void addNewOptionEditPanel(DtgeCore.Editing.OptionEditable optionEditable)
	{
		OptionEditPanel newOptionEditPanel =
			((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.OPTION_EDIT_PANEL_PATH)).Instantiate<OptionEditPanel>();

		if (newOptionEditPanel != null)
		{
			newOptionEditPanel.OptionEditable = optionEditable;
			newOptionEditPanel.OnOptionMovedUp = this.MoveOptionUp;
			newOptionEditPanel.OnOptionMovedDown = this.MoveOptionDown;
			newOptionEditPanel.OnOptionDeleted = this.HandleOptionDeleted;
			newOptionEditPanel.OnTryOpenScene = this.HandleTryOpenScene;
			this.optionEditListVBoxContainer.AddChild(newOptionEditPanel);
		}

		this.updateOptionLabelsAndTooManyWarning();
	}

	private void MoveOptionUp(OptionEditPanel targetOption)
	{
		//OptionEditPanel currentOptionEditPanel = null;
		//OptionEditPanel aboveOptionEditPanel = null;

		//for (int optionPanelChildIndex = 0; optionPanelChildIndex < this.optionEditListVBoxContainer.GetChildCount(); optionPanelChildIndex++)
		//{
		//	currentOptionEditPanel = this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(optionPanelChildIndex);
		//	if (currentOptionEditPanel != null &&
		//		aboveOptionEditPanel != null &&
		//		currentOptionEditPanel == targetOption)
		//	{
		//		// TODO: The adjustment to how options works means this needs to be totally redone.
		//		//DtgeCore.Option aboveOptionCopy = new DtgeCore.Option();
		//		//aboveOptionCopy.CopyFrom(aboveOptionEditPanel.BoundOption);
		//		//aboveOptionEditPanel.BoundOption.CopyFrom(currentOptionEditPanel.BoundOption);
		//		//currentOptionEditPanel.BoundOption.CopyFrom(aboveOptionCopy);
		//		break;
		//	}
		//	aboveOptionEditPanel = currentOptionEditPanel;
		//}

		//if (currentOptionEditPanel != null)
		//{
		//	currentOptionEditPanel.UpdateUIFromOption();
		//}

		//if (aboveOptionEditPanel != null)
		//{
		//	aboveOptionEditPanel.UpdateUIFromOption();
		//}
	}

	private void MoveOptionDown(OptionEditPanel targetOption)
	{
		//OptionEditPanel currentOptionEditPanel = null;
		//OptionEditPanel belowOptionEditPanel = null;
		//for (int optionPanelChildIndex = 0; optionPanelChildIndex < this.optionEditListVBoxContainer.GetChildCount(); optionPanelChildIndex++)
		//{
		//	currentOptionEditPanel = this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(optionPanelChildIndex);
		//	belowOptionEditPanel = this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(optionPanelChildIndex + 1);
		//	if (currentOptionEditPanel != null &&
		//		belowOptionEditPanel != null &&
		//		currentOptionEditPanel == targetOption)
		//	{
		//		DtgeCore.Option belowOptionCopy = new DtgeCore.Option();
		//		belowOptionCopy.CopyFrom(belowOptionEditPanel.BoundOption);
		//		belowOptionEditPanel.BoundOption.CopyFrom(currentOptionEditPanel.BoundOption);
		//		currentOptionEditPanel.BoundOption.CopyFrom(belowOptionCopy);

		//		break;
		//	}
		//}

		//if (currentOptionEditPanel != null)
		//{
		//	currentOptionEditPanel.UpdateUIFromOption();
		//}

		//if (belowOptionEditPanel != null)
		//{
		//	belowOptionEditPanel.UpdateUIFromOption();
		//}
	}
}
