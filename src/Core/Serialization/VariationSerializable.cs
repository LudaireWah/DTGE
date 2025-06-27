using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;

public class VariationSerializable : SceneElementSerializable
{
	[JsonIgnore]
	public string Name
	{
		get { return this.variationVersion0Data.Name; }
		set { this.variationVersion0Data.Name = value; }
	}
	[JsonIgnore]
	public string Text
	{
		get { return this.variationVersion0Data.Text; }
		set { this.variationVersion0Data.Text = value; }
	}

	private class VariationVersion0
	{
		public string Name { get; set; }
		public string Text { get; set; }
	}

	[JsonInclude]
	private VariationVersion0 variationVersion0Data { set; get; }

	public VariationSerializable()
		:base()
	{
		this.variationVersion0Data = new VariationVersion0();
	}
}
