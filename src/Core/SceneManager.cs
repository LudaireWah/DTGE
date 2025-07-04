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

	public SceneId(string sceneIdString)
	{
		string[] ids = sceneIdString.Split(".");
		this.sceneName = ids[0];
		if (ids.Length > 1)
		{
			this.subsceneName = ids[1];
		}
		else
		{
			this.subsceneName = null;
		}
		if (ids.Length > 2)
		{
			// error
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
	private readonly Dictionary<string, Scene> scenes;

	private SceneManager()
	{
		this.scenes= new Dictionary<string, Scene>();
	}

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

	public bool TryGetNextSceneFromOption(Option option, out Scene targetScene, out string message)
	{
		SceneId targetSceneId = new SceneId(option.TargetSceneId);
		Scene obtainedScene = null;
		bool foundScene = this.scenes.TryGetValue(targetSceneId.sceneName, out obtainedScene);
		bool subsceneSetSuccessfully = false;

		if (!foundScene)
		{
			targetScene = null;
			message = "Option [" + option.Name + "] attempted to open Scene [" + targetSceneId.sceneName + "], which was not found.";
		}
		else
		{
			subsceneSetSuccessfully = obtainedScene.SetCurrentSubscene(targetSceneId.subsceneName);
			if (subsceneSetSuccessfully)
			{
				targetScene = obtainedScene;
				message = null;
			}
			else
			{
				if (targetSceneId.subsceneName == null)
				{
					targetScene = null;
					message = "Option [" + option.Name + "] attempted to open Scene [" + targetSceneId.sceneName + "] without a subscene, which is not supported by that Scene.";
				}
				else
				{
					targetScene = null;
					message = "Option [" + option.Name + "] attempted to open Subscene [" + targetSceneId.subsceneName + "], which was not found in Scene [" + targetSceneId.sceneName + "].";
				}
			}
		}

		return foundScene && subsceneSetSuccessfully;
	}

	public void ClearScenes()
	{
		this.scenes.Clear();
	}
}
