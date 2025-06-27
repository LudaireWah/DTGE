using DtgeCore.Serialization;

namespace DtgeCore;

/**
 * Variations are used by Snippets to house the different variations of that Snippet's text. The
 * heavy lifting is done by the Snippets, and this class just houses the text, an author-readable
 * name, and an Id (inherited from SceneElement).
 */
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

    public Variation(Scene parentScene, VariationSerializable variationSerializable)
        : base(parentScene, variationSerializable)
    {
        this.Name = variationSerializable.Name;
        this.Text = variationSerializable.Text;
    }

    public Variation(Scene parentScene, string name, string text)
        : base(parentScene)
    {
        this.Name = name;
        this.Text = text;
    }
}
