using DtgeCore.Serialization;

namespace DtgeCore.Editing;

/**
 * SnippetEditable is the editable version of the Snippt to be used in editors for authoring DTGE
 * games. For more details on how Snippets work and how they interact with their implementations,
 * see Snippet.cs. For more details on how Editables work, see SceneEditable.cs.
 * 
 * As for information specific to the editable portion of snippets, the different snippet modes
 * are meant to be used mostly interchangeably by shell editors so that the bulk of the complexity
 * can be encapsulated. Ideally, an editor can determine how to display and modify a snippet
 * purely by calling the various functions and never needs to check CurrentMode (though this isn't
 * enforced strictly by fully encapsulating CurrentMode).
 * 
 * As an example, an editor shouldn't need to check the snippet mode and have a switch statement
 * on that mode to determine whether the snippet's current mode needs tabs or the ability to add
 * and remove variations. Instead, it should call AlwaysHasOneVariation or CanEditVariationCount.
 * 
 * As a result, the SnippetEditable has a lot more logic than Snippet, as it needs to be able to
 * answer questions about capabilities that vary by mode and convert between modes on top of the
 * usual extra logic needed by Editables. It also notably provides a variation on CalculateText,
 * CalculateTextStable, which will maintain the current variation across calls. This is mostly
 * used so that scene preview text isn't constantly changing between variations as a user types
 * (this is mostly relevant for snippets that include randomization).
 * 
 * Another less than obvious feature of the SnippetEditable is the copy/paste functionality. To
 * aid authors in writing, DTGE provides the ability to copy the text all of the snippets and
 * their variations to the clipboard to be pasted into a word processor for things like spell
 * check, grammar help, and similar advanced features that the Editor isn't designed to support.
 * The copied text provides clear boundaries between variations, and so long as the boundaries
 * aren't messed with, authors can do final editing on their scene and then paste the results back
 * in to have those updates reflected in their scene.
 * 
 * For more details on how Editables work, see SceneEditable.cs.
 */
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
		:base(parentSceneEditable)
	{
		if (serializable == null)
		{
			CoreErrorHandler.InvokeInitializationError("A Snippet was initialized with a null serializable.");
		}
		else
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
				EditingErrorHandler.InvokeLoadError("A Snippet was initialized with an unknown Snippet Mode.");
				break;
			}
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
			EditingErrorHandler.InvokeSaveError("A Snippet was serialized while set to an unknown Snippet Mode.");
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
			EditingErrorHandler.InvokeIllegalOperationError("Snippet's CanChangeToMode was called with an unknown Snippet Mode.");
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
			EditingErrorHandler.InvokeIllegalOperationError("An attempt was made to change a snippet to a mode it couldn't be changed to (Call CanChangeToMode first). Reason: " + changeFailedMessage);
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
				EditingErrorHandler.InvokeIllegalOperationError("Snippet's ChangeToMode was called with an unknown Snippet Mode.");
				break;
			}
		}

		this.notifyParentOfEdit();

		return successFullyConverted;
	}

	public bool AlwaysHasOneVariation()
	{
		return this.CurrentMode == Snippet.Mode.Simple;
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
			EditingErrorHandler.InvokeEditingError("Snippet's CanEditVariationCount was called while the Snippet was set to an unknown Snippet Mode.");
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
		this.CurrentImplementationEditable.RestoreFromPastedText(pastedTextSplitByVariations);
		this.notifyParentOfEdit();
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
			if (this.parentSceneEditable.GetSubsceneCount() == 0)
			{
				this.ChangeModeTo(Mode.Simple);
			}
			else
			{
				SubsceneSnippetImplementationEditable subsceneImplementation =
					this.CurrentImplementationEditable as SubsceneSnippetImplementationEditable;
				subsceneImplementation.UpdateVariationsFromSubscenes();
			}
		}
	}

	public bool IsFirst()
	{
		return this == this.parentSceneEditable.GetSnippetByIndex(0);
	}

	public bool IsLast()
	{
		return this == this.parentSceneEditable.GetSnippetByIndex(
			this.parentSceneEditable.GetSnippetCount() - 1);
	}

	private void notifyParentOfEdit()
	{
		if (this.parentSceneEditable != null)
		{
			this.parentSceneEditable.NotifyUIUpdateNeeded();
		}
	}
}
