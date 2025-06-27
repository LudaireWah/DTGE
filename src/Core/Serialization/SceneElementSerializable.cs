using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;

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
