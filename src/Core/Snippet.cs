using DtgeCore.Serialization;
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
	protected int snippetRandomizerSeed; // I'd like to make this readonly in some way, but that'll require some refactoring of the constructor, since SnippetEditable's constructor can't set this if it's readonly.
	protected int currentRandomizedVariationIndex;

	public Snippet(Scene parentScene)
		: base(parentScene)
    {
		this.Mode = SnippetMode.Simple;
		this.Variations = new Dictionary<SUID, Variation>();
		this.OrderedVariationIds = new List<SUID>();

		Random seedGenerator = new Random();
		this.snippetRandomizerSeed = seedGenerator.Next();
		this.snippetRandomizer = new Random(this.snippetRandomizerSeed);
		this.currentRandomizedVariationIndex = (int)this.snippetRandomizer.Next(this.Variations.Count);
	}

	public Snippet(Scene parentScene, SnippetSerializable serializable)
		: base(parentScene, serializable)
	{
		this.Mode = serializable.Mode;
		this.Variations = new Dictionary<SUID, Variation>();
		foreach (SUID id in serializable.Variations.Keys)
		{
			this.Variations.Add(id, new Variation(this.ParentScene, serializable.Variations[id]));
		}
		this.OrderedVariationIds = new List<SUID>();
		for (int variationIndex = 0; variationIndex < serializable.OrderedVariationIds.Count; variationIndex++)
		{
			this.OrderedVariationIds.Add(serializable.OrderedVariationIds[variationIndex]);
		}

		Random seedGenerator = new Random();
		this.snippetRandomizerSeed = seedGenerator.Next();
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
