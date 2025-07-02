using DtgeCore.Serialization;

namespace DtgeCore.Editing;

public partial class SnippetEditable
{
	private class RandomSnippetImplementationEditable
		: RandomSnippetImplementation, ISnippetEditableImplementation
	{
		private const string RANDOM_VARIATION_NAME_PREFIX = "Random ";

		private SceneEditable parentSceneEditable;

		private int currentVariationIndex;
		private int lastIntUsedToCreateName;

		public RandomSnippetImplementationEditable(SceneEditable parentSceneEditable)
			: base(parentSceneEditable)
		{
			this.parentSceneEditable = parentSceneEditable;
			this.currentVariationIndex = 0;
			this.lastIntUsedToCreateName = 1;
		}

		public RandomSnippetImplementationEditable(
			SceneEditable parentSceneEditable,
			ISnippetEditableImplementation other)
			: base(parentSceneEditable)
		{
			this.parentSceneEditable = parentSceneEditable;
			this.currentVariationIndex = 0;
			this.lastIntUsedToCreateName = 1;

			for (int variationIndex = 0;
				variationIndex < other.GetVariationCount();
				variationIndex++)
			{
				this.Variations.Add(new VariationEditable(
					parentSceneEditable,
					getVariationNameForIndex(this.lastIntUsedToCreateName++),
					other.GetVariationText(variationIndex)));
			}
		}

		public RandomSnippetImplementationEditable(
			SceneEditable parentSceneEditable,
			SnippetRandomSerializable serializable)
			: base(parentSceneEditable)
		{
			this.parentSceneEditable = parentSceneEditable;
			this.currentVariationIndex = 0;
			this.lastIntUsedToCreateName = 1;

			for (int variationIndex = 0;
				variationIndex < serializable.Variations.Count;
				variationIndex++)
			{
				this.Variations.Add(new VariationEditable(
					parentSceneEditable,
					serializable.Variations[variationIndex].Name,
					serializable.Variations[variationIndex].Text));
				this.lastIntUsedToCreateName++;
			}
		}

		public void PopulateSerializable(SnippetRandomSerializable serializable)
		{
			for (int variationIndex = 0; variationIndex < this.Variations.Count; variationIndex++)
			{
				VariationEditable variationEditable =
					this.Variations[variationIndex] as VariationEditable;
				serializable.Variations.Add(variationEditable.ToSerializable());
			}
		}

		public override string CalculateText()
		{
			int randomizedIndex =
				this.parentSceneEditable.SceneRandom.Next(this.Variations.Count);
			if (this.currentVariationIndex != randomizedIndex)
			{
				this.currentVariationIndex = randomizedIndex;
				this.notifyParentOfEdit();
			}

			return this.Variations[randomizedIndex].Text;
		}

		public string CalculateTextStable()
		{
			return this.Variations[this.currentVariationIndex].Text;
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
				for (int variationIndex = 0;
					variationIndex < this.Variations.Count;
					variationIndex++)
				{
					VariationEditable variationEditable =
						this.Variations[variationIndex] as VariationEditable;
					variationEditable.Text = pastedTextSplitByVariation[variationIndex];
				}
			}
		}

		public string GetVariationName(int variationIndex)
		{
			return this.Variations[variationIndex].Name;
		}

		public string GetVariationText(int variationIndex)
		{
			return this.Variations[variationIndex].Text;
		}

		public void SetVariationText(int variationIndex, string variationText)
		{
			VariationEditable variationEditable =
				this.Variations[variationIndex] as VariationEditable;

			if (variationText != variationEditable.Text)
			{
				variationEditable.Text = variationText;
				this.notifyParentOfEdit();
			}
		}

		public bool CanEditVariationCount()
		{
			return true;
		}

		public int GetVariationCount()
		{
			return this.Variations.Count;
		}

		public int GetCurrentVariationIndex()
		{
			return this.currentVariationIndex;
		}

		public void SetCurrentVariationIndex(int variationIndex)
		{
			if (this.currentVariationIndex != variationIndex)
			{
				this.currentVariationIndex = variationIndex;
				this.notifyParentOfEdit();
			}
		}

		public bool AddVariation()
		{
			VariationEditable newVariation = new VariationEditable(
				this.parentSceneEditable,
				getVariationNameForIndex(this.lastIntUsedToCreateName++),
				string.Empty);
			this.Variations.Add(newVariation);
			this.notifyParentOfEdit();
			return true;
		}

		public bool RemoveVariationEditable(int variationIndex)
		{
			this.Variations.RemoveAt(variationIndex);
			this.notifyParentOfEdit();
			return true;
		}

		private static string getVariationNameForIndex(int variationIndex)
		{
			return RANDOM_VARIATION_NAME_PREFIX + variationIndex;
		}

		private void notifyParentOfEdit()
		{
			if (this.parentSceneEditable != null)
			{
				this.parentSceneEditable.NotifyUIUpdateNeeded();
			}
		}
	}
}
