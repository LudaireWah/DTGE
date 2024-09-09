using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore;

/**
 * DTGE's Scenes are made up of a sequence of snippets, with each snippet containing
 * the desired text and any business logic of how the scene should unfold and change
 * in response to various kinds of information
 */
public class Snippet
{
	private const uint SIMPLE_DICTIONARY_KEY = 0;
	private const string SIMPLE_VARIATION_NAME = "simple";
	private const string RANDOM_KEY_PREFIX = "random_";
	private const uint IF_DICTIONARY_KEY = 0;
	private const string IF_VARIATION_NAME = "if";
	private const string ELSEIF_VARIATION_NAME = "else if";
	private const string ELSE_VARIATION_NAME = "else";

	public class Variation
	{
		public string Text { get; set; }
		public uint Id { get; private set; }
		public string ConditionalEntityName { get; set; }
		public Dictionary<uint, EntitySetter> EntitySetters { get; set; }

		private static uint nextId = 0;

		public Variation()
		{
			this.Text = string.Empty;
			this.Id = nextId++;
			this.ConditionalEntityName = string.Empty;
			this.EntitySetters = new Dictionary<uint, EntitySetter>();
		}

		public Variation(string text, Dictionary<uint, EntitySetter> entitySetters)
		{
			this.Text = text;
			this.Id = nextId++;
			this.ConditionalEntityName = string.Empty;
			this.EntitySetters = entitySetters;
		}
	}

	public class EntitySetter
	{
		public string Name { get; set; }
		public bool Value { get; set; }
		public uint Id { get; private set; }

		private static uint nextId = 0;

		public EntitySetter()
		{
			this.Name = string.Empty;
			this.Value = false;
			this.Id = nextId++;
		}

		public EntitySetter(string name, bool value)
		{
			this.Name = name;
			this.Value = value;
			this.Id = nextId++;
		}
	}

	public enum Mode
	{
		Simple,
		Subscene,
		If,
		IfElse,
		Random
	}

	public Mode CurrentMode
	{
		get; set;
	}

	public Dictionary<uint, Variation> variations { get; set; }

	private ISubsceneContextProvider subsceneContextProvider;

	private Random snippetRandomizer;
	private readonly int snippetRandomizerSeed;
	private uint currentRandomizedVariationIndex;

	public Snippet()
	{
		this.CurrentMode = Mode.Simple;

		this.variations = new Dictionary<uint, Variation>();
		this.variations.Add(SIMPLE_DICTIONARY_KEY, new Variation());

		this.subsceneContextProvider = null;

		Random seedGenerator = new Random();
		this.snippetRandomizerSeed = seedGenerator.Next();
		this.snippetRandomizer = new Random(this.snippetRandomizerSeed);
		this.currentRandomizedVariationIndex = (uint)snippetRandomizer.Next(this.variations.Count);
	}

	public Snippet(ISubsceneContextProvider subsceneContextProvider)
	{
		this.CurrentMode = Mode.Simple;

		this.variations = new Dictionary<uint, Variation>();
		this.variations.Add(SIMPLE_DICTIONARY_KEY, new Variation());

		this.subsceneContextProvider = subsceneContextProvider;
		this.registerHandlerFunctionsWithISubsceneContextProvider();

		Random seedGenerator = new Random();
		this.snippetRandomizerSeed = seedGenerator.Next();
		this.snippetRandomizer = new Random(this.snippetRandomizerSeed);
		this.currentRandomizedVariationIndex = (uint)snippetRandomizer.Next(this.variations.Count);
	}

	public void CopyFrom(Snippet other)
	{
		this.CurrentMode = other.CurrentMode;
		this.subsceneContextProvider = other.subsceneContextProvider;
		this.variations.Clear();
		foreach (uint key in other.variations.Keys)
		{
			this.variations.Add(key, other.variations[key]);
		}
	}

	public void SetSubsceneContextProvider(ISubsceneContextProvider subsceneContextProvider)
	{
		this.subsceneContextProvider = subsceneContextProvider;
		this.registerHandlerFunctionsWithISubsceneContextProvider();
	}

	public void Randomize()
	{
		this.currentRandomizedVariationIndex = (uint)this.snippetRandomizer.Next(this.variations.Count);
	}

	public string CalculateText()
	{
		string calculatedText = null;
		SimpleEntityManager simpleEntityManager = SimpleEntityManager.GetSimpleEntityManager();

		switch (this.CurrentMode)
		{
		case Mode.Simple:
			calculatedText = this.variations[SIMPLE_DICTIONARY_KEY].Text;
			break;
		case Mode.Subscene:
			calculatedText = this.variations[this.subsceneContextProvider.GetCurrentSubsceneId().Id].Text;
			break;
		case Mode.If:
			Variation ifVariation = this.variations[IF_DICTIONARY_KEY];
			if (simpleEntityManager.HasEntityValue(ifVariation.ConditionalEntityName) && simpleEntityManager.GetEntityValue(ifVariation.ConditionalEntityName))
			{
				calculatedText = ifVariation.Text;
			}
			break;
		case Mode.IfElse:
			for (int variationIndex = 0; variationIndex < this.variations.Count; variationIndex++)
			{
				Variation currentIfElseVariation = this.GetVariation(variationIndex);
				if (simpleEntityManager.HasEntityValue(currentIfElseVariation.ConditionalEntityName) && simpleEntityManager.GetEntityValue(currentIfElseVariation.ConditionalEntityName) ||
					variationIndex == this.variations.Count - 1)
				{
					calculatedText = currentIfElseVariation.Text;
					break;
				}
			}
			break;
		case Mode.Random:
			calculatedText = this.variations[currentRandomizedVariationIndex].Text;
			break;
		default:
			throw new NotImplementedException();
		}
		return calculatedText;
	}

	public void ExecuteEntitySetters()
	{
		SimpleEntityManager simpleEntityManager = SimpleEntityManager.GetSimpleEntityManager();
		Dictionary<uint, EntitySetter> entitySetters = null;

		switch (this.CurrentMode)
		{
		case Mode.Simple:
			entitySetters = this.variations[SIMPLE_DICTIONARY_KEY].EntitySetters;
			break;
		case Mode.Subscene:
			entitySetters = this.variations[this.subsceneContextProvider.GetCurrentSubsceneId().Id].EntitySetters;
			break;
		case Mode.If:
			Variation ifVariation = this.variations[IF_DICTIONARY_KEY];
			if (simpleEntityManager.HasEntityValue(ifVariation.ConditionalEntityName) && simpleEntityManager.GetEntityValue(ifVariation.ConditionalEntityName))
			{
				entitySetters = ifVariation.EntitySetters;
			}
			break;
		case Mode.IfElse:
			for (int variationIndex = 0; variationIndex < this.variations.Count; variationIndex++)
			{
				Variation currentIfElseVariation = this.GetVariation(variationIndex);
				if (simpleEntityManager.HasEntityValue(currentIfElseVariation.ConditionalEntityName) && simpleEntityManager.GetEntityValue(currentIfElseVariation.ConditionalEntityName) ||
					variationIndex == this.variations.Count - 1)
				{
					entitySetters = currentIfElseVariation.EntitySetters;
					break;
				}
			}
			break;
		case Mode.Random:
			entitySetters = this.variations[currentRandomizedVariationIndex].EntitySetters;
			break;
		default:
			throw new NotImplementedException();
		}

		if (entitySetters != null)
		{

			foreach (uint id in entitySetters.Keys)
			{
				EntitySetter currentEntitySetter = entitySetters[id];
				simpleEntityManager.SetEntityValue(currentEntitySetter.Name, currentEntitySetter.Value);
			}
		}
	}

	public void ChangeMode(Mode newMode)
	{
		switch(this.CurrentMode)
		{
		case Mode.Simple:
			this.changeFromSimpleMode(newMode);
			break;
		case Mode.Subscene:
			this.changeFromSubsceneMode(newMode);
			break;
		case Mode.If:
			this.changeFromIfMode(newMode);
			break;
		case Mode.IfElse:
			this.changeFromIfElseMode(newMode);
			break;
		case Mode.Random:
			this.changeFromRandomMode(newMode);
			break;
		default:
			throw new NotImplementedException();
		}

		this.CurrentMode = newMode;
	}

	public int GetVariationCount()
	{
		return this.variations.Count;
	}

	public void AddVariation()
	{
		switch(this.CurrentMode)
		{
		case Mode.Simple:
			// Error
			break;
		case Mode.Subscene:
			// Error
			break;
		case Mode.If:
			// Error
			break;
		case Mode.IfElse:
			this.variations.Add((uint)this.variations.Count, new Variation());
			break;
		case Mode.Random:
			this.variations.Add((uint)this.variations.Count, new Variation());
			break;
		default:
			throw new NotImplementedException();
		}
	}

	public void RemoveVariation(int variationIndexToRemove)
	{
		switch (this.CurrentMode)
		{
		case Mode.Simple:
			// Error
			break;
		case Mode.Subscene:
			// Error
			break;
		case Mode.If:
			// Error
			break;
		case Mode.IfElse:
			for (uint currentVariationIndex = (uint)variationIndexToRemove; currentVariationIndex < variations.Count - 1; currentVariationIndex++)
			{
				this.variations[currentVariationIndex] =
					this.variations[currentVariationIndex + 1];
			}
			this.variations.Remove((uint)variations.Count - 1);
			if (this.variations.Count > 1)
			{
				this.variations[(uint)this.variations.Count - 1].ConditionalEntityName = string.Empty;
			}
			break;
		case Mode.Random:
			for (uint currentVariationIndex = (uint)variationIndexToRemove; currentVariationIndex < variations.Count - 1; currentVariationIndex++)
			{
				this.variations[currentVariationIndex] =
					this.variations[currentVariationIndex + 1];
			}
			this.variations.Remove((uint)variations.Count - 1);
			break;
		default:
			throw new NotImplementedException();
		}
	}

	public string GetVariationName(int variationIndex)
	{
		string variationName = null;

		switch (this.CurrentMode)
		{
		case Mode.Simple:
			variationName = SIMPLE_VARIATION_NAME;
			break;
		case Mode.Subscene:
			variationName = this.subsceneContextProvider.GetSubsceneId(variationIndex).Name;
			break;
		case Mode.If:
			variationName = IF_VARIATION_NAME;
			break;
		case Mode.IfElse:
			if (variationIndex == 0)
			{
				variationName = IF_VARIATION_NAME;
			}
			else if (variationIndex == this.variations.Count - 1)
			{
				variationName = ELSE_VARIATION_NAME;
			}
			else
			{
				variationName = ELSEIF_VARIATION_NAME;
			}
			break;
		case Mode.Random:
			variationName = translateIndexToNameForRandomVariation(variationIndex);
			break;
		default:
			throw new NotImplementedException();
		}

		return variationName;
	}

	public Variation GetVariation(int variationIndex)
	{
		Variation variation = null;
		
		switch (this.CurrentMode)
		{
		case Mode.Simple:
			variation = this.variations[SIMPLE_DICTIONARY_KEY];
			break;
		case Mode.Subscene:
			variation = this.variations[this.subsceneContextProvider.GetSubsceneId(variationIndex).Id];
			break;
		case Mode.If:
			variation = this.variations[IF_DICTIONARY_KEY];
			break;
		case Mode.IfElse:
			variation = this.variations[(uint)variationIndex];
			break;
		case Mode.Random:
			variation = this.variations[(uint)variationIndex];
			break;
		default:
			throw new NotImplementedException();
		}

		return variation;
	}

	public void SetVariationText(int variationIndex, string text)
	{
		switch (this.CurrentMode)
		{
		case Mode.Simple:
			this.variations[SIMPLE_DICTIONARY_KEY].Text = text;
			break;
		case Mode.Subscene:
			this.variations[this.subsceneContextProvider.GetSubsceneId(variationIndex).Id].Text = text;
			break;
		case Mode.If:
			this.variations[IF_DICTIONARY_KEY].Text = text;
			break;
		case Mode.IfElse:
			this.variations[(uint)variationIndex].Text = text;
			break;
		case Mode.Random:
			this.variations[(uint)variationIndex].Text = text;
			break;
		default:
			throw new NotImplementedException();
		}
	}

	public string GetVariationTextByIndex(int variationIndex)
	{
		string variationText = null;

		switch (this.CurrentMode)
		{
		case Mode.Simple:
			variationText = this.variations[SIMPLE_DICTIONARY_KEY].Text;
			break;
		case Mode.Subscene:
			variationText = this.variations[this.subsceneContextProvider.GetSubsceneId(variationIndex).Id].Text;
			break;
		case Mode.If:
			variationText = this.variations[IF_DICTIONARY_KEY].Text;
			break;
		case Mode.IfElse:
			variationText = this.variations[(uint)variationIndex].Text;
			break;
		case Mode.Random:
			variationText = this.variations[(uint)variationIndex].Text;
			break;
		default:
			throw new NotImplementedException();
		}

		return variationText;
	}

	public bool IsSimpleModeDisabled()
	{
		return this.variations.Count > 1;
	}

	public bool IsSubsceneModeDisabled()
	{
		return this.subsceneContextProvider.GetSubsceneCount() == 0;
	}

	public string GetCopyableText(string variationBoundaryMarker)
	{
		string copyableVariationText = "";

		for (int variationIndex = 0; variationIndex < this.variations.Count; variationIndex++)
		{
			copyableVariationText += this.GetVariationTextByIndex(variationIndex);
			if (variationIndex < this.variations.Count - 1)
			{
				copyableVariationText += variationBoundaryMarker;
			}
		}

		return copyableVariationText;
	}

	public void RestoreFromPastedText(string[] pastedTextSplitByVariation)
	{
		for (int variationIndex = 0; variationIndex < this.variations.Count; variationIndex++)
		{
			this.SetVariationText(variationIndex, pastedTextSplitByVariation[variationIndex]);
		}
	}

	private void changeFromSimpleMode(Mode newMode)
	{
		Variation simpleVariation = this.variations[SIMPLE_DICTIONARY_KEY];

		switch (newMode)
		{
		case Mode.Simple:
			// No-op
			break;
		case Mode.Subscene:
			if (this.subsceneContextProvider.GetSubsceneCount() < 1)
			{
				// error
			}
			else
			{
				this.variations.Clear();
				Scene.SubsceneId firstSubsceneId = this.subsceneContextProvider.GetSubsceneId(0);
				this.variations.Add(firstSubsceneId.Id, new Variation(simpleVariation.Text, simpleVariation.EntitySetters));
				for (int subsceneIndex = 1; subsceneIndex < this.subsceneContextProvider.GetSubsceneCount(); subsceneIndex++)
				{
					Scene.SubsceneId currentSubsceneId = this.subsceneContextProvider.GetSubsceneId(subsceneIndex);
					this.variations.Add(currentSubsceneId.Id, new Variation());
				}
			}
			break;
		case Mode.If:
			this.variations[IF_DICTIONARY_KEY].ConditionalEntityName = string.Empty;
			break;
		case Mode.IfElse:
			this.variations[IF_DICTIONARY_KEY].ConditionalEntityName = string.Empty;
			this.variations.Add(1, new Variation());
			break;
		case Mode.Random:
			this.variations.Clear();
			this.variations.Add(0, new Variation(simpleVariation.Text, simpleVariation.EntitySetters));
			break;
		default:
			throw new NotImplementedException();
		}
	}
	
	private void changeFromSubsceneMode(Mode newMode)
	{
		Variation firstSubsceneVariation = this.variations[this.subsceneContextProvider.GetSubsceneId(0).Id];

		switch (newMode)
		{
		case Mode.Simple:
			this.variations.Clear();
			this.variations.Add(SIMPLE_DICTIONARY_KEY, new Variation(firstSubsceneVariation.Text, firstSubsceneVariation.EntitySetters));
			break;
		case Mode.Subscene:
			// No-op
			break;
		case Mode.If:
			this.variations.Clear();
			this.variations.Add(IF_DICTIONARY_KEY, new Variation(firstSubsceneVariation.Text, firstSubsceneVariation.EntitySetters));
			this.variations[IF_DICTIONARY_KEY].ConditionalEntityName = string.Empty;
			break;
		case Mode.IfElse:
			Variation[] cachedSubsceneVariationsForIfElse = new Variation[this.subsceneContextProvider.GetSubsceneCount()];
			for (int subsceneIndex = 0; subsceneIndex < this.subsceneContextProvider.GetSubsceneCount(); subsceneIndex++)
			{
				Scene.SubsceneId subsceneId = this.subsceneContextProvider.GetSubsceneId(subsceneIndex);
				cachedSubsceneVariationsForIfElse[subsceneIndex] = this.variations[subsceneId.Id];
			}
			this.variations.Clear();
			this.variations.Add(0, new Variation(cachedSubsceneVariationsForIfElse[0].Text, cachedSubsceneVariationsForIfElse[0].EntitySetters));
			if (cachedSubsceneVariationsForIfElse.Length > 1)
			{
				for (int variationIndex = 1; variationIndex < cachedSubsceneVariationsForIfElse.Length - 1; variationIndex++)
				{
					this.variations.Add(
						(uint)variationIndex,
						new Variation(cachedSubsceneVariationsForIfElse[variationIndex].Text, cachedSubsceneVariationsForIfElse[variationIndex].EntitySetters));
				}
				this.variations.Add((uint)cachedSubsceneVariationsForIfElse.Length - 1, new Variation());
			}
			else
			{
				this.variations.Add(1, new Variation());
			}
			break;
		case Mode.Random:
			Variation[] cachedSubsceneVariationsForRandom = new Variation[this.subsceneContextProvider.GetSubsceneCount()];
			for (int subsceneIndex = 0; subsceneIndex < this.subsceneContextProvider.GetSubsceneCount(); subsceneIndex++)
			{
				Scene.SubsceneId subsceneId = this.subsceneContextProvider.GetSubsceneId(subsceneIndex);
				cachedSubsceneVariationsForRandom[subsceneIndex] = this.variations[subsceneId.Id];
			}
			this.variations.Clear();
			for (int variationIndex = 0; variationIndex < cachedSubsceneVariationsForRandom.Length; variationIndex++)
			{
				this.variations.Add(
					(uint)variationIndex,
					new Variation(cachedSubsceneVariationsForRandom[variationIndex].Text, cachedSubsceneVariationsForRandom[variationIndex].EntitySetters));
			}
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void changeFromIfMode(Mode newMode)
	{
		Variation ifVariation = this.variations[IF_DICTIONARY_KEY];

		switch (newMode)
		{
		case Mode.Simple:
			ifVariation.ConditionalEntityName = string.Empty;
			this.variations[SIMPLE_DICTIONARY_KEY].ConditionalEntityName = string.Empty;
			break;
		case Mode.Subscene:
			if (this.subsceneContextProvider.GetSubsceneCount() < 1)
			{
				// error
			}
			else
			{
				ifVariation.ConditionalEntityName = string.Empty;
				this.variations.Clear();
				Scene.SubsceneId firstSubsceneId = this.subsceneContextProvider.GetSubsceneId(0);
				this.variations.Add(firstSubsceneId.Id, new Variation(ifVariation.Text, ifVariation.EntitySetters));
				for (int subsceneIndex = 1; subsceneIndex < this.subsceneContextProvider.GetSubsceneCount(); subsceneIndex++)
				{
					Scene.SubsceneId currentSubsceneId = this.subsceneContextProvider.GetSubsceneId(subsceneIndex);
					this.variations.Add(currentSubsceneId.Id, new Variation());
				}
			}
			break;
		case Mode.If:
			// No-op
			break;
		case Mode.IfElse:
			this.variations.Add(1, new Variation());
			break;
		case Mode.Random:
			ifVariation.ConditionalEntityName = string.Empty;
			this.variations.Clear();
			this.variations.Add(0, new Variation(ifVariation.Text, ifVariation.EntitySetters));
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void changeFromIfElseMode(Mode newMode)
	{
		Variation ifVariation = this.variations[IF_DICTIONARY_KEY];

		switch (newMode)
		{
		case Mode.Simple:
			this.variations.Clear();
			this.variations.Add(SIMPLE_DICTIONARY_KEY, new Variation(ifVariation.Text, ifVariation.EntitySetters));
			break;
		case Mode.Subscene:
			if (this.subsceneContextProvider.GetSubsceneCount() < 1)
			{
				// error
			}
			else
			{
				Variation[] cachedIfElseVariationsForSubscene = new Variation[this.variations.Count];
				for (int variationIndex = 0; variationIndex < this.variations.Count; variationIndex++)
				{
					cachedIfElseVariationsForSubscene[variationIndex] = this.variations[(uint)variationIndex];
				}
				this.variations.Clear();
				for (int subsceneIndex = 0; subsceneIndex < this.subsceneContextProvider.GetSubsceneCount(); subsceneIndex++)
				{
					Scene.SubsceneId currentSubsceneId = this.subsceneContextProvider.GetSubsceneId(subsceneIndex);
					if (subsceneIndex < cachedIfElseVariationsForSubscene.Length)
					{
						this.variations.Add(currentSubsceneId.Id, new Variation(cachedIfElseVariationsForSubscene[subsceneIndex].Text, cachedIfElseVariationsForSubscene[subsceneIndex].EntitySetters));
					}
					else
					{
						this.variations.Add(currentSubsceneId.Id, new Variation());
					}
				}
			}
			break;
		case Mode.If:
			this.variations.Clear();
			this.variations.Add(IF_DICTIONARY_KEY, new Variation(ifVariation.Text, ifVariation.EntitySetters));
			break;
		case Mode.IfElse:
			// No-op
			break;
		case Mode.Random:
			Variation[] cachedIfElseVariationsForRandom = new Variation[this.variations.Count];
			for (int variationIndex = 0; variationIndex < this.variations.Count; variationIndex++)
			{
				cachedIfElseVariationsForRandom[variationIndex] = this.variations[(uint)variationIndex];
			}
			this.variations.Clear();
			for (int variationIndex = 0; variationIndex < cachedIfElseVariationsForRandom.Length; variationIndex++)
			{
				this.variations.Add(
					(uint)variationIndex,
					new Variation(cachedIfElseVariationsForRandom[variationIndex].Text, cachedIfElseVariationsForRandom[variationIndex].EntitySetters));
			}
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void changeFromRandomMode(Mode newMode)
	{
		Variation firstRandomVariation = this.variations[0];

		switch (newMode)
		{
		case Mode.Simple:
			this.variations.Clear();
			this.variations.Add(SIMPLE_DICTIONARY_KEY, new Variation(firstRandomVariation.Text, firstRandomVariation.EntitySetters));
			break;
		case Mode.Subscene:
			if (this.subsceneContextProvider.GetSubsceneCount() < 1)
			{
				// error
			}
			else
			{
				Variation[] cachedRandomVariationsForSubscene = new Variation[this.variations.Count];
				for (int variationIndex = 0; variationIndex < this.variations.Count; variationIndex++)
				{
					cachedRandomVariationsForSubscene[variationIndex] = this.variations[(uint)variationIndex];
				}
				this.variations.Clear();
				for (int subsceneIndex = 0; subsceneIndex < this.subsceneContextProvider.GetSubsceneCount(); subsceneIndex++)
				{
					Scene.SubsceneId currentSubsceneId = this.subsceneContextProvider.GetSubsceneId(subsceneIndex);
					if (subsceneIndex < cachedRandomVariationsForSubscene.Length)
					{
						this.variations.Add(currentSubsceneId.Id, new Variation(cachedRandomVariationsForSubscene[subsceneIndex].Text, cachedRandomVariationsForSubscene[subsceneIndex].EntitySetters));
					}
					else
					{
						this.variations.Add(currentSubsceneId.Id, new Variation());
					}
				}
			}
			break;
		case Mode.If:
			this.variations.Clear();
			this.variations.Add(IF_DICTIONARY_KEY, new Variation(firstRandomVariation.Text, firstRandomVariation.EntitySetters));
			break;
		case Mode.IfElse:
			Variation[] cachedRandomVariationsForIfElse = new Variation[this.variations.Count];
			for (int variationIndex = 0; variationIndex < this.variations.Count; variationIndex++)
			{
				cachedRandomVariationsForIfElse[variationIndex] = this.variations[(uint)variationIndex];
			}
			this.variations.Clear();
			this.variations.Add(0, new Variation(cachedRandomVariationsForIfElse[0].Text, cachedRandomVariationsForIfElse[0].EntitySetters));
			if (cachedRandomVariationsForIfElse.Length > 1)
			{
				for (int variationIndex = 1; variationIndex < cachedRandomVariationsForIfElse.Length; variationIndex++)
				{
					this.variations.Add(
						(uint)variationIndex,
						new Variation(cachedRandomVariationsForIfElse[variationIndex].Text, cachedRandomVariationsForIfElse[variationIndex].EntitySetters));
				}
			}
			else
			{
				this.variations.Add(1, new Variation());
			}
			break;
		case Mode.Random:
			// No-op
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private static string translateIndexToNameForRandomVariation(int index)
	{
		return RANDOM_KEY_PREFIX + (index + 1).ToString();
	}

	private void registerHandlerFunctionsWithISubsceneContextProvider()
	{
		this.subsceneContextProvider.RegisterOnSubsceneAdded(this.handleSubsceneAdded);
		this.subsceneContextProvider.RegisterOnSubsceneRemoved(this.handleSubsceneRemoved);
	}

	private void handleSubsceneAdded(Scene.SubsceneId subsceneId)
	{
		if (this.CurrentMode == Mode.Subscene)
		{
			this.variations.Add(subsceneId.Id, new Variation());
		}
	}

	private void handleSubsceneRemoved(Scene.SubsceneId subsceneId)
	{
		if (this.CurrentMode == Mode.Subscene)
		{
			if (this.variations.Count == 1)
			{
				this.ChangeMode(Mode.Simple);
			}
			else
			{
				this.variations.Remove(subsceneId.Id);
			}
		}
	}
}
