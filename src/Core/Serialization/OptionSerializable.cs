using System.Text.Json.Serialization;

namespace DtgeCore.Serialization;

/**
 * OptionSerializable is used for easy and consistent serialization and deserialization of
 * Options, including providing backwards compatibility. This class follows the Serializable
 * pattern, which is explained in detail in SceneSerializable.cs.
 */
public class OptionSerializable : SceneElementSerializable
{
	[JsonIgnore]
	public string Name
	{
		get { return this.optionVersion0Data.Name; }
		set { this.optionVersion0Data.Name = value; }
	}
	[JsonIgnore]
	public string TargetSceneId
	{
		get { return this.optionVersion0Data.TargetSceneId; }
		set { this.optionVersion0Data.TargetSceneId = value; }
	}
	[JsonIgnore]
	public string DisplayName
	{
		get { return this.optionVersion0Data.DisplayName; }
		set { this.optionVersion0Data.DisplayName = value; }
	}
	[JsonIgnore]
	public string Tooltip
	{
		get { return this.optionVersion0Data.Tooltip; }
		set { this.optionVersion0Data.Tooltip = value; }
	}
	[JsonIgnore]
	public bool Enabled
	{
		get { return this.optionVersion0Data.Enabled; }
		set { this.optionVersion0Data.Enabled = value; }
	}

	private class OptionVersion0
	{
		public string Name { get; set; }
		public string TargetSceneId { get; set; }
		public string DisplayName { get; set; }
		public string Tooltip { get; set; }
		public bool Enabled { get; set; }
	}

	[JsonInclude]
	private OptionVersion0 optionVersion0Data { get; set; }

	public OptionSerializable()
		:base()
	{
		this.optionVersion0Data = new OptionVersion0();
	}
}
