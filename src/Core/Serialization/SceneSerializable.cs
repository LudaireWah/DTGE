using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;

/**
 * SceneSerializable and the rest of the classes ending in "Serializable" are used for easy and
 * consistent serialization and deserialization of DTGE classes, including providing backwards
 * compatibility. This class and all of the other Serializables follow a consistent pattern to
 * make it as easy as possible to use and maintain them over time.
 * 
 * There are several components to the Serializable pattern:
 *   - A set of Properties ignored by Json used for easy access by the rest of DtgeCore, keeping
 *     the complexity of serialization and backwards compatibility encapsulated within this class.
 *     These should always point to the most recent version of the data.
 *   - Classes for both the current and past versions of the Serializable data. Each one contains
 *     all the data needed to serialize and deserialize the Serializable. The most current
 *     released version can have things added to it, but you should never remove or rename
 *     anything from a released version. Generally, anything older than the most recently released
 *     version shouldn't be touched. In development versions are, of course, freely changeable.
 *   - Instances of each version of the data. Upon Deserialization, the Serializable should create
 *     the new version of the data from whatever old version exists. From then on, only the new
 *     version should be used, though the old version should be left intact in case bugs are
 *     discovered in the update code which requires access to the old, unmodified data. It's
 *     possible that if DTGE projects/Scene files start to get too large, we could null out
 *     sufficiently old data, though the class and instance should remain.
 *   - For any Serializable meant to be serialized or deserialized on its own, functions for
 *     serializing and deserializing will be provided. As an example, Scenes are often serialized
 *     on their own, but Options should only ever be serialized when part of a Scene.
 */
public class SceneSerializable
{
	[JsonIgnore]
	public string Name
	{
		get { return this.sceneVersion0Data.Name; }
		set { this.sceneVersion0Data.Name = value; }
	}
	[JsonIgnore]
	public bool NullSubsceneEnabled
	{
		get { return this.sceneVersion0Data.NullSubsceneEnabled; }
		set { this.sceneVersion0Data.NullSubsceneEnabled = value; }
	}
	[JsonIgnore]
	public bool RenderImage
	{
		get { return this.sceneVersion0Data.RenderImage; }
		set { this.sceneVersion0Data.RenderImage = value; }
	}
	[JsonIgnore]
	public Scene.SceneImagePosition ImagePosition
	{
		get { return this.sceneVersion0Data.ImagePosition; }
		set { this.sceneVersion0Data.ImagePosition = value; }
	}
	[JsonIgnore]
	public string ImagePath
	{
		get { return this.sceneVersion0Data.ImagePath; }
		set { this.sceneVersion0Data.ImagePath = value; }
	}
	[JsonIgnore]
	public List<OptionSerializable> OptionList
	{
		get { return this.sceneVersion0Data.OptionList; }
		set { this.sceneVersion0Data.OptionList = value; }
	}
	[JsonIgnore]
	public List<SubsceneSerializable> SubsceneList
	{
		get { return this.sceneVersion0Data.SubsceneList; }
		set { this.sceneVersion0Data.SubsceneList = value; }
	}
	[JsonIgnore]
	public List<SnippetSerializable> SnippetList
	{
		get { return this.sceneVersion0Data.SnippetList; }
		set { this.sceneVersion0Data.SnippetList = value; }
	}
	[JsonIgnore]
	public int NextSUID
	{
		get { return this.sceneVersion0Data.NextSUID; }
		set { this.sceneVersion0Data.NextSUID = value; }
	}

	private class SceneVersion0
	{
		public string Name { get; set; }
		public bool NullSubsceneEnabled { get; set; }
		public bool RenderImage { get; set; }
		public Scene.SceneImagePosition ImagePosition { get; set; }
		public string ImagePath { get; set; }
		public List<OptionSerializable> OptionList { get; set; } = new List<OptionSerializable>();
		public List<SubsceneSerializable> SubsceneList { get; set; } = new List<SubsceneSerializable>();
		public List<SnippetSerializable> SnippetList { get; set; } = new List<SnippetSerializable>();
		public int NextSUID { get; set; }
	}

	[JsonInclude]
	[JsonPropertyName(DtgeScenePropertyNames.SceneV0)]
	private SceneVersion0 sceneVersion0Data { get; set; }

	public SceneSerializable()
	{
		this.sceneVersion0Data = new SceneVersion0();
	}

	public string SerializeToString()
	{
		JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
		jsonSerializerOptions.Converters.Add(new SUIDConverter());
		string jsonString = "";
		try
		{
			jsonString = JsonSerializer.Serialize(this);
		}
		catch (Exception exception)
		{
			CoreErrorHandler.InvokeInitializationError("An exception was hit during serialization. Exception message: " + exception.Message);
		}

		return jsonString;
	}

	public static SceneSerializable DeserializeFromString(string sceneJson)
	{
		SceneSerializable sceneSerializable = null;

		if (sceneJson == null)
		{
			CoreErrorHandler.InvokeInitializationError("A Scene Serializable was initialized with a null json string.");
		}
		else
		{
			try
			{
				sceneSerializable = JsonSerializer.Deserialize<SceneSerializable>(sceneJson);
			}
			catch (JsonException jsonException)
			{
				CoreErrorHandler.InvokeInitializationError("A Scene Serializable failed to be deserialized. Exception: " + jsonException.Message);
			}

			if (sceneSerializable == null)
			{
				CoreErrorHandler.InvokeInitializationError("A Scene Serializable failed to be deserialized.");
			}
		}

		return sceneSerializable;
	}
}
