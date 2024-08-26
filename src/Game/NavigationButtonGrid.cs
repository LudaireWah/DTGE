using Godot;
using System;

namespace DtgeGame;

/**
 * NavigationButtonGrid is a GridContainer that contains a set of navigation
 * buttons used to present options to players. The NavigationButtonGrid is
 * responsible for tracking all the buttons, binding them to the right option,
 * and owns the layout as laid out in the Godot editor.
 */
public partial class NavigationButtonGrid : GridContainer
{

	private NavigationButton button1;
	private NavigationButton button2;
	private NavigationButton button3;
	private NavigationButton button4;
	private NavigationButton button5;
	private NavigationButton button6;
	private NavigationButton button7;
	private NavigationButton button8;
	private NavigationButton button9;
	private NavigationButton button10;
	private NavigationButton button11;
	private NavigationButton button12;
	private NavigationButton button13;
	private NavigationButton button14;
	private NavigationButton button15;
	#region compile_assert
	const uint DTGE_SCENE_MAX_OPTIONS_SHOULD_MATCH_BUTTON_COUNT = (DtgeCore.Scene.MAX_OPTION_NUMBER == 15 ? 0 : -1);
	#endregion
	private NavigationButton[] allButtons;

	private int currentButtonToBind = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.button1 = GetNode<NavigationButton>("NavigationButton1");
		this.button2 = GetNode<NavigationButton>("NavigationButton2");
		this.button3 = GetNode<NavigationButton>("NavigationButton3");
		this.button4 = GetNode<NavigationButton>("NavigationButton4");
		this.button5 = GetNode<NavigationButton>("NavigationButton5");
		this.button6 = GetNode<NavigationButton>("NavigationButton6");
		this.button7 = GetNode<NavigationButton>("NavigationButton7");
		this.button8 = GetNode<NavigationButton>("NavigationButton8");
		this.button9 = GetNode<NavigationButton>("NavigationButton9");
		this.button10 = GetNode<NavigationButton>("NavigationButton10");
		this.button11 = GetNode<NavigationButton>("NavigationButton11");
		this.button12 = GetNode<NavigationButton>("NavigationButton12");
		this.button13 = GetNode<NavigationButton>("NavigationButton13");
		this.button14 = GetNode<NavigationButton>("NavigationButton14");
		this.button15 = GetNode<NavigationButton>("NavigationButton15");
		#region compile_assert
#pragma warning disable CS0219 // Variable is assigned but its value is never used
		const uint DTGECORE_OPTION_MAX_OPTIONS_SHOULD_MATCH_2 = (DtgeCore.Scene.MAX_OPTION_NUMBER == 15 ? 0 : -1);
#pragma warning restore CS0219 // Variable is assigned but its value is never used
		#endregion

		this.allButtons = new NavigationButton[DtgeCore.Scene.MAX_OPTION_NUMBER];
		this.allButtons[0] = this.button1;
		this.allButtons[1] = this.button2;
		this.allButtons[2] = this.button3;
		this.allButtons[3] = this.button4;
		this.allButtons[4] = this.button5;
		this.allButtons[5] = this.button6;
		this.allButtons[6] = this.button7;
		this.allButtons[7] = this.button8;
		this.allButtons[8] = this.button9;
		this.allButtons[9] = this.button10;
		this.allButtons[10] = this.button11;
		this.allButtons[11] = this.button12;
		this.allButtons[12] = this.button13;
		this.allButtons[13] = this.button14;
		this.allButtons[14] = this.button15;
		#region compile_assert
#pragma warning disable CS0219 // Variable is assigned but its value is never used
		const uint DTGECORE_OPTION_MAX_OPTIONS_SHOULD_MATCH_3 = (DtgeCore.Scene.MAX_OPTION_NUMBER == 15 ? 0 : -1);
#pragma warning restore CS0219 // Variable is assigned but its value is never used
		#endregion

		this.clearButtons();
	}

	public void BindSceneOptionsToButtons(DtgeCore.Scene scene, Action<DtgeCore.Option> action)
	{
		this.clearButtons();

		if (scene == null)
		{
			return;
		}
		else
		{
			int optionCount = scene.GetOptionCount();
			for (int optionIndex = 0; optionIndex < optionCount; optionIndex++)
			{
				this.allButtons[optionIndex].BindButton(scene.GetOption(optionIndex), action);
			}
		}
	}

	private void clearButtons()
	{
		for (int buttonIndex = 0; buttonIndex < DtgeCore.Scene.MAX_OPTION_NUMBER; buttonIndex++)
		{
			NavigationButton currentButton = this.allButtons[buttonIndex];
			currentButton.BindButton(null, null);
		}

		this.currentButtonToBind = 0;
	}
}
