using DtgeCore.Serialization;

namespace DtgeCore.Editing;


/**
 * This file contains the editable implementation of a Random Snippet. Notable functionality for
 * this implementation is that the current index can be set independent of randomization, and it
 * only re-randomizes when CalculateText is called instead of CalculateTextStable.
 * 
 * For additional information on Snippets and their implementations, see SnippetEditable.cs and
 * Snippet.cs. For more information on Editables in general, see SceneEditable.cs.
 */
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

			if (other == null)
			{
				EditingErrorHandler.InvokeEditingError("A Random Snippet's Implementation was constructed from a null other implementation.");
			}
			else
			{
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
			if (serializable == null)
			{
				EditingErrorHandler.InvokeSaveError("SnippetRandomEditable's PopulateSerializable function received a null serializable.");
			}
			else
			{
				for (int variationIndex = 0; variationIndex < this.Variations.Count; variationIndex++)
				{
					VariationEditable variationEditable =
						this.Variations[variationIndex] as VariationEditable;
					if (variationEditable == null)
					{
						EditingErrorHandler.InvokeSaveError("A Random Snippet encountered a null variation at variation index + " + variationIndex + " while saving.");
					}
					else
					{
						serializable.Variations.Add(variationEditable.ToSerializable());
					}
				}
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
				VariationEditable variationEditable =
					this.Variations[variationIndex] as VariationEditable;
				
				if (variationEditable == null)
				{
					EditingErrorHandler.InvokeEditingError("A Random Snippet's GetCopyableText encountered a null variation at variation index + " + variationIndex + ".");
				}
				else
				{
					copyableVariationText += variationEditable.Text;

					if (variationIndex < this.parentSceneEditable.GetSubsceneCount() - 1)
					{
						copyableVariationText += variationBoundaryMarker;
					}
				}
			}

			return copyableVariationText;
		}

		public void RestoreFromPastedText(string[] pastedTextSplitByVariation)
		{
			if (pastedTextSplitByVariation == null)
			{
				EditingErrorHandler.InvokeEditingError("A Random Snippet's RestoreFromPastedText function received a null pasted strings array.");
			}
			else if (pastedTextSplitByVariation.Length != this.Variations.Count)
			{
				EditingErrorHandler.InvokeEditingError("A Random snippet received a pasted text array whose length did not match the number of variations.");
			}
			else
			{
				for (int variationIndex = 0;
					variationIndex < this.Variations.Count;
					variationIndex++)
				{
					VariationEditable variationEditable =
						this.Variations[variationIndex] as VariationEditable;
					
					if (variationEditable == null)
					{
						EditingErrorHandler.InvokeEditingError("A Random Snippet's RestoreFromPastedText function encountered a null variation.");
					}
					else
					{
						variationEditable.Text = pastedTextSplitByVariation[variationIndex];
					}
				}

				this.notifyParentOfEdit();
			}
		}

		public string GetVariationName(int variationIndex)
		{
			string variationName = null;

			if (!CoreErrorHandler.IsValidIndex(variationIndex, this.Variations.Count))
			{
				EditingErrorHandler.InvokeIllegalOperationError("A Random Snippet's GetVariationName was called with an invalid index. The index was " + variationIndex + ". The variation count was " + this.Variations.Count + ".");
			}
			else
			{
				variationName = this.Variations[variationIndex].Name;
			}

			return variationName;
		}

		public string GetVariationText(int variationIndex)
		{
			string variationText = null;

			if (!CoreErrorHandler.IsValidIndex(variationIndex, this.Variations.Count))
			{
				EditingErrorHandler.InvokeIllegalOperationError("Snippet's GetVariationText was called with an invalid index. The index was " + variationIndex + ". The variation count was " + this.Variations.Count + ".");
			}
			else
			{
				variationText = this.Variations[variationIndex].Text;
			}

			return variationText;
		}

		public void SetVariationText(int variationIndex, string variationText)
		{
			if (!CoreErrorHandler.IsValidIndex(variationIndex, this.Variations.Count))
			{
				EditingErrorHandler.InvokeIllegalOperationError("Snippet's SetVariationText was called with an invalid index. The index was " + variationIndex + ". The variation count was " + this.Variations.Count + ".");
			}
			else
			{
				VariationEditable variationEditable =
					this.Variations[variationIndex] as VariationEditable;

				if (variationText != variationEditable.Text)
				{
					variationEditable.Text = variationText;
					this.notifyParentOfEdit();
				}
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
			if (!CoreErrorHandler.IsValidIndex(variationIndex, this.Variations.Count))
			{
				EditingErrorHandler.InvokeIllegalOperationError("Snippet's SetCurrentVariationIndex was called with an invalid index. The index was " + variationIndex + ". The variation count was " + this.Variations.Count + ".");
			}
			else if (this.currentVariationIndex != variationIndex)
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
			bool successfullyRemoved = false;

			if (!CoreErrorHandler.IsValidIndex(variationIndex, this.Variations.Count))
			{
				EditingErrorHandler.InvokeIllegalOperationError("Snippet's RemoveVariationEditable was called with an invalid index. The index was " + variationIndex + ". The variation count was " + this.Variations.Count + ".");
			}
			else
			{
				this.Variations.RemoveAt(variationIndex);
				successfullyRemoved = true;
				this.notifyParentOfEdit();
			}

			return successfullyRemoved;
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
