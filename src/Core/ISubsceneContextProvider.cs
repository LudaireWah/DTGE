using System;

namespace DtgeCore;
public interface ISubsceneContextProvider
{
	public int GetSubsceneCount();
	public Subscene GetSubscene(int subsceneIndex);
	public Subscene GetCurrentSubscene();
	public void RegisterOnSubsceneAdded(Action<Subscene> onSubsceneAdded);
	public void UnregisterOnSubsceneAdded(Action<Subscene> onSubsceneAdded);
	public void RegisterOnSubsceneRemoved(Action<Subscene> onSubsceneRemoved);
	public void UnregisterOnSubsceneRemoved(Action<Subscene> onSubsceneRemoved);
	public void RegisterOnSubsceneRenamed(Action<Subscene, string> onSubsceneRenamed);
	public void UnregisterOnSubsceneRenamed(Action<Subscene, string> onSubsceneRenamed);
}
