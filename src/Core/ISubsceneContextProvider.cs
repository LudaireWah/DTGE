using System;

namespace DtgeCore;
public interface ISubsceneContextProvider
{
    public int GetSubsceneCount();
    public Scene.SubsceneId GetSubsceneId(int subsceneIndex);
    public Scene.SubsceneId GetCurrentSubsceneId();
    public void RegisterOnSubsceneAdded(Action<Scene.SubsceneId> onSubsceneAdded);
    public void UnregisterOnSubsceneAdded(Action<Scene.SubsceneId> onSubsceneAdded);
    public void RegisterOnSubsceneRemoved(Action<Scene.SubsceneId> onSubsceneRemoved);
    public void UnregisterOnSubsceneRemoved(Action<Scene.SubsceneId> onSubsceneRemoved);
    public void RegisterOnSubsceneRenamed(Action<Scene.SubsceneId, string> onSubsceneRenamed);
    public void UnregisterOnSubsceneRenamed(Action<Scene.SubsceneId, string> onSubsceneRenamed);
}
