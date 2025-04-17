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
public class SceneEditable
{
	private SceneData sceneData;

	public SceneEditable()
	{
		this.sceneData = new SceneData();
		this.NeedsUIUpdate = true;
	}

	public SceneEditable(string sceneJson)
	{
		this.sceneData = SceneData.Deserialize(sceneJson);
		this.NeedsUIUpdate = true;
	}

	public bool NeedsUIUpdate
	{
		get;
		private set;
	}

	public void NotifyUIUpdate()
	{
		this.NeedsUIUpdate = false;
	}

	public string Id
	{
		get { return this.sceneData.Id; }
		set
		{
			this.sceneData.Id = value;
			this.NeedsUIUpdate = true;
		}
	}
	public int CurrentSubsceneIndex
	{
		get { return this.sceneData.CurrentSubsceneIndex; }
		set
		{
			this.sceneData.CurrentSubsceneIndex = value;
			this.NeedsUIUpdate = true;
		}
	}
	public bool AllowNullSubscene
	{
		get { return this.sceneData.AllowNullSubscene; }
		set
		{
			this.sceneData.AllowNullSubscene = value;
			this.NeedsUIUpdate = true;
		}
	}
	public bool RenderImage
	{
		get { return this.sceneData.RenderImage; }
		set
		{
			this.sceneData.RenderImage = value;
			this.NeedsUIUpdate = true;
		}
	}
	public SceneImagePosition ImagePosition
	{
		get { return this.sceneData.ImagePosition; }
		set
		{
			this.sceneData.ImagePosition = value;
			this.NeedsUIUpdate = true;
		}
	}
	public string ImagePath
	{
		get { return this.sceneData.ImagePath; }
		set
		{
			this.sceneData.ImagePath = value;
			this.NeedsUIUpdate = true;
		}
	}

	public string CalculateDebugSceneText(bool preserveRandomization)
	{
		return this.sceneData.CalculateDebugSceneText(preserveRandomization);
	}

	public string GetCopyableText()
	{
		return this.sceneData.GetCopyableText();
	}

	public bool RestoreFromPastedText(string pastedText)
	{
		return this.sceneData.RestoreFromPastedText(pastedText);
	}

	public void RemoveSubsceneByIndex(int subsceneIndex)
	{
		this.sceneData.RemoveSubsceneByIndex(subsceneIndex);
	}

	public void SetSubsceneName(int subsceneIndex, string subsceneName)
	{
		this.sceneData.SetSubsceneName(subsceneIndex, subsceneName);
	}

	public int GetEditableSubsceneCount()
	{
		return this.sceneData.GetEditableSubsceneCount();
	}

	public SubsceneId GetEditableSubsceneId(int subsceneIndex)
	{
		return this.sceneData.GetEditableSubsceneId(subsceneIndex);
	}

	public void EnableNullSubscene()
	{
		this.sceneData.EnableNullSubscene();
	}

	public void DisableNullSubscene()
	{
		this.sceneData.DisableNullSubscene();
	}

	public int GetSubsceneCount()
	{
		return this.sceneData.GetSubsceneCount();
	}

	public SubsceneId GetSubsceneId(int subsceneIndex)
	{
		return this.sceneData.GetSubsceneId(subsceneIndex);
	}

	public bool SetCurrentSubsceneByIndex(int index)
	{
		return this.sceneData.SetCurrentSubsceneByIndex(index);
	}

	public bool SetCurrentSubscene(string subsceneName)
	{
		return this.sceneData.SetCurrentSubscene(subsceneName);
	}

	public bool SetCurrentSubscene(SubsceneId subsceneId)
	{
		return this.sceneData.SetCurrentSubscene(subsceneId);
	}

	public int GetCurrentSubsceneIndex()
	{
		return this.sceneData.GetCurrentSubsceneIndex();
	}

	public SubsceneId GetCurrentSubsceneId()
	{
		return this.sceneData.GetCurrentSubsceneId();
	}

	public int GetSnippetCount()
	{
		return this.sceneData.GetSnippetCount();
	}

	public Snippet.Mode GetSnippetMode(int snippetIndex)
	{
		return this.sceneData.SnippetList[snippetIndex].CurrentMode;
	}

	public void AddSubscene(string newSubsceneName)
	{
		this.sceneData.AddSubscene(newSubsceneName);
	}

	public string Serialize()
	{
		return this.sceneData.Serialize();
	}

	public void ClearAllSnippets()
	{
		this.sceneData.ClearAllSnippets();
	}

	public void AddSnippet(Snippet snippet)
	{
		this.sceneData.AddSnippet(snippet);
	}

	public void AddOption(Option option)
	{
		this.sceneData.AddOption(option);
	}

	public void ClearAllOptions()
	{
		this.sceneData.ClearAllOptions();
	}

	public List<Option> GetOptionList()
	{
		return this.sceneData.OptionList;
	}

	public ISubsceneContextProvider GetSubsceneContextProvider()
	{
		return this.sceneData;
	}

	public List<Snippet> GetSnippetList()
	{
		return this.sceneData.SnippetList;
	}
}