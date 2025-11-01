using System;

using XRL.Core;

using UD_Modding_Toolbox;
using XRL.Rules;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_CellHighlighter : IScribedPart
    {
        public const string DEBUG_HIGHLIGHT_CELLS = "UD_Debug_HighlightCells";

        public static readonly int ICON_COLOR_PRIORITY = 999;

        private string OriginalTile => 
            ParentObject?
            .GetBlueprint()?
            .GetPartParameter<string>(nameof(Parts.Render), nameof(Parts.Render.Tile));

        private long FrameOffset => (long)Stat.RandomCosmetic(-5, 5);

        public string TileColor;
        public string DetailColor;
        public string BackgroundColor;

        public UnityEngine.Color CharTileColor;
        public UnityEngine.Color CharForeground;
        public UnityEngine.Color CharDetail;

        public int HighlightPriority;

        public bool DoHighlight;
        public bool CheckDoHighlight => Options.DebugVerbosity > 3 && (The.Game?.GetBooleanGameState(DEBUG_HIGHLIGHT_CELLS)).GetValueOrDefault();

        public UD_CellHighlighter()
        {
            DoHighlight = true;
            BackgroundColor = "k";
            HighlightPriority = 0;

            CharTileColor = default;
            CharForeground = default;
            CharDetail = default;
        }

        public override bool Render(RenderEvent E)
        {
            if ((XRLCore.FrameTimer.ElapsedMilliseconds & 0x7F) == 0L)
            {
                DoHighlight = CheckDoHighlight;
            }
            if (DoHighlight
                && ParentObject.CurrentCell is Cell cell
                && cell.IsVisible()
                && (cell.GetHighestRenderLayerObject(GO => GO != ParentObject) ?? ParentObject) is GameObject highlightObject
                && highlightObject.Render is Render highlightRender
                && ParentObject.Render is Render parentRender
                && XRLCore.FrameTimer.ElapsedMilliseconds % 1000L < (250 + FrameOffset))
            {
                parentRender.Visible = true;
                if (highlightObject == ParentObject)
                {
                    parentRender.Tile = OriginalTile;
                }
                else
                {
                    parentRender.Tile = highlightRender.Tile;
                }
                E.ApplyColors(
                    Foreground: TileColor ?? E.ColorString,
                    Background: BackgroundColor,
                    Detail: DetailColor ?? E.DetailColor,
                    ForegroundPriority: ICON_COLOR_PRIORITY,
                    BackgroundPriority: ICON_COLOR_PRIORITY,
                    DetailPriority: ICON_COLOR_PRIORITY);
            }
            else
            {
                if (ParentObject.InheritsFrom("UD_Cell_Highlighter")
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
                && ParentObject.InheritsFrom("UD_Cell_Highlighter"))
            {
                ParentObject.Obliterate();
            }
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("BeginMove");
            base.Register(Object, Registrar);
        }
        public override bool WantEvent(int ID, int Cascade)
        {
            return base.WantEvent(ID, Cascade)
                || ID == LeavingCellEvent.ID;
        }
        public override bool HandleEvent(LeavingCellEvent E)
        {
            return false;
        }
        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeginMove")
            {
                return false;
            }
            return base.FireEvent(E);
        }
    }
}