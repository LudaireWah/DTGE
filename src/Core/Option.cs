using System.Text.Json;

namespace DtgeCore;

public class Option
{
    public string id { get; set; }
    public string targetSceneId { get; set; }
    public string displayName { get; set; }
    public string tooltip { get; set; }
    public bool enabled { get; set; }
    public Option()
    {
        this.id = "";
        this.targetSceneId = "";
        this.displayName = "";
        this.enabled = true;
    }
    public Option(string id, string targetSceneId, string displayName, bool enabled= true)
    {
        this.id= id;
        this.targetSceneId= targetSceneId;
        this.displayName= displayName;
        this.enabled= enabled;
    }

    public override string ToString()
    {
        string optionString;

        if (this.enabled)
        {
            optionString = 
                "Option:\n" +
                "  Id: " + this.id + "\n" +
                "  Target Scene: " + this.targetSceneId + "\n" +
                "  Display Name: " + this.displayName + "\n";
        }
        else
        {
            optionString = "Option:\n" + "  Disabled\n";
        }
        return optionString;
    }

    public string Serialize()
    {
        string toReturn = JsonSerializer.Serialize(this);
        return toReturn;
    }
}