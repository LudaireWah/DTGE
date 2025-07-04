using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;

/**
 * SnippetSimpleSerializable is used for easy and consistent serialization and deserialization of
 * Simple Snippets, including providing backwards compatibility. This class follows the
 * Serializable pattern, which is explained in detail in SceneSerializable.cs.
 */
public class SnippetSimpleSerializable : SnippetSerializable
{
	[JsonIgnore]
	public VariationSerializable SingleVariation
	{
		get { return this.snippetSimpleVersion0Data.SingleVariation; }
		set { this.snippetSimpleVersion0Data.SingleVariation = value; }
	}

	private class SnippetSimpleVersion0
	{
		public VariationSerializable SingleVariation { get; set; }

		public SnippetSimpleVersion0()
		{
			this.SingleVariation = new VariationSerializable();
		}
	}

	[JsonInclude]
	[JsonPropertyName(DtgeScenePropertyNames.SnippetSimpleV0)]
	private SnippetSimpleVersion0 snippetSimpleVersion0Data { get; set; }

	public SnippetSimpleSerializable()
		: base()
	{
		this.snippetSimpleVersion0Data = new SnippetSimpleVersion0();
		this.Mode = Snippet.Mode.Simple;
	}
}
