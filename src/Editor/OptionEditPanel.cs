using Godot;
using System;

namespace DtgeEditor;

/**
 * The root node for the Godot scene responsible for authoring
 * DTGE Options.
 */
public partial class OptionEditPanel : PanelContainer
{
	Label optionLocationLabel;
	LineEdit idLineEdit;
	LineEdit targetSceneLineEdit;
	LineEdit displayNameLineEdit;
	CheckButton optionEnabledCheckButton;

	private DtgeCore.OptionEditable optionEditable;
	public DtgeCore.OptionEditable OptionEditable
	{
		get
		{
			return this.optionEditable;
		}
		set
		{
			this.optionEditable = value;
			this.UiNeedsUpdate = true;
		}
	}
	public bool UiNeedsUpdate;
	
	public Action<OptionEditPanel> OnOptionMovedUp;
	public Action<OptionEditPanel> OnOptionMovedDown;
	public Action<OptionEditPanel> OnOptionDeleted;
	public Action<DtgeCore.SceneId> OnTryOpenScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionLocationLabel = this.GetNode<Label>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionEditHeaderContainer/OptionLocationLabel");
		this.idLineEdit = this.GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/IdLineEdit");
		this.targetSceneLineEdit = this.GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/TargetSceneHBoxContainer/TargetSceneLineEdit");
		this.displayNameLineEdit =this.GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/DisplayNameLineEdit");
		this.optionEnabledCheckButton = this.GetNode<CheckButton>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/OptionEnabledCheckButton");

		this.UpdateUIFromEditables();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.UiNeedsUpdate)
		{
			this.UpdateUIFromEditables();
			this.UiNeedsUpdate = false;
		}
	}

	private void updateEditablesFromUI()
	{
		this.OptionEditable.Name = this.idLineEdit.Text;
		this.OptionEditable.TargetSceneId = this.targetSceneLineEdit.Text;
		this.OptionEditable.DisplayName = this.displayNameLineEdit.Text;
		this.OptionEditable.Enabled = this.optionEnabledCheckButton.ButtonPressed;
	}

	private void UpdateUIFromEditables()
	{
		this.idLineEdit.Text = this.OptionEditable.Name;
		this.targetSceneLineEdit.Text = this.OptionEditable.TargetSceneId;
		this.displayNameLineEdit.Text = this.OptionEditable.DisplayName;
		this.optionEnabledCheckButton.ButtonPressed = this.OptionEditable.Enabled;
	}

	public void FlushChangesForSave()
	{
		this.updateEditablesFromUI();
	}

	public void UpdateOptionLocationLabel(int optionIndex)
	{
		this.optionLocationLabel.Text = "Option " + (optionIndex + 1);
	}

	public void _on_option_enabled_check_button_pressed()
	{
		this.updateEditablesFromUI();
	}

	public void _on_id_line_edit_text_changed(string newText)
	{
		this.updateEditablesFromUI();
	}

	public void _on_target_scene_line_edit_text_changed(string newText)
	{
		this.updateEditablesFromUI();
	}

	public void _on_display_name_line_edit_text_changed(string newText)
	{
		this.updateEditablesFromUI();
	}

	public void _on_navigate_to_target_scene_button_pressed()
	{
		DtgeCore.SceneId targetSceneId = new DtgeCore.SceneId(this.targetSceneLineEdit.Text);
		this.OnTryOpenScene(targetSceneId);
	}

	public void _on_move_up_button_pressed()
	{
		this.OnOptionMovedUp(this);
	}

	public void _on_move_down_button_pressed()
	{
		this.OnOptionMovedDown(this);
	}

	public void _on_delete_button_pressed()
	{
		this.OnOptionDeleted(this);
	}
}
