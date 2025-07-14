using System;

using DtgeCore.Serialization;

namespace DtgeCore.Editing;

/**
 * SceneEditable is the editable version of the Scene to be used in editors for authoring DTGE
 * games. It provides editing functionality, access to editable versions of the Scene's components
 * such as Options, and tracks when changes have been made to the scene. This allows editors to
 * distribute the specific pieces of the scene through their code but still be able to see if
 * edits have been made in a single place.
 * 
 * "Editables" refers to all the classes in DtgeCore.Editing which are extensions of classes in
 * DtgeCore that provide editing capabilities. Editables are responsible for calling
 * NotifyUIUpdateNeeded when changes have been made requiring a UI update. All Editables should
 * also be capable of being constructed from or saving themselves to a matching Serializable for
 * saving and loading scene files.
 */
public class SceneEditable : Scene
{
	private const string COPYPASTE_SNIPPET_BOUNDARY_MARKER =
		">>>\r\n[DTGESnippetBoundary]\r\n<<<";
	private const string COPYPASTE_VARIATION_BOUNDARY_MARKER =
		">>>\r\n[DTGEVariationBoundary]\r\n<<<";

	public const string NULL_SUBSCENE_NAME = "(None)";

	public new string Name
	{
		get { return base.Name; }
		set
		{
			if (!UserDefinedNameValidator.IsValidName(value))
			{
				EditingErrorHandler.InvokeIllegalOperationError("A Scene's Name was set to an invalid string. It should only contain alphanumeric characters; use UserDefinedValidation to validate and correct strings before setting them on DtgeCore elements.");
			}
			else if (base.Name != value)
			{
				base.Name = value;
				this.NotifyUIUpdateNeeded();
			}
		}
	}
	public new bool NullSubsceneEnabled
	{
		get { return base.NullSubsceneEnabled; }
		private set
		{
			if (base.NullSubsceneEnabled != value)
			{
				base.NullSubsceneEnabled = value;
				this.NotifyUIUpdateNeeded();
			}
		}
	}
	public new int CurrentSubsceneIndex
	{
		get { return base.CurrentSubsceneIndex; }
		set
		{
			if (base.CurrentSubsceneIndex != value)
			{
				base.CurrentSubsceneIndex = value;
				this.NotifyUIUpdateNeeded();
			}
		}
	}
	public new SubsceneEditable CurrentSubscene
	{
		get { return base.CurrentSubscene as SubsceneEditable; }
	}
	public new bool RenderImage
	{
		get { return base.RenderImage; }
		set
		{
			if (base.RenderImage != value)
			{
				base.RenderImage = value;
				this.NotifyUIUpdateNeeded();
			}
		}
	}
	public new SceneImagePosition ImagePosition
	{
		get { return base.ImagePosition; }
		set
		{
			if (base.ImagePosition != value)
			{
				base.ImagePosition = value;
				this.NotifyUIUpdateNeeded();
			}
		}
	}
	public new string ImagePath
	{
		get { return base.ImagePath; }
		set
		{
			if (base.ImagePath != value)
			{
				base.ImagePath = value;
				this.NotifyUIUpdateNeeded();
			}
		}
    }
	public bool NeedsUIUpdate
	{
		get;
		private set;
	}

	public SceneEditable()
		: base()
	{
		this.NotifyUIUpdateNeeded();
	}

	protected SceneEditable(SceneSerializable serializable)
		: base()
	{
		if (serializable == null)
		{
			EditingErrorHandler.InvokeLoadError("A Scene Editable was initialized with a null serializable.");
		}
		else
		{
			this.Name = serializable.Name;
			this.NullSubsceneEnabled = serializable.NullSubsceneEnabled;
			this.CurrentSubsceneIndex = 0;
			this.RenderImage = serializable.RenderImage;
			this.ImagePath = serializable.ImagePath;

			for (int optionIndex = 0;
				optionIndex < serializable.OptionList.Count;
				optionIndex++)
			{
				this.OptionList.Add(
					new OptionEditable(this, serializable.OptionList[optionIndex]));
			}

			for (int subsceneIndex = 0;
				subsceneIndex < serializable.SubsceneList.Count;
				subsceneIndex++)
			{
				this.SubsceneList.Add(
					new SubsceneEditable(this, serializable.SubsceneList[subsceneIndex]));
			}

			for (int snippetIndex = 0;
				snippetIndex < serializable.SnippetList.Count;
				snippetIndex++)
			{
				SnippetEditable snippetEditable = new SnippetEditable(
					this,
					serializable.SnippetList[snippetIndex]);
				this.SnippetList.Add(snippetEditable);
			}

			this.nextSUID = serializable.NextSUID;

			this.NotifyUIUpdateNeeded();
		}
	}

	private SceneSerializable ToSerializable()
	{
		bool errorHit = false;

		SceneSerializable serializable = new SceneSerializable();
		serializable.Name = this.Name;
		serializable.NullSubsceneEnabled = this.NullSubsceneEnabled;
		serializable.RenderImage = this.RenderImage;
		serializable.ImagePosition = this.ImagePosition;
		serializable.ImagePath = this.ImagePath;

		for (int optionIndex = 0; optionIndex < this.OptionList.Count; optionIndex++)
		{
			OptionEditable optionEditable = this.OptionList[optionIndex] as OptionEditable;
			if (optionEditable == null)
			{
				EditingErrorHandler.InvokeSaveError("When serializing Scene [" + this.Name + "], the Option at index " + optionIndex + " was null.");
				errorHit = true;
			}
			else
			{
				serializable.OptionList.Add(optionEditable.ToSerializable());
			}
		}

		for (int subsceneIndex = 0; subsceneIndex < this.SubsceneList.Count; subsceneIndex++)
		{
			SubsceneEditable subsceneEditable =
				this.SubsceneList[subsceneIndex] as SubsceneEditable;
			if (subsceneEditable == null)
			{
				EditingErrorHandler.InvokeSaveError("When serializing Scene [" + this.Name + "], the Subscene at index " + subsceneIndex + " was null.");
				errorHit = true;
			}
			else
			{
				serializable.SubsceneList.Add(subsceneEditable.ToSerializable());
			}
		}

		for (int snippetIndex = 0;  snippetIndex < this.SnippetList.Count; snippetIndex++)
		{
			SnippetEditable snippetEditable = this.SnippetList[snippetIndex] as SnippetEditable;
			if (snippetEditable == null)
			{
				EditingErrorHandler.InvokeSaveError("When serializing Scene [" + this.Name + "], the Snippet at index " + snippetIndex + " was null.");
				errorHit = true;
			}
			else
			{
				serializable.SnippetList.Add(snippetEditable.ToSerializable());
			}
		}

		serializable.NextSUID = this.nextSUID;

		if (errorHit)
		{
			serializable = null;
		}

		return serializable;
	}

	public string SerializeToJsonString()
	{
		string jsonString = null;
		SceneSerializable sceneSerializable = this.ToSerializable();
		
		if (sceneSerializable == null)
		{
			EditingErrorHandler.InvokeSaveError("There was an error while saving the scene.");
		}
		else
		{
			jsonString = sceneSerializable.SerializeToString();
		}

		return jsonString;
	}

	public static new SceneEditable DeserializeFromJsonString(string jsonString)
	{
		SceneEditable sceneEditable = new SceneEditable();

		if (jsonString == null)
		{
			EditingErrorHandler.InvokeLoadError("A scene was deserialized from a null string.");
		}

		else
		{
			SceneSerializable sceneSerializable =
				SceneSerializable.DeserializeFromString(jsonString);
			sceneEditable = new SceneEditable(sceneSerializable);
		}

		return sceneEditable;
	}

	public void NotifyUIUpdateDone()
    {
        this.NeedsUIUpdate = false;
    }

	public void NotifyUIUpdateNeeded()
	{
		this.NeedsUIUpdate = true;
	}

	public string CalculateDebugSceneText(bool preserveRandomization)
	{
		string sceneText = "";
		sceneText += this.SerializeToJsonString();
		sceneText += "\r\n\r\nCalculatedText:\r\n";

		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; ++snippetIndex)
		{
			SnippetEditable currentSnippet = this.SnippetList[snippetIndex] as SnippetEditable;
			if (currentSnippet == null)
			{
				EditingErrorHandler.InvokeEditingError("Scene [" + this.Name + "] encountered a null snippet at index " + snippetIndex + " while calculating debug scene text.");
			}
			else
			{
				try
				{
					if (preserveRandomization)
					{
						sceneText += currentSnippet.CalculateTextStable();
					}
					else
					{
						sceneText += currentSnippet.CalculateText();
					}
				}
				catch (Exception exception)
				{
					EditingErrorHandler.InvokeEditingError("Scene [" + this.Name + "] encountered an unknown error while calculating scene text for snippet index " + snippetIndex + ". Exception: " + exception.Message);
				}
			}
		}

		return sceneText;
	}

	public string GetCopyableText()
	{
		string copyableText = "";
		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; snippetIndex++)
		{
			SnippetEditable currentSnippetEditable =
				this.SnippetList[snippetIndex] as SnippetEditable;
			if (currentSnippetEditable == null)
			{
				EditingErrorHandler.InvokeEditingError("Scene [" + this.Name + "] encountered a null snippet at index " + snippetIndex + " in GetCopyableText.");
			}
			else
			{
				string snippetCopyableText =
					currentSnippetEditable.GetCopyableText(COPYPASTE_VARIATION_BOUNDARY_MARKER);
				if (snippetCopyableText == null)
				{
					EditingErrorHandler.InvokeEditingError("In Scene [" + this.Name + "], the snippet at index " + snippetIndex + " returned null text while generating copyable text.");
				}
				else
				{
					copyableText += snippetCopyableText;

					if (snippetIndex != this.SnippetList.Count - 1)
					{
						copyableText += COPYPASTE_SNIPPET_BOUNDARY_MARKER;
					}
				}
			}
		}
		return copyableText;
	}

	public bool RestoreFromPastedText(string pastedText)
	{
		bool canRestoreFromPastedText = true;

		if (pastedText == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s RestoreFromPastedText function received a null string.");
		}
		else
		{
			string[] pastedTextSplitIntoSnippets =
				pastedText.Split(COPYPASTE_SNIPPET_BOUNDARY_MARKER);

			if (pastedTextSplitIntoSnippets.Length != this.SnippetList.Count)
			{
				canRestoreFromPastedText = false;
			}
			else
			{
				string[][] pastedTextSplitIntoVariations =
					new string[pastedTextSplitIntoSnippets.Length][];
				for (int snippetIndex = 0;
					snippetIndex < pastedTextSplitIntoSnippets.Length && canRestoreFromPastedText;
					snippetIndex++)
				{
					SnippetEditable currentSnippetEditable =
						this.SnippetList[snippetIndex] as SnippetEditable;
					pastedTextSplitIntoVariations[snippetIndex] =
						pastedTextSplitIntoSnippets[snippetIndex].Split(
							COPYPASTE_VARIATION_BOUNDARY_MARKER);

					if (pastedTextSplitIntoVariations[snippetIndex].Length
						!= currentSnippetEditable.GetVariationCount())
					{
						canRestoreFromPastedText = false;
					}
				}

				if (canRestoreFromPastedText)
				{
					for (int snippetIndex = 0;
						snippetIndex < pastedTextSplitIntoSnippets.Length;
						snippetIndex++)
					{
						SnippetEditable snippetEditable =
							this.SnippetList[snippetIndex] as SnippetEditable;

						if (snippetEditable == null)
						{
							EditingErrorHandler.InvokeEditingError("Scene [" + this.Name + "] encountered a null snippet at index " + snippetIndex + " in RestoreFromPastedText.");
						}
						else
						{
							snippetEditable.RestoreFromPastedText(
								pastedTextSplitIntoVariations[snippetIndex]);
						}
					}
				}
			}

			this.NotifyUIUpdateNeeded();
		}

		return canRestoreFromPastedText;
	}

	public SubsceneEditable AllocateNewSubscene()
	{
		SubsceneEditable newSubscene = new SubsceneEditable(this);
		this.SubsceneList.Add(newSubscene);
		this.NotifyUIUpdateNeeded();
		this.NotifySubsceneSnippetsOfChange();
		return newSubscene;
	}

	public void RemoveSubscene(SubsceneEditable subsceneEditable)
	{
		if (subsceneEditable == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s RemoveSubscene function was called with a null subscene.");
		}
		else if (!this.SubsceneList.Contains(subsceneEditable))
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s RemoveSubscene function received a subscene that does not exist within its subscenes.");
		}
		else if (subsceneEditable.IsReadOnly)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s RemoveSubscene function received a read only subscene. Read only subscenes cannot be added, removed, or modified directly by the editor.");
		}
		else
		{
			SubsceneEditable currentSubsceneBeforeRemoval = this.CurrentSubscene;
			this.SubsceneList.Remove(subsceneEditable);

			if (subsceneEditable == currentSubsceneBeforeRemoval)
			{
				if (this.CurrentSubsceneIndex != 0)
				{
					this.CurrentSubsceneIndex--;
				}
			}
			else
			{
				this.CurrentSubsceneIndex = this.SubsceneList.IndexOf(currentSubsceneBeforeRemoval);
			}
			this.NotifySubsceneSnippetsOfChange();
			this.NotifyUIUpdateNeeded();
		}
	}

	public void RemoveSubsceneByIndex(int subsceneIndex)
	{
		if (!CoreErrorHandler.IsValidIndex(subsceneIndex, this.SubsceneList.Count))
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s RemoveSubsceneByIndex function received an invalid index of " + subsceneIndex + ". The scene had " + this.SubsceneList.Count + " subscenes.");
		}
		else
		{
			this.RemoveSubscene(this.SubsceneList[subsceneIndex] as SubsceneEditable);
		}
	}

	public void EnableNullSubscene()
	{
		if (this.NullSubsceneEnabled)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s EnableNullSubscene function was called while the null subscene was already enabled.");
		}
		else
		{
			SubsceneEditable nullSubscene =
				new SubsceneEditable(this, SceneEditable.NULL_SUBSCENE_NAME, true);
			this.SubsceneList.Insert(0, nullSubscene);
			this.NullSubsceneEnabled = true;
			this.CurrentSubsceneIndex = 0;
			this.NotifySubsceneSnippetsOfChange();
			this.NotifyUIUpdateNeeded();
		}
	}

	public void DisableNullSubscene()
	{
		if (!this.NullSubsceneEnabled)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s DisableNullSubscene function was called while the null subscene was not enabled.");
		}
		else
		{
			this.SubsceneList.RemoveAt(0);
			this.NullSubsceneEnabled = false;

			if (this.CurrentSubsceneIndex != 0)
			{
				this.CurrentSubsceneIndex--;
			}

			this.NotifySubsceneSnippetsOfChange();
			this.NotifyUIUpdateNeeded();
		}
	}

	public OptionEditable AllocateNewOption()
	{
		OptionEditable newOptionEditable = new OptionEditable(this);
		this.OptionList.Add(newOptionEditable);
		this.NotifyUIUpdateNeeded();

		return newOptionEditable;
	}

	public void RemoveOption(OptionEditable optionEditable)
	{
		if (optionEditable == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s RemoveOption function was called with a null option.");
		}
		else if (!this.OptionList.Contains(optionEditable))
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s RemoveOption function received an option that does not exist within its options.");
		}
		else
		{
			this.OptionList.Remove(optionEditable);
			this.NotifyUIUpdateNeeded();
		}
	}

	public void MoveOption(OptionEditable optionEditable, int offset)
	{
		if (optionEditable == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s MoveOption function was called with a null Option.");
		}
		else if (offset == 0)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s MoveOption function was called with a zero offest, which is not allowed.");
		}
		else
		{
			int currentIndex = this.OptionList.IndexOf(optionEditable);
			int targetIndex = currentIndex + offset;

			if (!CoreErrorHandler.IsValidIndex(targetIndex, this.OptionList.Count))
			{
				EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s MoveOption function was called with an invalid offset. The offset was " + offset + ". The option's index was " + currentIndex + ". The resulting target index was " + targetIndex + ". Option count was " + this.OptionList.Count + ".");
			}
			else
			{
				if (targetIndex >= 0 && targetIndex < this.OptionList.Count - 1)
				{
					this.OptionList.Remove(optionEditable);
					this.OptionList.Insert(targetIndex, optionEditable);
				}
				else if (targetIndex == this.OptionList.Count - 1)
				{
					this.OptionList.Remove(optionEditable);
					this.OptionList.Add(optionEditable);
				}

				this.NotifyUIUpdateNeeded();
			}
		}
	}

	public void ClearAllOptions()
	{
		this.OptionList.Clear();
		this.NotifyUIUpdateNeeded();
	}

	public OptionEditable GetOptionByIndex(int optionIndex)
	{
		OptionEditable optionEditable = null;

		if (!CoreErrorHandler.IsValidIndex(optionIndex, this.OptionList.Count))
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s GetOptionByIndex function received an invalid index of " + optionIndex + ". The scene had " + this.OptionList.Count + " options.");
		}
		else
		{
			optionEditable = this.OptionList[optionIndex] as OptionEditable;
		}

		return optionEditable;
	}

	public int GetSubsceneCount()
	{
		return this.SubsceneList.Count;
	}

	public SubsceneEditable GetSubscene(int subsceneIndex)
	{
		SubsceneEditable subsceneEditable = null;

		if (!CoreErrorHandler.IsValidIndex(subsceneIndex, this.SubsceneList.Count))
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s GetOptionByIndex function received an invalid index of " + subsceneIndex + ". The scene had " + this.OptionList.Count + " options.");
		}
		else
		{
			subsceneEditable = this.SubsceneList[subsceneIndex] as SubsceneEditable;
		}

		return subsceneEditable;
	}
	
	public bool HasMatchingSubscene(SUID subsceneId)
	{
		bool foundMatch = false;

		if (subsceneId == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s HasMatchingSubscene function receieved a null Id.");
		}
		else
		{
			for (int subsceneIndex = 0;
				subsceneIndex < this.SubsceneList.Count && !foundMatch;
				subsceneIndex++)
			{
				Subscene subscene = this.SubsceneList[subsceneIndex];

				if (subscene == null)
				{
					EditingErrorHandler.InvokeEditingError("Scene [" + this.Name + "] encountered a null subscene at index " + subsceneIndex + " in HasMatchingSubscene.");
				}
				else if (subscene.Id == subsceneId)
				{
					foundMatch = true;
				}
			}
		}

		return foundMatch;
	}
	
	public SnippetEditable AllocateNewSnippet()
	{
		SnippetEditable newSnippetEditable = new SnippetEditable(this);
		this.SnippetList.Add(newSnippetEditable);
		this.NotifyUIUpdateNeeded();

		return newSnippetEditable;
	}

	public void RemoveSnippet(SnippetEditable snippetEditable)
	{
		if (snippetEditable == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s RemoveSnippet function was called with a null snippet.");
		}
		else if (!this.SnippetList.Contains(snippetEditable))
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s RemoveSnippet function received a snippet that does not exist within its snippets.");
		}
		else
		{
			this.SnippetList.Remove(snippetEditable);
			this.NotifyUIUpdateNeeded();
		}
	}

	public int GetSnippetCount()
	{
		return this.SnippetList.Count;
	}

	public SnippetEditable GetSnippetByIndex(int snippetIndex)
	{
		SnippetEditable snippetEditable = null;

		if (!CoreErrorHandler.IsValidIndex(snippetIndex, this.SnippetList.Count))
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s GetSnippetByIndex function received an invalid index of " + snippetIndex + ". The scene had " + this.SnippetList.Count + " snippets.");
		}
		else
		{
			snippetEditable = this.SnippetList[snippetIndex] as SnippetEditable;
		}

		return snippetEditable;
	}

	public void MoveSnippet(SnippetEditable snippetEditable, int offset)
	{
		if (snippetEditable == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s MoveSnippet function was called with a null Snippet.");
		}
		else if (offset == 0)
		{
			EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s MoveSnippet function was called with a zero offest, which is not allowed.");
		}
		else
		{
			int currentIndex = this.SnippetList.IndexOf(snippetEditable);
			int targetIndex = currentIndex + offset;

		if (!CoreErrorHandler.IsValidIndex(targetIndex, this.SnippetList.Count))
			{
				EditingErrorHandler.InvokeIllegalOperationError("Scene [" + this.Name + "]'s MoveSnippet function was called with an invalid offset. The offset was " + offset + ". The snippet's index was " + currentIndex + ". The resulting target index was " + targetIndex + ". Snippet count was " + this.SnippetList.Count + ".");
			}
			else
			{
				if (targetIndex >= 0 && targetIndex < this.SnippetList.Count - 1)
				{
					this.SnippetList.Remove(snippetEditable);
					this.SnippetList.Insert(targetIndex, snippetEditable);
				}
				else if (targetIndex == this.SnippetList.Count - 1)
				{
					this.SnippetList.Remove(snippetEditable);
					this.SnippetList.Add(snippetEditable);
				}

				this.NotifyUIUpdateNeeded();
			}
		}
	}

	public void NotifySubsceneSnippetsOfChange()
	{
		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; snippetIndex++)
		{
			SnippetEditable snippetEditable = this.SnippetList[snippetIndex] as SnippetEditable;

			if (snippetEditable == null)
			{
				EditingErrorHandler.InvokeEditingError("Scene [" + this.Name + "] encountered a null snippet at index " + snippetIndex + " in NotifySubsceneSnippetsOfChange.");
			}
			else
			{
				snippetEditable.NotifyOfSubsceneChange();
			}
		}
	}
}
