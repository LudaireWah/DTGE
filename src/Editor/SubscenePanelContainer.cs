using Godot;
using System;

namespace DtgeEditor;

public partial class SubscenePanelContainer : PanelContainer
{
	LineEdit nameLineEdit;

	public Action<SubscenePanelContainer> OnSubsceneDeleted;

	private DtgeCore.SubsceneEditable subsceneEditable;
	public DtgeCore.SubsceneEditable SubsceneEditable
	{
		get { return this.subsceneEditable; }
		set
		{
			this.subsceneEditable = value;
			this.UiNeedsUpdate = true;
		}
	}
	public bool UiNeedsUpdate;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.nameLineEdit = this.GetNode<LineEdit>("SubsceneMarginContainer/SubsceneHBoxContainer/SubsceneLineEdit");
		if (this.UiNeedsUpdate)
		{
			this.nameLineEdit.Text = this.subsceneEditable.Name;
			this.UiNeedsUpdate = false;
		}
	}

	public void _on_subscene_delete_button_pressed()
	{
		this.OnSubsceneDeleted(this);
	}

	public void _on_subscene_line_edit_text_changed(string newText)
	{
		if (newText != this.subsceneEditable.Name)
		{
			this.subsceneEditable.Name = newText;
		}
	}
}
