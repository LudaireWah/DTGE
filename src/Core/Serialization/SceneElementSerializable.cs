using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;

/**
 * SceneElementSerializable is used for easy and consistent serialization and deserialization of
 * SceneElements, including providing backwards compatibility. This class follows the Serializable
 * pattern, which is explained in detail in SceneSerializable.cs.
 */
public class SceneElementSerializable
{
	[JsonIgnore]
	public SUID Id
	{
		get { return this.sceneElementVersion0Data.Id; }
		set { this.sceneElementVersion0Data.Id = value; }
	}

	public class SceneElementVersion0
	{
		[JsonConverter(typeof(SUIDConverter))]
		public SUID Id { get; set; }
	}

	[JsonInclude]
	private SceneElementVersion0 sceneElementVersion0Data { get; set; }

	public SceneElementSerializable()
	{
		this.sceneElementVersion0Data = new SceneElementVersion0();
	}
}
