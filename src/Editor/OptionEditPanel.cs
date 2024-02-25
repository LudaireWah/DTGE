using Godot;
using System;

namespace DtgeEditor;

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
    
    public Action<bool> OptionUpdatedAction;
	public Action<OptionEditPanel> OptionDeletedAction;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.optionLocationLabel = GetNode<Label>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionEditHeaderContainer/OptionLocationLabel");
		this.idLineEdit = GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/IdLineEdit");
        this.targetSceneLineEdit = GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/TargetSceneLineEdit");
        this.displayNameLineEdit = GetNode<LineEdit>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/DisplayNameLineEdit");
        this.optionEnabledCheckButton = GetNode<CheckButton>("OptionEditMarginContainer/OptionEditVBoxContainer/OptionPropertiesContainer/OptionPropertiesEntryContainer/OptionEnabledCheckButton");

		if (this.boundOption == null)
        {
            this.BoundOption = new DtgeCore.Option();
        }
		this.updateUIFromOption();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.uiNeedsUpdate)
		{
			this.updateUIFromOption();
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
        this.boundOption.id = this.idLineEdit.Text;
		this.boundOption.targetSceneId = this.targetSceneLineEdit.Text;
		this.boundOption.displayName = this.displayNameLineEdit.Text;
        this.boundOption.enabled = this.optionEnabledCheckButton.ButtonPressed;
        this.OptionUpdatedAction(newOptionAdded);
	}

	private void updateUIFromOption()
	{
		if (this.boundOption == null)
        {
            this.idLineEdit.Text = "";
            this.targetSceneLineEdit.Text = "";
            this.displayNameLineEdit.Text = "";
            this.optionEnabledCheckButton.ButtonPressed = false;
        }
		else
        {
            this.idLineEdit.Text = this.boundOption.id;
            this.targetSceneLineEdit.Text = this.boundOption.targetSceneId;
            this.displayNameLineEdit.Text = this.boundOption.displayName;
            this.optionEnabledCheckButton.ButtonPressed = this.boundOption.enabled;
        }
    }

	public void UpdateOptionLocationLabel(string shortcutString)
	{
		this.optionLocationLabel.Text = "Option " + shortcutString;
	}

    public void _on_option_enabled_check_button_pressed()
	{
		if (this.boundOption == null)
		{
			this.boundOption = new DtgeCore.Option();
		}
		this.boundOption.enabled = this.optionEnabledCheckButton.ButtonPressed;
        this.OptionUpdatedAction(true);
	}

	public void _on_id_line_edit_text_submitted(string text)
	{
		this.updateOptionFromUI();
	}

	public void _on_id_line_edit_focus_exited()
	{
		this.updateOptionFromUI();
	}

	public void _on_target_scene_line_edit_text_submitted(string text)
	{
		this.updateOptionFromUI();
	}

	public void _on_target_scene_line_edit_focus_exited()
	{
		this.updateOptionFromUI();
	}

    public void _on_display_name_line_edit_text_submitted()
	{
		this.updateOptionFromUI();
	}

	public void _on_display_name_line_edit_focus_exited()
	{
		this.updateOptionFromUI();
	}

    public void _on_delete_button_pressed()
	{
		this.OptionDeletedAction(this);
	}
}
