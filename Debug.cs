using HarmonyLib;
using NAudio.SoundFont;
using Qud.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UD_Modding_Toolbox;
using XRL;
using XRL.Core;
using XRL.Wish;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using static UD_Modding_Toolbox.Const;
using Debug = UD_Modding_Toolbox.Debug;
using Options = UD_Modding_Toolbox.Options;

namespace UD_Modding_Toolbox
{
    [HasWishCommand]
    public static class Debug
    {
        private static int VerbosityOption => Options.DebugVerbosity;
        // Verbosity translates in roughly the following way:
        // 0 : Critical. Use sparingly, if at all, as they show up without the option. Move these to 1 when pushing to main.
        // 1 : Show. Initial debugging entries. Broad, general "did it happen?" style entries for basic trouble-shooting.
        // 2 : Verbose. Entries have more information in them, indicating how values are passed around and changed.
        // 3 : Very Verbose. Entries in more locations, or after fewer steps. These contribute to tracing program flow.
        // 4 : Maximally Verbose. Just like, all of it. Every step of a process, as much detail as possible.

        private static bool IncludeInMessage => Options.DebugIncludeInMessage;

        public static int LastIndent = 0;

        private static void Message(string Text)
        {
            XRL.Messages.MessageQueue.AddPlayerMessage("{{Y|" + Text + "}}");
        }

        private static void Log(string Text)
        {
            UnityEngine.Debug.LogError(Text);
        }

        public static void Entry(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Debug.Indent(Verbosity, Text, Indent, Toggle: Toggle);
        }

        public static void Entry(string Text, int Indent = 0, bool Toggle = true)
        {
            int Verbosity = 0;
            Debug.Indent(Verbosity, Text, Indent, Toggle: Toggle);
        }

        public static void Entry(int Verbosity, string Label, string Text, int Indent = 0, bool Toggle = true)
        {
            string output = Label + ": " + Text;
            Entry(Verbosity, output, Indent, Toggle: Toggle);
        }

        public static void Indent(int Verbosity, string Text, int Spaces = 0, bool Toggle = true)
        {
            if (Verbosity > VerbosityOption || !Toggle) return;
            int factor = 4;
            // NBSP  \u00A0
            // Space \u0020
            string space = "\u0020";
            string indent = "";
            for (int i = 0; i < Spaces * factor; i++)
            {
                indent += space;
            }
            LastIndent = Spaces;
            string output = indent + Text;
            Log(output);
            if (IncludeInMessage)
                Message(output);
        }

        public static void Divider(int Verbosity = 0, string String = null, int Count = 60, int Indent = 0, bool Toggle = true)
        {
            string output = "";
            if (String == null) String = "\u003D"; // =
            else String = String[..1];
            for (int i = 0; i < Count; i++)
            {
                output += String;
            }
            Entry(Verbosity, output, Indent, Toggle: Toggle);
        }

        public static void Header(int Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            string divider = "\u2550"; // ═ (box drawing, double horizontal)
            Divider(Verbosity, divider, Toggle: Toggle);
            string output = "@START: " + ClassName + "." + MethodName;
            Entry(Verbosity, output, Toggle: Toggle);
        }
        public static void Footer(int Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            string divider = "\u2550"; // ═ (box drawing, double horizontal)
            string output = "///END: " + ClassName + "." + MethodName + " !//";
            Entry(Verbosity, output, Toggle: Toggle);
            Divider(Verbosity, divider, Toggle: Toggle);
        }

        public static void DiveIn(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Divider(Verbosity, "\u003E", 25, Indent); // >
            Entry(Verbosity, Text, Indent + 1, Toggle: Toggle);
        }
        public static void DiveOut(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Entry(Verbosity, Text, Indent + 1);
            Divider(Verbosity, "\u003C", 25, Indent, Toggle: Toggle); // <
        }

        public static void Warn(int Verbosity, string ClassName, string MethodName, string Issue = null, int Indent = 0)
        {
            string noIssue = "Something didn't go as planned";
            string output = $">!< WARN | {ClassName}.{MethodName}: {Issue ?? noIssue}";
            Entry(Verbosity, output, Indent, Toggle: true);
        }

        public static void LoopItem(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = null, bool Toggle = true)
        {
            string good = TICK;  // √
            string bad = CROSS;  // X
            string goodOrBad = string.Empty;
            if (Good != null) goodOrBad = ((bool)Good ? good : bad) + "\u005D "; // ]
            string output = Text != string.Empty ? Label + ": " + Text : Label;
            Entry(Verbosity, "\u005B" + goodOrBad + output, Indent, Toggle: Toggle);
        }
        public static void CheckYeh(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = true, bool Toggle = true)
        {
            LoopItem(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }
        public static void CheckNah(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = false, bool Toggle = true)
        {
            LoopItem(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }

        public static void TreeItem(int Verbosity, string Label, string Text = "", bool Last = false, int Branch = 0, int Distance = 0, int Indent = 0, bool Toggle = true)
        {
            // ITEM: "├── "
            // BRAN: "│   "
            // LAST: "└── "
            // DIST: "    "

            string Output = string.Empty;
            for (int i = 0; i < Branch; i++)
            {
                Output += BRAN;
            }
            for (int i = 0; i < Distance; i++)
            {
                Output += DIST;
            }
            Output += Last ? LAST : ITEM;

            Output += Text != string.Empty ? Label + ": " + Text : Label;

            Entry(Verbosity, Output, Indent, Toggle: Toggle);
        }
        public static void TreeLast(int Verbosity, string Label, string Text = "", int Branch = 0, int Distance = 0, int Indent = 0, bool Toggle = true)
        {
            TreeItem(Verbosity, Label, Text, true, Branch, Distance, Indent, Toggle: Toggle);
        }

        // Class Specific Debugs
        public static void Vomit(int Verbosity, string Source, string Context = null, int Indent = 0, bool Toggle = true)
        {
            string context = Context == null ? "" : $"{Context}:";
            Entry(Verbosity, $"% Vomit: {Source} {context}", Indent, Toggle: Toggle);
        }

        public static Raffle<T> Vomit<T>(
            this Raffle<T> Raffle,
            int Verbosity,
            string Source,
            string Context = null,
            bool ShowChance = false,
            T Drawn = default,
            T Sampled = default,
            int Indent = 0,
            bool Toggle = true)
        {
            string context = Context == null ? "" : $"{Context}:";
            Entry(Verbosity, $"Vomit: {Source} {context}", Indent, Toggle: Toggle);

            bool noneDrawn = Equals(Drawn, default);
            T picked = !Equals(Drawn, default) ? Drawn : Sampled;
            bool nonePicked = Equals(picked, default);
            foreach (Raffle<T>.Entry entry in Raffle)
            {
                bool? wasPicked = nonePicked ? null : entry.Equals(picked, false);
                bool wasDrawn = !noneDrawn && entry.Equals(picked, false);
                int weightAdjust = wasDrawn ? 1 : 0;
                string chance = ShowChance
                    ? Math.Round(Raffle.GetTotalChance(entry) * 100f, 2).ToString() + "%"
                    : "";
                string chanceText = ShowChance ? chance + ", " : "";
                string message = (entry.Weight + weightAdjust).ToString() + ", " + chanceText + entry.Token.ExtendedToString();
                LoopItem(Verbosity, message, Good: wasPicked, Indent: Indent + 1, Toggle: Toggle);
            }
            LastIndent = Indent;
            return Raffle;
        }

        public static MeleeWeapon Vomit(this MeleeWeapon MeleeWeapon, int Verbosity, string Title = null, List<string> Categories = null, int Indent = 0, bool Toggle = true)
        {
            int indent = Indent;
            Vomit(Verbosity, MeleeWeapon.ParentObject.DebugName, Title, Indent, Toggle);
            List<string> @default = new()
            {
                "Damage",
                "Combat",
                "Render",
                "etc"
            };
            Categories ??= @default;
            indent++;
            foreach (string category in Categories)
            {
                if (@default.Contains(category)) Entry(Verbosity, $"{category}", indent, Toggle: Toggle);
                indent++;
                switch (category)
                {
                    case "Damage":
                        LoopItem(Verbosity, "BaseDamage", $"{MeleeWeapon.BaseDamage}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "MaxStrengthBonus", $"{MeleeWeapon.MaxStrengthBonus}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "HitBonus", $"{MeleeWeapon.HitBonus}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "PenBonus", $"{MeleeWeapon.PenBonus}", indent, Toggle: Toggle);
                        break;
                    case "Combat":
                        LoopItem(Verbosity, "Stat", $"{MeleeWeapon.Stat}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "Skill", $"{MeleeWeapon.Skill}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "Slot", $"{MeleeWeapon.Slot}", indent, Toggle: Toggle);
                        break;
                    case "Render":
                        Render Render = MeleeWeapon.ParentObject.Render;
                        LoopItem(Verbosity, "DisplayName", $"{Render.DisplayName}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "Tile", $"{Render.Tile}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "ColorString", $"{Render.ColorString}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "DetailColor", $"{Render.DetailColor}", indent, Toggle: Toggle);
                        break;
                    case "etc":
                        LoopItem(Verbosity, "Ego", $"{MeleeWeapon.Ego}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "IsEquippedOnPrimary", $"{MeleeWeapon.IsEquippedOnPrimary()}", indent, Toggle: Toggle);
                        LoopItem(Verbosity, "IsImprovisedWeapon", $"{MeleeWeapon.IsImprovisedWeapon()}", indent, Toggle: Toggle);
                        break;
                }
                indent--;
            }
            return MeleeWeapon;
        }

        public static bool WasEventHandlerRegistered<H, E>(this XRLGame Game, bool Toggle = true)
            where H : IEventHandler
            where E : MinEvent, new()
        {
            bool flag = false;
            E e = new();
            if (Game != null && Game.RegisteredEvents.ContainsKey(e.ID))
            {
                Entry(2, $"Registered", $"{typeof(H).Name} ({typeof(E).Name}.ID: {e.ID})", Indent: 2, Toggle: Toggle);
                flag = true;
            }
            else if (Game != null)
            {
                Entry(2, $"Failed to register {typeof(H).Name} ({typeof(E).Name}.ID: {e.ID})", Indent: 2, Toggle: Toggle);
            }
            else
            {
                Entry(2, $"The.Game null, couldn't register {typeof(H).Name} ({typeof(E).Name}.ID: {e.ID})", Indent: 2, Toggle: Toggle);
            }
            return flag;
        }
        public static bool WasModEventHandlerRegistered<H, E>(this XRLGame Game, bool Toggle = true)
            where H : IEventHandler, IModEventHandler<E>
            where E : MinEvent, new()
        {
            return Game.WasEventHandlerRegistered<H, E>(Toggle: Toggle);
        }

        public static CodeInstruction Vomit(
            this CodeInstruction Instruction,
            int Pos,
            int PosPadding,
            Dictionary<Label, int> LabelInstructions = null,
            bool HaveILGen = false,
            bool IncludeEnd = false,
            bool Do = false)
        {
            if (!Do)
            {
                return Instruction;
            }
            string ciOperand = Instruction?.operand?.VomitOperand(PosPadding, LabelInstructions, HaveILGen, Do);
            string codePos = $"[{Pos.ToString().PadLeft(PosPadding, '0')}]";
            if (HaveILGen)
            {
                codePos = $"IL_{Pos:X4}:";
            }
            UnityEngine.Debug.Log($"{codePos} {Instruction.opcode,-10} {ciOperand}");
            if (IncludeEnd && Instruction.opcode.IsEndOfSection())
            {
                UnityEngine.Debug.Log("");
            }
            return Instruction;
        }

        public static CodeMatch Vomit(
            this CodeMatch CodeMatch,
            int Pos,
            int PosPadding,
            Dictionary<Label, int> LabelInstructions = null,
            bool HaveILGen = false,
            bool IncludeEnd = false,
            bool Do = false)
        {
            if (!Do)
            {
                return CodeMatch;
            }
            string ciOperand = CodeMatch?.operand?.VomitOperand(PosPadding, LabelInstructions, HaveILGen, Do);
            string codePos = $"[{Pos.ToString().PadLeft(PosPadding, '0')}]";
            if (HaveILGen)
            {
                codePos = $"IL_{Pos:X4}:";
            }
            UnityEngine.Debug.Log($"{codePos} {CodeMatch.opcode,-10} {ciOperand}");
            if (IncludeEnd && CodeMatch.opcode.IsEndOfSection())
            {
                UnityEngine.Debug.Log("");
            }
            return CodeMatch;
        }
        public static CodeMatch[] Vomit(
            this CodeMatch[] CodeMatchs,
            string Context = null,
            string EndContext = null,
            Dictionary<Label, int> LabelInstructions = null,
            bool HaveILGen = false,
            bool IncludeEnd = false,
            bool Do = false)
        {
            if (!Do)
            {
                return CodeMatchs;
            }
            int pos = 0;
            int posPadding = Math.Max(4, (CodeMatchs.Length + 1).ToString().Length);

            if (!Context.IsNullOrEmpty())
            {
                UnityEngine.Debug.Log(Context);
            }
            foreach (CodeMatch codeMatch in CodeMatchs)
            {
                bool includeEnd = (pos < CodeMatchs.Length - 1) && IncludeEnd;
                codeMatch.Vomit(pos++, posPadding, LabelInstructions, HaveILGen, includeEnd, Do);
            }
            if (!EndContext.IsNullOrEmpty())
            {
                UnityEngine.Debug.Log(EndContext);
            }
            return CodeMatchs;
        }

        public static string VomitOperand(
            this object Operand,
            int PosPadding,
            Dictionary<Label, int> LabelInstructions = null,
            bool HaveILGen = false,
            bool Do = false)
        {
            if (!Do)
            {
                return null;
            }
            string operandString = Operand?.ToString();
            if (Operand?.GetType() == typeof(string))
            {
                operandString = Operand?.ToString()?.ToLiteral(Quotes: true);
            }
            else
            if (Operand is Label operandLabel)
            {
                string labelString = "????";
                if (LabelInstructions.IsNullOrEmpty() && LabelInstructions.ContainsKey(operandLabel))
                {
                    labelString = LabelInstructions[operandLabel].ToString().PadLeft(PosPadding, '0');
                    if (HaveILGen)
                    {
                        labelString = $"IL_{LabelInstructions[operandLabel]:X4}";
                    }
                }
                operandString = $"[{labelString}]";
            }
            return operandString;
        }
        public static CodeMatcher Vomit(this CodeMatcher CodeMatcher, ILGenerator Generator, bool Do = false)
        {
            if (Do)
            {
                bool haveILGen = false && Generator != null;
                Dictionary<Label, int> labelInstructions = new();
                int originalPos = CodeMatcher.Pos;
                CodeMatcher.Start();
                do
                {
                    CodeInstruction ci = CodeMatcher.Instruction;
                    int labelPos = haveILGen ? Generator.ILOffset : CodeMatcher.Pos;
                    if (ci.labels.IsNullOrEmpty())
                    {
                        continue;
                    }
                    foreach (Label label in ci.labels)
                    {
                        if (!labelInstructions.ContainsKey(label))
                        {
                            labelInstructions.Add(label, labelPos);
                        }
                        else
                        {
                            labelInstructions[label] = labelPos;
                        }
                    }
                }
                while (CodeMatcher.Advance(1).IsValid);
                CodeMatcher.Start();
                
                int counterPadding = Math.Max(4, (CodeMatcher.Instructions().Count + 1).ToString().Length);
                do
                {
                    int counter = haveILGen ? Generator.ILOffset : CodeMatcher.Pos;
                    if (CodeMatcher.Instruction is CodeInstruction ci)
                    {
                        ci.Vomit(
                            Pos: counter, 
                            PosPadding: counterPadding,
                            LabelInstructions: labelInstructions,
                            HaveILGen: haveILGen,
                            IncludeEnd: true,
                            Do: Do);
                    }
                }
                while (CodeMatcher.Advance(1).IsValid);
                CodeMatcher.Start().Advance(originalPos);
            }
            return CodeMatcher;
        }
        public static CodeMatcher Vomit(this CodeMatcher CodeMatcher, bool Do = false)
        {
            return CodeMatcher.Vomit(null, Do);
        }
        public static IEnumerable<CodeInstruction> Vomit(this IEnumerable<CodeInstruction> Instructions, bool Do = false)
        {
            return new CodeMatcher(Instructions).Vomit(Do).InstructionEnumeration();
        }
        public static CodeMatcher VomitInstruction(this CodeMatcher CodeMatcher, string Context = null)
        {
            int counter = CodeMatcher.Pos;
            int counterPadding = Math.Max(4, (CodeMatcher.Length + 1).ToString().Length);

            CodeInstruction ci = CodeMatcher.Instruction;
            string ciOperand = ci?.operand?.ToString();
            if (ci?.operand?.GetType() == typeof(string))
            {
                ciOperand = ci.operand?.ToString()?.ToLiteral(Quotes: true);
            }
            UnityEngine.Debug.Log($"[{counter.ToString().PadLeft(counterPadding, '0')}] {ci.opcode,-10} {ciOperand} {Context}");
            return CodeMatcher;
        }

        public static string Vomit(this string @string, int Verbosity, string Label = "", bool LoopItem = false, bool? Good = null, int Indent = 0, bool Toggle = true)
        {
            string Output = Label != "" ? $"{Label}: {@string}" : @string;
            if (LoopItem) Debug.LoopItem(Verbosity, Output, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Output, Indent: Indent, Toggle: Toggle);
            return @string;
        }
        public static int Vomit(this int @int, int Verbosity, string Label = "", bool LoopItem = false, bool? Good = null, int Indent = 0, bool Toggle = true)
        {
            string Output = Label != "" ? $"{Label}: {@int}" : $"{@int}";
            if (LoopItem) Debug.LoopItem(Verbosity, Output, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Output, Indent: Indent, Toggle: Toggle);
            return @int;
        }
        public static bool Vomit(this bool @bool, int Verbosity, string Label = "", bool LoopItem = false, bool? Good = null, int Indent = 0, bool Toggle = true)
        {
            string Output = Label != "" ? $"{Label}: {@bool}" : $"{@bool}";
            if (LoopItem) Debug.LoopItem(Verbosity, Output, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Output, Indent: Indent, Toggle: Toggle);
            return @bool;
        }
        public static List<T> Vomit<T>(
            this List<T> List,
            int Verbosity,
            string Label = "",
            bool LoopItem = false,
            bool? Good = null,
            string DivAfter = "",
            int Indent = 0,
            bool Toggle = true)
        {
            string Output = Label != "" ? $"{Label}: {nameof(List)}" : $"{nameof(List)}";
            if (LoopItem) Debug.LoopItem(Verbosity, Output, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Output, Indent: Indent, Toggle: Toggle);
            foreach (T item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, item.ToString(), Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, item.ToString(), Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static List<object> Vomit(
            this List<object> List,
            int Verbosity,
            string Label,
            bool LoopItem = false,
            bool? Good = null,
            string DivAfter = "",
            int Indent = 0,
            bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (object item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static List<MutationEntry> Vomit(
            this List<MutationEntry> List,
            int Verbosity,
            string Label,
            bool LoopItem = false,
            bool? Good = null,
            string DivAfter = "",
            int Indent = 0,
            bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (MutationEntry item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item.Mutation.Name}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item.Mutation.Name}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static List<MutationCategory> Vomit(
            this List<MutationCategory> List,
            int Verbosity,
            string Label,
            bool LoopItem = false,
            bool? Good = null,
            string DivAfter = "",
            int Indent = 0,
            bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (MutationCategory item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item.Name}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item.Name}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static List<GameObject> Vomit(
            this List<GameObject> List,
            int Verbosity,
            string Label,
            bool LoopItem = false,
            bool? Good = null,
            string DivAfter = "",
            int Indent = 0,
            bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (GameObject item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item.DebugName}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item.DebugName}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static List<BaseMutation> Vomit(
            this List<BaseMutation> List,
            int Verbosity,
            string Label,
            bool LoopItem = false,
            bool? Good = null,
            string DivAfter = "",
            int Indent = 0,
            bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (BaseMutation item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item.GetMutationClass()}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item.GetMutationClass()}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }

        public static void InheritanceTree(GameObject Object, bool Toggle = true)
        {
            GameObjectBlueprint objectBlueprint = Object.GetBlueprint();

            Entry(4, $"objectBlueprint: {objectBlueprint.Name}", Indent: 0, Toggle: Toggle);
            GameObjectBlueprint shallowParent = objectBlueprint.ShallowParent;
            while (shallowParent != null)
            {
                Entry(4, $"shallowParent: {shallowParent.Name}", Indent: 0, Toggle: Toggle);
                shallowParent = shallowParent.ShallowParent;
            }
        }

        [WishCommand]
        public static void ToggleCellHighlighting()
        {
            The.Game.SetBooleanGameState(DEBUG_HIGHLIGHT_CELLS, !The.Game.GetBooleanGameState(DEBUG_HIGHLIGHT_CELLS));
        }
        [WishCommand]
        public static void debug_ToggleCH()
        {
            ToggleCellHighlighting();
        }

        [WishCommand]
        public static void RemoveCellHighlighting()
        {
            foreach (GameObject @object in The.ActiveZone.GetObjects())
            {
                UD_CellHighlighter highlighter = @object.RequirePart<UD_CellHighlighter>();
                @object.RemovePart(highlighter);
            }
        }
        public static Cell HighlightColor(this Cell Cell, string TileColor, string DetailColor, string BackgroundColor = "k", int Priority = 0, bool Solid = false)
        {
            if (!The.Game.HasBooleanGameState(DEBUG_HIGHLIGHT_CELLS))
                The.Game.SetBooleanGameState(DEBUG_HIGHLIGHT_CELLS, Options.DebugVerbosity > 3);
            if (Cell.IsEmpty() && Cell.GetFirstVisibleObject() == null && Cell.GetHighestRenderLayerObject() == null)
                Cell.AddObject("Cell Highlighter");

            GameObject gameObject = null;
            foreach (GameObject Object in Cell.GetObjects())
            {
                gameObject ??= Object;
                if (Object.Render.RenderLayer >= gameObject.Render.RenderLayer)
                    gameObject = Object;
            }
            gameObject = Cell.GetHighestRenderLayerObject();
            UD_CellHighlighter highlighter = gameObject.RequirePart<UD_CellHighlighter>();
            if (Priority >= highlighter.HighlightPriority)
            {
                highlighter.HighlightPriority = Priority;
                highlighter.TileColor = $"&{(!Solid ? TileColor : DetailColor)}";
                highlighter.DetailColor = $"{(!Solid ? DetailColor : BackgroundColor)}";
                highlighter.BackgroundColor = $"^{(!Solid ? BackgroundColor : TileColor)}";
            }
            return Cell;
        }
        public static Cell HighlightRed(this Cell Cell, int Priority = 0, bool Solid = false)
        {
            return Cell.HighlightColor(TileColor: "r", DetailColor: "R", BackgroundColor: "k", Priority, Solid);
        }
        public static Cell HighlightGreen(this Cell Cell, int Priority = 0, bool Solid = false)
        {
            return Cell.HighlightColor(TileColor: "g", DetailColor: "G", BackgroundColor: "k", Priority, Solid);
        }
        public static Cell HighlightYellow(this Cell Cell, int Priority = 0, bool Solid = false)
        {
            return Cell.HighlightColor(TileColor: "w", DetailColor: "W", BackgroundColor: "k", Priority, Solid);
        }
        public static Cell HighlightPurple(this Cell Cell, int Priority = 0, bool Solid = false)
        {
            return Cell.HighlightColor(TileColor: "m", DetailColor: "M", BackgroundColor: "k", Priority, Solid);
        }
        public static Cell HighlightBlue(this Cell Cell, int Priority = 0, bool Solid = false)
        {
            return Cell.HighlightColor(TileColor: "b", DetailColor: "B", BackgroundColor: "k", Priority, Solid);
        }
        public static Cell HighlightCyan(this Cell Cell, int Priority = 0, bool Solid = false)
        {
            return Cell.HighlightColor(TileColor: "c", DetailColor: "C", BackgroundColor: "k", Priority, Solid);
        }
    }
}

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_CellHighlighter : IScribedPart
    {
        public static readonly int ICON_COLOR_PRIORITY = 999;

        public string TileColor;
        public string DetailColor;
        public string BackgroundColor;

        public int HighlightPriority;

        public bool DoHighlight;

        public UD_CellHighlighter()
        {
            BackgroundColor = "k";
            DoHighlight = 
                Options.DebugVerbosity > 3
             && The.Game.GetBooleanGameState(DEBUG_HIGHLIGHT_CELLS);
            HighlightPriority = 0;
        }

        public override bool Render(RenderEvent E)
        {
            if ((XRLCore.FrameTimer.ElapsedMilliseconds & 0x7F) == 0L)
            {
                DoHighlight =
                    Options.DebugVerbosity > 3 
                 && The.Game.GetBooleanGameState(DEBUG_HIGHLIGHT_CELLS);
            }
            if (DoHighlight)
            {
                if (ParentObject.InheritsFrom("Cell Highlighter"))
                    ParentObject.Render.Visible = true;

                E.ApplyColors(
                    Foreground: TileColor ?? E.DetailColor, 
                    Background: BackgroundColor, 
                    Detail: DetailColor ?? E.DetailColor,
                    ICON_COLOR_PRIORITY, 
                    ICON_COLOR_PRIORITY, 
                    ICON_COLOR_PRIORITY);
            }
            else
            {
                if (ParentObject.InheritsFrom("Cell Highlighter"))
                    ParentObject.Render.Visible = false;
            }
            return base.Render(E);
        }

        public override void Remove()
        {
            if (ParentObject != null && ParentObject.InheritsFrom("Cell Highlighter"))
            {
                ParentObject.Obliterate();
            }
            base.Remove();
        }
    }
}