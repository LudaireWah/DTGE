using DtgeCore.Serialization;

namespace DtgeCore;

public class SnippetSimple : SceneElement, ISnippet
{
	public Snippet.Mode Mode { get { return Snippet.Mode.Simple; } }

	protected Variation SingleVariation { get; set; }

	public SnippetSimple(Scene parentScene)
		: base(parentScene)
	{
		this.SingleVariation = new Variation(parentScene, "(Simple)", string.Empty);
	}

	public SnippetSimple(Scene parentScene, SnippetSimpleSerializable serializable)
		: base(parentScene, serializable)
	{
		this.SingleVariation =
			new Variation(parentScene, serializable.SingleVariation);
	}

	public string CalculateText()
	{
		return this.SingleVariation.Text;
	}
}
