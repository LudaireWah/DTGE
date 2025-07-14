using System;
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
 * responsible for managing these elements as well as some scene level things such as an optional
 * image that can be displayed alongside the text.
 * 
 * Scenes are identified by their name and optionally a subscene name. This id is entered by
 * authors as scene_name.subscene_name. Scene names should be unique within a DTGE game while
 * subscene names should be unique within the subscene they're a part of. Within a scene,
 * different elements are identified by a special identifier called a SUID. You can find more
 * details on SUIDs and their use in SceneElement.cs and Suid.cs.
 * 
 * A Scene is mostly read only while running the game. The class and its elements reflect that.
 * Any changes of state that happen in the game should be represented by moving between scenes or
 * the Entity system (still upcoming DTGE-12). There are a few exceptions such as subscenes and
 * randomization, which are mainly used by Snippets. Other than this, Scenes should only be
 * modified while editing the game, which uses the SceneEditable.
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

	public string Name { get; protected set; } = "";
	public bool NullSubsceneEnabled { get; protected set; } = false;
	public int CurrentSubsceneIndex { get; set; } = 0;
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
	public bool RenderImage { get; protected set; } = false;
	public SceneImagePosition ImagePosition {  get; protected set; } = SceneImagePosition.Top;
	public string ImagePath { get; protected set; } = null;

	public Random SceneRandom { get; private set; }

	protected List<Option> OptionList { get; private set; } = new List<Option>();
	protected List<Subscene> SubsceneList { get; private set; } = new List<Subscene>();
	protected List<Snippet> SnippetList { get; private set; } = new List<Snippet>();

	protected int nextSUID = SUID.FIRST_VALID_SUID;
	private readonly int sceneRandomSeed;

	public Scene()
	{
		Random seedGenerator = new Random();
		this.sceneRandomSeed = seedGenerator.Next();
		this.SceneRandom = new Random(this.sceneRandomSeed);
	}

	protected Scene(SceneSerializable serializable)
	{
		if (serializable == null)
		{
			CoreErrorHandler.InvokeInitializationError("A Scene was initialized with a null serializable.");
		}
		else
		{
			this.Name = serializable.Name;
			this.NullSubsceneEnabled = serializable.NullSubsceneEnabled;
			this.CurrentSubsceneIndex = 0;
			this.RenderImage = serializable.RenderImage;
			this.ImagePath = serializable.ImagePath;

			for (int optionIndex = 0; optionIndex < serializable.OptionList.Count; optionIndex++)
			{
				this.OptionList.Add(new Option(this, serializable.OptionList[optionIndex]));
			}
			for (int subsceneIndex = 0;
				subsceneIndex < serializable.SubsceneList.Count;
				subsceneIndex++)
			{
				this.SubsceneList.Add(
					new Subscene(this, serializable.SubsceneList[subsceneIndex]));
			}
			for (int snippetIndex = 0;
				snippetIndex < serializable.SnippetList.Count;
				snippetIndex++)
			{
				this.SnippetList.Add(new Snippet(this, serializable.SnippetList[snippetIndex]));
			}

			this.nextSUID = serializable.NextSUID;
		}

		Random seedGenerator = new Random();
		this.sceneRandomSeed = seedGenerator.Next();
		this.SceneRandom = new Random(this.sceneRandomSeed);
	}

	public static Scene DeserializeFromJsonString(string jsonString)
	{
		Scene scene = null;

		if (jsonString == null)
		{
			CoreErrorHandler.InvokeInitializationError("A scene was deserialized from a null string.");
		}
		else
		{
			SceneSerializable sceneSerializable = 
				SceneSerializable.DeserializeFromString(jsonString);
			scene = new Scene(sceneSerializable);
		}

		return scene;
	}

	public SUID GetSUID()
	{
		return new SUID(this.nextSUID++);
	}

	public string CalculateSceneText()
	{
		string sceneText = "";
		bool errorEncountered = false;

		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; ++snippetIndex)
		{
			Snippet currentSnippet = this.SnippetList[snippetIndex];
			if (currentSnippet == null)
			{
				CoreErrorHandler.InvokePlayError("Scene [" + this.Name + "] encountered a null snippet at index " + snippetIndex + " while calculating scene text.");
			}
			else
			{
				try
				{
					sceneText += currentSnippet.CalculateText();
				}
				catch (Exception exception)
				{
					errorEncountered = true;
					CoreErrorHandler.InvokePlayError("Scene [" + this.Name + "] encountered an unknown error while calculating scene text for snippet index " + snippetIndex + ". Exception: " + exception.Message);
				}
			}
		}

		return errorEncountered ? "" : sceneText;
	}

	public int GetOptionCount()
	{
		return this.OptionList.Count;
	}

	public Option GetOption(int index)
	{
		Option option = null;

		if (CoreErrorHandler.IsValidIndex(index, this.OptionList.Count))
		{
			option = this.OptionList[index];
		}
		else
		{
			CoreErrorHandler.InvokePlayError("Scene [" + this.Name + "]'s GetOption function was called with an invalid index of " + index + ". The scene has " + this.OptionList.Count + " Options.");
		}

		return option;
	}

	public bool TrySetCurrentSubscene(string subsceneName)
	{
		bool success = false;
		if (subsceneName == null)
		{
			if (this.NullSubsceneEnabled || this.SubsceneList.Count == 0)
			{
				success = true;
				this.CurrentSubsceneIndex = 0;
			}
		}
		else
		{
			int desiredSubsceneIndex = -1;

			for (int subsceneIndex = 0; subsceneIndex < this.SubsceneList.Count; subsceneIndex++)
			{
				if (this.SubsceneList[subsceneIndex].Name == subsceneName)
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
}
