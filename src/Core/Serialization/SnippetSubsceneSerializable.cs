using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;

public class SnippetSubsceneSerializable : SnippetSerializable
{
	[JsonIgnore]
	public Dictionary<SUID, VariationSerializable> VariationsBySubsceneId
	{
		get { return this.snippetSubsceneVersion0Data.VariationsBySubsceneId; }
		set { this.snippetSubsceneVersion0Data.VariationsBySubsceneId = value; }
	}

	private class SnippetSubsceneVersion0
	{
		public Dictionary<SUID, VariationSerializable> VariationsBySubsceneId { get; set; } = new Dictionary<SUID, VariationSerializable>();
	}

	[JsonInclude]
	[JsonPropertyName(DtgeScenePropertyNames.SnippetSubsceneV0)]
	private SnippetSubsceneVersion0 snippetSubsceneVersion0Data { get; set; }

	public SnippetSubsceneSerializable()
		: base()
	{
		this.snippetSubsceneVersion0Data = new SnippetSubsceneVersion0();
		this.Mode = Snippet.Mode.Subscene;
	}
}
