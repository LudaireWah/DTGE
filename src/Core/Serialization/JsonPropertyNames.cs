namespace DtgeCore.Serialization;

/**
 * This class contains shortened property names for cutting down the size of DTGE Scene files. It
 * should be alphebatized based on the Json property name (not the C# variable name) and aligned
 * with the rest to make it obvious if there's accidental overlap, which should be avoided.
 * 
 * This is mainly used for the version data, as that has a long name, is very common, and it's
 * important to make sure there isn't overlap in the names since inheritence is involved.
 */
public static class DtgeScenePropertyNames
{
	// This list should be alphebatized based on the Json property name so it's easier to spot
	//   any collisions, which should be avoided.
	public const string SceneElementV0 =	"elV0";
	public const string OptionV0 =			"opV0";
	public const string SceneV0 =			"scV0";
	public const string SnippetSimpleV0 =	"snSiV0";
	public const string SnippetSubsceneV0 = "SnSuV0";
	public const string SnippetV0 =			"snV0";
	public const string SubsceneV0 =		"suV0";
	public const string VariationV0 =		"vaV0";
}
