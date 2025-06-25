using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DtgeCore;

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

	public string Serialize()
	{
		string toReturn = JsonSerializer.Serialize(this);
		return toReturn;
	}

	private void notifyParentOfEdit()
    {
        if (this.parentSceneEditable == null)
        {
            this.parentSceneEditable.NotifyUIUpdateNeeded();
        }
    }
}
