using DtgeCore.Serialization;

namespace DtgeCore.Editing;

/**
 * VariationEditable is the editable version of the Variation to be used in editors for authoring
 * DTGE games. For more detailed information on how Editables work, see SceneEditable.cs.
 */
public class VariationEditable : Variation
{
	private SceneEditable parentSceneEditable;

	public new string Name
	{
		get { return base.Name; }
		set
		{
			if (base.Name != value)
			{
				base.Name = value;
				this.notifyParentOfEdit();
			}
		}
	}
	public new string Text
	{
		get { return base.Text; }
		set
		{
			if (base.Text != value)
			{
				base.Text = value;
				this.notifyParentOfEdit();
			}
		}
	}

	public VariationEditable(SceneEditable parentSceneEditable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;
	}

	public VariationEditable(
		SceneEditable parentSceneEditable,
		VariationSerializable serializable)
		:base(parentSceneEditable, serializable)
	{
		this.parentSceneEditable = parentSceneEditable;
	}

	public VariationEditable(SceneEditable parentSceneEditable, VariationEditable other)
		: base(parentSceneEditable)
	{
		this.Name = other.Name;
		this.Text = other.Text;
	}

	public VariationEditable(SceneEditable parentSceneEditable, string name, string text)
		: base(parentSceneEditable, name, text)
	{
		this.parentSceneEditable = parentSceneEditable;
	}

	public VariationSerializable ToSerializable()
	{
		VariationSerializable serializable = this.CreateSerializable<VariationSerializable>();

		serializable.Id = this.Id;
		serializable.Name = this.Name;
		serializable.Text = this.Text;

		return serializable;
	}

	private void notifyParentOfEdit()
	{
		if (this.parentSceneEditable != null)
		{
			this.parentSceneEditable.NotifyUIUpdateNeeded();
		}
	}
}
