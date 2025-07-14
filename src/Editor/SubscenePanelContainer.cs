using System;

using Godot;

using DtgeGodotCommon;

namespace DtgeEditor;

/**
 * The root node for the Godot scene responsible for authoring DTGE Subscenes.
 */
public partial class SubscenePanelContainer : PanelContainer
{
	Button deleteSubsceneButton;
	LineEdit nameLineEdit;
	Label readOnlyNameLabel;

	public Action<DtgeCore.Editing.SubsceneEditable> OnSubsceneDeleted;

	public DtgeCore.Editing.SubsceneEditable SubsceneEditable { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.deleteSubsceneButton = GodotUtilities.GetNodeSmart<Button>(this, "SubsceneMarginContainer/SubsceneHBoxContainer/SubsceneDeleteButton", GodotEditorErrorHandler.InvokeError);
		this.nameLineEdit = GodotUtilities.GetNodeSmart<LineEdit>(this, "SubsceneMarginContainer/SubsceneHBoxContainer/NameLineEdit", GodotEditorErrorHandler.InvokeError);
		this.readOnlyNameLabel = GodotUtilities.GetNodeSmart<Label>(this, "SubsceneMarginContainer/SubsceneHBoxContainer/ReadOnlyNameLabel", GodotEditorErrorHandler.InvokeError);
	}

	public void UpdateUIFromEditables()
	{
		if (this.SubsceneEditable.IsReadOnly)
		{
			this.deleteSubsceneButton.Visible = false;
			this.deleteSubsceneButton.Disabled = true;
			this.nameLineEdit.Visible = false;
			this.readOnlyNameLabel.Visible = true;
			this.readOnlyNameLabel.Text = this.SubsceneEditable.Name;

		}
		else
		{
			this.deleteSubsceneButton.Visible = true;
			this.deleteSubsceneButton.Disabled = false;
			this.nameLineEdit.Visible = true;
			GodotUtilities.UpdateNodeText(this.nameLineEdit, this.SubsceneEditable.Name);
			this.readOnlyNameLabel.Visible = false;
		}
	}

	public void _on_subscene_delete_button_pressed()
	{
		this.OnSubsceneDeleted(this.SubsceneEditable);
	}

	public void _on_subscene_line_edit_text_changed(string newText)
	{
		if (newText != this.SubsceneEditable.Name)
		{
			this.SubsceneEditable.Name = newText;
		}
	}
}
