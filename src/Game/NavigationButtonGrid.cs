using Godot;
using System;

namespace DtgeGame;

/**
 * NavigationButtonGrid is a GridContainer that contains a set of navigation
 * buttons used to present options to players. The NavigationButtonGrid is
 * responsible for tracking all the buttons, binding them to the right option,
 * and owns the layout as laid out in the Godot editor.
 */
public partial class NavigationButtonGrid : GridContainer
{
	public Action<DtgeCore.Option> OnOptionSelected;
	public DtgeCore.GameData.NavigationGridShortcutMode NavigationGridShortcutMode;

	private int columnCount;
	private int rowCount;

	private static readonly string[,] navigationGridShortcutStringsKeyboard =
		{ {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0" },
		 { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
		 { "A", "S", "D", "F", "G", "H", "J", "K", "L", ";" },
		 { "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/" } };
	private static readonly Key[,] navigationGridShortcutKeysKeyboard =
		{ { Key.Key1, Key.Key2, Key.Key3, Key.Key4, Key.Key5, Key.Key6, Key.Key7, Key.Key8, Key.Key9, Key.Key0 },
			{ Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P },
			{ Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.Semicolon },
			{ Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.Comma, Key.Period, Key.Slash } };
	private static readonly int maximumShortcutColumn = 10;
	private static readonly int maximumShortcutRow = 4;

	private static readonly string[] navigationGridShortcutStringsNumeric =
		{"1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
	private static readonly Key[] navigationGridShortcutKeysNumeric =
		{ Key.Key1, Key.Key2, Key.Key3, Key.Key4, Key.Key5, Key.Key6, Key.Key7, Key.Key8, Key.Key9, Key.Key0 };

	private static readonly string navigationGridShortcutStringNone = "";
	private static readonly Key navigationGridShortcutKeyNone = Key.None;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void ChangeGridDimensions(int columns, int rows)
	{
		this.columnCount = columns;
		this.rowCount = rows;
		this.Columns = this.columnCount;
		int desiredButtonCount = this.rowCount * this.columnCount;

		while (this.GetChildCount() > desiredButtonCount)
		{
			this.RemoveChild(this.GetChild(this.GetChildCount() - 1));
		}

		while (this.GetChildCount() < desiredButtonCount)
		{
			NavigationButton newNavigationButton =
				((PackedScene)GD.Load(DtgeGodotCommon.GodotConstants.NAVIGATION_BUTTON_PATH)).Instantiate<NavigationButton>();
			newNavigationButton.OnOptionSelected = this.handleOptionSelected;

			this.AddChild(newNavigationButton);
		}

		for (int navigationButtonIndex = 0; navigationButtonIndex < this.GetChildCount(); navigationButtonIndex++)
		{
			NavigationButton currentNavigationButton = this.GetChildOrNull<NavigationButton>(navigationButtonIndex);
			currentNavigationButton.SetOptionShortcut(
				this.getStringFromNavigationButtonIndex(navigationButtonIndex),
				this.getKeyFromNavigationButtonIndex(navigationButtonIndex));
		}
	}

	public void BindSceneOptionsToButtons(DtgeCore.Scene scene)
	{
		if (scene == null)
		{
			return;
		}
		else
		{
			int sceneOptionCount = scene.GetOptionCount();
			for (int navigationButtonIndex = 0; navigationButtonIndex < this.GetChildCount(); navigationButtonIndex++)
			{
				NavigationButton currentNavigationButton = this.GetChildOrNull<NavigationButton>(navigationButtonIndex);
				if (navigationButtonIndex < sceneOptionCount)
				{
					currentNavigationButton.BindToOption(scene.GetOption(navigationButtonIndex));
				}
				else
				{
					currentNavigationButton.ClearBoundOption();
				}
			}
		}
	}

	private void bindCachedSceneOptionsToButtons()
	{
		
	}

	private void handleOptionSelected(DtgeCore.Option option)
	{
		this.OnOptionSelected(option);
	}

	private string getStringFromNavigationButtonIndex(int optionIndex)
	{
		string result = NavigationButtonGrid.navigationGridShortcutStringNone;

		switch(this.NavigationGridShortcutMode)
		{
		case DtgeCore.GameData.NavigationGridShortcutMode.Keyboard:
			if (optionIndex < NavigationButtonGrid.maximumShortcutColumn * NavigationButtonGrid.maximumShortcutRow)
			{
				int columnIndex = optionIndex % this.columnCount;
				int rowIndex = optionIndex / this.columnCount;
				result = NavigationButtonGrid.navigationGridShortcutStringsKeyboard[rowIndex, columnIndex];
			}
			break;
		case DtgeCore.GameData.NavigationGridShortcutMode.Numeric:
			if (optionIndex < NavigationButtonGrid.navigationGridShortcutStringsNumeric.Length)
			{
				result = NavigationButtonGrid.navigationGridShortcutStringsNumeric[optionIndex];
			}
			break;
		default:
			throw new NotImplementedException();
		}
		return result;
	}

	private Key getKeyFromNavigationButtonIndex(int optionIndex)
	{
		Key result = NavigationButtonGrid.navigationGridShortcutKeyNone;

		switch(this.NavigationGridShortcutMode)
		{
		case DtgeCore.GameData.NavigationGridShortcutMode.Keyboard:
			if (optionIndex < NavigationButtonGrid.maximumShortcutColumn * NavigationButtonGrid.maximumShortcutRow)
			{
				int columnIndex = optionIndex % this.columnCount;
				int rowIndex = optionIndex / this.columnCount;
				result = NavigationButtonGrid.navigationGridShortcutKeysKeyboard[rowIndex, columnIndex];
			}
			break;
		case DtgeCore.GameData.NavigationGridShortcutMode.Numeric:
			if (optionIndex < NavigationButtonGrid.navigationGridShortcutKeysNumeric.Length)
			{
				result = NavigationButtonGrid.navigationGridShortcutKeysNumeric[optionIndex];
			}
			break;
		default:
			throw new NotImplementedException();
		}
		return result;
	}
}
