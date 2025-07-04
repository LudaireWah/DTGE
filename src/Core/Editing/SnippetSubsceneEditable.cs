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
			foreach (SUID subsceneId in this.VariationsBySubsceneId.Keys)
			{
				VariationEditable variationEditable =
					this.VariationsBySubsceneId[subsceneId] as VariationEditable;
				serializable.VariationsBySubsceneId[subsceneId] =
					variationEditable.ToSerializable();
			}
		}

		public static bool CanConvertFrom(
			SceneEditable parentSceneEditable,
			ISnippetEditableImplementation otherSnippetEditable,
			out string message)
		{
			bool canConvert = false;
			message = string.Empty;

			if (otherSnippetEditable.GetVariationCount()
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
			if (pastedTextSplitByVariation.Length != this.parentSceneEditable.GetSubsceneCount())
			{
				GlobalErrorHandler.InvokeError("There was a mismatch between subscene count and pasted variation count when restoring from pasted text.");
			}
			else
			{
				for (int subsceneIndex = 0; subsceneIndex < this.VariationsBySubsceneId.Count; subsceneIndex++)
				{
					Subscene subscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
					VariationEditable variationEditable =
						this.VariationsBySubsceneId[subscene.Id] as VariationEditable;
					variationEditable.Text = pastedTextSplitByVariation[subsceneIndex];
				}
			}

			this.notifyParentOfEdit();
		}

		public bool CanEditVariationCount()
		{
			return false;
		}

		public VariationEditable GetVariationEditable(int variationIndex)
		{
			return this.VariationsBySubsceneId[this.orderedSubceneSuids[variationIndex]]
				as VariationEditable;
		}

		public string GetVariationName(int variationIndex)
		{
			return this.VariationsBySubsceneId[this.orderedSubceneSuids[variationIndex]].Name;
		}

		public string GetVariationText(int variationIndex)
		{
			return this.VariationsBySubsceneId[this.orderedSubceneSuids[variationIndex]].Text;
		}

		public void SetVariationText(int variationIndex, string variationText)
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
			GlobalErrorHandler.InvokeError("An attempt was made to add a new variation to a subscene snippet.");

			return false;
		}

		public bool RemoveVariationEditable(int variationIndex)
		{
			GlobalErrorHandler.InvokeError("An attempt was made to remove a variation from a subscene snippet.");

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
