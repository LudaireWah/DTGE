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
	private readonly Dictionary<string, SceneReadOnly> scenes;

	private SceneManager()
	{
		this.scenes= new Dictionary<string, SceneReadOnly>();
	}

	public static SceneManager GetSceneManager()
	{
		if (SceneManager.instance == null)
		{
			SceneManager.instance= new SceneManager();
		}

		return SceneManager.instance;
	}

	public void AddScene(SceneReadOnly newScene)
	{
		this.scenes[newScene.Id] = newScene;
	}

	public GetSceneSuccessValue GetSceneById(string id, out SceneReadOnly outScene)
	{
		GetSceneSuccessValue successValue = GetSceneSuccessValue.Success;
		bool foundScene = this.scenes.TryGetValue(id, out outScene);

		if (!foundScene)
		{
			successValue = GetSceneSuccessValue.SceneNotFound;
		}

		return successValue;
	}

	public GetSceneSuccessValue GetSceneAndSubsceneById(SceneId id, out SceneReadOnly outScene)
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
