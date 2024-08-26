using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore;

/**
 * GameData is the class containing the high level information used to run
 * the game as a whole rather than used only in the context of a single scene.
 */
public class GameData
{
	public const string GAME_DATA_FILE_PATH = "dtge.gamedata";
	private const string DEFAULT_SCENE_DIRECTORY_PATH = "DTGEScenes";
	private const string DEFAULT_START_SCENE_NAME = "startscene";
	private const NavigationGridShortcutMode DEFAULT_NAVIGATION_GRID_SHORTCUT_MODE = NavigationGridShortcutMode.Keyboard;
	private const int DEFAULT_NAVIGATION_GRID_COLUMNS = 5;
	private const int DEFAULT_NAVIGATION_GRID_ROWS = 3;

	public enum NavigationGridShortcutMode
	{
		Keyboard,
		Numeric
	}

	public string SceneDirectoryPath { get; private set; }
	public string StartSceneName { get; set; }
	public string StartScenePath { get { return StartSceneName + ".dscn"; } }
	public NavigationGridShortcutMode ActiveNavigationGridShortcutMode {  get; set; }
	public int NavigationGridColumns {  get; set; }
	public int NavigationGridRows { get; set; }
	public int MaximumSupportedOptions { get { return this.NavigationGridColumns * this.NavigationGridRows; } }

	private static GameData instance;

	public static GameData GetGameData()
	{
		if (GameData.instance == null)
		{
			GameData.instance = new GameData();
		}
		return GameData.instance;
	}

	public static void PopulateFromJson(string json)
	{
		GameData.instance = JsonSerializer.Deserialize<GameData>(json);
	}

	public GameData()
	{
		// TODO, DTGE-98: Once we move to custom serialization, we should move this back to private to seal off the singleton pattern, but it needs to be public until we can do that.
		this.SceneDirectoryPath = DEFAULT_SCENE_DIRECTORY_PATH;
		this.StartSceneName = DEFAULT_START_SCENE_NAME;
		this.ActiveNavigationGridShortcutMode = DEFAULT_NAVIGATION_GRID_SHORTCUT_MODE;
		this.NavigationGridColumns = DEFAULT_NAVIGATION_GRID_COLUMNS;
		this.NavigationGridRows = DEFAULT_NAVIGATION_GRID_ROWS;
	}
}