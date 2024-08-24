namespace DtgeCore;
public interface ISceneContextProvider
{
    public int GetSubsceneCount();
    public string GetSubsceneName(int subsceneIndex);
    public int GetCurrentSubsceneIndex();
}
