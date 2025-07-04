using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;


/**
 * SnippetRandomSerializable is used for easy and consistent serialization and deserialization of
 * Random Snippets, including providing backwards compatibility. This class follows the
 * Serializable pattern, which is explained in detail in SceneSerializable.cs.
 */
public class SnippetRandomSerializable : SnippetSerializable
{
	[JsonIgnore]
	public List<VariationSerializable> Variations
	{
		get { return this.snippetRandomVersion0Data.Variations; }
		set { this.snippetRandomVersion0Data.Variations = value; }
	}

	private class SnippetRandomVersion0
	{
		public List<VariationSerializable> Variations { get; set; }

		public SnippetRandomVersion0()
		{
			this.Variations = new List<VariationSerializable>();
		}
	}

	[JsonInclude]
	[JsonPropertyName(DtgeScenePropertyNames.SnippetRandomV0)]
	private SnippetRandomVersion0 snippetRandomVersion0Data { get; set; }

	public SnippetRandomSerializable()
		:base()
	{
		this.snippetRandomVersion0Data = new SnippetRandomVersion0();
		this.Mode = Snippet.Mode.Random;
	}
}
