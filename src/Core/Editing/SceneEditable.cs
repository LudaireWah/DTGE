using DtgeCore.Serialization;
using System;
using System.Collections.Generic;

namespace DtgeCore.Editing;

/**
 * SceneEditable is the editable version of the Scene to be used in editors for authoring DTGE
 * games. It provides editing functionality, access to editable versions of the Scene's components
 * such as Options, and tracks when changes have been made to the scene. This allows editors to
 * distribute the specific pieces of the scene through their code but still be able to see if
 * edits have been made in a single place.
 */
public class SceneEditable : Scene
{
	private const string COPYPASTE_SNIPPET_BOUNDARY_MARKER =
		">>>\r\n[DTGESnippetBoundary]\r\n<<<";
	private const string COPYPASTE_VARIATION_BOUNDARY_MARKER =
		">>>\r\n[DTGEVariationBoundary]\r\n<<<";

	public const string NULL_SUBSCENE_NAME = "(None)";

	public new string Id
	{
		get { return base.Id; }
		set
		{
			base.Id = value;
			this.NeedsUIUpdate = true;
		}
	}
	public new bool NullSubsceneEnabled
	{
		get { return base.NullSubsceneEnabled; }
		private set
		{
			base.NullSubsceneEnabled = value;
			this.NeedsUIUpdate = true;
		}
	}
	public new int CurrentSubsceneIndex
	{
		get { return base.CurrentSubsceneIndex; }
		set
		{
			base.CurrentSubsceneIndex = value;
			this.NeedsUIUpdate = true;
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
			base.RenderImage = value;
			this.NeedsUIUpdate = true;
		}
	}
	public new SceneImagePosition ImagePosition
	{
		get { return base.ImagePosition; }
		set
		{
			base.ImagePosition = value;
			this.NeedsUIUpdate = true;
		}
	}
	public new string ImagePath
	{
		get { return base.ImagePath; }
		set
		{
			base.ImagePath = value;
			this.NeedsUIUpdate = true;
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
		this.NeedsUIUpdate = true;
	}

	protected SceneEditable(SceneSerializable serializable)
		:base()
	{
		this.Id = serializable.Id;
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
			SnippetSerializable deserializingSnippetSerializable =
				serializable.SnippetList[snippetIndex];
			switch (deserializingSnippetSerializable.Mode)
			{
			case Snippet.Mode.Simple:
				SnippetSimpleEditable snippetSimple = new SnippetSimpleEditable(
					this,
					deserializingSnippetSerializable as SnippetSimpleSerializable);
				this.SnippetList.Add(snippetSimple);
				break;
			case Snippet.Mode.Subscene:
				SnippetSubsceneEditable snippetSubscene = new SnippetSubsceneEditable(
					this,
					deserializingSnippetSerializable as SnippetSubsceneSerializable);
				this.SnippetList.Add(snippetSubscene);
				break;
			case Snippet.Mode.Random:
				break;
			default:
				GlobalErrorHandler.InvokeError("Unknown snippt type");
				break;
			}
		}

		this.NeedsUIUpdate = true;

		this.nextSUID = serializable.NextSUID;
	}

	private SceneSerializable ToSerializable()
	{
		SceneSerializable serializable = new SceneSerializable();
		serializable.Id = this.Id;
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
			ISnippetEditable snippetEditable = this.SnippetList[snippetIndex] as ISnippetEditable;
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
			ISnippetEditable currentSnippet = this.SnippetList[snippetIndex] as ISnippetEditable;
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
			ISnippetEditable currentSnippetEditable =
				this.SnippetList[snippetIndex] as ISnippetEditable;
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
				ISnippetEditable currentSnippetEditable =
					this.SnippetList[snippetIndex] as ISnippetEditable;
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
					ISnippetEditable currentSnippetEditable =
						this.SnippetList[snippetIndex] as ISnippetEditable;
					currentSnippetEditable.RestoreFromPastedText(
						pastedTextSplitIntoVariations[snippetIndex]);
				}
			}
		}

		this.NeedsUIUpdate = true;

		return canRestoreFromPastedText;
	}

	public SubsceneEditable AllocateNewSubscene()
	{
		SubsceneEditable newSubscene = new SubsceneEditable(this);
		this.SubsceneList.Add(newSubscene);
		this.NeedsUIUpdate = true;
		this.NotifySubsceneSnippetsOfChange();
		return newSubscene;
	}

	public void RemoveSubscene(SubsceneEditable subsceneEditable)
	{
		this.SubsceneList.Remove(subsceneEditable);
		this.NotifySubsceneSnippetsOfChange();
		this.NeedsUIUpdate = true;
	}

	public void RemoveSubsceneByIndex(int subsceneIndex)
	{
		this.SubsceneList.RemoveAt(subsceneIndex);
		this.NotifySubsceneSnippetsOfChange();
		this.NeedsUIUpdate = true;
	}

	public void EnableNullSubscene()
	{
		if (!this.NullSubsceneEnabled)
		{
			SubsceneEditable nullSubscene =
				new SubsceneEditable(this, SceneEditable.NULL_SUBSCENE_NAME);
			this.SubsceneList.Insert(0, nullSubscene);
			this.NullSubsceneEnabled = true;
			this.NotifySubsceneSnippetsOfChange();
			this.NeedsUIUpdate = true;
		}
	}

	public void DisableNullSubscene()
	{
		if (this.NullSubsceneEnabled)
		{
			this.SubsceneList.RemoveAt(0);
			this.NullSubsceneEnabled = false;
			this.NotifySubsceneSnippetsOfChange();
			this.NeedsUIUpdate = true;
		}
	}

	public OptionEditable AllocateNewOption()
	{
		OptionEditable newOptionEditable = new OptionEditable(this);
		this.OptionList.Add(newOptionEditable);
		this.NeedsUIUpdate = true;

		return newOptionEditable;
	}

	public void RemoveOption(OptionEditable option)
	{
		this.OptionList.Remove(option);
		this.NeedsUIUpdate = true;
	}

	public void ClearAllOptions()
	{
		this.OptionList.Clear();
		this.NeedsUIUpdate = true;
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
			if (this.SubsceneList[0].Id == subsceneId)
			{
				foundMatch = true;
			}
		}

		return foundMatch;
	}
	
	public ISnippetEditable AllocateNewSnippet()
	{
		ISnippetEditable newSnippetEditable = new SnippetSimpleEditable(this);
		this.SnippetList.Add(newSnippetEditable);
		this.NeedsUIUpdate = true;

		return newSnippetEditable;
	}

	public void RemoveSnippet(ISnippetEditable snippetEditable)
	{
		this.SnippetList.Remove(snippetEditable);
		this.NeedsUIUpdate = true;
	}

	public int GetSnippetCount()
	{
		return this.SnippetList.Count;
	}

	public ISnippetEditable GetSnippetByIndex(int snippetIndex)
	{
		return this.SnippetList[snippetIndex] as ISnippetEditable;
	}

	public void ReplaceSnippet(ISnippetEditable toReplace, ISnippetEditable replaceWith)
	{
		for (int snippetIndex = 0; snippetIndex <  this.SnippetList.Count; snippetIndex++)
		{
			if (this.SnippetList[snippetIndex] == toReplace)
			{
				this.SnippetList[snippetIndex] = replaceWith;
				this.NeedsUIUpdate = true;
			}
		}

		this.NotifySubsceneSnippetsOfChange();
	}

	public void NotifySubsceneSnippetsOfChange()
	{
		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; snippetIndex++)
		{
			ISnippetEditable snippet = this.SnippetList[snippetIndex] as ISnippetEditable;
			if (snippet.Mode == Snippet.Mode.Subscene)
			{
				SnippetSubsceneEditable snippetSubsceneEditable =
					snippet as SnippetSubsceneEditable;
				snippetSubsceneEditable.UpdateVariationsFromSubscenes();
			}
		}
	}
}
