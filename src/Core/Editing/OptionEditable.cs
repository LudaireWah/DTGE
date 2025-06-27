using DtgeCore.Serialization;

namespace DtgeCore.Editing;

/**
 * OptionEditable is the editable version of the Option to be used in editors for authoring DTGE
 * games. For more detailed information on how Editables work, see SceneEditable.cs.
 */
public class OptionEditable : Option
{
    public new string Name
    {
        get { return base.Name; }
        set
        {
            base.Name = value;
			this.notifyParentOfEdit();
		}
    }
    public new string TargetSceneId
    {
        get { return base.TargetSceneId; }
        set
        {
            base.TargetSceneId = value;
			this.notifyParentOfEdit();
		}
    }
    public new string DisplayName
    {
        get { return base.DisplayName; }
        set
        {
            base.DisplayName = value;
			this.notifyParentOfEdit();
		}
    }
    public new string Tooltip
    {
        get { return base.Tooltip; }
        set
        {
            base.Tooltip = value;
			this.notifyParentOfEdit();
		}
    }
    public new bool Enabled
    {
        get { return base.Enabled; }
        set
        {
            base.Enabled = value;
			this.notifyParentOfEdit();
		}
    }

	private SceneEditable parentSceneEditable;

	public OptionEditable(SceneEditable parentSceneEditable)
        : base(parentSceneEditable)
    {
        this.parentSceneEditable = parentSceneEditable;
	}

    public OptionEditable(SceneEditable parentSceneEditable, OptionSerializable optionSerializable)
        : base(parentSceneEditable, optionSerializable)
    {
        this.parentSceneEditable = parentSceneEditable;
    }

    public OptionSerializable ToSerializable()
    {
        OptionSerializable serializable = this.CreateSerializable<OptionSerializable>();

        serializable.Name = this.Name;
        serializable.TargetSceneId = this.TargetSceneId;
        serializable.DisplayName = this.DisplayName;
        serializable.Tooltip = this.Tooltip;
        serializable.Enabled = this.Enabled;

        return serializable;
    }

	public override string ToString()
	{
		string tooltipText = "(None)";
		if (this.Tooltip != null)
		{
			tooltipText = this.Tooltip;

		}

		string optionString =
			"Option:\n" +
			"  Id: " + this.Id + "\n" +
			"  Target Scene: " + this.TargetSceneId + "\n" +
			"  Display Name: " + this.DisplayName + "\n" +
			"  Tooltip: " + tooltipText + "\n" +
			"  Enabled: " + this.Enabled + "\n";
		return optionString;
	}

	private void notifyParentOfEdit()
    {
        if (this.parentSceneEditable != null)
        {
            this.parentSceneEditable.NotifyUIUpdateNeeded();
        }
    }
}
