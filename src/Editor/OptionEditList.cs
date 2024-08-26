using Godot;
using System;
using System.Collections.Generic;

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

	private bool uiNeedsUpdate;

	private DtgeCore.Scene dtgeScene;
	public DtgeCore.Scene DtgeScene
	{
		get { return dtgeScene; }
		set
		{
			this.dtgeScene = value;
			this.uiNeedsUpdate = true;
		}
	}

	private readonly string[] OptionLocationLabelValues =
	{
		"1",
		"2",
		"3",
		"4",
		"5",
		"Q",
		"W",
		"E",
		"R",
		"T",
		"A",
		"S",
		"D",
		"F",
		"G"
	};

	public Action<DtgeCore.Scene.SceneId> OnTryOpenScene;
	public Action OnOptionListUpdated;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionEditListVBoxContainer = this.GetNode<VBoxContainer>("OptionEditListScrollContainer/OptionEditListVBoxContainer");

		this.updateOptionLocationLabels();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.uiNeedsUpdate)
		{
			this.updateOptionEditPanelsFromScene();
			this.uiNeedsUpdate = false;
		}
	}

	public void FlushChangesForSave()
	{
		for (int optionPanelIndex = 0; optionPanelIndex < this.optionEditListVBoxContainer.GetChildCount(); optionPanelIndex++)
		{
			this.optionEditListVBoxContainer.GetChild<OptionEditPanel>(optionPanelIndex).FlushChangesForSave();
		}
	}

	public void HandleOptionUpdated(bool countChanged)
	{
		if (countChanged)
		{
			this.dtgeScene.ClearAllOptions();
			for (int optionPanelIndex = 0; optionPanelIndex < this.optionEditListVBoxContainer.GetChildCount(); optionPanelIndex++)
			{
				OptionEditPanel currentOptionEditPanel = this.optionEditListVBoxContainer.GetChild<OptionEditPanel>(optionPanelIndex);
				this.dtgeScene.AddOption(currentOptionEditPanel.BoundOption);
			}

			this.updateOptionLocationLabels();
		}

		if (this.OnOptionListUpdated != null)
		{
			this.OnOptionListUpdated();
		}
	}

	public void MoveOptionUp(OptionEditPanel targetOption)
	{
		OptionEditPanel currentOptionEditPanel = null;
		OptionEditPanel aboveOptionEditPanel = null;

		for (int optionPanelChildIndex = 0; optionPanelChildIndex < this.optionEditListVBoxContainer.GetChildCount(); optionPanelChildIndex++)
		{
			currentOptionEditPanel = this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(optionPanelChildIndex);
			if (currentOptionEditPanel != null &&
				aboveOptionEditPanel != null &&
				currentOptionEditPanel == targetOption)
			{
				DtgeCore.Option aboveOptionCopy = new DtgeCore.Option();
				aboveOptionCopy.CopyFrom(aboveOptionEditPanel.BoundOption);
				aboveOptionEditPanel.BoundOption.CopyFrom(currentOptionEditPanel.BoundOption);
				currentOptionEditPanel.BoundOption.CopyFrom(aboveOptionCopy);
				break;
			}
			aboveOptionEditPanel = currentOptionEditPanel;
		}

		if (currentOptionEditPanel != null)
		{
			currentOptionEditPanel.UpdateUIFromOption();
		}
		
		if (aboveOptionEditPanel != null)
		{
			aboveOptionEditPanel.UpdateUIFromOption();
		}
	}

	public void MoveOptionDown(OptionEditPanel targetOption)
	{
		OptionEditPanel currentOptionEditPanel = null;
		OptionEditPanel belowOptionEditPanel = null;
		for (int optionPanelChildIndex = 0; optionPanelChildIndex < this.optionEditListVBoxContainer.GetChildCount(); optionPanelChildIndex++)
		{
			currentOptionEditPanel = this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(optionPanelChildIndex);
			belowOptionEditPanel = this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(optionPanelChildIndex + 1);
			if (currentOptionEditPanel != null &&
				belowOptionEditPanel != null &&
				currentOptionEditPanel == targetOption)
			{
				DtgeCore.Option belowOptionCopy = new DtgeCore.Option();
				belowOptionCopy.CopyFrom(belowOptionEditPanel.BoundOption);
				belowOptionEditPanel.BoundOption.CopyFrom(currentOptionEditPanel.BoundOption);
				currentOptionEditPanel.BoundOption.CopyFrom(belowOptionCopy);

				break;
			}
		}

		if (currentOptionEditPanel != null)
		{
			currentOptionEditPanel.UpdateUIFromOption();
		}

		if (belowOptionEditPanel != null)
		{
			belowOptionEditPanel.UpdateUIFromOption();
		}
	}

	public void HandleOptionDeleted(OptionEditPanel toRemove)
	{
		this.optionEditListVBoxContainer.RemoveChild(toRemove);
		this.HandleOptionUpdated(true);
	}

	public void HandleTryOpenScene(DtgeCore.Scene.SceneId sceneId)
	{
		this.OnTryOpenScene(sceneId);
	}

	public void _on_add_option_button_pressed()
	{
		DtgeCore.Option newOption = new DtgeCore.Option();
		this.DtgeScene.AddOption(newOption);
		this.addNewOptionEditPanel(newOption);
		this.OnOptionListUpdated();
	}

	private void updateOptionLocationLabels()
	{
		for (int optionPanelIndex = 0, optionlocationLabelIndex = 0;
			optionPanelIndex < this.optionEditListVBoxContainer.GetChildCount();
			optionPanelIndex++, optionlocationLabelIndex++)
		{
			OptionEditPanel currentOptionEditPanel = this.optionEditListVBoxContainer.GetChild<OptionEditPanel>(optionPanelIndex);
			currentOptionEditPanel.UpdateOptionLocationLabel(this.OptionLocationLabelValues[optionlocationLabelIndex]);
		}
	}

	private void updateOptionEditPanelsFromScene()
	{
		DtgeCore.Option[] updatedOptions = this.dtgeScene.OptionList;
		int nonNullOptionCount = 0;

		for (int optionIndex = 0; optionIndex < updatedOptions.Length; optionIndex++)
		{
			DtgeCore.Option currentOption = updatedOptions[optionIndex];
			if (currentOption == null)
			{
				break;
			}
			else
			{
				nonNullOptionCount++;
				OptionEditPanel currentOptionEditPanel = this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(optionIndex);
				if (currentOptionEditPanel != null)
				{
					currentOptionEditPanel.BoundOption = updatedOptions[optionIndex];
				}
				else
				{
					this.addNewOptionEditPanel(updatedOptions[optionIndex]);
				}
			}
		}

		while (nonNullOptionCount < this.optionEditListVBoxContainer.GetChildCount())
		{
			OptionEditPanel excessOptionEditPanel
				= this.optionEditListVBoxContainer.GetChildOrNull<OptionEditPanel>(
					this.optionEditListVBoxContainer.GetChildCount() - 1);
			this.optionEditListVBoxContainer.RemoveChild(excessOptionEditPanel);
		}
	}

	private void addNewOptionEditPanel(DtgeCore.Option option)
	{
		if (this.optionEditListVBoxContainer.GetChildCount() >= 15)
		{
			GD.Print("Cannot add more than 15 options");
		}
		else
		{
			OptionEditPanel newOptionEditPanel =
				((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.OPTION_EDIT_PANEL_PATH)).Instantiate<OptionEditPanel>();

			if (newOptionEditPanel != null)
			{
				newOptionEditPanel.BoundOption = option;
				newOptionEditPanel.OnOptionUpdated = this.HandleOptionUpdated;
				newOptionEditPanel.OnOptionMovedUp = this.MoveOptionUp;
				newOptionEditPanel.OnOptionMovedDown = this.MoveOptionDown;
				newOptionEditPanel.OnOptionDeleted = this.HandleOptionDeleted;
				newOptionEditPanel.OnTryOpenScene = this.HandleTryOpenScene;
				this.optionEditListVBoxContainer.AddChild(newOptionEditPanel);
			}

			this.updateOptionLocationLabels();
		}
	}
}
