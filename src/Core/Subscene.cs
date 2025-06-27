using DtgeCore.Serialization;

namespace DtgeCore;

/**
 * Subscenes represent variations on a specific scene which are invoked by the Option targeting
 * the scene. The current subscene is one of the few properties on a scene that's not read-only
 * when running the game, as each time the Scene is provided to the shell by the SceneManager,
 * the current Subscene can change.
 * 
 * Subscenes don't do much work themselves, but are used extensively by Snippets.
 */
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
