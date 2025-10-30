using System;

using XRL.Core;

using UD_Modding_Toolbox;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_CellHighlighter : IScribedPart
    {
        public const string DEBUG_HIGHLIGHT_CELLS = "UD_Debug_HighlightCells";

        public static readonly int ICON_COLOR_PRIORITY = 999;

        public string TileColor;
        public string DetailColor;
        public string BackgroundColor;

        public int HighlightPriority;

        public bool DoHighlight => Options.DebugVerbosity > 3 && (The.Game?.GetBooleanGameState(DEBUG_HIGHLIGHT_CELLS)).GetValueOrDefault();

        public UD_CellHighlighter()
        {
            BackgroundColor = "k";
            HighlightPriority = 0;
        }

        public override bool Render(RenderEvent E)
        {
            bool doHighlight = true;
            if ((XRLCore.FrameTimer.ElapsedMilliseconds & 0x7F) == 0L)
            {
                doHighlight = DoHighlight;
            }
            if (doHighlight)
            {
                if (ParentObject.InheritsFrom("Cell Highlighter")
                    && ParentObject.Render is Render render)
                {
                    render.Visible = true;
                }

                E.ApplyColors(
                    Foreground: TileColor ?? E.DetailColor,
                    Background: BackgroundColor,
                    Detail: DetailColor ?? E.DetailColor,
                    ForegroundPriority: ICON_COLOR_PRIORITY,
                    BackgroundPriority: ICON_COLOR_PRIORITY,
                    DetailPriority: ICON_COLOR_PRIORITY);
            }
            else
            {
                if (ParentObject.InheritsFrom("Cell Highlighter")
                    && ParentObject.Render is Render render)
                {
                    render.Visible = false;
                }
            }
            return base.Render(E);
        }

        public override void Remove()
        {
            base.Remove();
            if (ParentObject != null
                && ParentObject.InheritsFrom("Cell Highlighter"))
            {
                ParentObject.Obliterate();
            }
        }
    }
}