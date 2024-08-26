using Godot;

[Tool]
public partial class MultiClickButton : Button
{
	public override void _EnterTree()
	{
		Pressed += Clicked;
	}

	public void Clicked()
	{
		GD.Print("You clicked me!");
	}
}
