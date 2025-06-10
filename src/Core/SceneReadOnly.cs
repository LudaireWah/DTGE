using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtgeCore;

/**
 * A wrapper for a DTGE Scene that exposes the read only operations to be used when running the game
 */
public class SceneReadOnly
{
	private SceneData sceneData;

	public string Id { get { return sceneData.Id; } }
	public bool RenderImage { get { return this.sceneData.RenderImage; } }
	public SceneImagePosition ImagePosition { get { return this.sceneData.ImagePosition; } }
	public string ImagePath { get { return this.sceneData.ImagePath; } }

	public SceneReadOnly(string sceneJson)
	{
		this.sceneData = SceneData.Deserialize(sceneJson);
	}

	public string CalculateSceneText()
	{
		return this.sceneData.CalculateSceneText();
	}

	public bool SetCurrentSubsceneByIndex(int index)
	{
		return this.sceneData.SetCurrentSubsceneByIndex(index);
	}

	public bool SetCurrentSubscene(string subsceneName)
	{
		return this.sceneData.SetCurrentSubscene(subsceneName);
	}

	public bool SetCurrentSubscene(Subscene subsceneId)
	{
		return this.sceneData.SetCurrentSubscene(subsceneId);
	}

	public int GetOptionCount()
	{
		return this.sceneData.GetOptionCount();
	}

	public Option GetOption(int index)
	{
		return this.sceneData.GetOption(index);
	}
}
