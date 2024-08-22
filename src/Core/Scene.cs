using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore;

/**
 * DTGE's Scene is the fundamental unit of the engine. Nearly everything displayed to the
 * player is displayed via a scene. Thus, navigation through the game is navigating
 * through a set of scenes, each presenting the player with a description of the player's
 * situation and which options the player may choose to advance in the game.
 */
public class Scene
{
    public const int MAX_OPTION_NUMBER = 15;

    public string Id { get; set; }
    public Option[] OptionList { get; set; }
    public List<Snippet> SnippetList { get; set; }
    public string SceneText { get; set; }

    public Scene()
    {
        this.Id = "";
        this.OptionList = new Option[MAX_OPTION_NUMBER];
        this.SnippetList = new List<Snippet>();
        this.AddOption(new Option());
        this.AddSnippet(new Snippet());
    }

    public Scene(string id)
    {
        this.Id= id;
        this.OptionList = new Option[MAX_OPTION_NUMBER];
        this.SnippetList = new List<Snippet>();
        this.AddOption(new Option());
        this.AddSnippet(new Snippet());
    }

    public override string ToString()
    {
        string sceneString =
            "Id: " + this.Id + "\n" +
            "Scene Text: \n" +
            this.CalculateSceneText(true) + "\n\n" +
            "Options:\n";

        for (int optionIndex = 0; optionIndex < this.OptionList.Count(); optionIndex++)
        {
            Option option = this.OptionList[optionIndex];
            if (option != null)
            {
                sceneString += option.ToString();
            }
        }

        return sceneString;
    }

    public string Serialize()
    {   
        string toReturn = JsonSerializer.Serialize(this);
        return toReturn;
    }

    public static Scene Deserialize(string sceneJson)
    {
        Scene deserializedScene = JsonSerializer.Deserialize<DtgeCore.Scene>(sceneJson);
        if (deserializedScene.SceneText != null && deserializedScene.SceneText.Length != 0)
        {
            Snippet snippetFromSceneText = new Snippet();
            snippetFromSceneText.SetSnippetVariationText(0,(deserializedScene.SceneText));
            deserializedScene.ClearAllSnippets();
            deserializedScene.SnippetList.Add(snippetFromSceneText);
            deserializedScene.SceneText = null;
        }
        return deserializedScene;
    }

    public bool AddOption(Option option)
    {
        int assignedOptionIndex = -1;

        for (int optionIndex = 0; optionIndex < MAX_OPTION_NUMBER && assignedOptionIndex == -1; optionIndex++)
        {
            if (this.OptionList[optionIndex] == null)
            {
                this.OptionList[optionIndex] = option;
                assignedOptionIndex = optionIndex;
            }
        }

        return assignedOptionIndex > -1;
    }

    public void ClearAllOptions()
    {
        for (int optionIndex = 0; optionIndex < MAX_OPTION_NUMBER; optionIndex++)
        {
            this.OptionList[optionIndex] = null;
        }
    }

    public int GetOptionCount()
    {
        return this.OptionList.Length;
    }

    public Option GetOption(int index)
    {
        return this.OptionList[index];
    }

    public void AddSnippet(Snippet snippet)
    {
        this.SnippetList.Add(snippet);
    }

    public void ClearAllSnippets()
    {
        this.SnippetList.Clear();
    }

    public int GetSnippetCount()
    {
        return this.SnippetList.Count;
    }

    public string CalculateSceneText(bool preserveRandomization)
    {
        string sceneText = "";

        // Normal scene text concatenation
        for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; ++snippetIndex)
        {
            Snippet currentSnippet = this.SnippetList[snippetIndex];
            sceneText += currentSnippet.CalculateText(preserveRandomization);
        }

        // Json serialization for debug purposes
        sceneText = this.Serialize();

        return sceneText;
    }
}
