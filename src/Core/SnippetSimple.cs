using DtgeCore.Serialization;

namespace DtgeCore;

/**
 * The Simple Snippet contains a single variation with zero logic. This is mainly used for blocks
 * of text within scenes that don't vary based subscene, Entity state, or anything else.
 */
public partial class Snippet
{
	protected class SimpleSnippetImplementation : ISnippetImplementation
	{
		public Mode CurrentMode { get { return Mode.Simple; } }

		protected Variation SingleVariation { get; set; }

		public SimpleSnippetImplementation(Scene parentScene)
		{
			this.SingleVariation = new Variation(parentScene, "(Simple)", string.Empty);
		}

		public SimpleSnippetImplementation(
			Scene parentScene,
			SnippetSimpleSerializable serializable)
		{
			this.SingleVariation =
				new Variation(parentScene, serializable.SingleVariation);
		}

		public string CalculateText()
		{
			return this.SingleVariation.Text;

		}
	}
}
