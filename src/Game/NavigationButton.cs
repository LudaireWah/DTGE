using Godot;
using System;

namespace DtgeGame;

/**
 * The NavigationButton collects together a Godot button plus a few
 * other things to display a few things like keyboard shortcuts,
 * placeholders for options that don't exist, and similar stuff. The
 * bulk of the button logic is handled by the Godot Button.
 */
public partial class NavigationButton : MarginContainer
{
    public DtgeCore.Option boundOption;
    public Action<DtgeCore.Option> boundAction;

    Button button;
    MarginContainer shortcutMarginContainer;
    ColorRect navigationButtonPlaceholderColorRect;

    public override void _Ready()
    {
        this.button = GetNode<Button>("MainButton");
        this.shortcutMarginContainer = GetNode<MarginContainer>("ShortcutContainer");
        this.navigationButtonPlaceholderColorRect = GetNode<ColorRect>("NavigationButtonPlaceholderColorRect");
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
            this.button.Text = this.boundOption.DisplayName;
            this.button.Disabled = !this.boundOption.Enabled;
            this.button.Visible = true;
            this.shortcutMarginContainer.Visible = true;
            this.navigationButtonPlaceholderColorRect.Visible = false;
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
