using Godot;
using System;

namespace DtgeEditor;

public partial class EntitySetterHBoxContainer : HBoxContainer
{
	private bool uiNeedsUpdate;
	private DtgeCore.Snippet.EntitySetter boundEntitySetter;
	public DtgeCore.Snippet.EntitySetter BoundEntitySetter
	{
		get
		{
			return this.boundEntitySetter;
		}
		set
		{
			this.boundEntitySetter = value;
			this.uiNeedsUpdate = true;
		}
	}

	public Action OnEntitySetterUpdated { get; set; }
	public Action<EntitySetterHBoxContainer> OnEntitySetterDeleted { get; set; }

	private LineEdit entityKeyLineEdit;
	private OptionButton entityValueOptionButton;

	public override void _Ready()
	{
		this.entityKeyLineEdit = GetNode<LineEdit>("EntityKeyLineEdit");
		this.entityValueOptionButton = GetNode<OptionButton>("EntityValueOptionButton");

		if (uiNeedsUpdate)
		{
			this.UpdateUI();
		}
	}

	public override void _Process(double delta)
	{
	}

	public void UpdateUI()
	{
		if (!this.IsNodeReady())
		{
			this.uiNeedsUpdate = true;
		}
		else
		{
			this.entityKeyLineEdit.Text = this.BoundEntitySetter.Name;
			if (this.BoundEntitySetter.Value == true)
			{
				this.entityValueOptionButton.Selected = 1;
			}
			else
			{
				this.entityValueOptionButton.Selected = 0;
			}

			this.uiNeedsUpdate = false;
		}
	}

	public void _on_entity_setter_line_edit_text_changed(string newText)
	{
		this.boundEntitySetter.Name = newText;
		if (this.OnEntitySetterUpdated != null)
		{
			this.OnEntitySetterUpdated();
		}
	}

	public void _on_entity_setter_option_button_item_selected(int index)
	{
		this.boundEntitySetter.Value = (index  == 1);
		if (this.OnEntitySetterUpdated != null)
		{
			this.OnEntitySetterUpdated();
		}
	}

	public void _on_entity_setter_delete_button_pressed()
	{
		if (this.OnEntitySetterDeleted != null)
		{
			this.OnEntitySetterDeleted(this);
		}
	}
}
