using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;

/**
 * SubsceneSerializable is used for easy and consistent serialization and deserialization of
 * Subscenes, including providing backwards compatibility. This class follows the Serializable
 * pattern, which is explained in detail in SceneSerializable.cs.
 */
public class SubsceneSerializable : SceneElementSerializable
{
	[JsonIgnore]
	public string Name
	{
		get { return this.subsceneVersion0Data.Name; }
		set { this.subsceneVersion0Data.Name = value; }
	}

	[JsonIgnore]
	public bool IsReadOnly
	{
		get { return this.subsceneVersion0Data.IsReadOnly; }
		set { this.subsceneVersion0Data.IsReadOnly = value; }
	}

	private class SubsceneVersion0
	{
		public string Name { get; set; }
		public bool IsReadOnly { get; set; }
	}

	[JsonInclude]
	[JsonPropertyName(DtgeScenePropertyNames.SubsceneV0)]
	private SubsceneVersion0 subsceneVersion0Data { get; set; }

	public SubsceneSerializable()
		:base()
	{
		this.subsceneVersion0Data = new SubsceneVersion0();
	}
}
