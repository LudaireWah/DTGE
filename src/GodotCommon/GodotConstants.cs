using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtgeGodotCommon;

public class GodotConstants
{
	// Scene Paths
	public const string GAME_SCENE_PATH = "src/Game/game.tscn";

	public const string OPTION_EDIT_PANEL_PATH = "src/Editor/option_edit_panel.tscn";
	public const string SNIPPET_PANEL_CONTAINER_PATH = "src/Editor/snippet_panel_container.tscn";
	public const string SUBSCENE_PANEL_CONTAINER_PATH = "src/Editor/subscene_panel_container.tscn";

	// Modifier Key enums
	public const Key KEY_CTRL_N = (Key)(KeyModifierMask.MaskCtrl) | Key.N;
	public const Key KEY_CTRL_O = (Key)(KeyModifierMask.MaskCtrl) | Key.O;
	public const Key KEY_CTRL_S = (Key)(KeyModifierMask.MaskCtrl) | Key.S;
	public const Key KEY_CTRL_SHIFT_S = (Key)(KeyModifierMask.MaskCtrl) | (Key)(KeyModifierMask.MaskShift) | Key.S;
	public const Key KEY_CTRL_SHIFT_ALT_S = (Key)(KeyModifierMask.MaskCtrl) | (Key)(KeyModifierMask.MaskShift) | (Key)(KeyModifierMask.MaskAlt) | Key.S;
	public const Key KEY_CTRL_F5 = (Key)(KeyModifierMask.MaskCtrl) | Key.F5;
}

