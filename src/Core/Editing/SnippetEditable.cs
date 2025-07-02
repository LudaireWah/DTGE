using DtgeCore.Serialization;

namespace DtgeCore.Editing;

public partial class SnippetEditable : Snippet
{
	private interface ISnippetEditableImplementation : ISnippetImplementation
	{
		public string CalculateTextStable();
		public string GetCopyableText(string variationBoundaryMarker);
		public void RestoreFromPastedText(string[] pastedTextSplitByVariation);
		public string GetVariationName(int variationIndex);
		public string GetVariationText(int variationIndex);
		public void SetVariationText(int variationIndex, string variationText);
		public int GetVariationCount();
		public int GetCurrentVariationIndex();
		public void SetCurrentVariationIndex(int variationIndex);
		public bool AddVariation();
		public bool RemoveVariationEditable(int variationIndex);
	}

	private SceneEditable parentSceneEditable;

	private ISnippetEditableImplementation CurrentImplementationEditable
	{
		get
		{
			return base.currentImplementation as ISnippetEditableImplementation;
		}
		set
		{
			base.currentImplementation = value;
		}
	}

	public SnippetEditable(SceneEditable parentSceneEditable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;
		this.currentImplementation = new SimpleSnippetImplementationEditable(parentSceneEditable);
	}

	public SnippetEditable(SceneEditable parentSceneEditable, SnippetSerializable serializable)
		:base(parentSceneEditable, serializable)
	{
		this.parentSceneEditable = parentSceneEditable;

		switch (serializable.Mode)
		{
		case Mode.Simple:
			SnippetSimpleSerializable simpleSerializable =
				serializable as SnippetSimpleSerializable;
			this.CurrentImplementationEditable =
				new SimpleSnippetImplementationEditable(parentSceneEditable, simpleSerializable);
			break;
		case Mode.Subscene:
			SnippetSubsceneSerializable subsceneSerializable =
				serializable as SnippetSubsceneSerializable;
			this.CurrentImplementationEditable =
				new SubsceneSnippetImplementationEditable(
					parentSceneEditable,
					subsceneSerializable);
			break;
		case Mode.Random:
			SnippetRandomSerializable randomSerializable =
				serializable as SnippetRandomSerializable;
			this.CurrentImplementationEditable =
				new RandomSnippetImplementationEditable(parentSceneEditable, randomSerializable);
			break;
		default:
			GlobalErrorHandler.InvokeError("An unknown snippet mode was encountered.");
			break;
		}
	}

	public SnippetSerializable ToSerializable()
	{
		SnippetSerializable serializable = null;

		switch (this.CurrentMode)
		{
		case Mode.Simple:
			SnippetSimpleSerializable simpleSerializable =
				this.CreateSerializable<SnippetSimpleSerializable>();
			SimpleSnippetImplementationEditable simpleImplementation =
				this.CurrentImplementationEditable as SimpleSnippetImplementationEditable;
			simpleImplementation.PopulateSerializable(simpleSerializable);
			serializable = simpleSerializable;
			break;
		case Mode.Subscene:
			SnippetSubsceneSerializable subsceneSerializable =
				this.CreateSerializable<SnippetSubsceneSerializable>();
			SubsceneSnippetImplementationEditable subsceneImplementation =
				this.CurrentImplementationEditable as SubsceneSnippetImplementationEditable;
			subsceneImplementation.PopulateSerializable(subsceneSerializable);
			serializable = subsceneSerializable;
			break;
		case Mode.Random:
			SnippetRandomSerializable randomSerializable =
				this.CreateSerializable<SnippetRandomSerializable>();
			RandomSnippetImplementationEditable randomImplementation =
				this.CurrentImplementationEditable as RandomSnippetImplementationEditable;
			randomImplementation.PopulateSerializable(randomSerializable);
			serializable = randomSerializable;
			break;
		default:
			GlobalErrorHandler.InvokeError("An unknown snippet mode was encountered.");
			break;
		}

		return serializable;
	}

	public bool CanChangeToMode(
		Snippet.Mode mode,
		out string message)
	{
		bool canConvert = false;
		message = string.Empty;

		switch (mode)
		{
		case Snippet.Mode.Simple:
			canConvert = SimpleSnippetImplementationEditable.CanConvertFrom(
				this.CurrentImplementationEditable,
				out message);
			break;
		case Snippet.Mode.Subscene:
			canConvert = SubsceneSnippetImplementationEditable.CanConvertFrom(
				this.parentSceneEditable,
				this.CurrentImplementationEditable,
				out message);
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

	public bool ChangeModeTo(Snippet.Mode mode)
	{
		bool successFullyConverted = false;
		string changeFailedMessage = string.Empty;

		if (!this.CanChangeToMode(mode, out changeFailedMessage))
		{
			GlobalErrorHandler.InvokeError("An attempt was made to change a snippet to a mode it couldn't be changed to. " + changeFailedMessage);
		}
		else
		{
			successFullyConverted = true;

			switch (mode)
			{
			case Snippet.Mode.Simple:
				this.currentImplementation =
					new SimpleSnippetImplementationEditable(
						this.parentSceneEditable,
						this.CurrentImplementationEditable);
				break;
			case Snippet.Mode.Subscene:
				this.currentImplementation =
					new SubsceneSnippetImplementationEditable(
						this.parentSceneEditable,
						this.CurrentImplementationEditable);
				break;
			case Snippet.Mode.Random:
				this.currentImplementation =
					new RandomSnippetImplementationEditable(
						this.parentSceneEditable,
						this.CurrentImplementationEditable);
				break;
			default:
				GlobalErrorHandler.InvokeError("An unknown snippet mode was encountered.");
				break;
			}
		}

		this.notifyParentOfEdit();

		return successFullyConverted;
	}

	public bool CanEditVariationCount()
	{
		bool canEditVariationCount = false;

		switch (this.CurrentMode)
		{
		case Mode.Simple:
			canEditVariationCount = false;
			break;
		case Mode.Subscene:
			canEditVariationCount = false;
			break;
		case Mode.Random:
			canEditVariationCount = true;
			break;
		default:
			GlobalErrorHandler.InvokeError("An unknown snippet mode was encountered.");
			break;
		}

		return canEditVariationCount;
	}

	public int GetCurrentVariationIndex()
	{
		return this.CurrentImplementationEditable.GetCurrentVariationIndex();
	}

	public void SetCurrentVariationIndex(int variationIndex)
	{
		this.CurrentImplementationEditable.SetCurrentVariationIndex(variationIndex);
	}

	public override string CalculateText()
	{
		return this.CurrentImplementationEditable.CalculateText();
	}

	public string CalculateTextStable()
	{
		return this.CurrentImplementationEditable.CalculateTextStable();
	}

	public string GetCopyableText(string variationBoundaryMarker)
	{
		return this.CurrentImplementationEditable.GetCopyableText(variationBoundaryMarker);
	}

	public void RestoreFromPastedText(string[] pastedTextSplitByVariations)
	{
		this.notifyParentOfEdit();
		this.CurrentImplementationEditable.RestoreFromPastedText(pastedTextSplitByVariations);
	}

	public string GetVariationName(int variationIndex)
	{
		return this.CurrentImplementationEditable.GetVariationName(variationIndex);
	}

	public string GetVariationText(int variationIndex)
	{
		return this.CurrentImplementationEditable.GetVariationText(variationIndex);
	}

	public void SetVariationText(int variationIndex, string variationText)
	{
		this.CurrentImplementationEditable.SetVariationText(variationIndex, variationText);
	}

	public int GetVariationCount()
	{
		return this.CurrentImplementationEditable.GetVariationCount();
	}

	public bool AddVariation()
	{
		bool success = this.CurrentImplementationEditable.AddVariation();
		this.notifyParentOfEdit();
		return success;
	}

	public bool RemoveVariationEditable(int variationIndex)
	{
		bool success = this.CurrentImplementationEditable.RemoveVariationEditable(variationIndex);
		this.notifyParentOfEdit();
		return success;
	}

	public void NotifyOfSubsceneChange()
	{
		if (this.CurrentMode == Mode.Subscene)
		{
			SubsceneSnippetImplementationEditable subsceneImplementation =
				this.CurrentImplementationEditable as SubsceneSnippetImplementationEditable;
			subsceneImplementation.UpdateVariationsFromSubscenes();
		}
	}

	private void notifyParentOfEdit()
	{
		if (this.parentSceneEditable != null)
		{
			this.parentSceneEditable.NotifyUIUpdateNeeded();
		}
	}
}
