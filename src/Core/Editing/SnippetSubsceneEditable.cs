using DtgeCore.Serialization;
using System.Runtime.CompilerServices;

namespace DtgeCore.Editing;

public class SnippetSubsceneEditable : SnippetSubscene, ISnippetEditable
{
	private SceneEditable parentSceneEditable;

	public SnippetSubsceneEditable(SceneEditable parentSceneEditable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;
	}

	public SnippetSubsceneEditable(SceneEditable parentSceneEditable, ISnippetEditable other)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;

		for (int subsceneIndex = 0;
			subsceneIndex < this.parentSceneEditable.GetSubsceneCount();
			subsceneIndex++)
		{
			Subscene subscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
			if (subsceneIndex < other.GetVariationCount())
			{
				this.VariationsBySubsceneId[subscene.Id] = new VariationEditable(
					parentSceneEditable,
					subscene.Name,
					other.GetVariationEditable(subsceneIndex).Text);
			}
			else
			{
				this.VariationsBySubsceneId[subscene.Id] =
					new VariationEditable(parentSceneEditable, subscene.Name, string.Empty);
			}
		}
	}

	public SnippetSubsceneEditable(
		SceneEditable parentSceneEditable,
		SnippetSubsceneSerializable serializable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;

		foreach (SUID subsceneId in serializable.VariationsBySubsceneId.Keys)
		{
			this.VariationsBySubsceneId[subsceneId] = new VariationEditable(
				parentSceneEditable,
				serializable.VariationsBySubsceneId[subsceneId]);
		}
	}

	public SnippetSerializable ToSerializable()
	{
		SnippetSubsceneSerializable serializable =
			this.CreateSerializable<SnippetSubsceneSerializable>();

		serializable.Mode = this.Mode;

		foreach (SUID subsceneId in this.VariationsBySubsceneId.Keys)
		{
			VariationEditable variationEditable =
				this.VariationsBySubsceneId[subsceneId] as VariationEditable;
			serializable.VariationsBySubsceneId[subsceneId] =
				variationEditable.ToSerializable();
		}

		return serializable;
	}

	public static bool CanConvertFrom(
		SceneEditable parentSceneEditable,
		ISnippetEditable otherSnippetEditable,
		out string message)
	{
		bool canConvert = false;
		message = string.Empty;

		if (otherSnippetEditable.GetVariationCount() <= parentSceneEditable.GetSubsceneCount())
		{
			canConvert = true;
		}
		else if (parentSceneEditable.GetSubsceneCount() == 0)
		{
			message = "To switch to subscene mode, you have to add at least one subscene to the scene.";
		}
		else
		{
			message = "To switch to subscene mode, delete variations manually until you have no more than the number of subscenes.";
		}

		return canConvert;
	}

	public string CalculateTextStable()
	{
		return this.CalculateText();
	}

	public string GetCopyableText(string variationBoundaryMarker)
	{
		string copyableVariationText = "";

		for (int subsceneIndex = 0; subsceneIndex < this.VariationsBySubsceneId.Count; subsceneIndex++)
		{
			Subscene subscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
			copyableVariationText += this.VariationsBySubsceneId[subscene.Id].Text;
			if (subsceneIndex < this.parentSceneEditable.GetSubsceneCount() - 1)
			{
				copyableVariationText += variationBoundaryMarker;
			}
		}

		return copyableVariationText;
	}

	public void RestoreFromPastedText(string[] pastedTextSplitByVariation)
	{
		if (pastedTextSplitByVariation.Length != this.parentSceneEditable.GetSubsceneCount())
		{
			GlobalErrorHandler.InvokeError("There was a mismatch between subscene count and pasted variation count when restoring from pasted text.");
		}
		else
		{
			for (int subsceneIndex = 0; subsceneIndex < this.VariationsBySubsceneId.Count; subsceneIndex++)
			{
				Subscene subscene = this.parentSceneEditable.GetSubscene(subsceneIndex);
				VariationEditable variationEditable =
					this.VariationsBySubsceneId[subscene.Id] as VariationEditable;
				variationEditable.Text = pastedTextSplitByVariation[subsceneIndex];
			}
		}
	}

	public bool CanEditVariationCount()
	{
		return false;
	}

	public VariationEditable GetVariationEditable(int variationIndex)
	{
		Subscene subscene = this.parentSceneEditable.GetSubscene(variationIndex);
		return this.VariationsBySubsceneId[subscene.Id] as VariationEditable;
	}

	public int GetVariationCount()
	{
		return this.parentSceneEditable.GetSubsceneCount();
	}

	public bool AddVariation()
	{
		GlobalErrorHandler.InvokeError("An attempt was made to add a new variation to a subscene snippet.");

		return false;
	}

	public bool RemoveVariationEditable(int variationIndex)
	{
		GlobalErrorHandler.InvokeError("An attempt was made to remove a variation from a subscene snippet.");

		return false;
	}

	public void UpdateVariationsFromSubscenes()
	{
		for (int subsceneIndex = 0;
			subsceneIndex < this.parentSceneEditable.GetSubsceneCount();
			subsceneIndex++)
		{
			SubsceneEditable subsceneEditable =
				this.parentSceneEditable.GetSubscene(subsceneIndex);
			if (!this.VariationsBySubsceneId.ContainsKey(subsceneEditable.Id))
			{
				this.VariationsBySubsceneId[subsceneEditable.Id] = new VariationEditable(
					this.parentSceneEditable,
					subsceneEditable.Name,
					string.Empty);
			}
			else
			{
				VariationEditable variationEditable =
					this.VariationsBySubsceneId[subsceneEditable.Id] as VariationEditable;
				variationEditable.Name = subsceneEditable.Name;
			}
		}

		if (this.VariationsBySubsceneId.Count > this.parentSceneEditable.GetSubsceneCount())
		{
			foreach (SUID subsceneId in this.VariationsBySubsceneId.Keys)
			{
				if (!this.parentSceneEditable.HasMatchingSubscene(subsceneId))
				{
					this.VariationsBySubsceneId.Remove(subsceneId);
				}
			}
		}
	}
}
