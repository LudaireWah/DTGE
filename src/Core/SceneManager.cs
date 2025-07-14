using System.Collections.Generic;

namespace DtgeCore;

/**
 * A SceneId is a combination of a scene name and subscene name used mainly by the SceneManager
 * and Options to navigate between scenes.
 */
public struct SceneId
{
	public string sceneName;
	public string subsceneName;

	public SceneId(string targetString)
	{
		if (targetString == null)
		{
			CoreErrorHandler.InvokePlayError("A SceneId was constructed with a null string.");
		}
		else
		{
			string[] ids = targetString.Split(".");

			if (ids.Length == 1)
			{
				this.sceneName = ids[0];
				this.subsceneName = null;
			}
			else if (ids.Length == 2)
			{
				this.sceneName = ids[0];
				this.subsceneName = ids[1];
			}
			else
			{
				CoreErrorHandler.InvokePlayError("A SceneId was constructed with a malformed string. There should be no more than one '.' in the target string.");
			}
		}
	}

	public SceneId(string sceneName, string subsceneName)
	{
		this.sceneName = sceneName;
		this.subsceneName = subsceneName;
	}
}

/**
 * The SceneManager is a singleton class responsible for holding onto all scenes within the game
 * and providing navigation between scenes.
 */
public class SceneManager
{
	public enum GetSceneSuccessValue
	{
		Success,
		SceneNotFound,
		SubsceneNotFound
	}

	private static SceneManager instance;
	private readonly Dictionary<string, Scene> scenes = new Dictionary<string, Scene>();

	public static SceneManager GetSceneManager()
	{
		if (SceneManager.instance == null)
		{
			SceneManager.instance= new SceneManager();
		}

		return SceneManager.instance;
	}

	public void AddScene(Scene newScene)
	{
		this.scenes[newScene.Name] = newScene;
	}

	public Scene GetNextSceneFromOption(Option option)
	{
		Scene targetScene = null;

		SceneId targetSceneId = new SceneId(option.TargetSceneId);
		Scene scene = null;
		bool foundScene = this.scenes.TryGetValue(targetSceneId.sceneName, out scene);
		bool subsceneSetSuccessfully = false;

		if (!foundScene)
		{
			CoreErrorHandler.InvokePlayError("Option [" + option.Name + "] attempted to open Scene [" + targetSceneId.sceneName + "], which was not found.");
		}
		else
		{
			subsceneSetSuccessfully = scene.TrySetCurrentSubscene(targetSceneId.subsceneName);
			if (!subsceneSetSuccessfully)
			{
				scene = null;

				if (targetSceneId.subsceneName == null)
				{
					CoreErrorHandler.InvokePlayError("Option [" + option.Name + "] attempted to open Scene [" + targetSceneId.sceneName + "] without a subscene, which is not supported by that Scene.");
				}
				else
				{
					CoreErrorHandler.InvokePlayError("Option [" + option.Name + "] attempted to open Subscene [" + targetSceneId.subsceneName + "], which was not found in Scene [" + targetSceneId.sceneName + "].");
				}
			}
		}

		return targetScene;
	}

	public void ClearScenes()
	{
		this.scenes.Clear();
	}
}
