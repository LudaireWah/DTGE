namespace DtgeCore;

public class Variation : SceneElement
{
    public string Name { get; protected set; }
    public string Text { get; protected set; }

    public Variation(Scene parentScene)
        : base(parentScene)
    {
        this.Name = "";
        this.Text = "";
    }

    public Variation(Scene parentScene, string name, string text)
        : base(parentScene)
    {
        this.Name = name;
        this.Text = text;
    }
}
