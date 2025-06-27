using DtgeCore.Serialization;

namespace DtgeCore.Editing;

public class SubsceneEditable : Subscene
{
	public const string DEFAULT_NEW_SUBSCENE_NAME = "New Subscene";

	public new string Name
	{
		get { return base.Name; }
		set
		{
			this.notifyParentOfEdit();
			base.Name = value;
		}
	}

	private SceneEditable parentSceneEditable;

	public SubsceneEditable(SceneEditable parentSceneEditable)
		: base(parentSceneEditable, SubsceneEditable.DEFAULT_NEW_SUBSCENE_NAME)
	{
		this.parentSceneEditable = parentSceneEditable;
	}

	public SubsceneEditable(SceneEditable parentSceneEditable, SubsceneSerializable serializable)
		: base(parentSceneEditable, serializable)
	{
		this.parentSceneEditable = parentSceneEditable;
	}

	public SubsceneSerializable ToSerializable()
	{
		SubsceneSerializable serializable = this.CreateSerializable<SubsceneSerializable>();

		serializable.Name = this.Name;

		return serializable;
	}

	public SubsceneEditable(SceneEditable parentSceneEditable, string name)
		: base(parentSceneEditable, name)
	{
		this.parentSceneEditable = parentSceneEditable;
	}

	private void notifyParentOfEdit()
	{
		if (this.parentSceneEditable != null)
		{
			this.parentSceneEditable.NotifyUIUpdateNeeded();
		}
	}
}
