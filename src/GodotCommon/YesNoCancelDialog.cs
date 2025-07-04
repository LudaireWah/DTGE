using System;

using Godot;

/**
 * This is a simple window that provides the three titular buttons and On functions for each,
 * allowing easy use of this very common pattern in UX where you ask whether or not to do some
 * special action as part of what a user did hwile also giving them the option to back out of
 * taking that action in the first place.
 */
public partial class YesNoCancelDialog : Window
{
	MarginContainer rootMarginContainer;
	RichTextLabel dialogText;

	public Action OnYesSelected;
	public Action OnNoSelected;
	public Action OnCancelSelected;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.rootMarginContainer = this.GetNode<MarginContainer>("RootMarginContainer");
		this.dialogText = this.GetNode<RichTextLabel>("RootMarginContainer/VBoxContainer/VBoxContainer/Dialog Text");

		this.SizeChanged += this.HandleWindowSizeChanged;
	}

	public void HandleWindowSizeChanged()
	{
		Rect2 newViewport = this.GetViewport().GetVisibleRect();
		this.rootMarginContainer.SetSize(newViewport.Size);
	}

	public void SetDialogText(string text)
	{
		this.dialogText.Text = "[center]" + text + "[/center]";
	}

	public void _on_close_requested()
	{
		this._on_cancel_button_pressed();
	}

	public void _on_yes_button_pressed()
	{
		if (this.OnYesSelected != null)
		{
			this.OnYesSelected();
		}
		this.Hide();
	}

	public void _on_no_button_pressed()
	{
		if (this.OnNoSelected != null)
		{
			this.OnNoSelected();
		}
		this.Hide();
	}

	public void _on_cancel_button_pressed()
	{
		if (this.OnCancelSelected != null)
		{
			this.OnCancelSelected();
		}
		this.Hide();
	}
}
