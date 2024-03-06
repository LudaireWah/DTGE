using System.Linq;
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
    public string SceneText
    {
        get
        {
            if (this.sceneText == null && this.EncodedSceneText != null)
            {
                this.sceneText = System.Text.Encoding.Unicode.GetString(this.EncodedSceneText);
                this.EncodedSceneText = null;
            }
            return sceneText;
        }
        set
        {
            this.sceneText = value;
        }
    }
    public byte[] EncodedSceneText { get; set; }
    public Option[] OptionList {  get; set; }

    private string sceneText;

    public Scene()
    {
        this.Id = "";
        this.sceneText = null;
        this.OptionList = new Option[MAX_OPTION_NUMBER];
    }
    public Scene(string id)
    {
        this.Id= id;
        this.sceneText= null;
        this.OptionList = new Option[MAX_OPTION_NUMBER];
    }

    public override string ToString()
    {
        string sceneString =
            "Id: " + this.Id + "\n" +
            "Scene Text: \n" +
            this.sceneText + "\n\n" +
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
        if (this.sceneText != null)
        {
            this.EncodedSceneText = System.Text.Encoding.Unicode.GetBytes(this.sceneText);
        }
        this.sceneText = null;
        string toReturn = JsonSerializer.Serialize(this);
        return toReturn;
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
}
