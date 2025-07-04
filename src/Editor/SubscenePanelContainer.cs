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

	public Action<SubscenePanelContainer> OnSubsceneDeleted;

	public DtgeCore.Editing.SubsceneEditable SubsceneEditable { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.deleteSubsceneButton = this.GetNode<Button>("SubsceneMarginContainer/SubsceneHBoxContainer/SubsceneDeleteButton");
		this.nameLineEdit = this.GetNode<LineEdit>("SubsceneMarginContainer/SubsceneHBoxContainer/NameLineEdit");
		this.readOnlyNameLabel = this.GetNode<Label>("SubsceneMarginContainer/SubsceneHBoxContainer/ReadOnlyNameLabel");
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
		this.OnSubsceneDeleted(this);
	}

	public void _on_subscene_line_edit_text_changed(string newText)
	{
		if (newText != this.SubsceneEditable.Name)
		{
			this.SubsceneEditable.Name = newText;
		}
	}
}
