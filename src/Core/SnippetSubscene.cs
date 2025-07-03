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

		protected Dictionary<SUID, Variation> VariationsBySubsceneId { get; private set; }

		protected Scene parentScene;

		public SubsceneSnippetImplementation(Scene parentScene)
		{
			this.parentScene = parentScene;
			this.VariationsBySubsceneId = new Dictionary<SUID, Variation>();
		}

		public SubsceneSnippetImplementation(
			Scene parentScene,
			SnippetSubsceneSerializable serializable)
		{
			this.parentScene = parentScene;
			this.VariationsBySubsceneId = new Dictionary<SUID, Variation>();

			foreach (SUID subsceneSuid in serializable.VariationsBySubsceneId.Keys)
			{
				this.VariationsBySubsceneId[subsceneSuid] =
					new Variation(parentScene, serializable.VariationsBySubsceneId[subsceneSuid]);
			}
		}

		public string CalculateText()
		{
			Variation variation =
				this.VariationsBySubsceneId[this.parentScene.CurrentSubscene.Id];
			return variation.Text;
		}
	}
}
