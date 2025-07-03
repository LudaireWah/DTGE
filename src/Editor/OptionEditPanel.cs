using System;

using Godot;

using DtgeGodotCommon;

namespace DtgeEditor;

/**
 * The root node for the Godot scene responsible for authoring DTGE Options.
 */
public partial class OptionEditPanel : PanelContainer
{
	Label optionLocationLabel;
	LineEdit idLineEdit;
	LineEdit targetSceneLineEdit;
	LineEdit displayNameLineEdit;
	CheckButton optionEnabledCheckButton;
	Button moveOptionUpButton;
	Button moveOptionDownButton;

	public DtgeCore.Editing.OptionEditable OptionEditable;
	
	public Action<DtgeCore.Editing.OptionEditable> OnOptionMovedUp;
	public Action<DtgeCore.Editing.OptionEditable> OnOptionMovedDown;
	public Action<DtgeCore.Editing.OptionEditable> OnOptionDeleted;
	public Action<DtgeCore.Scene.SceneId> OnTryOpenScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionLocationLabel = this.GetNode<Label>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionEditHeaderContainer/OptionLocationLabel");
		this.idLineEdit = this.GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/IdLineEdit");
		this.targetSceneLineEdit = this.GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/TargetSceneHBoxContainer/TargetSceneLineEdit");
		this.displayNameLineEdit = this.GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/DisplayNameLineEdit");
		this.optionEnabledCheckButton = this.GetNode<CheckButton>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/OptionEnabledCheckButton");
		this.moveOptionUpButton = this.GetNode<Button>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionEditHeaderContainer/MoveUpButton");
		this.moveOptionDownButton = this.GetNode<Button>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionEditHeaderContainer/MoveDownButton");
	}

	public void UpdateFromEditables()
	{
		GodotUtilities.UpdateNodeText(this.idLineEdit, this.OptionEditable.Name);
		GodotUtilities.UpdateNodeText(this.targetSceneLineEdit, this.OptionEditable.TargetSceneId);
		GodotUtilities.UpdateNodeText(this.displayNameLineEdit, this.OptionEditable.DisplayName);
		this.optionEnabledCheckButton.ButtonPressed = this.OptionEditable.Enabled;

		this.moveOptionUpButton.Disabled = this.OptionEditable.isFirst();
		this.moveOptionUpButton.FocusMode =
			this.OptionEditable.isFirst() ? FocusModeEnum.None : FocusModeEnum.All;

		this.moveOptionDownButton.Disabled = this.OptionEditable.isLast();
		this.moveOptionDownButton.FocusMode =
			this.OptionEditable.isLast() ? FocusModeEnum.None : FocusModeEnum.All;
	}

	public void UpdateOptionLocationLabel(int optionIndex)
	{
		this.optionLocationLabel.Text = "Option " + (optionIndex + 1);
	}

	public void _on_option_enabled_check_button_pressed()
	{
		this.OptionEditable.Enabled = this.optionEnabledCheckButton.ButtonPressed;
	}

	public void _on_id_line_edit_text_changed(string newText)
	{
		this.OptionEditable.Name = this.idLineEdit.Text;
	}

	public void _on_target_scene_line_edit_text_changed(string newText)
	{
		this.OptionEditable.TargetSceneId = this.targetSceneLineEdit.Text;
	}

	public void _on_display_name_line_edit_text_changed(string newText)
	{
		this.OptionEditable.DisplayName = this.displayNameLineEdit.Text;
	}

	public void _on_move_up_button_pressed()
	{
		this.OnOptionMovedUp(this.OptionEditable);
	}

	public void _on_move_down_button_pressed()
	{
		this.OnOptionMovedDown(this.OptionEditable);
	}

	public void _on_delete_button_pressed()
	{
		this.OnOptionDeleted(this.OptionEditable);
	}

	public void _on_navigate_to_target_scene_button_pressed()
	{
		DtgeCore.Scene.SceneId targetSceneId = new DtgeCore.Scene.SceneId(this.targetSceneLineEdit.Text);
		this.OnTryOpenScene(targetSceneId);
	}
}
