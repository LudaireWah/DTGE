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
	public Action<DtgeCore.SceneId> OnTryOpenScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionLocationLabel = GodotUtilities.GetNodeSmart<Label>(this, "OptionEditMarginContainer/OptionEditVBoxContainer/OptionEditHeaderContainer/OptionLocationLabel", GodotEditorErrorHandler.InvokeError);
		this.idLineEdit = GodotUtilities.GetNodeSmart<LineEdit>(this, "OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/IdLineEdit", GodotEditorErrorHandler.InvokeError);
		this.targetSceneLineEdit = GodotUtilities.GetNodeSmart<LineEdit>(this, "OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/TargetSceneHBoxContainer/TargetSceneLineEdit", GodotEditorErrorHandler.InvokeError);
		this.displayNameLineEdit = GodotUtilities.GetNodeSmart<LineEdit>(this, "OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/DisplayNameLineEdit", GodotEditorErrorHandler.InvokeError);
		this.optionEnabledCheckButton = GodotUtilities.GetNodeSmart<CheckButton>(this, "OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/OptionEnabledCheckButton", GodotEditorErrorHandler.InvokeError);
		this.moveOptionUpButton = GodotUtilities.GetNodeSmart<Button>(this, "OptionEditMarginContainer/OptionEditVBoxContainer/OptionEditHeaderContainer/MoveUpButton", GodotEditorErrorHandler.InvokeError);
		this.moveOptionDownButton = GodotUtilities.GetNodeSmart<Button>(this, "OptionEditMarginContainer/OptionEditVBoxContainer/OptionEditHeaderContainer/MoveDownButton", GodotEditorErrorHandler.InvokeError);
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
		if (!DtgeCore.Editing.UserDefinedNameValidator.IsValidNestableName(newText))
		{
			GodotUtilities.PlayTextEntryErrorSound();
			string correctedName =
				DtgeCore.Editing.UserDefinedNameValidator.CorrectNestableName(newText);
			this.OptionEditable.TargetSceneId = correctedName;
			int oldCaretColumn = this.targetSceneLineEdit.CaretColumn;
			this.targetSceneLineEdit.Text = correctedName;
			this.targetSceneLineEdit.CaretColumn = oldCaretColumn;


		}
		else
		{
			this.OptionEditable.TargetSceneId = this.targetSceneLineEdit.Text;
		}
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
		DtgeCore.SceneId targetSceneId = new DtgeCore.SceneId(this.targetSceneLineEdit.Text);
		this.OnTryOpenScene(targetSceneId);
	}
}
