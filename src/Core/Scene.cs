using System.Collections.Generic;
using System.Text.Json;

using DtgeCore.Serialization;

namespace DtgeCore;

/**
 * DTGE's Scene is the fundamental unit of the engine. Nearly everything displayed to the player
 * is displayed via a scene. Thus, navigation through the game is navigating through a set of
 * scenes, each presenting the player with a description of the player's situation and a set of
 * options the player may choose from to advance in the game.
 *
 * Most of the Scene's heavy lifting is done by the various SceneElements. The scene itself is
 * responsible for managing these elementsm as well as some meta level things such as an optional
 * image that can be displayed alongside the text.
 * 
 * A Scene is mostly read only while running the game, and the class and its elements reflect
 * that. Any changes of state that happen in the game should be represented by moving between
 * scenes or, in the future, the Entity system (DTGE-12). There are a few exceptions such as
 * subscenes and randomization, which are mainly used by Snippets. Other than this,Scenes should
 * only be modified while editing the game, which uses the SceneEditable.
 */
public class Scene
{
	public enum SceneImagePosition
	{
		Left,
		Right,
		Top,
		Bottom,
		OnlyImage
	}

	public struct SceneId
	{
		public string scene;
		public string subscene;

		public SceneId(string sceneIdString)
		{
			string[] ids = sceneIdString.Split(".");
			this.scene = ids[0];
			if (ids.Length > 1)
			{
				this.subscene = ids[1];
			}
			else
			{
				this.subscene = null;
			}
			if (ids.Length > 2)
			{
				// error
			}
		}

		public SceneId(string sceneName, string subsceneName)
		{
			this.scene = sceneName;
			this.subscene = subsceneName;
		}
	}

	public string Id { get; protected set; }
	public bool NullSubsceneEnabled { get; protected set; }
	public int CurrentSubsceneIndex { get; set; }
	public Subscene CurrentSubscene
	{
		get
		{
			Subscene currentSubscene = null; 
			if (this.SubsceneList.Count > 0)
			{
				currentSubscene = this.SubsceneList[this.CurrentSubsceneIndex];
			}
			return currentSubscene;
		}
	}
	public bool RenderImage { get; protected set; }
	public SceneImagePosition ImagePosition {  get; protected set; }
	public string ImagePath {  get; protected set; }

	protected List<Option> OptionList { get; private set; }
	protected List<Subscene> SubsceneList { get; private set; }
	protected List<Snippet> SnippetList { get; private set; }

	protected int nextSUID = SUID.FIRST_VALID_SUID;

	public Scene()
	{
		this.Id = "";
		this.NullSubsceneEnabled = false;
		this.CurrentSubsceneIndex = 0;
		this.RenderImage = false;
		this.ImagePath = null;
		this.OptionList = new List<Option>();
		this.SubsceneList = new List<Subscene>();
		this.SnippetList = new List<Snippet>();
	}

	protected Scene(SceneSerializable serializable)
	{
		this.Id = serializable.Id;
		this.NullSubsceneEnabled = serializable.NullSubsceneEnabled;
		this.CurrentSubsceneIndex = 0;
		this.RenderImage= serializable.RenderImage;
		this.ImagePath= serializable.ImagePath;

		this.OptionList = new List<Option>();
		for (int optionIndex = 0; optionIndex < serializable.OptionList.Count; optionIndex++)
		{
			this.OptionList.Add(new Option(this, serializable.OptionList[optionIndex]));
		}

		this.SubsceneList = new List<Subscene>();
		for (int subsceneIndex = 0; subsceneIndex < serializable.SubsceneList.Count; subsceneIndex++)
		{
			this.SubsceneList.Add(new Subscene(this, serializable.SubsceneList[subsceneIndex]));
		}

		this.SnippetList = new List<Snippet>();
		for (int snippetIndex = 0; snippetIndex < serializable.SnippetList.Count; snippetIndex++)
		{
			this.SnippetList.Add(new Snippet(this, serializable.SnippetList[snippetIndex]));
		}
	}

	public static Scene DeserializeFromJsonString(string jsonString)
	{
		SceneSerializable sceneSerializable =
			JsonSerializer.Deserialize<SceneSerializable>(jsonString);

		return new Scene(sceneSerializable);
	}

	public SUID GetSUID()
	{
		return new SUID(this.nextSUID++);
	}

	public string CalculateSceneText()
	{
		string sceneText = "";

		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; ++snippetIndex)
		{
			Snippet currentSnippet = this.SnippetList[snippetIndex];
			sceneText += currentSnippet.CalculateText();
		}

		return sceneText;
	}

	public int GetOptionCount()
	{
		return this.OptionList.Count;
	}

	public Option GetOption(int index)
	{
		return this.OptionList[index];
	}

	public bool SetCurrentSubsceneByIndex(int index)
	{
		bool success = false;
		if (index < this.SubsceneList.Count)
		{
			this.CurrentSubsceneIndex = index;
			success = true;
		}
		return success;
	}

	public bool SetCurrentSubscene(string subsceneName)
	{
		bool success = false;
		if (subsceneName == null)
		{
			this.CurrentSubsceneIndex = 0;
			success = true;
		}
		else
		{
			int desiredSubsceneIndex = -1;

			for (int subsceneIndex = 0; subsceneIndex < this.SubsceneList.Count; subsceneIndex++)
			{
				if (SubsceneList[subsceneIndex].Name == subsceneName)
				{
					desiredSubsceneIndex = subsceneIndex;
					break;
				}
			}

			if (desiredSubsceneIndex != -1)
			{
				this.CurrentSubsceneIndex = desiredSubsceneIndex;
				success = true;
			}
		}

		return success;
	}

	public bool SetCurrentSubscene(Subscene subscene)
	{
		bool success = false;
		int desiredSubsceneIndex = -1;

		for (int subsceneIndex = 0; subsceneIndex < this.SubsceneList.Count; subsceneIndex++)
		{
			if (SubsceneList[subsceneIndex].Id == subscene.Id)
			{
				desiredSubsceneIndex = subsceneIndex;
				break;
			}
		}
		
		if (desiredSubsceneIndex != -1)
		{
			this.CurrentSubsceneIndex = desiredSubsceneIndex;
			success = true;
		}

		return success;
	}
}
