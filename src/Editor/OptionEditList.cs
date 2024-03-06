using Godot;
using System;
using System.Collections.Generic;

namespace DtgeEditor;

public partial class OptionEditList : VBoxContainer
{
	VBoxContainer optionEditListVBoxContainer;
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionEditListVBoxContainer = this.GetNode<VBoxContainer>("OptionEditListScrollContainer/OptionEditListVBoxContainer");

		this.updateOptionLocationLabels();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.uiNeedUpdate)
		{
			this.updateOptionEditPanelsFromScene();
			this.uiNeedUpdate = false;
		}
	}

	public void FlushChangesForSave()
    {
        for (int optionPanelIndex = 0; optionPanelIndex < this.optionEditListVBoxContainer.GetChildCount(); optionPanelIndex++)
        {
            this.optionEditListVBoxContainer.GetChild<OptionEditPanel>(optionPanelIndex).FlushChangesForSave();
        }
    }

	public void OnOptionUpdate(bool countChanged)
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
	}

	public void OnOptionDeleted(OptionEditPanel toRemove)
	{
        this.optionEditListVBoxContainer.RemoveChild(toRemove);
		this.OnOptionUpdate(true);
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
                newOptionEditPanel.OptionUpdatedAction = this.OnOptionUpdate;
                newOptionEditPanel.OptionDeletedAction = this.OnOptionDeleted;
                this.optionEditListVBoxContainer.AddChild(newOptionEditPanel);
            }

            this.updateOptionLocationLabels();
        }
    }

    public void _on_add_option_button_pressed()
	{
		DtgeCore.Option newOption = new DtgeCore.Option();
		this.DtgeScene.AddOption(newOption);
		this.addNewOptionEditPanel(newOption);
	}
}
