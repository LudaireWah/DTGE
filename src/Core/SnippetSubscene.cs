using System.Collections.Generic;

using DtgeCore.Serialization;

namespace DtgeCore;

public class SnippetSubscene : SceneElement, ISnippet
{
	public Snippet.Mode Mode { get { return Snippet.Mode.Subscene; } }

	protected Dictionary<SUID, Variation> VariationsBySubsceneId { get; private set; }

	public SnippetSubscene(Scene parentScene)
		:base(parentScene)
	{
		this.VariationsBySubsceneId = new Dictionary<SUID, Variation>();
	}

	public SnippetSubscene(Scene parentScene, SnippetSubsceneSerializable serializable)
		: base(parentScene, serializable)
	{
		this.VariationsBySubsceneId = new Dictionary<SUID, Variation>();

		foreach (SUID subsceneSuid in serializable.VariationsBySubsceneId.Keys)
		{
			this.VariationsBySubsceneId[subsceneSuid] =
				new Variation(parentScene, serializable.VariationsBySubsceneId[subsceneSuid]);
		}
	}

	public string CalculateText()
	{
		Variation variation = this.VariationsBySubsceneId[this.ParentScene.CurrentSubscene.Id];
		return variation.Text;
	}
}
