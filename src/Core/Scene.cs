using DtgeCore.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore;

/**
 * DTGE's Scene is the fundamental unit of the engine. Nearly everything displayed to the
 * player is displayed via a scene. Thus, navigation through the game is navigating
 * through a set of scenes, each presenting the player with a description of the player's
 * situation and which options the player may choose to advance in the game.
 * 
 * A Scene is completely read only while running the game, and the class and its elements
 * reflect that. Any changes of state that happen in the game should be represented by
 * scene changes or the entity system. Scenes should only be modified while editing the
 * game, which uses the SceneEditable.
 */

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

/**
 * A SUID, short for Scene Unique Identifier, is used to  identify elements within a scene.
 * It's mainly used at edit time, allowing editing code to track and reference different
 * elements of the scene instead of more fluid alternatives like indices or author visible
 * names. SUIDs should only be obtained by the Scene's GetNewSUID() function and never
 * created manually.
 * 
 * SUID.None is used for certain "elements" that are static in nature and thus don't need
 * a proper SUID. As an example, null subscenes in many ways act like a regular subscene, but
 * it shouldn't be allocated a SUID as if it's a true element. (This might be something to
 * reconsider, but it's how the none subscene acted before the introduction of SUIDs, so
 * to minimize churn, I'm going to maintain that behavior. If I do reconsider this, I should
 * remove SUID.None entirely.)
 */
[JsonConverter(typeof(SUIDConverter))]
public class SUID
{
	private readonly int suid;
	public static SUID None = new SUID(0);
	public const int FIRST_VALID_SUID = 1;

	public SUID(int suid)
	{
		this.suid = suid;
	}

	public static bool operator ==(SUID left, SUID right)
	{
		return left.suid == right.suid;
	}

	public static bool operator !=(SUID left, SUID right)
	{
		return !(left == right);
	}

	public override bool Equals(object other)
	{
		bool isEqual = false;

		if (other.GetType() == typeof(SUID))
		{
			isEqual = this == (SUID)other;
		}

		return isEqual;
	}

	public override int GetHashCode()
	{
		return this.suid;
	}

	public int ToInt()
	{
		return this.suid;
	}
}

public class SUIDConverter : JsonConverter<SUID>
{
	public override SUID Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		return new SUID(reader.GetInt32());
	}

	public override void Write(Utf8JsonWriter writer, SUID value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.ToInt(), options);
	}

	public override SUID ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new SUID(Int32.Parse(reader.GetString()));
	}

	public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] SUID value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(value.ToInt().ToString());
	}
}

public class Scene
{
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
		SceneSerializable sceneSerializable = JsonSerializer.Deserialize<SceneSerializable>(jsonString);

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
