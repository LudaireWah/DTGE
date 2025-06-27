namespace DtgeGame;

/**
 * GameSettings contains the settings for running the game, modified by the player. This is likely
 * to be overhauled as part of DTGE-135
 */
public class GameSettings
{
	public int SceneTextSize { get; set; }

	const int SCENE_TEXT_SIZE_DEFAULT = 24;

	public GameSettings()
	{
		this.SceneTextSize = SCENE_TEXT_SIZE_DEFAULT;
	}

	public GameSettings(GameSettings other)
	{
		this.SceneTextSize = other.SceneTextSize;
	}
}
