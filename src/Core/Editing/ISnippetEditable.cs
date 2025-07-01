using System;

using DtgeCore.Serialization;

namespace DtgeCore.Editing;

public static class SnippetConverter
{
	public static bool CanConvertSnippetToMode(
		SceneEditable parentSceneEditable,
		ISnippetEditable snippetEditable,
		Snippet.Mode mode,
		out string message)
	{
		bool canConvert = false;
		message = string.Empty;

		switch (mode)
		{
		case Snippet.Mode.Simple:
			canConvert = SnippetSimpleEditable.CanConvertFrom(snippetEditable, out message);
			break;
		case Snippet.Mode.Subscene:
			canConvert = SnippetSubsceneEditable.CanConvertFrom(parentSceneEditable, snippetEditable, out message);
			break;
		case Snippet.Mode.Random:
			canConvert = true;
			break;
		default:
			GlobalErrorHandler.InvokeError("An unknown snippet mode was encountered.");
			break;
		}

		return canConvert;
	}

	public static ISnippetEditable ConvertSnippetToNewMode(
		SceneEditable parentSceneEditable,
		ISnippetEditable snippetEditable,
		Snippet.Mode mode)
	{
		ISnippetEditable convertedSnippet = null;

		switch (mode)
		{
		case Snippet.Mode.Simple:
			convertedSnippet = new SnippetSimpleEditable(parentSceneEditable, snippetEditable);
			break;
		case Snippet.Mode.Subscene:
			convertedSnippet = new SnippetSubsceneEditable(parentSceneEditable, snippetEditable);
			break;
		case Snippet.Mode.Random:
			convertedSnippet = new SnippetRandomEditable(parentSceneEditable, snippetEditable);
			break;
		default:
			GlobalErrorHandler.InvokeError("An unknown snippet mode was encountered.");
			break;
		}

		return convertedSnippet;
	}
}

public interface ISnippetEditable : ISnippet
{
	public SnippetSerializable ToSerializable();
	public string CalculateTextStable();
	public string GetCopyableText(string variationBoundaryMarker);
	public void RestoreFromPastedText(string[] pastedTextSplitByVariation);
	public VariationEditable GetVariationEditable(int variationIndex);
	public bool CanEditVariationCount();
	public int GetVariationCount();
	public bool AddVariation();
	public bool RemoveVariationEditable(int variationIndex);
}
