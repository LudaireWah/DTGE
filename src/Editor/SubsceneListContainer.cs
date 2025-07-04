using DtgeEditor;

using Godot;
using System;

public partial class SubsceneListContainer : HBoxContainer
{
	Button newSubsceneButton;
	CheckButton allowNoSubsceneCheckButton;
	HBoxContainer subsceneListHBoxContainer;

	public DtgeCore.Editing.SceneEditable DtgeSceneEditable { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.newSubsceneButton = this.GetNode<Button>("NewSubsceneButton");
		this.allowNoSubsceneCheckButton = this.GetNode<CheckButton>("AllowNoSubsceneCheckButton");
		this.subsceneListHBoxContainer = this.GetNode<HBoxContainer>("SubsceneListScrollContainer/SubsceneListHBoxContainer");
	}

	public void UpdateFromEditables()
	{
		for (int subsceneIndex = 0;
			subsceneIndex < this.DtgeSceneEditable.GetSubsceneCount();
			subsceneIndex++)
		{
			DtgeCore.Editing.SubsceneEditable subsceneEditable =
				this.DtgeSceneEditable.GetSubscene(subsceneIndex);
			SubscenePanelContainer subscenePanelContainer =
				this.subsceneListHBoxContainer
				.GetChildOrNull<SubscenePanelContainer>(subsceneIndex);
			if (subscenePanelContainer != null)
			{
				subscenePanelContainer.SubsceneEditable = subsceneEditable;
				subscenePanelContainer.UpdateUIFromEditables();
			}
			else
			{
				SubscenePanelContainer newSubscenePanelContainer =
					((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.SUBSCENE_PANEL_CONTAINER_PATH))
					.Instantiate<SubscenePanelContainer>();

				if (newSubscenePanelContainer != null)
				{
					newSubscenePanelContainer.SubsceneEditable = subsceneEditable;
					newSubscenePanelContainer.OnSubsceneDeleted = this.HandleSubsceneDeleted;
					this.subsceneListHBoxContainer.AddChild(newSubscenePanelContainer);
					newSubscenePanelContainer.UpdateUIFromEditables();
				}
			}
		}

		while (this.DtgeSceneEditable.GetSubsceneCount() <
			this.subsceneListHBoxContainer.GetChildCount())
		{
			SubscenePanelContainer excessSubscenePanelContainer =
				this.subsceneListHBoxContainer.GetChildOrNull<SubscenePanelContainer>(
					this.subsceneListHBoxContainer.GetChildCount() - 1);
			this.subsceneListHBoxContainer.RemoveChild(excessSubscenePanelContainer);
		}

		this.allowNoSubsceneCheckButton.ButtonPressed =
			this.DtgeSceneEditable.NullSubsceneEnabled;

		if (this.DtgeSceneEditable.GetSubsceneCount() == 0)
		{
			this.allowNoSubsceneCheckButton.Visible = false;
		}
		else
		{
			this.allowNoSubsceneCheckButton.Visible = true;
		}
	}

	public void _on_new_subscene_button_pressed()
	{
		this.DtgeSceneEditable.AllocateNewSubscene();
	}

	public void _on_allow_no_subscene_check_button_toggled(bool on)
	{
		if (on)
		{
			this.DtgeSceneEditable.EnableNullSubscene();
		}
		else
		{
			this.DtgeSceneEditable.DisableNullSubscene();
		}
	}

	private void HandleSubsceneDeleted(DtgeCore.Editing.SubsceneEditable subsceneEditable)
	{
		this.DtgeSceneEditable.RemoveSubscene(subsceneEditable);
	}
}
