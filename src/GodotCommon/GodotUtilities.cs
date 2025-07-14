using System;
using System.Media;
using System.Runtime.Versioning;

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
	 * Godot silently fails when a GetNode call fails, which can make it harder to diagnose
	 * simple issues of typos or renaming in paths. Using this function makes this fail faster,
	 * allowing you to immediately know if the call to get a child node failed.
	 */
	public static T GetNodeSmart<T>(
		Node node,
		string childPath,
		Action<string> errorAction)
		where T : Node
	{
		T childNode = null;
		childNode = node.GetNode<T>(childPath);

		if (childNode == null)
		{
			errorAction("A Godot node wasn't found. Path: " + childPath);
		}

		return childNode;
	}

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

	public static void PlayTextEntryErrorSound()
	{
		if (OperatingSystem.IsWindows())
		{
			SystemSounds.Exclamation.Play();
		}
	}
}
