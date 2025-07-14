using System.Collections.Generic;

using DtgeCore.Serialization;

namespace DtgeCore;

/**
 * Subscene Snippets always contain a single variation per subscene on the parent scene and 
 * provide the text of the snippet matching the parent scene's current subscene.
 */
public partial class Snippet
{
	protected class SubsceneSnippetImplementation : ISnippetImplementation
	{
		public Mode CurrentMode { get { return Mode.Subscene; } }

		protected Dictionary<SUID, Variation> VariationsBySubsceneId { get; private set; } = new Dictionary<SUID, Variation>();

		protected Scene parentScene;

		public SubsceneSnippetImplementation(Scene parentScene)
		{
			if (parentScene == null)
			{
				CoreErrorHandler.InvokeInitializationError("A Subscene Snippet's Implementation was initialized with a null parent Scene.");
			}
			else
			{
				this.parentScene = parentScene;
			}
		}

		public SubsceneSnippetImplementation(
			Scene parentScene,
			SnippetSubsceneSerializable serializable)
		{
			if (parentScene == null)
			{
				CoreErrorHandler.InvokeInitializationError("A Subscene Snippet's Implementation was initialized with a null parent Scene.");
			}
			else if (serializable == null)
			{
				CoreErrorHandler.InvokeInitializationError("A Subscene Snippet's Implementation was initialized with a null serializable.");
			}
			else
			{
				this.parentScene = parentScene;

				foreach (SUID subsceneSuid in serializable.VariationsBySubsceneId.Keys)
				{
					this.VariationsBySubsceneId[subsceneSuid] =
						new Variation(parentScene, serializable.VariationsBySubsceneId[subsceneSuid]);
				}
			}
		}

		public string CalculateText()
		{
			string subsceneText = null;

			if (this.parentScene.CurrentSubscene == null)
			{
				CoreErrorHandler.InvokePlayError("A Subscene Snippet attempted to calculate text with a null current subscene.");
			}
			else
			{
				Variation subsceneVariation =
					this.VariationsBySubsceneId[this.parentScene.CurrentSubscene.Id];

				if (subsceneVariation == null)
				{
					CoreErrorHandler.InvokePlayError("A Subscene Snippet did not have a variation for the current subscene.");
				}
				else
				{
					subsceneText = subsceneVariation.Text;
				}
			}

			return subsceneText;
		}
	}
}
