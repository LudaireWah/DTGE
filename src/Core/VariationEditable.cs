namespace DtgeCore;

public class VariationEditable : Variation
{
	private SceneEditable parentSceneEditable;

	public new string Name
	{
		get { return base.Name; }
		set
		{
			this.notifyParentOfEdit();
			base.Name = value;
		}
	}
	public new string Text
	{
		get { return base.Text; }
		set
		{
			this.notifyParentOfEdit();
			base.Text = value;
		}
	}

	public VariationEditable(SceneEditable parentSceneEditable)
		: base(parentSceneEditable)
	{
		this.parentSceneEditable = parentSceneEditable;
	}

	public VariationEditable(SceneEditable parentSceneEditable, string name, string text)
		: base(parentSceneEditable, name, text)
	{
		this.parentSceneEditable = parentSceneEditable;
	}

	private void notifyParentOfEdit()
	{
		if (this.parentSceneEditable == null)
		{
			this.parentSceneEditable.NotifyUIUpdateNeeded();
		}
	}
}
