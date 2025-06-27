using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;

public struct SceneSerializable
{
	[JsonIgnore]
	public string Id
	{
		get { return this.sceneVersion0Data.Id; }
		set { this.sceneVersion0Data.Id = value; }
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
	public SceneImagePosition ImagePosition
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
		public string Id { get; set; }
		public bool NullSubsceneEnabled { get; set; }
		public bool RenderImage { get; set; }
		public SceneImagePosition ImagePosition { get; set; }
		public string ImagePath { get; set; }
		public List<OptionSerializable> OptionList { get; set; }
		public List<SubsceneSerializable> SubsceneList { get; set; }
		public List<SnippetSerializable> SnippetList { get; set; }
		public int NextSUID { get; set; }

		public SceneVersion0()
		{
			this.OptionList = new List<OptionSerializable>();
			this.SubsceneList = new List<SubsceneSerializable>();
			this.SnippetList = new List<SnippetSerializable>();
		}
	}

	[JsonInclude]
	private SceneVersion0 sceneVersion0Data { get; set; }

	public SceneSerializable()
	{
		this.sceneVersion0Data = new SceneVersion0();
	}
	public string SerializeToString()
	{
		string stringSerialization = JsonSerializer.Serialize(this);
		return stringSerialization;
	}

	public static SceneSerializable Deserialize(string sceneJson)
	{
		SceneSerializable sceneSerializable = JsonSerializer.Deserialize<SceneSerializable>(sceneJson);
		return sceneSerializable;
	}
}
