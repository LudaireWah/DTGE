using Godot;

namespace DtgeGodotCommon;


/**
 * This class contains a variety of utility functions for working with Godot. In most cases, these
 * functions represent best practices when doing an operation on Godot nodes when working in
 * DtgeEditor or DtgeGame.
 */
public class GodotUtilities
{
	/**
	 * Setting the Text property on a LineEdit resets the caret position, so we should only call
	 * the property's setter if the text is different.
	 */
	public static void UpdateNodeText(LineEdit element, string newText)
	{
		if (element.Text != newText)
		{
			element.Text = newText;
		}
	}

	/**
	 * Setting the Text property on a TextEdit resets the caret position, so we should only call
	 * the property's setter if the text is different.
	 */
	public static void UpdateNodeText(TextEdit element, string newText)
	{
		if (element.Text != newText)
		{
			element.Text = newText;
		}
	}
}
