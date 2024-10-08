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

	private DtgeCore.Option boundOption;
	private bool uiNeedsUpdate;

	public DtgeCore.Option BoundOption
	{
		get
		{
			return this.boundOption;
		}
		set
		{
			this.boundOption = value;
			this.uiNeedsUpdate = true;
		}
	}
	
	public Action<bool> OnOptionUpdated;
	public Action<OptionEditPanel> OnOptionMovedUp;
	public Action<OptionEditPanel> OnOptionMovedDown;
	public Action<OptionEditPanel> OnOptionDeleted;
	public Action<DtgeCore.Scene.SceneId> OnTryOpenScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionLocationLabel = GetNode<Label>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionEditHeaderContainer/OptionLocationLabel");
		this.idLineEdit = GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/IdLineEdit");
		this.targetSceneLineEdit = GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/TargetSceneHBoxContainer/TargetSceneLineEdit");
		this.displayNameLineEdit = GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/DisplayNameLineEdit");
		this.optionEnabledCheckButton = GetNode<CheckButton>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/OptionEnabledCheckButton");

		if (this.boundOption == null)
		{
			this.BoundOption = new DtgeCore.Option();
		}
		this.UpdateUIFromOption();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.uiNeedsUpdate)
		{
			this.UpdateUIFromOption();
			this.uiNeedsUpdate = false;
		}
	}

	public void FlushChangesForSave()
	{
		this.updateOptionFromUI();
	}

	private void updateOptionFromUI()
	{
		bool newOptionAdded = false;
		if (this.boundOption == null)
		{
			this.boundOption = new DtgeCore.Option();
			newOptionAdded = true;
		}
		this.boundOption.Id = this.idLineEdit.Text;
		this.boundOption.TargetSceneId = this.targetSceneLineEdit.Text;
		this.boundOption.DisplayName = this.displayNameLineEdit.Text;
		this.boundOption.Enabled = this.optionEnabledCheckButton.ButtonPressed;
		this.OnOptionUpdated(newOptionAdded);
	}

	public void UpdateUIFromOption()
	{
		if (this.boundOption == null)
		{
			this.boundOption = new DtgeCore.Option();
		}
		this.idLineEdit.Text = this.boundOption.Id;
		this.targetSceneLineEdit.Text = this.boundOption.TargetSceneId;
		this.displayNameLineEdit.Text = this.boundOption.DisplayName;
		this.optionEnabledCheckButton.ButtonPressed = this.boundOption.Enabled;
	}

	public void UpdateOptionLocationLabel(int optionIndex)
	{
		this.optionLocationLabel.Text = "Option " + (optionIndex + 1);
	}

	public void _on_option_enabled_check_button_pressed()
	{
		if (this.boundOption == null)
		{
			this.boundOption = new DtgeCore.Option();
		}
		this.boundOption.Enabled = this.optionEnabledCheckButton.ButtonPressed;
		this.OnOptionUpdated(true);
	}

	public void _on_id_line_edit_text_changed(string newText)
	{
		this.updateOptionFromUI();
	}

	public void _on_target_scene_line_edit_text_changed(string newText)
	{
		this.updateOptionFromUI();
	}

	public void _on_display_name_line_edit_text_changed(string newText)
	{
		this.updateOptionFromUI();
	}


	public void _on_navigate_to_target_scene_button_pressed()
	{
		DtgeCore.Scene.SceneId targetSceneId = new DtgeCore.Scene.SceneId(this.targetSceneLineEdit.Text);
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
