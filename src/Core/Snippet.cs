using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore;

/**
 * DTGE's Scenes are made up of a sequence of snippets, with each snippet containing
 * the desired text and any business logic of how the scene should unfold and change
 * in response to various kinds of state within the game.
 */
public class Snippet : SceneElement
{
	public enum SnippetMode
	{
		Simple,
		Subscene,
		Random
	}

	public SnippetMode Mode { get; protected set; }

	protected Dictionary<SUID, Variation> Variations { get; private set; }
	protected List<SUID> OrderedVariationIds { get; private set; }

	protected Random snippetRandomizer;
	protected readonly int snippetRandomizerSeed;
	protected int currentRandomizedVariationIndex;

	public Snippet(Scene parentScene)
		: base(parentScene)
    {
        Random seedGenerator = new Random();
        this.snippetRandomizerSeed = seedGenerator.Next();

		this.Mode = SnippetMode.Simple;

		this.Variations = new Dictionary<SUID, Variation>();
		this.OrderedVariationIds = new List<SUID>();

		this.snippetRandomizer = new Random(this.snippetRandomizerSeed);
		this.currentRandomizedVariationIndex = (int)this.snippetRandomizer.Next(this.Variations.Count);
	}

	public string CalculateText()
	{
		string calculatedText = null;

		switch (this.Mode)
		{
		case SnippetMode.Simple:
			calculatedText = this.Variations[this.OrderedVariationIds.First()].Text;
			break;
		case SnippetMode.Subscene:
			calculatedText = this.Variations[this.ParentScene.CurrentSubscene.Id].Text;
			break;
		case SnippetMode.Random:
			this.currentRandomizedVariationIndex = (int)this.snippetRandomizer.Next(this.Variations.Count);
			calculatedText = this.Variations[this.OrderedVariationIds[this.currentRandomizedVariationIndex]].Text;
			break;
		default:
			throw new NotImplementedException();
		}
		return calculatedText;
	}
}
