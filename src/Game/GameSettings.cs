using Godot;
using System.Text.Json;

namespace DtgeGame;

public class GameSettings
{
	public int SceneTextSize { get; set; }

	const int SCENE_TEXT_SIZE_DEFAULT = 24;

	public GameSettings()
	{
		SceneTextSize = SCENE_TEXT_SIZE_DEFAULT;
	}

	public GameSettings(GameSettings other)
	{
		this.SceneTextSize = other.SceneTextSize;
	}
}
