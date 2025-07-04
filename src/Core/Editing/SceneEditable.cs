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
			if (base.Name != value)
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

	private SceneSerializable ToSerializable()
	{
		SceneSerializable serializable = new SceneSerializable();
		serializable.Name = this.Name;
		serializable.NullSubsceneEnabled = this.NullSubsceneEnabled;
		serializable.RenderImage = this.RenderImage;
		serializable.ImagePosition = this.ImagePosition;
		serializable.ImagePath = this.ImagePath;

		for (int optionIndex = 0; optionIndex < this.OptionList.Count; optionIndex++)
		{
			OptionEditable optionEditable = this.OptionList[optionIndex] as OptionEditable;
			serializable.OptionList.Add(optionEditable.ToSerializable());
		}

		for (int subsceneIndex = 0; subsceneIndex < this.SubsceneList.Count; subsceneIndex++)
		{
			SubsceneEditable subsceneEditable =
				this.SubsceneList[subsceneIndex] as SubsceneEditable;
			serializable.SubsceneList.Add(subsceneEditable.ToSerializable());
		}

		for (int snippetIndex = 0;  snippetIndex < this.SnippetList.Count; snippetIndex++)
		{
			SnippetEditable snippetEditable = this.SnippetList[snippetIndex] as SnippetEditable;
			serializable.SnippetList.Add(snippetEditable.ToSerializable());
		}

		serializable.NextSUID = this.nextSUID;

		return serializable;
	}

	public string SerializeToJsonString()
	{
		SceneSerializable sceneSerializable = this.ToSerializable();
		string jsonString = sceneSerializable.SerializeToString();
		return jsonString;
	}

	public static new SceneEditable DeserializeFromJsonString(string jsonString)
	{
		SceneSerializable sceneSerializable = SceneSerializable.DeserializeFromString(jsonString);

		return new SceneEditable(sceneSerializable);
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
			if (preserveRandomization)
			{
				sceneText += currentSnippet.CalculateTextStable();
			}
			else
			{
				sceneText += currentSnippet.CalculateText();
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
			string snippetCopyableText =
				currentSnippetEditable.GetCopyableText(COPYPASTE_VARIATION_BOUNDARY_MARKER);
			copyableText += snippetCopyableText;
			if (snippetIndex != this.SnippetList.Count - 1)
			{
				copyableText += COPYPASTE_SNIPPET_BOUNDARY_MARKER;
			}
		}
		return copyableText;
	}

	public bool RestoreFromPastedText(string pastedText)
	{
		bool canRestoreFromPastedText = true;
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
					SnippetEditable currentSnippetEditable =
						this.SnippetList[snippetIndex] as SnippetEditable;
					currentSnippetEditable.RestoreFromPastedText(
						pastedTextSplitIntoVariations[snippetIndex]);
				}
			}
		}

		this.NotifyUIUpdateNeeded();

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
		if (subsceneEditable.IsReadOnly)
		{
			GlobalErrorHandler.InvokeError("An attempt was made to delete a read only subscene when only the SceneEditable should do that.");
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
		this.RemoveSubscene(this.SubsceneList[subsceneIndex] as SubsceneEditable);
	}

	public void EnableNullSubscene()
	{
		if (!this.NullSubsceneEnabled)
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
		if (this.NullSubsceneEnabled)
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

	public void RemoveOption(OptionEditable option)
	{
		this.OptionList.Remove(option);
		this.NotifyUIUpdateNeeded();
	}

	public bool TryMoveOption(OptionEditable optionEditable, int numberOfPositionsToMove)
	{
		int currentIndex = this.OptionList.IndexOf(optionEditable);
		int targetIndex = currentIndex + numberOfPositionsToMove;
		bool optionMoved = false;

		if (targetIndex >= 0 && targetIndex < this.OptionList.Count - 1)
		{
			this.OptionList.Remove(optionEditable);
			this.OptionList.Insert(targetIndex, optionEditable);
			optionMoved = true;
		}
		else if (targetIndex == this.OptionList.Count - 1)
		{
			this.OptionList.Remove(optionEditable);
			this.OptionList.Add(optionEditable);
			optionMoved = true;
		}

		if (optionMoved)
		{
			this.NotifyUIUpdateNeeded();
		}

		return optionMoved;
	}

	public void ClearAllOptions()
	{
		this.OptionList.Clear();
		this.NotifyUIUpdateNeeded();
	}

	public OptionEditable GetOptionByIndex(int optionIndex)
	{
		return this.OptionList[optionIndex] as OptionEditable;
	}

	public int GetSubsceneCount()
	{
		return this.SubsceneList.Count;
	}

	public SubsceneEditable GetSubscene(int subsceneIndex)
	{
		return this.SubsceneList[subsceneIndex] as SubsceneEditable;
	}
	
	public bool HasMatchingSubscene(SUID subsceneId)
	{
		bool foundMatch = false;

		for (int subsceneIndex = 0;
			subsceneIndex < this.SubsceneList.Count && !foundMatch;
			subsceneIndex++)
		{
			if (this.SubsceneList[subsceneIndex].Id == subsceneId)
			{
				foundMatch = true;
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
		this.SnippetList.Remove(snippetEditable);
		this.NotifyUIUpdateNeeded();
	}

	public int GetSnippetCount()
	{
		return this.SnippetList.Count;
	}

	public SnippetEditable GetSnippetByIndex(int snippetIndex)
	{
		return this.SnippetList[snippetIndex] as SnippetEditable;
	}

	public bool TryMoveSnippet(SnippetEditable snippetEditable, int numberOfPositionsToMove)
	{
		int currentIndex = this.SnippetList.IndexOf(snippetEditable);
		int targetIndex = currentIndex + numberOfPositionsToMove;
		bool snippetMoved = false;

		if (targetIndex >= 0 && targetIndex < this.SnippetList.Count - 1)
		{
			this.SnippetList.Remove(snippetEditable);
			this.SnippetList.Insert(targetIndex, snippetEditable);
			snippetMoved = true;
		}
		else if (targetIndex == this.SnippetList.Count - 1)
		{
			this.SnippetList.Remove(snippetEditable);
			this.SnippetList.Add(snippetEditable);
			snippetMoved = true;
		}

		if (snippetMoved)
		{
			this.NotifyUIUpdateNeeded();
		}

		return snippetMoved;
	}

	public void NotifySubsceneSnippetsOfChange()
	{
		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; snippetIndex++)
		{
			SnippetEditable snippetEditable = this.SnippetList[snippetIndex] as SnippetEditable;
			snippetEditable.NotifyOfSubsceneChange();
		}
	}
}
