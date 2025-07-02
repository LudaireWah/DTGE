using DtgeCore.Serialization;

namespace DtgeCore.Editing;

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

			this.SingleVariation = new VariationEditable(
				parentSceneEditable,
				SIMPLE_SINGLE_VARIATION_NAME,
				other.GetVariationText(0));
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
			serializable.SingleVariation.Name = this.SingleVariation.Name;
			serializable.SingleVariation.Text = this.SingleVariation.Text;
		}

		public static bool CanConvertFrom(
			ISnippetEditableImplementation otherSnippetEditable,
			out string message)
		{
			bool canConvert = false;
			message = string.Empty;

			if (otherSnippetEditable.GetVariationCount() == 1)
			{
				canConvert = true;
			}
			else
			{
				message = "To switch to simple mode you must have exactly one variation.";
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
			VariationEditable singleVariationEditable = this.SingleVariation as VariationEditable;
			singleVariationEditable.Text = pastedTextSplitByVariation[0];
			this.notifyParentOfEdit();
		}

		public VariationEditable GetVariationEditable(int variationIndex)
		{
			return this.SingleVariation as VariationEditable;
		}

		public string GetVariationName(int variationIndex)
		{
			return this.SingleVariation.Name;
		}

		public string GetVariationText(int variationIndex)
		{
			return this.SingleVariation.Text;
		}

		public void SetVariationText(int variationIndex, string variationText)
		{
			VariationEditable variationEditable = this.SingleVariation as VariationEditable;
			if (variationEditable.Text != variationText)
			{
				variationEditable.Text = variationText;
				this.notifyParentOfEdit();
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
			GlobalErrorHandler.InvokeErrorIf(variationIndex != 0, "An attempt was made to set the CurrentVariationIndex of a simple snippet to something other than 0.");
		}

		public bool AddVariation()
		{
			GlobalErrorHandler.InvokeError("An attempt was made to add a new variation to a simple snippet.");

			return false;
		}

		public bool RemoveVariationEditable(int variationIndex)
		{
			GlobalErrorHandler.InvokeError("An attempt was made to remove a variation from a simple snippet.");

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
