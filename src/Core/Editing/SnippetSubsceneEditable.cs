using System.Collections.Generic;

using DtgeCore.Serialization;

namespace DtgeCore.Editing;

/**
 * This file contains the editable implementation of a Subscene Snippet, which provides a
 * variation for each subscene and calculates text based on the scene's current subscene. Notable
 * functionality includes changing the parent SceneEditable's current subscene when this
 * SnippetEditable's current subscene is displayed so that the current subscene being worked on
 * can be synced across all snippets and the preview text.
 * 
 * For additional information on Snippets and their implementations, see SnippetEditable.cs and
 * Snippet.cs. For more information on Editables in general, see SceneEditable.cs.
 */
public partial class SnippetEditable
{
	private class SubsceneSnippetImplementationEditable
		: SubsceneSnippetImplementation, ISnippetEditableImplementation
	{
		private SceneEditable parentSceneEditable;

		private List<SUID> orderedSubceneSuids;

		public SubsceneSnippetImplementationEditable(SceneEditable parentSceneEditable)
			: base(parentSceneEditable)
		{
			this.parentSceneEditable = parentSceneEditable;
			this.orderedSubceneSuids = new List<SUID>();
		}

		public SubsceneSnippetImplementationEditable(
			SceneEditable parentSceneEditable,
			ISnippetEditableImplementation other)
			: base(parentSceneEditable)
		{
			this.parentSceneEditable = parentSceneEditable;

			if (other == null)
			{
				EditingErrorHandler.InvokeEditingError("A Subscene Snippet's Implementation was constructed from a null other implementation.");
			}
			else
			{
				this.orderedSubceneSuids = new List<SUID>();

				for (int subsceneIndex = 0;
					subsceneIndex < this.parentSceneEditable.GetSubsceneCount();
					subsceneIndex++)
				{
					Subscene subscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
					this.orderedSubceneSuids.Add(subscene.Id);
					if (subsceneIndex < other.GetVariationCount())
					{
						this.VariationsBySubsceneId[subscene.Id] = new VariationEditable(
							parentSceneEditable,
							subscene.Name,
							other.GetVariationText(subsceneIndex));
					}
					else
					{
						this.VariationsBySubsceneId[subscene.Id] =
							new VariationEditable(parentSceneEditable, subscene.Name, string.Empty);
					}
				}
			}
		}

		public SubsceneSnippetImplementationEditable(
			SceneEditable parentSceneEditable,
			SnippetSubsceneSerializable serializable)
			: base(parentSceneEditable)
		{
			this.parentSceneEditable = parentSceneEditable;
			this.orderedSubceneSuids = new List<SUID>();

			for (int subsceneIndex = 0;
				subsceneIndex < this.parentSceneEditable.GetSubsceneCount();
				subsceneIndex++)
			{
				SubsceneEditable subsceneEditable =
					this.parentSceneEditable.GetSubscene(subsceneIndex);
				this.orderedSubceneSuids.Add(subsceneEditable.Id);
				this.VariationsBySubsceneId[subsceneEditable.Id] = new VariationEditable(
					parentSceneEditable,
					serializable.VariationsBySubsceneId[subsceneEditable.Id]);
			}
		}

		public void PopulateSerializable(SnippetSubsceneSerializable serializable)
		{
			if (serializable == null)
			{
				EditingErrorHandler.InvokeSaveError("SnippetSubsceneEditable's PopulateSerializable function received a null serializable.");
			}
			else
			{
				foreach (SUID subsceneId in this.VariationsBySubsceneId.Keys)
				{
					VariationEditable variationEditable =
						this.VariationsBySubsceneId[subsceneId] as VariationEditable;
					serializable.VariationsBySubsceneId[subsceneId] =
						variationEditable.ToSerializable();
				}
			}
		}

		public static bool CanConvertFrom(
			SceneEditable parentSceneEditable,
			ISnippetEditableImplementation otherSnippetEditable,
			out string message)
		{
			bool canConvert = false;
			message = string.Empty;

			if (otherSnippetEditable == null)
			{
				EditingErrorHandler.InvokeEditingError("SnippetSubsceneEditable's CanConvertFrom function received a null other snippet implementation.");
			}
			else if (otherSnippetEditable.GetVariationCount()
				<= parentSceneEditable.GetSubsceneCount())
			{
				canConvert = true;
			}
			else if (parentSceneEditable.GetSubsceneCount() == 0)
			{
				message = "To switch to subscene mode, you have to add at least one subscene to the scene.";
			}
			else
			{
				message = "To switch to subscene mode, delete variations manually until you have no more than the number of subscenes.";
			}

			return canConvert;
		}

		public string CalculateTextStable()
		{
			return this.CalculateText();
		}

		public string GetCopyableText(string variationBoundaryMarker)
		{
			string copyableVariationText = "";

			for (int subsceneIndex = 0; subsceneIndex < this.VariationsBySubsceneId.Count; subsceneIndex++)
			{
				Subscene subscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
				copyableVariationText += this.VariationsBySubsceneId[subscene.Id].Text;
				if (subsceneIndex < this.parentSceneEditable.GetSubsceneCount() - 1)
				{
					copyableVariationText += variationBoundaryMarker;
				}
			}

			return copyableVariationText;
		}

		public void RestoreFromPastedText(string[] pastedTextSplitByVariation)
		{
			if (pastedTextSplitByVariation == null)
			{
				EditingErrorHandler.InvokeEditingError("A Subscene Snippet's RestoreFromPastedText function received a null pasted strings array.");
			}
			else if (pastedTextSplitByVariation.Length != this.parentSceneEditable.GetSubsceneCount())
			{
				EditingErrorHandler.InvokeEditingError("A Subscene snippet received a pasted text array whose length did not match the number of subscenes.");
			}
			else
			{
				for (int subsceneIndex = 0; subsceneIndex < this.VariationsBySubsceneId.Count; subsceneIndex++)
				{
					Subscene subscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
					VariationEditable variationEditable =
						this.VariationsBySubsceneId[subscene.Id] as VariationEditable;

					if (variationEditable == null)
					{
						EditingErrorHandler.InvokeEditingError("A Subscene Snippet's RestoreFromPastedText function encountered a null variation.");
					}
					else
					{
						variationEditable.Text = pastedTextSplitByVariation[subsceneIndex];
					}
				}

				this.notifyParentOfEdit();
			}
		}

		public bool CanEditVariationCount()
		{
			return false;
		}

		public string GetVariationName(int variationIndex)
		{
			string variationName = null;

			if (!CoreErrorHandler.IsValidIndex(variationIndex, this.parentSceneEditable.GetSubsceneCount()))
			{
				EditingErrorHandler.InvokeIllegalOperationError("A Subscene Snippet's GetVariationName was called with an invalid index. The index was " + variationIndex + ". The subscene count was " + this.parentSceneEditable.GetSubsceneCount() + ".");
			}
			else
			{
				variationName =
					this.VariationsBySubsceneId[this.orderedSubceneSuids[variationIndex]].Name;
			}

			return variationName;
		}

		public string GetVariationText(int variationIndex)
		{
			string variationText = null;

			if (!CoreErrorHandler.IsValidIndex(variationIndex, this.parentSceneEditable.GetSubsceneCount()))
			{
				EditingErrorHandler.InvokeIllegalOperationError("A Subscene Snippet's GetVariationText was called with an invalid index. The index was " + variationIndex + ". The subscene count was " + this.parentSceneEditable.GetSubsceneCount() + ".");
			}
			else
			{
				variationText =
					this.VariationsBySubsceneId[this.orderedSubceneSuids[variationIndex]].Text;
			}

			return variationText;
		}

		public void SetVariationText(int variationIndex, string variationText)
		{

			if (!CoreErrorHandler.IsValidIndex(variationIndex, this.parentSceneEditable.GetSubsceneCount()))
			{
				EditingErrorHandler.InvokeIllegalOperationError("A Subscene Snippet's SetVariationText was called with an invalid index. The index was " + variationIndex + ". The subscene count was " + this.parentSceneEditable.GetSubsceneCount() + ".");
			}
			else
			{
				VariationEditable variationEditable =
				this.VariationsBySubsceneId[this.orderedSubceneSuids[variationIndex]]
				as VariationEditable;
				if (variationEditable.Text != variationText)
				{
					variationEditable.Text = variationText;
					this.notifyParentOfEdit();
				}
			}
		}

		public int GetVariationCount()
		{
			return this.orderedSubceneSuids.Count;
		}

		public int GetCurrentVariationIndex()
		{
			return this.parentSceneEditable.CurrentSubsceneIndex;
		}

		public void SetCurrentVariationIndex(int variationIndex)
		{
			if (this.parentSceneEditable.CurrentSubsceneIndex != variationIndex)
			{
				this.parentSceneEditable.CurrentSubsceneIndex = variationIndex;
				this.parentSceneEditable.NotifyUIUpdateNeeded();
			}
		}

		public bool AddVariation()
		{
			EditingErrorHandler.InvokeIllegalOperationError("A Subscene Snippet's AddVariation was called, which should never be called (check CanEditVariationCount).");
			return false;
		}

		public bool RemoveVariationEditable(int variationIndex)
		{
			EditingErrorHandler.InvokeIllegalOperationError("A Subscene Snippet's RemoveVariationEditable was called, which should never be called (check CanEditVariationCount).");
			return false;
		}

		public void UpdateVariationsFromSubscenes()
		{
			this.orderedSubceneSuids.Clear();

			for (int subsceneIndex = 0;
				subsceneIndex < this.parentSceneEditable.GetSubsceneCount();
				subsceneIndex++)
			{
				SubsceneEditable subsceneEditable =
					this.parentSceneEditable.GetSubscene(subsceneIndex);
				this.orderedSubceneSuids.Add(subsceneEditable.Id);
				if (!this.VariationsBySubsceneId.ContainsKey(subsceneEditable.Id))
				{
					this.VariationsBySubsceneId[subsceneEditable.Id] = new VariationEditable(
						this.parentSceneEditable,
						subsceneEditable.Name,
						string.Empty);
				}
				else
				{
					VariationEditable variationEditable =
						this.VariationsBySubsceneId[subsceneEditable.Id] as VariationEditable;
					variationEditable.Name = subsceneEditable.Name;
				}
			}

			if (this.VariationsBySubsceneId.Count > this.parentSceneEditable.GetSubsceneCount())
			{
				foreach (SUID subsceneId in this.VariationsBySubsceneId.Keys)
				{
					if (!this.parentSceneEditable.HasMatchingSubscene(subsceneId))
					{
						this.VariationsBySubsceneId.Remove(subsceneId);
					}
				}
			}

			this.parentSceneEditable.NotifyUIUpdateNeeded();
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
