using DtgeCore.Serialization;
using System;
using System.Linq;

namespace DtgeCore.Editing;

/**
 * The class responsible for editing logic for Snippets.
 * 
 * This is planned to be heavily restructured in the near future, as
 * the current implementation of Snippets is a bit of a nightmare.
 * 
 * However, to note a few quirks that might not be apparent:
 *  - For Simple and Random mode, the key for the Variations dictionary
 *    is the Variation's SUID. For Subscene mode, it's the Subscene's
 *    SUID instead.
 */
public class SnippetEditable : Snippet
{
	private const string SIMPLE_VARIATION_NAME = "simple";
	private const string RANDOM_VARIATION_PREFIX = "Random_";

	public new SnippetMode Mode
	{
		get {  return base.Mode; }
		set
		{
			this.notifyParentOfEdit();
			base.Mode = value;
		}
	}

	private SceneEditable parentSceneEditable;
	private int nextRandomVariationIndex = 1;

	public SnippetEditable(SceneEditable parentSceneEditable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;

		this.Mode = SnippetMode.Simple;
		VariationEditable newVariation = new VariationEditable(this.parentSceneEditable, SnippetEditable.SIMPLE_VARIATION_NAME, "");
		this.Variations.Add(newVariation.Id, newVariation);
		this.OrderedVariationIds.Add(newVariation.Id);
	}

	public SnippetEditable(SceneEditable parentSceneEditable, SnippetSerializable serializable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;

		this.Mode = serializable.Mode;
		foreach (SUID id in serializable.Variations.Keys)
		{
			this.Variations.Add(id, new VariationEditable(parentSceneEditable, serializable.Variations[id]));
		}
		for (int variationIndex = 0; variationIndex < serializable.OrderedVariationIds.Count; variationIndex++)
		{
			this.OrderedVariationIds.Add(serializable.OrderedVariationIds[variationIndex]);
		}

		Random seedGenerator = new Random();
		this.snippetRandomizerSeed = seedGenerator.Next();
		this.snippetRandomizer = new Random(this.snippetRandomizerSeed);
		this.currentRandomizedVariationIndex = (int)this.snippetRandomizer.Next(this.Variations.Count);
	}

	public SnippetSerializable ToSerializable()
	{
		SnippetSerializable serializable = this.CreateSerializable<SnippetSerializable>();

		serializable.Mode = this.Mode;
		foreach (SUID key in this.Variations.Keys)
		{
			VariationEditable variationEditable = this.Variations[key] as VariationEditable;
			serializable.Variations.Add(key, variationEditable.ToSerializable());
		}
		for (int variationIdIndex = 0; variationIdIndex < this.OrderedVariationIds.Count; variationIdIndex++)
		{
			serializable.OrderedVariationIds.Add(this.OrderedVariationIds[variationIdIndex]);
		}

		return serializable;
	}

	public void CopyFrom(SnippetEditable other)
	{
		base.CopyFrom(other);

		this.Mode = other.Mode;
		this.Variations.Clear();
		foreach (SUID key in other.Variations.Keys)
		{
			this.Variations.Add(key, other.Variations[key]);
		}
		this.OrderedVariationIds.Clear();
		for (int orderedVariationIdIndex = 0; orderedVariationIdIndex < other.OrderedVariationIds.Count; orderedVariationIdIndex++)
		{
			this.OrderedVariationIds[orderedVariationIdIndex] = other.OrderedVariationIds[orderedVariationIdIndex];
		}
		this.parentSceneEditable = other.parentSceneEditable;

		this.notifyParentOfEdit();
	}

	public string CalculateTextWithoutRandomization()
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
			calculatedText = this.Variations[this.OrderedVariationIds[this.currentRandomizedVariationIndex]].Text;
			break;
		default:
			throw new NotImplementedException();
		}
		return calculatedText;
	}

	public void AddVariation()
	{
		switch (this.Mode)
		{
		case SnippetMode.Simple:
			// Error
			break;
		case SnippetMode.Subscene:
			// Error
			break;
		case SnippetMode.Random:
			VariationEditable newVariation = new VariationEditable(
				this.parentSceneEditable,
				this.getNextNewRandomName(),
				"");
			this.Variations.Add(newVariation.Id, newVariation);
			this.OrderedVariationIds.Add(newVariation.Id);
			break;
		default:
			throw new NotImplementedException();
		}

		this.notifyParentOfEdit();
	}

	public void RemoveVariation(SUID variationId)
	{
		switch (this.Mode)
		{
		case SnippetMode.Simple:
			// Error
			break;
		case SnippetMode.Subscene:
			// Error
			break;
		case SnippetMode.Random:
			this.Variations.Remove(variationId);
			this.removeVariationFromOrderedListAndShift(variationId);
			break;
		default:
			throw new NotImplementedException();
		}

		this.notifyParentOfEdit();
	}

	public void RemoveVariationByIndex(int variationIndex)
	{
		this.RemoveVariation(this.OrderedVariationIds[variationIndex]);
	}

	public void ChangeMode(SnippetMode newMode)
	{
		switch (this.Mode)
		{
		case SnippetMode.Simple:
			this.changeFromSimpleMode(newMode);
			break;
		case SnippetMode.Subscene:
			this.changeFromSubsceneMode(newMode);
			break;
		case SnippetMode.Random:
			this.changeFromRandomMode(newMode);
			break;
		default:
			throw new NotImplementedException();
		}

		this.Mode = newMode;
		this.notifyParentOfEdit();
	}

	public int GetVariationCount()
	{
		return this.Variations.Count;
	}

	public string GetVariationName(int variationIndex)
	{
		string variationName = null;

		switch (this.Mode)
		{
		case SnippetMode.Simple:
			variationName = SnippetEditable.SIMPLE_VARIATION_NAME;
			break;
		case SnippetMode.Subscene:
			variationName = this.parentSceneEditable.GetSubscene(variationIndex).Name;
			break;
		case SnippetMode.Random:
			variationName = this.Variations[this.OrderedVariationIds[variationIndex]].Name;
			break;
		default:
			throw new NotImplementedException();
		}

		return variationName;
	}

	public VariationEditable GetVariationEditable(int variationIndex)
	{
		Variation variationInfo;

		switch (this.Mode)
		{
		case SnippetMode.Simple:
			variationInfo = this.Variations[this.OrderedVariationIds.First()];
			break;
		case SnippetMode.Subscene:
			variationInfo = this.Variations[this.parentSceneEditable.GetSubscene(variationIndex).Id];
			break;
		case SnippetMode.Random:
			variationInfo = this.Variations[this.OrderedVariationIds[variationIndex]];
			break;
		default:
			throw new NotImplementedException();
		}

		return variationInfo as VariationEditable;
	}

	public void SetVariationText(int variationIndex, string text)
	{
		switch (this.Mode)
		{
		case SnippetMode.Simple:
			this.getVariationEditable(this.OrderedVariationIds.First()).Text = text;
			break;
		case SnippetMode.Subscene:
			this.getVariationEditable(this.parentSceneEditable.GetSubscene(variationIndex).Id).Text = text;
			break;
		case SnippetMode.Random:
			this.getVariationEditable(this.OrderedVariationIds[variationIndex]).Text = text;
			break;
		default:
			throw new NotImplementedException();
		}
	}

	public string GetVariationTextByIndex(int variationIndex)
	{
		string variationText = null;

		switch (this.Mode)
		{
		case SnippetMode.Simple:
			variationText = this.Variations[this.OrderedVariationIds.First()].Text;
			break;
		case SnippetMode.Subscene:
			variationText = this.Variations[this.parentSceneEditable.GetSubscene(variationIndex).Id].Text;
			break;
		case SnippetMode.Random:
			variationText = this.Variations[this.OrderedVariationIds[variationIndex]].Text;
			break;
		default:
			throw new NotImplementedException();
		}

		return variationText;
	}

	public bool IsSimpleModeDisabled()
	{
		return this.Variations.Count > 1;
	}

	public bool IsSubsceneModeDisabled()
	{
		return this.parentSceneEditable.GetSubsceneCount() == 0;
	}

	public string GetCopyableText(string variationBoundaryMarker)
	{
		string copyableVariationText = "";

		for (int variationIndex = 0; variationIndex < this.Variations.Count; variationIndex++)
		{
			copyableVariationText += this.GetVariationTextByIndex(variationIndex);
			if (variationIndex < this.Variations.Count - 1)
			{
				copyableVariationText += variationBoundaryMarker;
			}
		}

		return copyableVariationText;
	}

	public void RestoreFromPastedText(string[] pastedTextSplitByVariation)
	{
		for (int variationIndex = 0; variationIndex < this.Variations.Count; variationIndex++)
		{
			this.SetVariationText(variationIndex, pastedTextSplitByVariation[variationIndex]);
		}
	}

	public void HandleSubsceneAdded(Subscene subscene)
	{
		if (this.Mode == SnippetMode.Subscene)
		{
			VariationEditable newSubsceneVariationEditable = new VariationEditable(this.parentSceneEditable, subscene.Name, string.Empty);
			this.Variations.Add(newSubsceneVariationEditable.Id, newSubsceneVariationEditable);
			this.OrderedVariationIds.Add(newSubsceneVariationEditable.Id);
		}
	}

	public void HandleSubsceneRemoved(Subscene subscene)
	{
		if (this.Mode == SnippetMode.Subscene)
		{
			if (this.Variations.Count == 1)
			{
				this.ChangeMode(SnippetMode.Simple);
			}
			else
			{
				VariationEditable removedVariation = this.Variations[subscene.Id] as VariationEditable;
				this.removeVariationFromOrderedListAndShift(removedVariation.Id);
			}
		}
	}

	public void HandleSubsceneRenamed(Subscene subscene, string newName)
	{
		if (this.Mode == SnippetMode.Subscene)
		{
			VariationEditable subsceneVariation = this.getVariationEditable(subscene.Id);
			subsceneVariation.Name = subscene.Name;
		}
	}

	private void changeFromSimpleMode(SnippetMode newMode)
	{
		switch (newMode)
		{
		case SnippetMode.Simple:
			// No-op
			break;
		case SnippetMode.Subscene:
			if (this.parentSceneEditable.GetSubsceneCount() < 1)
			{
				// error
			}
			else
			{
				string simpleTextToTransferToFirstSubsceneVariation = this.Variations[this.OrderedVariationIds.First()].Text;
				this.Variations.Clear();
				this.OrderedVariationIds.Clear();
				Subscene firstSubscene = this.parentSceneEditable.GetSubscene(0);
				this.Variations.Add(firstSubscene.Id, new VariationEditable(this.parentSceneEditable, firstSubscene.Name, simpleTextToTransferToFirstSubsceneVariation));
				for (int subsceneIndex = 1; subsceneIndex < this.parentSceneEditable.GetSubsceneCount(); subsceneIndex++)
				{
					Subscene nextSubscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
					this.Variations.Add(nextSubscene.Id, new VariationEditable(this.parentSceneEditable, nextSubscene.Name, string.Empty));
					this.OrderedVariationIds.Add(nextSubscene.Id);
				}
			}
			break;
		case SnippetMode.Random:
			string simpleTextToTransferToFirstRandomVariation = this.Variations[this.OrderedVariationIds.First()].Text;
			this.Variations.Clear();
			this.OrderedVariationIds.Clear();
			VariationEditable newVariationEditable = new VariationEditable(this.parentSceneEditable, translateIndexToKeyForRandomVariation(0), simpleTextToTransferToFirstRandomVariation);
			this.Variations.Add(newVariationEditable.Id, newVariationEditable);
			this.OrderedVariationIds.Add(newVariationEditable.Id);
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void changeFromSubsceneMode(SnippetMode newMode)
	{
		switch (newMode)
		{
		case SnippetMode.Simple:
			string firstSubsceneTextToTransferToSimpleVariation = this.Variations[this.parentSceneEditable.GetSubscene(0).Id].Text;
			this.Variations.Clear();
			this.OrderedVariationIds.Clear();
			VariationEditable newSimpleVariationEditable = new VariationEditable(this.parentSceneEditable, SIMPLE_VARIATION_NAME, firstSubsceneTextToTransferToSimpleVariation);
			this.Variations.Add(newSimpleVariationEditable.Id, newSimpleVariationEditable);
			this.OrderedVariationIds.Add(newSimpleVariationEditable.Id);
			break;
		case SnippetMode.Subscene:
			// No-op
			break;
		case SnippetMode.Random:
			string[] cachedSubsceneTextsToTranslateToRandom = new string[this.parentSceneEditable.GetSubsceneCount()];
			for (int subsceneIndex = 0; subsceneIndex < this.parentSceneEditable.GetSubsceneCount(); subsceneIndex++)
			{
				Subscene subscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
				cachedSubsceneTextsToTranslateToRandom[subsceneIndex] = this.Variations[subscene.Id].Text;
			}
			this.Variations.Clear();
			this.OrderedVariationIds.Clear();
			for (int variationIndex = 0; variationIndex < cachedSubsceneTextsToTranslateToRandom.Length; variationIndex++)
			{
				VariationEditable newRandomVariationEditable = new VariationEditable(this.parentSceneEditable, getNextNewRandomName(), cachedSubsceneTextsToTranslateToRandom[variationIndex]);
				this.Variations.Add(newRandomVariationEditable.Id, newRandomVariationEditable);
				this.OrderedVariationIds.Add(newRandomVariationEditable.Id);
			}
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void changeFromRandomMode(SnippetMode newMode)
	{
		switch (newMode)
		{
		case SnippetMode.Simple:
			string firstRandomTextToTransferToSimpleVariation = this.Variations[this.OrderedVariationIds.First()].Text;
			this.Variations.Clear();
			this.OrderedVariationIds.Clear();
			VariationEditable newSimpleVariation = new VariationEditable(this.parentSceneEditable, SIMPLE_VARIATION_NAME, firstRandomTextToTransferToSimpleVariation);
			this.Variations.Add(newSimpleVariation.Id, newSimpleVariation);
			this.OrderedVariationIds.Add(newSimpleVariation.Id);
			break;
		case SnippetMode.Subscene:
			if (this.parentSceneEditable.GetSubsceneCount() < 1)
			{
				// error
			}
			else
			{
				string[] cachedRandomTextsToTranslateToSubscenes = new string[this.parentSceneEditable.GetSubsceneCount()];
				for (int variationIndex = 0; variationIndex < this.Variations.Count; variationIndex++)
				{
					cachedRandomTextsToTranslateToSubscenes[variationIndex] = this.Variations[this.OrderedVariationIds[variationIndex]].Text;
				}
				this.Variations.Clear();
				this.OrderedVariationIds.Clear();
				for (int subsceneIndex = 0; subsceneIndex < this.parentSceneEditable.GetSubsceneCount(); subsceneIndex++)
				{
					Subscene currentSubscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
					VariationEditable newRandomVariationEditable = new VariationEditable(this.parentSceneEditable, currentSubscene.Name, cachedRandomTextsToTranslateToSubscenes[subsceneIndex]);
					this.Variations.Add(newRandomVariationEditable.Id, newRandomVariationEditable);
					this.OrderedVariationIds.Add(newRandomVariationEditable.Id);
				}
			}
			break;
		case SnippetMode.Random:
			// No-op
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private static string translateIndexToKeyForRandomVariation(int index)
	{
		return RANDOM_VARIATION_PREFIX + (index + 1).ToString();
	}

	private void notifyParentOfEdit()
	{
		if (this.parentSceneEditable != null)
		{
			this.parentSceneEditable.NotifyUIUpdateNeeded();
		}
	}

	private VariationEditable getVariationEditable(int index)
	{
		return this.getVariationEditable(this.OrderedVariationIds[index]);
	}

	private VariationEditable getVariationEditable(SUID id)
	{
		VariationEditable variationEditable = this.Variations[id] as VariationEditable;

		if (variationEditable == null)
		{
			// Error
		}

		return variationEditable;
	}

	private string getNextNewRandomName()
	{
		return SnippetEditable.RANDOM_VARIATION_PREFIX + this.nextRandomVariationIndex++;
	}

	private void removeVariationFromOrderedListAndShift(SUID variationId)
	{
		int removedVariationIndex = this.OrderedVariationIds.FindIndex((SUID id) => id == variationId);
		for (int currentVariationIndex = removedVariationIndex; currentVariationIndex < this.OrderedVariationIds.Count - 1; currentVariationIndex++)
		{
			this.OrderedVariationIds[currentVariationIndex] =
				this.OrderedVariationIds[currentVariationIndex + 1];
		}
		this.OrderedVariationIds.RemoveAt(this.OrderedVariationIds.Count - 1);
	}
}
