using System;
using System.Collections.Generic;

namespace DtgeCore;
/**
 * The SceneManager is a singleton class responsible for having all Scenes
 * within the game based on their scene id.
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
		this.scenes[newScene.Id] = newScene;
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

	public GetSceneSuccessValue GetSceneAndSubsceneById(Scene.SceneId id, out Scene outScene)
	{
		GetSceneSuccessValue successValue = this.GetSceneById(id.scene, out outScene);
		
		if (successValue == GetSceneSuccessValue.Success)
		{
			bool subsceneSetSuccessfully = outScene.SetCurrentSubscene(id.subscene);
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
