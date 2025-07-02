using System.Collections.Generic;

using DtgeCore.Serialization;

namespace DtgeCore;

/**
 * Random Snippets have a set of variations of any number and will randomly decide which to use.
 * The Random Snippet should always use the random number generator provided by the parent Scene
 * so that a certain scene state can be deterministically recreated by the scene for debugging
 * purposes.
 */
public partial class Snippet
{
	protected class RandomSnippetImplementation : ISnippetImplementation
	{
		public Mode CurrentMode { get { return Mode.Random; } }

		protected List<Variation> Variations { get; private set; }

		protected Scene parentScene;

		public RandomSnippetImplementation(Scene parentScene)
		{
			this.parentScene = parentScene;
			this.Variations = new List<Variation>();
		}

		public RandomSnippetImplementation(
			Scene parentScene,
			SnippetRandomSerializable serializable)
		{
			this.parentScene = parentScene;
			this.Variations = new List<Variation>();

			for (int variationIndex = 0;
				variationIndex < serializable.Variations.Count;
				variationIndex++)
			{
				this.Variations.Add(
					new Variation(parentScene, serializable.Variations[variationIndex]));
			}
		}

		public virtual string CalculateText()
		{
			int randomizedIndex = this.parentScene.SceneRandom.Next(this.Variations.Count);

			return this.Variations[randomizedIndex].Text;
		}
	}
}
