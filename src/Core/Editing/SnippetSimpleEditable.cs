using DtgeCore.Serialization;

namespace DtgeCore.Editing;

/**
 * This file contains the editable implementation of a Simple Snippet.
 * 
 * For additional information on Snippets and their implementations, see SnippetEditable.cs and
 * Snippet.cs. For more information on Editables in general, see SceneEditable.cs.
 */
public partial class SnippetEditable
{
	private class SimpleSnippetImplementationEditable
		: SimpleSnippetImplementation, ISnippetEditableImplementation
	{
		private const string SIMPLE_SINGLE_VARIATION_NAME = "(Simple)";

		private SceneEditable parentSceneEditable;

		public SimpleSnippetImplementationEditable(SceneEditable parentSceneEditable)
			: base(parentSceneEditable)
		{
			this.parentSceneEditable = parentSceneEditable;

			this.SingleVariation = new VariationEditable(
				parentSceneEditable,
				SIMPLE_SINGLE_VARIATION_NAME,
				string.Empty);
		}

		public SimpleSnippetImplementationEditable(
			SceneEditable parentSceneEditable,
			ISnippetEditableImplementation other)
			: base(parentSceneEditable)
		{
			this.parentSceneEditable = parentSceneEditable;

			if (other == null)
			{
				EditingErrorHandler.InvokeEditingError("A Simple Snippet's Implementation was constructed from a null other implementation.");
			}
			else
			{
				this.SingleVariation = new VariationEditable(
					parentSceneEditable,
					SIMPLE_SINGLE_VARIATION_NAME,
					other.GetVariationText(0));
			}
		}

		public SimpleSnippetImplementationEditable(
			SceneEditable parentSceneEditable,
			SnippetSimpleSerializable serializable)
			: base(parentSceneEditable)
		{
			this.parentSceneEditable = parentSceneEditable;

			this.SingleVariation = new VariationEditable(
				parentSceneEditable,
				SIMPLE_SINGLE_VARIATION_NAME,
				serializable.SingleVariation.Text);
		}

		public void PopulateSerializable(SnippetSimpleSerializable serializable)
		{
			if (serializable == null)
			{
				EditingErrorHandler.InvokeSaveError("SnippetSimpleEditable's PopulateSerializable function received a null serializable.");
			}
			else
			{
				serializable.SingleVariation.Name = this.SingleVariation.Name;
				serializable.SingleVariation.Text = this.SingleVariation.Text;
			}
		}

		public static bool CanConvertFrom(
			ISnippetEditableImplementation otherSnippetEditable,
			out string message)
		{
			bool canConvert = false;
			message = string.Empty;

			if (otherSnippetEditable == null)
			{
				EditingErrorHandler.InvokeEditingError("SnippetSimpleEditable's CanConvertFrom function received a null other snippet implementation.");
			}
			else if (otherSnippetEditable.GetVariationCount() != 1)
			{
				message = "To switch to simple mode you must have exactly one variation.";
			}
			else
			{
				canConvert = true;
			}

			return canConvert;
		}

		public string CalculateTextStable()
		{
			return this.CalculateText();
		}

		public string GetCopyableText(string variationBoundaryMarker)
		{
			return this.SingleVariation.Text;
		}

		public void RestoreFromPastedText(string[] pastedTextSplitByVariation)
		{
			if (pastedTextSplitByVariation == null)
			{
				EditingErrorHandler.InvokeEditingError("A simple Snippet's RestoreFromPastedText function received a null pasted strings array.");
			}
			else if (pastedTextSplitByVariation.Length != 0)
			{
				EditingErrorHandler.InvokeEditingError("A Simple snippet received a pasted text array whose length was not exactly one.");
			}
			else
			{
				VariationEditable singleVariationEditable = this.SingleVariation as VariationEditable;
				singleVariationEditable.Text = pastedTextSplitByVariation[0];
				this.notifyParentOfEdit();
			}
		}

		public string GetVariationName(int variationIndex)
		{
			string variationName = null;

			if (variationIndex != 0)
			{
				EditingErrorHandler.InvokeIllegalOperationError("A Simple Snippet's GetVariationName was called with a non-zero index.");
			}
			else
			{
				variationName = this.SingleVariation.Name;
			}

			return variationName;
		}

		public string GetVariationText(int variationIndex)
		{
			string variationText = null;

			if (variationIndex != 0)
			{
				EditingErrorHandler.InvokeIllegalOperationError("A Simple Snippet's GetVariationText was called with a non-zero index.");
			}
			else
			{
				variationText = this.SingleVariation.Text;
			}
			
			return variationText;
		}

		public void SetVariationText(int variationIndex, string variationText)
		{
			if (variationIndex != 0)
			{
				EditingErrorHandler.InvokeIllegalOperationError("A Simple Snippet's SetVariationText was called with a non-zero index.");
			}
			else
			{
				VariationEditable variationEditable = this.SingleVariation as VariationEditable;
				if (variationEditable.Text != variationText)
				{
					variationEditable.Text = variationText;
					this.notifyParentOfEdit();
				}
			}
		}

		public bool CanEditVariationCount()
		{
			return false;
		}

		public int GetVariationCount()
		{
			return 1;
		}

		public int GetCurrentVariationIndex()
		{
			return 0;
		}

		public void SetCurrentVariationIndex(int variationIndex)
		{
			if (variationIndex != 0)
			{
				EditingErrorHandler.InvokeIllegalOperationError("A Simple Snippet's SetCurrentVariationIndex was called with a non-zero index.");
			}
		}

		public bool AddVariation()
		{
			EditingErrorHandler.InvokeIllegalOperationError("A Simple Snippet's AddVariation was called, which should never be called (check CanEditVariationCount).");
			return false;
		}

		public bool RemoveVariationEditable(int variationIndex)
		{
			EditingErrorHandler.InvokeIllegalOperationError("A Simple Snippet's RemoveVariationEditable was called, which should never be called (check CanEditVariationCount).");
			return false;
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
