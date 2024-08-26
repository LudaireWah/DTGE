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

	Button button;
	MarginContainer shortcutMarginContainer;
	Label shortcutLabel;
	ColorRect navigationButtonPlaceholderColorRect;

	public Action<DtgeCore.Option> OnOptionSelected;

	private string desiredShortcutText;
	private Key desiredShortcutKey;
	private bool shortcutNeedsUpdate;

	public override void _Ready()
	{
		this.button = GetNode<Button>("MainButton");
		this.shortcutMarginContainer = GetNode<MarginContainer>("ShortcutContainer");
		this.shortcutLabel = GetNode<Label>("ShortcutContainer/ShortcutLabel");
		this.navigationButtonPlaceholderColorRect = GetNode<ColorRect>("NavigationButtonPlaceholderColorRect");
		if (this.shortcutNeedsUpdate)
		{
			this.SetOptionShortcut(this.desiredShortcutText, this.desiredShortcutKey);
		}
	}

	public void BindToOption(DtgeCore.Option option)
	{
		this.boundOption = option;
		this.updateStateFromOption();
	}

	public void ClearBoundOption()
	{
		this.boundOption = null;
		this.updateStateFromOption();
	}

	public void SetOptionShortcut(string shortcutText, Key key)
	{
		if (this.IsNodeReady())
		{
			this.shortcutLabel.Text = shortcutText;

			if (this.button.Shortcut == null)
			{
				this.button.Shortcut = new Shortcut();
			}
			this.button.Shortcut.Events.Clear();
			InputEventKey hotkey = new InputEventKey();
			hotkey.Keycode = key;
			this.button.Shortcut.Events.Add(hotkey);

			this.shortcutNeedsUpdate = false;
		}
		else
		{
			this.desiredShortcutText = shortcutText;
			this.desiredShortcutKey = key;
			this.shortcutNeedsUpdate = true;
		}
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
		if (boundOption != null && this.OnOptionSelected != null)
		{
			this.OnOptionSelected(this.boundOption);
		}
	}
}
