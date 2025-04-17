using System;

namespace DtgeCore;
public interface ISubsceneContextProvider
{
	public int GetSubsceneCount();
	public SubsceneId GetSubsceneId(int subsceneIndex);
	public SubsceneId GetCurrentSubsceneId();
	public void RegisterOnSubsceneAdded(Action<SubsceneId> onSubsceneAdded);
	public void UnregisterOnSubsceneAdded(Action<SubsceneId> onSubsceneAdded);
	public void RegisterOnSubsceneRemoved(Action<SubsceneId> onSubsceneRemoved);
	public void UnregisterOnSubsceneRemoved(Action<SubsceneId> onSubsceneRemoved);
	public void RegisterOnSubsceneRenamed(Action<SubsceneId, string> onSubsceneRenamed);
	public void UnregisterOnSubsceneRenamed(Action<SubsceneId, string> onSubsceneRenamed);
}
