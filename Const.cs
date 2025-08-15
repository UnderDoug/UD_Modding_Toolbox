using static UD_Modding_Toolbox.Options;

namespace UD_Modding_Toolbox
{
    public static class Const
    {
        public const string MOD_ID = "UD_Modding_Toolbox";
        public const string HNPS_GIGANTISMPLUS_MOD_ID = "gigantism_Plus";
        public const string UD_BLINK_MUTATION_MOD_ID = "UD_Blink_Mutation";

        public const string DEBUG_OBJECT_CREATION_ANALYSIS = "UD_Debug_DoObjectCreationAnalysis";
        public const string DEBUG_HIGHLIGHT_CELLS = "UD_Debug_HighlightCells";

        public const string NULL = "\"null\"";

        public const string TICK = "\u221A";  // √
        public const string CROSS = "\u0058"; // X
        public const string BLOCK = "\u2588"; // █
        public const string STAR = "\u263C";  // ☼
        public const string SMLE = "\u263A";  // ☺︎
        public const string SMLE2 = "\u263B"; // ☻

        public const string VANDR = "\u251C"; // ├
        public const string VONLY = "\u2502"; // │
        public const string TANDR = "\u2514"; // └
        public const string HONLY = "\u2500"; // ─
        public const string SPACE = "\x20";   //" "
        public const string NBSP  = "\xFF";   //" " (NBSP)

        public const string ITEM = VANDR + HONLY + HONLY + SPACE; // "├── "
        public const string BRAN = VONLY + SPACE + SPACE + SPACE; // "│   "
        public const string LAST = TANDR + HONLY + HONLY + SPACE; // "└── "
        public const string DIST = SPACE + SPACE + SPACE + SPACE; // "    "
        public const string NACE = NBSP + NBSP + NBSP + NBSP;     // "    " (NBSP)
    }
}