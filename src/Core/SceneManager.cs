using System.Collections.Generic;

namespace DtgeCore;

public class SceneManager
{
    private static SceneManager instance;
    private Dictionary<string, Scene> scenes;
    private const int max_scene_number= 5;

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

    public void addScene(Scene newScene)
    {
        this.scenes[newScene.Id] = newScene;
    }

    public Scene getSceneById(string id)
    {
        Scene scene = null;
        
        bool success = this.scenes.TryGetValue(id, out scene);

        if (!success)
        {
            scene = null;
        }

        return scene;
    }

    public void clearScenes()
    {
        this.scenes.Clear();
    }
}
