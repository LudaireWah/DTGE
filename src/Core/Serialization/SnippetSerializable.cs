using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace DtgeCore.Serialization;

/**
 * SnippetSerializable is used for easy and consistent serialization and deserialization of
 * Snippets, including providing backwards compatibility. This class follows the Serializable
 * pattern, which is explained in detail in SceneSerializable.cs.
 */
public class SnippetSerializable : SceneElementSerializable
{
	[JsonIgnore]
	public Snippet.SnippetMode Mode
	{
		get { return (Snippet.SnippetMode)this.snippetVersion0Data.Mode; }
		set { this.snippetVersion0Data.Mode = (int)value; }
	}
	[JsonIgnore]
	public Dictionary<SUID, VariationSerializable> Variations
	{
		get { return this.snippetVersion0Data.Variations; }
		set { this.snippetVersion0Data.Variations = value; }
	}
	[JsonIgnore]
	public List<SUID> OrderedVariationIds
	{
		get { return this.snippetVersion0Data.OrderedVariationIds; }
		set { this.snippetVersion0Data.OrderedVariationIds = value; }
	}

	private class SnippetVersion0
	{
		public int Mode { get; set; }
		//[JsonIgnore]
		public Dictionary<SUID, VariationSerializable> Variations { get; set; }
		public List<SUID> OrderedVariationIds { get; set; }

		public SnippetVersion0()
		{
			this.Variations = new Dictionary<SUID, VariationSerializable>();
			this.OrderedVariationIds= new List<SUID>();
		}
	}

	[JsonInclude]
	private SnippetVersion0 snippetVersion0Data { get; set; }
	
	public SnippetSerializable()
		:base()
	{
		this.snippetVersion0Data = new SnippetVersion0();
	}
}
