using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore;

public class Scene
{
    public string Id { get; set; }
    private string sceneText;
    public string SceneText
    {
        get
        {
            if (this.sceneText == null && this.encodedSceneText != null)
            {
                this.sceneText = System.Text.Encoding.Unicode.GetString(this.encodedSceneText);
                this.encodedSceneText = null;
            }
            return sceneText;
        }
        set
        {
            this.sceneText = value;
        }
    }
    public byte[] encodedSceneText { get; set; }

    public const int MAX_OPTION_NUMBER = 15;
    public Option[] optionList {  get; set; }

    public Scene()
    {
        this.Id = "";
        this.sceneText = null;
        this.optionList = new Option[MAX_OPTION_NUMBER];
    }
    public Scene(string id)
    {
        this.Id= id;
        this.sceneText= null;
        this.optionList = new Option[MAX_OPTION_NUMBER];
    }

    public override string ToString()
    {
        string sceneString =
            "Id: " + this.Id + "\n" +
            "Scene Text: \n" +
            this.sceneText + "\n\n" +
            "Options:\n";

        for (int optionIndex = 0; optionIndex < this.optionList.Count(); optionIndex++)
        {
            Option option = this.optionList[optionIndex];
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
            this.encodedSceneText = System.Text.Encoding.Unicode.GetBytes(this.sceneText);
        }
        this.sceneText = null;
        string toReturn = JsonSerializer.Serialize(this);
        return toReturn;
    }

    public bool MatchesId(string id)
    {
        return this.Id == id;
    }

    public bool AddOption(Option option)
    {
        int assignedOptionIndex = -1;

        for (int optionIndex = 0; optionIndex < MAX_OPTION_NUMBER && assignedOptionIndex == -1; optionIndex++)
        {
            if (this.optionList[optionIndex] == null)
            {
                this.optionList[optionIndex] = option;
                assignedOptionIndex = optionIndex;
            }
        }

        return assignedOptionIndex > -1;
    }

    public void ClearAllOptions()
    {
        for (int optionIndex = 0; optionIndex < MAX_OPTION_NUMBER; optionIndex++)
        {
            this.optionList[optionIndex] = null;
        }
    }

    public int GetOptionCount()
    {
        return optionList.Count();
    }

    public Option GetOption(int index)
    {
        return optionList[index];
    }
}
