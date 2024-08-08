using System.Collections.Generic;

namespace DtgeCore;
/**
 * The SceneManager is a singleton class responsible for having all Scenes
 * within the game based on their scene id.
 */
public class SceneManager
{
    private static SceneManager instance;
    private readonly Dictionary<string, Scene> scenes;

    private SceneManager()
    {
        this.scenes= new Dictionary<string, Scene>();
    }

    public static SceneManager GetSceneManager()
    {
        if (SceneManager.instance == null)
        {
            SceneManager.instance= new SceneManager();
        }

        return SceneManager.instance;
    }

    public void AddScene(Scene newScene)
    {
        this.scenes[newScene.Id] = newScene;
    }

    public Scene GetSceneById(string id)
    {
        Scene scene = null;
        
        bool success = this.scenes.TryGetValue(id, out scene);

        if (!success)
        {
            scene = null;
        }

        return scene;
    }

    public void ClearScenes()
    {
        this.scenes.Clear();
    }
}
