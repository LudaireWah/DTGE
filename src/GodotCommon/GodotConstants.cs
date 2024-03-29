﻿using Godot;
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

    // Modifier Key enums
    public const Key KEY_CTRL_N = (Key)(KeyModifierMask.MaskCtrl) | Key.N;
    public const Key KEY_CTRL_O = (Key)(KeyModifierMask.MaskCtrl) | Key.O;
    public const Key KEY_CTRL_S = (Key)(KeyModifierMask.MaskCtrl) | Key.S;
    public const Key KEY_CTRL_SHIFT_S = (Key)(KeyModifierMask.MaskCtrl) | (Key)(KeyModifierMask.MaskShift) | Key.S;
}

