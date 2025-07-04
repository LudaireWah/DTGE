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

	public GetSceneSuccessValue GetSceneById(string id, out Scene outScene)
	{
		GetSceneSuccessValue successValue = GetSceneSuccessValue.Success;
		bool foundScene = this.scenes.TryGetValue(id, out outScene);

		if (!foundScene)
		{
			successValue = GetSceneSuccessValue.SceneNotFound;
		}

		return successValue;
	}

	public GetSceneSuccessValue GetSceneAndSetSubsceneById(SceneId id, out Scene outScene)
	{
		GetSceneSuccessValue successValue = this.GetSceneById(id.sceneName, out outScene);
		
		if (successValue == GetSceneSuccessValue.Success)
		{
			
			bool subsceneSetSuccessfully = outScene.SetCurrentSubscene(id.subsceneName);
			if (!subsceneSetSuccessfully)
			{
				successValue = GetSceneSuccessValue.SubsceneNotFound;
			}
		}
		return successValue;
	}

	public void ClearScenes()
	{
		this.scenes.Clear();
	}
}
