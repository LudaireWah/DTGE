using System.Collections.Generic;

using DtgeCore.Serialization;

namespace DtgeCore;

public class SnippetRandom : SceneElement, ISnippet
{
	public Snippet.Mode Mode { get { return Snippet.Mode.Random; } }

	protected List<Variation> Variations { get; private set; }

	public SnippetRandom(Scene parentScene)
		: base(parentScene)
	{
		this.Variations = new List<Variation>();
	}

	public SnippetRandom(Scene parentScene, SnippetRandomSerializable serializable)
		:base(parentScene, serializable)
	{
		this.Variations = new List<Variation>();
	}

	public virtual string CalculateText()
	{
		int randomizedIndex = this.ParentScene.SceneRandom.Next(this.Variations.Count);

		return this.Variations[randomizedIndex].Text;
	}
}
