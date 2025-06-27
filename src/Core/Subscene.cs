using DtgeCore.Serialization;

namespace DtgeCore;

public class Subscene : SceneElement
{
	public string Name { get; protected set; }

	public Subscene(Scene parentScene, string name)
		: base(parentScene)
	{
		this.Name = name;
	}

	public Subscene(Scene parentScene, SubsceneSerializable subsceneSerializable)
		:base(parentScene, subsceneSerializable)
	{
		this.Name = subsceneSerializable.Name;
	}
}
