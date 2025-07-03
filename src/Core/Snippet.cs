using DtgeCore.Serialization;

namespace DtgeCore;

/**
 * The snippet is the most complex element of a DTGE scene, and it's the element that's most
 * responsible for the "dynamic" in the name. Conceptually, Snippets contain one or more
 * variation, and they calculate their text by running some kind of logic to determine which
 * variation should be used and returning that variation so that the results of each Snippet can
 * be concatenated by the scene and displayed as the scene's text.
 * 
 * The complexity is that snippets have multiple modes of operation. While the CalculateText entry
 * point is the same across modes, the internal logic of how it chooses between variations and
 * what capabilities it offers at edit time varies wildly.
 * 
 * As a result, the Snippet class is mostly a wrapper. It contains the current mode and the entry
 * point for CalculateText, but otherwise, it contains a single ISnippetImplementation which is
 * the matching implementation for the mode.
 * 
 * This class is a partial class because implementations are classes protected within the Snippet
 * for encapsulation, but they're split into separate files for readability.
 */
public partial class Snippet : SceneElement
{
	public enum Mode
	{
		Simple,
		Subscene,
		Random
	}

	public interface ISnippetImplementation
	{
		public Mode CurrentMode { get; }
		public abstract string CalculateText();
	}

	public Mode CurrentMode { get { return this.currentImplementation.CurrentMode; } }

	protected ISnippetImplementation currentImplementation;

	public Snippet(Scene parentScene)
		: base(parentScene)
	{
	}

	public Snippet(Scene parentScene, SnippetSerializable serializable)
		: base(parentScene, serializable)
	{
		this.currentImplementation = null;

		switch (serializable.Mode)
		{
		case Mode.Simple:
			SnippetSimpleSerializable simpleSerializable =
				serializable as SnippetSimpleSerializable;
			this.currentImplementation =
				new SimpleSnippetImplementation(parentScene, simpleSerializable);
			break;
		case Mode.Subscene:
			SnippetSubsceneSerializable subsceneSerializable =
				serializable as SnippetSubsceneSerializable;
			this.currentImplementation =
				new SubsceneSnippetImplementation(parentScene, subsceneSerializable);
			break;
		case Mode.Random:
			SnippetRandomSerializable randomSerializable =
				serializable as SnippetRandomSerializable;
			this.currentImplementation =
				new RandomSnippetImplementation(parentScene, randomSerializable);
			break;
		default:
			GlobalErrorHandler.InvokeError("An unknown snippet mode was encountered.");
			break;
		}
	}

	public virtual string CalculateText()
	{
		return this.currentImplementation.CalculateText();
	}
}
