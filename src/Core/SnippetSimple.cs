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
			if (parentScene == null)
			{
				CoreErrorHandler.InvokeInitializationError("A Simple Snippet's Implementation was initialized with a null parent Scene.");
			}
			else
			{
				this.SingleVariation = new Variation(parentScene, "(Simple)", string.Empty);
			}
		}

		public SimpleSnippetImplementation(
			Scene parentScene,
			SnippetSimpleSerializable serializable)
		{
			if (parentScene == null)
			{
				CoreErrorHandler.InvokeInitializationError("A Simple Snippet's Implementation was initialized with a null parent Scene.");
			}
			else if (serializable == null)
			{
				CoreErrorHandler.InvokeInitializationError("A Simple Snippet's Implementation was initialized with a null serializable.");
			}
			else
			{
				this.SingleVariation =
					new Variation(parentScene, serializable.SingleVariation);
			}
		}

		public string CalculateText()
		{
			string variationText = null;

			if (this.SingleVariation == null)
			{
				CoreErrorHandler.InvokePlayError("A Simple snippet had a null variation.");
			}
			else
			{
				variationText = this.SingleVariation.Text;
			}

			return variationText;

		}
	}
}
