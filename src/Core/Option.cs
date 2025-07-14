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

	public Option(Scene parentScene, OptionSerializable serializable)
		:base(parentScene, serializable)
	{
		if (serializable == null)
		{
			CoreErrorHandler.InvokeInitializationError("An Option was initialized with a null serializable.");
		}
		else
		{
			this.Name = serializable.Name;
			this.TargetSceneId = serializable.TargetSceneId;
			this.DisplayName = serializable.DisplayName;
			this.Tooltip = serializable.Tooltip;
			this.Enabled = serializable.Enabled;
		}
	}
}
