using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtgeCore;

/**
 * A wrapper for a DTGE Scene that encapsulates a scene and provides editor-time functionality
 * used during scene creation.
 */
public class SceneEditable : Scene
{
	private const string COPYPASTE_SNIPPET_BOUNDARY_MARKER = ">>>\r\n[DTGESnippetBoundary]\r\n<<<";
	private const string COPYPASTE_VARIATION_BOUNDARY_MARKER = ">>>\r\n[DTGEVariationBoundary]\r\n<<<";

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

	public SceneEditable(string sceneJson)
		: base(sceneJson)
	{
		this.NeedsUIUpdate = true;
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

		//sceneText += this.Serialize();
		//sceneText += "\r\n\r\nCalculatedText:\r\n";

		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; ++snippetIndex)
		{
			SnippetEditable currentSnippet = this.SnippetList[snippetIndex] as SnippetEditable;
			if (preserveRandomization)
			{
				sceneText += currentSnippet.CalculateTextWithoutRandomization();
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
			SnippetEditable currentSnippetEditable = this.SnippetList[snippetIndex] as SnippetEditable;
			string snippetCopyableText = currentSnippetEditable.GetCopyableText(COPYPASTE_VARIATION_BOUNDARY_MARKER);
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
		string[] pastedTextSplitIntoSnippets = pastedText.Split(COPYPASTE_SNIPPET_BOUNDARY_MARKER);

		if (pastedTextSplitIntoSnippets.Length != this.SnippetList.Count)
		{
			canRestoreFromPastedText = false;
		}
		else
		{

			string[][] pastedTextSplitIntoVariations = new string[pastedTextSplitIntoSnippets.Length][];
			for (int snippetIndex = 0; snippetIndex < pastedTextSplitIntoSnippets.Length && canRestoreFromPastedText; snippetIndex++)
			{
				SnippetEditable currentSnippetEditable = this.SnippetList[snippetIndex] as SnippetEditable;
				pastedTextSplitIntoVariations[snippetIndex] = pastedTextSplitIntoSnippets[snippetIndex].Split(COPYPASTE_VARIATION_BOUNDARY_MARKER);
				if (pastedTextSplitIntoVariations[snippetIndex].Length != currentSnippetEditable.GetVariationCount())
				{
					canRestoreFromPastedText = false;
				}
			}

			if (canRestoreFromPastedText)
			{
				for (int snippetIndex = 0; snippetIndex < pastedTextSplitIntoSnippets.Length; snippetIndex++)
				{
					SnippetEditable currentSnippetEditable = this.SnippetList[snippetIndex] as SnippetEditable;
					currentSnippetEditable.RestoreFromPastedText(pastedTextSplitIntoVariations[snippetIndex]);
				}
			}
		}

		return canRestoreFromPastedText;
	}

	public SubsceneEditable AllocateNewSubscene()
	{
		SubsceneEditable newSubscene = new SubsceneEditable(this);
		this.Subscenes.Add(newSubscene);
		this.NeedsUIUpdate = true;
		return newSubscene;
	}

	public void RemoveSubscene(SubsceneEditable subsceneEditable)
	{
		this.Subscenes.Remove(subsceneEditable);
		this.NeedsUIUpdate = true;
	}

	public void RemoveSubsceneByIndex(int subsceneIndex)
	{
		this.Subscenes.RemoveAt(subsceneIndex);
		this.NeedsUIUpdate = true;
	}

	public void EnableNullSubscene()
	{
		if (!this.NullSubsceneEnabled)
		{
			SubsceneEditable nullSubscene = new SubsceneEditable(this, SceneEditable.NULL_SUBSCENE_NAME);
			this.Subscenes.Insert(0, nullSubscene);
			this.NullSubsceneEnabled = true;
			this.NeedsUIUpdate = true;
		}
	}

	public void DisableNullSubscene()
	{
		if (this.NullSubsceneEnabled)
		{
			this.Subscenes.RemoveAt(0);
			this.NullSubsceneEnabled = false;
			this.NeedsUIUpdate = true;
		}
	}

	public OptionEditable AllocateNewOption()
	{
		OptionEditable newOptionEditable = new OptionEditable(this);
		this.OptionList.Add(newOptionEditable);

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
		return this.Subscenes.Count;
	}

	public SubsceneEditable GetSubscene(int subsceneIndex)
	{
		return this.Subscenes[subsceneIndex] as SubsceneEditable;
	}
	
	public SnippetEditable AllocateNewSnippet()
	{
		SnippetEditable newSnippetEditable = new SnippetEditable(this);
		this.SnippetList.Add(newSnippetEditable);

		return newSnippetEditable;
	}

	public void RemoveSnippet(SnippetEditable snippetEditable)
	{
		this.SnippetList.Remove(snippetEditable);
	}

	public int GetSnippetCount()
	{
		return this.SnippetList.Count;
	}

	public SnippetEditable GetSnippetByIndex(int snippetIndex)
	{
		return this.SnippetList[snippetIndex] as SnippetEditable;
	}
}
