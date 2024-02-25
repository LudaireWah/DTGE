using Godot;
using System;

namespace DtgeGame;

public partial class NavigationButton : MarginContainer
{
    public DtgeCore.Option boundOption;
    public Action<DtgeCore.Option> boundAction;

    Button button;
    MarginContainer shortcutMarginContainer;
    ColorRect navigationButtonPlaceholderColorRect;

    ColorRect rightClickMenuCapture;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.button = GetNode<Button>("NavigationButton");
        this.shortcutMarginContainer = GetNode<MarginContainer>("ShortcutContainer");
        this.navigationButtonPlaceholderColorRect = GetNode<ColorRect>("NavigationButtonPlaceholderColorRect");

        this.rightClickMenuCapture = GetNode<ColorRect>("NavigationButton/RightClickMenuCapture");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void SetEditMode(bool editModeEnabled)
    {
        this.rightClickMenuCapture.Visible = editModeEnabled;
    }

    public void BindButton(DtgeCore.Option option, Action<DtgeCore.Option> action)
    {
        this.boundOption = option;
        this.boundAction = action;
        this.updateStateFromOption();
    }

    private void updateStateFromOption()
    {
        if (this.boundOption == null)
        {
            this.button.Text = "";
            this.button.Disabled = true;
            this.button.Visible = false;
            this.shortcutMarginContainer.Visible = false;
            this.navigationButtonPlaceholderColorRect.Visible = true;
        }
        else
        {
            this.button.Text = this.boundOption.displayName;
            this.button.Disabled = !this.boundOption.enabled;
            this.button.Visible = true;
            this.shortcutMarginContainer.Visible = true;
            this.navigationButtonPlaceholderColorRect.Visible = false;
        }
    }

    private void _on_right_click_menu_capture_gui_input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton)
        {
            InputEventMouseButton inputEventMouseButton = (InputEventMouseButton)inputEvent;
            if (inputEventMouseButton.Pressed && inputEventMouseButton.ButtonIndex == MouseButton.Right)
            {
                GD.Print("Right click!");
                this.GetViewport().SetInputAsHandled();
            }
        }
    }

    private void _on_navigation_button_pressed()
    {
        if (boundOption != null && boundAction != null)
        {
            this.boundAction(this.boundOption);
        }
    }
}
