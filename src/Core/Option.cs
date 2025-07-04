using DtgeCore.Serialization;

namespace DtgeCore;

/**
 * Options are how players move from scene to scene, providing the main form of interactivity for
 * the engine.
 */
public class Option : SceneElement
{
	public string Name { get; protected set; }
	public string TargetSceneId { get; protected set; }
	public string DisplayName { get; protected set; }
	public string Tooltip { get; protected set; }
	public bool Enabled { get; protected set; }

	public Option(Scene parentScene)
		: base(parentScene)
	{
		this.Name = "";
		this.TargetSceneId = "";
		this.DisplayName = "";
		this.Tooltip = null;
		this.Enabled = true;
	}

	public Option(Scene parentScene, OptionSerializable optionSerializable)
		:base(parentScene, optionSerializable)
	{
		this.Name = optionSerializable.Name;
		this.TargetSceneId= optionSerializable.TargetSceneId;
		this.DisplayName = optionSerializable.DisplayName;
		this.Tooltip = optionSerializable.Tooltip;
		this.Enabled= optionSerializable.Enabled;
	}

	public void CopyFrom(Option other)
	{
		base.CopyFrom(other);

		this.Name = other.Name;
		this.TargetSceneId = other.TargetSceneId;
		this.DisplayName = other.DisplayName;
		this.Tooltip = other.Tooltip;
		this.Enabled = other.Enabled;
	}
}
