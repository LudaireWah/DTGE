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
	private const int SIMPLE_DICTIONARY_KEY = 0;
	private const string SIMPLE_VARIATION_NAME = "simple";
	private const string RANDOM_KEY_PREFIX = "Random_";

	public class VariationInfo
	{
		public string Name { get; set; }
		public string Text { get; set; }
		public uint Id { get; private set; }

		private static uint nextId = 0;

		public VariationInfo()
		{
			this.Id = nextId++;
		}

		public VariationInfo(string name, string text)
		{
			this.Name = name;
			this.Text = text;
			this.Id = nextId++;
		}
	}

	public enum Mode
	{
		Simple,
		Subscene,
		Random
	}

	public SUID Id { get; private set; }

	public Mode CurrentMode { get; set; }

	public Dictionary<int, VariationInfo> variations { get; set; }

	private ISubsceneContextProvider subsceneContextProvider;

	private Random snippetRandomizer;
	private readonly int snippetRandomizerSeed;
	private int currentRandomizedVariationIndex;

	public Snippet(SceneData parentSceneData)
	{
		this.Id = parentSceneData.GetSUID();
		this.CurrentMode = Mode.Simple;

		this.variations = new Dictionary<int, VariationInfo>();
		this.variations.Add(SIMPLE_DICTIONARY_KEY, new VariationInfo(SIMPLE_VARIATION_NAME, string.Empty));

		this.subsceneContextProvider = parentSceneData;
		this.registerHandlerFunctionsWithISubsceneContextProvider();

		Random seedGenerator = new Random();
		this.snippetRandomizerSeed = seedGenerator.Next();
		this.snippetRandomizer = new Random(this.snippetRandomizerSeed);
		this.currentRandomizedVariationIndex = (int)snippetRandomizer.Next(this.variations.Count);
	}

	public void CopyFrom(Snippet other)
	{
		this.CurrentMode = other.CurrentMode;
		this.subsceneContextProvider = other.subsceneContextProvider;
		this.variations.Clear();
		foreach (int key in other.variations.Keys)
		{
			this.variations.Add(key, other.variations[key]);
		}
	}

	public void SetSubsceneContextProvider(ISubsceneContextProvider subsceneContextProvider)
	{
		this.subsceneContextProvider = subsceneContextProvider;
		this.registerHandlerFunctionsWithISubsceneContextProvider();
	}

	public string CalculateText(bool doNotRandomize)
	{
		string calculatedText = null;

		switch (this.CurrentMode)
		{
		case Mode.Simple:
			calculatedText = this.variations[SIMPLE_DICTIONARY_KEY].Text;
			break;
		case Mode.Subscene:
			calculatedText = this.variations[this.subsceneContextProvider.GetCurrentSubscene().Id.GetHashCode()].Text;
			break;
		case Mode.Random:
			if (!doNotRandomize)
			{
				currentRandomizedVariationIndex = (int)this.snippetRandomizer.Next(this.variations.Count);
			}
			calculatedText = this.variations[currentRandomizedVariationIndex].Text;
			break;
		default:
			throw new NotImplementedException();
		}
		return calculatedText;
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
		case Mode.Random:
			this.variations.Add((int)this.variations.Count, new VariationInfo(translateIndexToKeyForRandomVariation(this.variations.Count), string.Empty));
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
		case Mode.Random:
			for (int currentVariationIndex = variationIndexToRemove; currentVariationIndex < variations.Count - 1; currentVariationIndex++)
			{
				this.variations[currentVariationIndex] =
					this.variations[currentVariationIndex + 1];
			}
			this.variations.Remove(variations.Count - 1);
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
			variationName = this.subsceneContextProvider.GetSubscene(variationIndex).Name;
			break;
		case Mode.Random:
			variationName = this.variations[variationIndex].Name;
			break;
		default:
			throw new NotImplementedException();
		}

		return variationName;
	}

	public VariationInfo GetVariationInfo(int variationIndex)
	{
		VariationInfo variationInfo;

		switch (this.CurrentMode)
		{
		case Mode.Simple:
			variationInfo = this.variations[SIMPLE_DICTIONARY_KEY];
			break;
		case Mode.Subscene:
			variationInfo = this.variations[this.subsceneContextProvider.GetSubscene(variationIndex).Id.GetHashCode()];
			break;
		case Mode.Random:
			variationInfo = this.variations[variationIndex];
			break;
		default:
			throw new NotImplementedException();
		}

		return variationInfo;
	}

	public void SetVariationText(int variationIndex, string text)
	{
		switch (this.CurrentMode)
		{
		case Mode.Simple:
			this.variations[SIMPLE_DICTIONARY_KEY].Text = text;
			break;
		case Mode.Subscene:
			this.variations[this.subsceneContextProvider.GetSubscene(variationIndex).Id.GetHashCode()].Text = text;
			break;
		case Mode.Random:
			this.variations[variationIndex].Text = text;
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
			variationText = this.variations[this.subsceneContextProvider.GetSubscene(variationIndex).Id.GetHashCode()].Text;
			break;
		case Mode.Random:
			variationText = this.variations[variationIndex].Text;
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
				string simpleTextToTransferToFirstSubsceneVariation = this.variations[SIMPLE_DICTIONARY_KEY].Text;
				this.variations.Clear();
				Subscene firstSubscene = this.subsceneContextProvider.GetSubscene(0);
				this.variations.Add(firstSubscene.Id.GetHashCode(), new VariationInfo(firstSubscene.Name, simpleTextToTransferToFirstSubsceneVariation));
				for (int subsceneIndex = 1; subsceneIndex < this.subsceneContextProvider.GetSubsceneCount(); subsceneIndex++)
				{
					Subscene currentSubscene = this.subsceneContextProvider.GetSubscene(subsceneIndex);
					this.variations.Add(currentSubscene.Id.GetHashCode(), new VariationInfo(currentSubscene.Name, string.Empty));
				}
			}
			break;
		case Mode.Random:
			string simpleTextToTransferToFirstRandomVariation = this.variations[SIMPLE_DICTIONARY_KEY].Text;
			this.variations.Clear();
			this.variations.Add(0, new VariationInfo(translateIndexToKeyForRandomVariation(0), simpleTextToTransferToFirstRandomVariation));
			break;
		default:
			throw new NotImplementedException();
		}

	}
	
	private void changeFromSubsceneMode(Mode newMode)
	{
		switch (newMode)
		{
		case Mode.Simple:
			string firstSubsceneTextToTransferToSimpleVariation = this.variations[this.subsceneContextProvider.GetSubscene(0).Id.GetHashCode()].Text;
			this.variations.Clear();
			this.variations.Add(SIMPLE_DICTIONARY_KEY, new VariationInfo(SIMPLE_VARIATION_NAME, firstSubsceneTextToTransferToSimpleVariation));
			break;
		case Mode.Subscene:
			// No-op
			break;
		case Mode.Random:
			string[] cachedSubsceneTextsToTranslateToRandom = new string[this.subsceneContextProvider.GetSubsceneCount()];
			for (int subsceneIndex = 0; subsceneIndex < this.subsceneContextProvider.GetSubsceneCount(); subsceneIndex++)
			{
				Subscene subscene = this.subsceneContextProvider.GetSubscene(subsceneIndex);
				cachedSubsceneTextsToTranslateToRandom[subsceneIndex] = this.variations[subscene.Id.GetHashCode()].Text;
			}
			this.variations.Clear();
			for (int variationIndex = 0; variationIndex < cachedSubsceneTextsToTranslateToRandom.Length; variationIndex++)
			{
				this.variations.Add(
					variationIndex,
					new VariationInfo(translateIndexToKeyForRandomVariation(variationIndex), cachedSubsceneTextsToTranslateToRandom[variationIndex]));
			}
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void changeFromRandomMode(Mode newMode)
	{
		switch (newMode)
		{
		case Mode.Simple:
			string firstRandomTextToTransferToSimpleVariation = this.variations[0].Text;
			this.variations.Clear();
			this.variations.Add(SIMPLE_DICTIONARY_KEY, new VariationInfo(SIMPLE_VARIATION_NAME, firstRandomTextToTransferToSimpleVariation));
			break;
		case Mode.Subscene:
			if (this.subsceneContextProvider.GetSubsceneCount() < 1)
			{
				// error
			}
			else
			{
				string[] cachedRandomTextsToTranslateToSubscenes = new string[this.subsceneContextProvider.GetSubsceneCount()];
				for (int variationIndex = 0; variationIndex < this.variations.Count; variationIndex++)
				{
					cachedRandomTextsToTranslateToSubscenes[variationIndex] = this.variations[variationIndex].Text;
				}
				this.variations.Clear();
				for (int subsceneIndex = 0; subsceneIndex < this.subsceneContextProvider.GetSubsceneCount(); subsceneIndex++)
				{
					Subscene currentSubscene = this.subsceneContextProvider.GetSubscene(subsceneIndex);
					this.variations.Add(currentSubscene.Id.GetHashCode(), new VariationInfo(currentSubscene.Name, cachedRandomTextsToTranslateToSubscenes[subsceneIndex]));
				}
			}
			break;
		case Mode.Random:
			// No-op
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private static string translateIndexToKeyForRandomVariation(int index)
	{
		return RANDOM_KEY_PREFIX + (index + 1).ToString();
	}

	private void registerHandlerFunctionsWithISubsceneContextProvider()
	{
		this.subsceneContextProvider.RegisterOnSubsceneAdded(this.handleSubsceneAdded);
		this.subsceneContextProvider.RegisterOnSubsceneRemoved(this.handleSubsceneRemoved);
		this.subsceneContextProvider.RegisterOnSubsceneRenamed(this.handleSubsceneRenamed);
	}
	private void unregisterHandlerFunctionsWithISubsceneContextProvider()
	{
		this.subsceneContextProvider.UnregisterOnSubsceneAdded(this.handleSubsceneAdded);
		this.subsceneContextProvider.UnregisterOnSubsceneRemoved(this.handleSubsceneRemoved);
		this.subsceneContextProvider.UnregisterOnSubsceneRenamed(this.handleSubsceneRenamed);
	}

	private void handleSubsceneAdded(Subscene subscene)
	{
		if (this.CurrentMode == Mode.Subscene)
		{
			this.variations.Add(subscene.Id.GetHashCode(), new VariationInfo(subscene.Name, string.Empty));
		}
	}

	private void handleSubsceneRemoved(Subscene subscene)
	{
		if (this.CurrentMode == Mode.Subscene)
		{
			if (this.variations.Count == 1)
			{
				this.ChangeMode(Mode.Simple);
			}
			else
			{
				this.variations.Remove(subscene.Id.GetHashCode());
			}
		}
	}

	private void handleSubsceneRenamed(Subscene subscene, string newName)
	{
		if (this.CurrentMode == Mode.Subscene)
		{
			this.variations[subscene.Id.GetHashCode()].Name = subscene.Name;
		}
	}
}
