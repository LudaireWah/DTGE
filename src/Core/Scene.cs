using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore;

/**
 * DTGE's Scene is the fundamental unit of the engine. Nearly everything displayed to the
 * player is displayed via a scene. Thus, navigation through the game is navigating
 * through a set of scenes, each presenting the player with a description of the player's
 * situation and which options the player may choose to advance in the game.
 */
public class Scene : ISubsceneContextProvider
{
	public const int MAX_OPTION_NUMBER = 15;
	private const string NULL_SUBSCENE_DISPLAY_NAME = "(None)";
	private const string COPYPASTE_SNIPPET_BOUNDARY_MARKER = ">>>\r\n[DTGESnippetBoundary]\r\n<<<";
	private const string COPYPASTE_VARIATION_BOUNDARY_MARKER = ">>>\r\n[DTGEVariationBoundary]\r\n<<<";

	public struct SceneId
	{
		public string scene;
		public string subscene;

		public SceneId(string sceneIdString)
		{
			string[] ids = sceneIdString.Split(".");
			this.scene = ids[0];
			if (ids.Length > 1)
			{
				this.subscene = ids[1];
			}
			else
			{
				this.subscene = null;
			}
			if (ids.Length > 2)
			{
				// error
			}
		}

		public SceneId(string sceneName, string subsceneName)
		{
			this.scene = sceneName;
			this.subscene = subsceneName;
		}
	}

	public class SubsceneId
	{
		public string Name { get; set; }
		public uint Id { get; set; }
		public static SubsceneId None = new SubsceneId(NULL_SUBSCENE_DISPLAY_NAME, 0);

		private static uint nextIdNumber = 1;

		public SubsceneId()
		{
			this.Name = "New Subscene";
			this.Id = nextIdNumber++;
		}

		public SubsceneId(string name)
		{
			this.Name = name;
			this.Id = nextIdNumber++;
		}

		private SubsceneId(string name, uint id)
		{
			this.Name = name;
			this.Id = id;
		}
	}

	public string Id { get; set; }
	public Option[] OptionList { get; set; }
	public List<SubsceneId> Subscenes { get; set; }
	public int CurrentSubsceneIndex { get; set; }
	public bool AllowNullSubscene { get; set; }
	
	public List<Snippet> SnippetList { get; set; }
	public string SceneText { get; set; }

	private List<Action<SubsceneId>> OnSubsceneAddedList;
	private List<Action<SubsceneId>> OnSubsceneRemovedList;
	private List<Action<SubsceneId, string>> OnSubsceneRenamedList;


	public Scene()
	{
		this.Id = "";
		this.OptionList = new Option[MAX_OPTION_NUMBER];
		this.Subscenes = new List<SubsceneId>();
		this.CurrentSubsceneIndex = 0;
		this.AllowNullSubscene = false;
		this.SnippetList = new List<Snippet>();
		this.OnSubsceneAddedList = new List<Action<SubsceneId>>();
		this.OnSubsceneRemovedList = new List<Action<SubsceneId>>();
		this.OnSubsceneRenamedList = new List<Action<SubsceneId, string>>();

		this.AddOption(new Option());
		this.AddSnippet(new Snippet(this));
	}

	public Scene(string id)
	{
		this.Id= id;
		this.OptionList = new Option[MAX_OPTION_NUMBER];
		this.Subscenes = new List<SubsceneId>();
		this.CurrentSubsceneIndex = 0;
		this.AllowNullSubscene = false;
		this.SnippetList = new List<Snippet>();
		this.OnSubsceneAddedList = new List<Action<SubsceneId>>();
		this.OnSubsceneRemovedList = new List<Action<SubsceneId>>();
		this.OnSubsceneRenamedList = new List<Action<SubsceneId, string>>();

		this.AddOption(new Option());
		this.AddSnippet(new Snippet(this));
	}

	public string Serialize()
	{   
		string toReturn = JsonSerializer.Serialize(this);
		return toReturn;
	}

	public static Scene Deserialize(string sceneJson)
	{
		Scene deserializedScene = JsonSerializer.Deserialize<DtgeCore.Scene>(sceneJson);
		if (deserializedScene.SceneText != null && deserializedScene.SceneText.Length != 0)
		{
			Snippet snippetFromSceneText = new Snippet(deserializedScene);
			snippetFromSceneText.SetVariationText(0,(deserializedScene.SceneText));

			deserializedScene.ClearAllSnippets();
			deserializedScene.SnippetList.Add(snippetFromSceneText);
			deserializedScene.SceneText = null;
		}
		for (int snippetIndex = 0; snippetIndex < deserializedScene.SnippetList.Count; snippetIndex++)
		{
			deserializedScene.SnippetList[snippetIndex].SetSubsceneContextProvider(deserializedScene);
		}
		return deserializedScene;
	}

	public bool AddOption(Option option)
	{
		int assignedOptionIndex = -1;

		for (int optionIndex = 0; optionIndex < MAX_OPTION_NUMBER && assignedOptionIndex == -1; optionIndex++)
		{
			if (this.OptionList[optionIndex] == null)
			{
				this.OptionList[optionIndex] = option;
				assignedOptionIndex = optionIndex;
			}
		}

		return assignedOptionIndex > -1;
	}

	public void ClearAllOptions()
	{
		for (int optionIndex = 0; optionIndex < MAX_OPTION_NUMBER; optionIndex++)
		{
			this.OptionList[optionIndex] = null;
		}
	}

	public int GetOptionCount()
	{
		return this.OptionList.Length;
	}

	public Option GetOption(int index)
	{
		return this.OptionList[index];
	}

	public void AddSnippet(Snippet snippet)
	{
		this.SnippetList.Add(snippet);
	}

	public void ClearAllSnippets()
	{
		this.SnippetList.Clear();
	}

	public int GetSnippetCount()
	{
		return this.SnippetList.Count;
	}

	public string CalculateSceneText()
	{
		string sceneText = "";

		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; ++snippetIndex)
		{
			Snippet currentSnippet = this.SnippetList[snippetIndex];
			sceneText += currentSnippet.CalculateText(false);
		}

		return sceneText;
	}

	public string CalculateDebugSceneText(bool preserveRandomization)
	{
		string sceneText = "";
		
		sceneText += this.Serialize();
		sceneText += "\r\n\r\nCalculatedText:\r\n";

		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; ++snippetIndex)
		{
			Snippet currentSnippet = this.SnippetList[snippetIndex];
			sceneText += currentSnippet.CalculateText(preserveRandomization);
		}

		return sceneText;
	}

	public string GetCopyableText()
	{
		string copyableText = "";
		for (int snippetIndex = 0; snippetIndex < this.SnippetList.Count; snippetIndex++)
		{
			string snippetCopyableText = this.SnippetList[snippetIndex].GetCopyableText(COPYPASTE_VARIATION_BOUNDARY_MARKER);
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
				pastedTextSplitIntoVariations[snippetIndex] = pastedTextSplitIntoSnippets[snippetIndex].Split(COPYPASTE_VARIATION_BOUNDARY_MARKER);
				if (pastedTextSplitIntoVariations[snippetIndex].Length != this.SnippetList[snippetIndex].GetVariationCount())
				{
					canRestoreFromPastedText = false;
				}
			}

			if (canRestoreFromPastedText)
			{
				for (int snippetIndex = 0; snippetIndex < pastedTextSplitIntoSnippets.Length; snippetIndex++)
				{
					this.SnippetList[snippetIndex].RestoreFromPastedText(pastedTextSplitIntoVariations[snippetIndex]);
				}
			}
		}

		return canRestoreFromPastedText;
	}

	public void EnableNullSubscene()
	{
		if (!this.AllowNullSubscene)
		{
			this.AllowNullSubscene = true;
			this.OnSubsceneAdded(SubsceneId.None);
		}
	}

	public void DisableNullSubscene()
	{
		if (this.AllowNullSubscene)
		{
			this.AllowNullSubscene = false;
			this.OnSubsceneRemoved(SubsceneId.None);
		}
	}

	public void AddSubscene(string newSubsceneName)
	{
		SubsceneId newId = new SubsceneId(newSubsceneName);
		this.Subscenes.Add(newId);
		this.OnSubsceneAdded(newId);
	}

	public void RemoveSubsceneByIndex(int subsceneIndex)
	{
		SubsceneId toRemove = this.Subscenes[subsceneIndex];
		this.OnSubsceneRemoved(toRemove);
		this.Subscenes.RemoveAt(subsceneIndex);
	}

	public bool SetCurrentSubsceneByIndex(int index)
	{
		bool success = false;
		if (index < this.Subscenes.Count)
		{
			this.CurrentSubsceneIndex = index;
			success = true;
		}
		return success;
	}

	public bool SetCurrentSubscene(string subsceneName)
	{
		bool success = false;
		if (subsceneName == null)
		{
			if (this.AllowNullSubscene || this.Subscenes.Count == 0)
			{
				this.CurrentSubsceneIndex = 0;
				success = true;
			}
		}
		if (subsceneName != null)
		{
			int desiredSubsceneIndex = -1;

			for (int subsceneIndex = 0; subsceneIndex < this.Subscenes.Count; subsceneIndex++)
			{
				if (Subscenes[subsceneIndex].Name == subsceneName)
				{
					desiredSubsceneIndex = subsceneIndex;
					break;
				}
			}

			if (desiredSubsceneIndex != -1)
			{
				this.CurrentSubsceneIndex = desiredSubsceneIndex;
				success = true;
			}
		}

		return success;
	}

	public bool SetCurrentSubscene(SubsceneId subsceneId)
	{
		bool success = false;
		int desiredSubsceneIndex = -1;

		for (int subsceneIndex = 0; subsceneIndex < this.Subscenes.Count; subsceneIndex++)
		{
			if (Subscenes[subsceneIndex].Id == subsceneId.Id)
			{
				desiredSubsceneIndex = subsceneIndex;
				break;
			}
		}

		if (this.AllowNullSubscene && desiredSubsceneIndex >= 0)
		{
			desiredSubsceneIndex++;
		}
		
		if (desiredSubsceneIndex != -1)
		{
			this.CurrentSubsceneIndex = desiredSubsceneIndex;
			success = true;
		}

		return success;
	}

	public void SetSubsceneName(int subsceneIndex, string newSubsceneName)
	{
		if (this.AllowNullSubscene && subsceneIndex == this.Subscenes.Count)
		{
			// Error; cannot set the name of a "None" subscene
		}
		else
		{
			this.Subscenes[subsceneIndex].Name = newSubsceneName;
			(this.Subscenes[subsceneIndex]).Name = newSubsceneName;
		}
	}

	public int GetSubsceneCount()
	{
		int subsceneCount = this.Subscenes.Count;
		if (this.AllowNullSubscene)
		{
			subsceneCount++;
		}
		return subsceneCount;
	}

	public int GetEditableSubsceneCount()
	{
		return this.Subscenes.Count;
	}

	public SubsceneId GetSubsceneId(int subsceneIndex)
	{
		SubsceneId subsceneId = null;

		if (this.AllowNullSubscene)
		{
			if (subsceneIndex == 0)
			{
				subsceneId = SubsceneId.None;
			}
			else
			{
				subsceneId = this.Subscenes[subsceneIndex - 1];
			}
		}
		else
		{
			subsceneId = this.Subscenes[subsceneIndex];
		}

		return subsceneId;
	}

	public SubsceneId GetEditableSubsceneId(int subsceneIndex)
	{
		return this.Subscenes[subsceneIndex];
	}

	public int GetCurrentSubsceneIndex()
	{
		return this.CurrentSubsceneIndex;
	}

	public SubsceneId GetCurrentSubsceneId()
	{
		return this.GetSubsceneId(this.CurrentSubsceneIndex);
	}

	public void RegisterOnSubsceneAdded(Action<SubsceneId> onSubsceneAdded)
	{
		if (onSubsceneAdded != null)
		{
			this.OnSubsceneAddedList.Add(onSubsceneAdded);
		}
		else
		{
			// Error
		}
	}

	public void UnregisterOnSubsceneAdded(Action<SubsceneId> onSubsceneAdded)
	{
		this.OnSubsceneAddedList.Remove(onSubsceneAdded);
	}

	public void RegisterOnSubsceneRemoved(Action<SubsceneId> onSubsceneRemoved)
	{
		if (onSubsceneRemoved != null)
		{
			this.OnSubsceneRemovedList.Add(onSubsceneRemoved);
		}
		else
		{
			// Error
		}
	}

	public void UnregisterOnSubsceneRemoved(Action<SubsceneId> onSubsceneRemoved)
	{
		this.OnSubsceneRemovedList.Remove(onSubsceneRemoved);
	}

	public void RegisterOnSubsceneRenamed(Action<SubsceneId, string> onSubsceneRenamed)
	{
		if (onSubsceneRenamed != null)
		{
			this.OnSubsceneRenamedList.Add(onSubsceneRenamed);
		}
		else
		{
			// Error
		}
	}

	public void UnregisterOnSubsceneRenamed(Action<SubsceneId, string> onSubsceneRenamed)
	{
		this.OnSubsceneRenamedList.Remove(onSubsceneRenamed);
	}

	private void OnSubsceneAdded(SubsceneId subsceneName)
	{
		// Temporarily hijacking this as a "changed" On/Handle
		for (int registrarIndex = 0; registrarIndex < this.OnSubsceneAddedList.Count; registrarIndex++)
		{
			this.OnSubsceneAddedList[registrarIndex](subsceneName);
		}

	}

	private void OnSubsceneRemoved(SubsceneId subsceneName)
	{
		for (int registrarIndex = 0; registrarIndex < this.OnSubsceneRemovedList.Count; registrarIndex++)
		{
			this.OnSubsceneRemovedList[registrarIndex](subsceneName);
		}
	}

	private void OnSubsceneRenamed(SubsceneId oldName, string newName)
	{
		for (int registrarIndex = 0; registrarIndex < this.OnSubsceneRenamedList.Count; registrarIndex++)
		{
			this.OnSubsceneRenamedList[registrarIndex](oldName, newName);
		}
	}
}
