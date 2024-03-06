using System.Text.Json;

namespace DtgeCore;

/**
 * Options are how players move from scene to scene, providing the main form of
 * interactivity for the engine.
 */
public class Option
{
    public string Id { get; set; }
    public string TargetSceneId { get; set; }
    public string DisplayName { get; set; }
    public string Tooltip { get; set; }
    public bool Enabled { get; set; }

    public Option()
    {
        this.Id = "";
        this.TargetSceneId = "";
        this.DisplayName = "";
        this.Enabled = true;
    }
    public Option(string id, string targetSceneId, string displayName, bool enabled= true)
    {
        this.Id= id;
        this.TargetSceneId= targetSceneId;
        this.DisplayName= displayName;
        this.Enabled= enabled;
    }

    public override string ToString()
    {
        string optionString = 
            "Option:\n" +
            "  Id: " + this.Id + "\n" +
            "  Target Scene: " + this.TargetSceneId + "\n" +
            "  Display Name: " + this.DisplayName + "\n" +
            "  Enabled: " + this.Enabled +"\n";
        return optionString;
    }

    public string Serialize()
    {
        string toReturn = JsonSerializer.Serialize(this);
        return toReturn;
    }
}