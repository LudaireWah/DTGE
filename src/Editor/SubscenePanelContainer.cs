using System;

using Godot;

using DtgeGodotCommon;

namespace DtgeEditor;

/**
 * The root node for the Godot scene responsible for authoring DTGE Subscenes.
 */
public partial class SubscenePanelContainer : PanelContainer
{
	LineEdit nameLineEdit;

	public Action<SubscenePanelContainer> OnSubsceneDeleted;

	public DtgeCore.Editing.SubsceneEditable SubsceneEditable { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.nameLineEdit = this.GetNode<LineEdit>("SubsceneMarginContainer/SubsceneHBoxContainer/SubsceneLineEdit");
	}

	public void UpdateUIFromEditables()
	{
		if (this.nameLineEdit.Text != this.SubsceneEditable.Name)
		{
			GodotUtilities.UpdateNodeText(this.nameLineEdit, this.SubsceneEditable.Name);	
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
