using DtgeCore.Serialization;

namespace DtgeCore.Editing;

public class SnippetSimpleEditable : SnippetSimple, ISnippetEditable
{
	private SceneEditable parentSceneEditable;

	public SnippetSimpleEditable(SceneEditable parentSceneEditable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;

		this.SingleVariation = new VariationEditable(
			parentSceneEditable,
			"(Simple)",
			string.Empty);
	}

	public SnippetSimpleEditable(SceneEditable parentSceneEditable, ISnippetEditable other)
		:base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;

		VariationEditable othersFirstVariation = other.GetVariationEditable(0);
		this.SingleVariation = new VariationEditable(
			parentSceneEditable,
			othersFirstVariation.Name,
			othersFirstVariation.Text);
	}

	public SnippetSimpleEditable(
		SceneEditable parentSceneEditable,
		SnippetSimpleSerializable serializable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;

		this.SingleVariation = new VariationEditable(
			parentSceneEditable,
			serializable.SingleVariation.Name,
			serializable.SingleVariation.Text);

	}

	public SnippetSerializable ToSerializable()
	{
		SnippetSimpleSerializable serializable =
			this.CreateSerializable<SnippetSimpleSerializable>();

		serializable.Mode = this.Mode;
		serializable.SingleVariation.Name = this.SingleVariation.Name;
		serializable.SingleVariation.Text = this.SingleVariation.Text;

		return serializable;
	}

	public static bool CanConvertFrom(ISnippetEditable otherSnippetEditable, out string message)
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
	}

	public VariationEditable GetVariationEditable(int variationIndex)
	{
		return this.SingleVariation as VariationEditable;
	}

	public bool CanEditVariationCount()
	{
		return false;
	}

	public int GetVariationCount()
	{
		return 1;
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
}
