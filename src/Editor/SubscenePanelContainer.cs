using Godot;
using System;

namespace DtgeEditor;

public partial class SubscenePanelContainer : PanelContainer
{
	LineEdit nameLineEdit;

	public Action OnSubsceneUpdated;
	public Action<SubscenePanelContainer> OnSubsceneDeleted;

	private string pendingSubsceneName;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.nameLineEdit = GetNode<LineEdit>("SubsceneMarginContainer/SubsceneHBoxContainer/SubsceneLineEdit");
		if (this.pendingSubsceneName != null)
		{
			nameLineEdit.Text = pendingSubsceneName;
			pendingSubsceneName = null;
		}
	}

	public void SetSubsceneName(string subsceneName)
	{
		if (this.IsNodeReady())
		{
			this.nameLineEdit.Text = subsceneName;
		}
		else
		{
			this.pendingSubsceneName = subsceneName;
		}
	}

	public string GetSubsceneName()
	{
		return this.nameLineEdit.Text;
	}

	public void _on_subscene_delete_button_pressed()
	{
		this.OnSubsceneDeleted(this);
	}

	public void _on_subscene_line_edit_text_changed(string newText)
	{
		this.OnSubsceneUpdated();
	}
}
