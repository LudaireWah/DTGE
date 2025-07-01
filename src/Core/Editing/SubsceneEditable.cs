using DtgeCore.Serialization;

namespace DtgeCore.Editing;

/**
 * SubsceneEditable is the editable version of the Subscene to be used in editors for authoring
 * DTGE games. For more detailed information on how Editables work, see SceneEditable.cs.
 */
public class SubsceneEditable : Subscene
{
	public const string DEFAULT_NEW_SUBSCENE_NAME = "New Subscene";

	public new string Name
	{
		get { return base.Name; }
		set
		{
			base.Name = value;
			this.notifyParentOfEdit();
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

		serializable.Id = this.Id;
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
			this.parentSceneEditable.NotifySubsceneSnippetsOfChange();
		}
	}
}
