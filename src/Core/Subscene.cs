namespace DtgeCore;

public class Subscene : SceneElement
{
	public string Name { get; protected set; }

	public Subscene(Scene parentScene, string name)
		: base(parentScene)
	{
		this.Name = name;
	}
}
