using System.Text.Json.Serialization;


namespace DtgeCore.Serialization;

/**
 * SnippetSerializable provides the base class for Serializables for snippets, holding the mode
 * and allowing for polymorphic serialization/deserialization. In addition to the usual
 * Serializable pattern outlined in SceneSerializable, Snippets should inherit from this class and
 * register themselves with a unique typeDiscriminator. Note that this typeDiscriminator CANNOT
 * be changed after a release.
 */
[JsonDerivedType(typeof(SnippetSimpleSerializable), typeDiscriminator: "simple")]
[JsonDerivedType(typeof(SnippetSubsceneSerializable), typeDiscriminator: "subscene")]
public abstract class SnippetSerializable : SceneElementSerializable
{
	[JsonIgnore]
	public Snippet.Mode Mode
	{
		get { return (Snippet.Mode)this.snippetVersion0Data.Mode; }
		set { this.snippetVersion0Data.Mode = (int)value; }
	}

	private class SnippetVersion0
	{
		public int Mode { get; set; }
	}

	[JsonInclude]
	[JsonPropertyName(DtgeScenePropertyNames.SnippetV0)]
	private SnippetVersion0 snippetVersion0Data { get; set; }
	
	public SnippetSerializable()
		:base()
	{
		this.snippetVersion0Data = new SnippetVersion0();
	}
}
