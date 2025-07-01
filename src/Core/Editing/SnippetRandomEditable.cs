using DtgeCore.Serialization;
using System.Collections.Generic;

namespace DtgeCore.Editing;

public class SnippetRandomEditable : SnippetRandom, ISnippetEditable
{
	private SceneEditable parentSceneEditable;

	private int lastRandomizedVariationIndex;

	public SnippetRandomEditable(SceneEditable parentSceneEditable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;
		this.lastRandomizedVariationIndex = 0;
	}

	public SnippetRandomEditable(SceneEditable parentSceneEditable, ISnippetEditable other)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;
		this.lastRandomizedVariationIndex = 0;
	}

	public SnippetRandomEditable(
		SceneEditable parentSceneEditable,
		SnippetRandomSerializable serializable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;
		this.lastRandomizedVariationIndex = 0;
	}

	public SnippetSerializable ToSerializable()
	{
		SnippetRandomSerializable serializable =
			this.CreateSerializable<SnippetRandomSerializable>();

		serializable.Mode = this.Mode;

		for (int variationIndex = 0; variationIndex < this.Variations.Count; variationIndex++)
		{
			VariationEditable variationEditable =
				this.Variations[variationIndex] as VariationEditable;
			serializable.Variations[variationIndex] = variationEditable.ToSerializable();
		}

		return null;
	}

	public override string CalculateText()
	{
		int randomizedIndex = this.ParentScene.SceneRandom.Next(this.Variations.Count);
		this.lastRandomizedVariationIndex = randomizedIndex;

		return this.Variations[randomizedIndex].Text;
	}

	public string CalculateTextStable()
	{
		return this.Variations[this.lastRandomizedVariationIndex].Text;
	}

	public string GetCopyableText(string variationBoundaryMarker)
	{
		string copyableVariationText = "";

		for (int variationIndex = 0; variationIndex < this.Variations.Count; variationIndex++)
		{
			copyableVariationText += this.Variations[variationIndex].Text;
			if (variationIndex < this.parentSceneEditable.GetSubsceneCount() - 1)
			{
				copyableVariationText += variationBoundaryMarker;
			}
		}

		return copyableVariationText;
	}

	public void RestoreFromPastedText(string[] pastedTextSplitByVariation)
	{
		if (pastedTextSplitByVariation.Length != this.Variations.Count)
		{
			GlobalErrorHandler.InvokeError("There was a mismatch between variation count and pasted variation count when restoring from pasted text.");
		}
		else
		{
			for (int variationIndex = 0; variationIndex < this.Variations.Count; variationIndex++)
			{
				VariationEditable variationEditable =
					this.Variations[variationIndex] as VariationEditable;
				variationEditable.Text = pastedTextSplitByVariation[variationIndex];
			}
		}
	}

	public VariationEditable GetVariationEditable(int variationIndex)
	{
		return this.Variations[variationIndex] as VariationEditable;
	}

	public bool CanEditVariationCount()
	{
		return true;
	}

	public int GetVariationCount()
	{
		return this.Variations.Count;
	}

	public bool AddVariation()
	{
		this.Variations.Add(new VariationEditable(this.parentSceneEditable));
		return true;
	}

	public bool RemoveVariationEditable(int variationIndex)
	{
		this.Variations.RemoveAt(variationIndex);

		return true;
	}
}
