using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using HarmonyLib;

using Qud.API;

using XRL;
using XRL.Core;
using XRL.Wish;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

using UD_Modding_Toolbox;
using static UD_Modding_Toolbox.Const;
using Debug = UD_Modding_Toolbox.Debug;
using Options = UD_Modding_Toolbox.Options;

namespace UD_Modding_Toolbox
{
    [HasWishCommand]
    public static class Debug
    {
        private static int VerbosityOption => Options.DebugVerbosity;
        private static bool IncludeInMessage => Options.DebugIncludeInMessage;

        public static UD_Logger Logger = new(
            ThisMod: Utils.ThisMod,
            OptionClass: typeof(Options),
            VerbosityOptionField: nameof(Options.DebugVerbosity),
            IncludeInMessageOptionField: nameof(Options.DebugIncludeInMessage));

        private static int _LastIndent = 0;

        [Obsolete(
            "Prefer " + nameof(Debug) + "." + nameof(GetIndent) + " or " + 
            nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.GetIndent) + 
            "; this property will persist for a while (into 2026).")]
        public static int LastIndent
        {
            get
            {
                Logger.GetIndent(out _LastIndent);
                return _LastIndent;
            }
            set
            {
                _LastIndent = value;
                Logger.SetIndent(value);
            }
        }

        public static void ResetIndent()
        {
            Logger.ResetIndent();
        }
        public static void ResetIndent(out int Indent)
        {
            Logger.ResetIndent(out Indent);
        }
        public static void GetIndent(out int Indent)
        {
            Logger.GetIndent(out Indent);
        }
        public static void SetIndent(int Indent)
        {
            Logger.SetIndent(Indent);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Entry((UD_Logger.Verbosity)Verbosity, Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(UD_Logger.Verbosity Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Logger.Entry(Verbosity, Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(string Text, int Indent = 0, bool Toggle = true)
        {
            Logger.Entry(Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(int Verbosity, string Label, string Text, int Indent = 0, bool Toggle = true)
        {
            Entry((UD_Logger.Verbosity)Verbosity, Label, Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(UD_Logger.Verbosity Verbosity, string Label, string Text, int Indent = 0, bool Toggle = true)
        {
            string output = Label + ": " + Text;
            Logger.Entry(Verbosity, output, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Divider) + "; this method will persist for a while (into 2026).")]
        public static void Divider(int Verbosity = 0, string String = null, int Count = 60, int Indent = 0, bool Toggle = true)
        {
            Divider((UD_Logger.Verbosity)Verbosity, String, Count, Indent, Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Divider) + "; this method will persist for a while (into 2026).")]
        public static void Divider(UD_Logger.Verbosity Verbosity = 0, string String = null, int Count = 60, int Indent = 0, bool Toggle = true)
        {
            Logger.Divider(Verbosity, String, Count, Indent, Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Header) + "; this method will persist for a while (into 2026).")]
        public static void Header(int Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            Header((UD_Logger.Verbosity)Verbosity, ClassName, MethodName, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Header) + "; this method will persist for a while (into 2026).")]
        public static void Header(UD_Logger.Verbosity Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            Logger.Header(Verbosity, ClassName, MethodName, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Footer) + "; this method will persist for a while (into 2026).")]
        public static void Footer(int Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            Footer((UD_Logger.Verbosity)Verbosity, ClassName, MethodName, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Footer) + "; this method will persist for a while (into 2026).")]
        public static void Footer(UD_Logger.Verbosity Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            Logger.Footer(Verbosity, ClassName, MethodName, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.DiveIn) + "; this method will persist for a while (into 2026).")]
        public static void DiveIn(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            DiveIn((UD_Logger.Verbosity)Verbosity, Text, Indent, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.DiveIn) + "; this method will persist for a while (into 2026).")]
        public static void DiveIn(UD_Logger.Verbosity Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Logger.DiveIn(Verbosity, Text, Indent, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.DiveOut) + "; this method will persist for a while (into 2026).")]
        public static void DiveOut(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            DiveOut((UD_Logger.Verbosity)Verbosity, Text, Indent, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.DiveOut) + "; this method will persist for a while (into 2026).")]
        public static void DiveOut(UD_Logger.Verbosity Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Logger.DiveOut(Verbosity, Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Warn) + "; this method will persist for a while (into 2026).")]
        public static void Warn(int Verbosity, string ClassName, string MethodName, string Issue = null, int Indent = 0)
        {
            Warn((UD_Logger.Verbosity)Verbosity, ClassName, MethodName, Issue, Indent);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Warn) + "; this method will persist for a while (into 2026).")]
        public static void Warn(UD_Logger.Verbosity Verbosity, string ClassName, string MethodName, string Issue = null, int Indent = 0)
        {
            Logger.Warn(Verbosity, ClassName, MethodName, Issue, Indent);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.LoopItem) + "; this method will persist for a while (into 2026).")]
        public static void LoopItem(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = null, bool Toggle = true)
        {
            LoopItem((UD_Logger.Verbosity)Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.LoopItem) + "; this method will persist for a while (into 2026).")]
        public static void LoopItem(UD_Logger.Verbosity Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = null, bool Toggle = true)
        {
            Logger.LoopItem(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckYeh) + "; this method will persist for a while (into 2026).")]
        public static void CheckYeh(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = true, bool Toggle = true)
        {
            CheckYeh((UD_Logger.Verbosity)Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckYeh) + "; this method will persist for a while (into 2026).")]
        public static void CheckYeh(UD_Logger.Verbosity Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = true, bool Toggle = true)
        {
            Logger.CheckYeh(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckNah) + "; this method will persist for a while (into 2026).")]
        public static void CheckNah(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = false, bool Toggle = true)
        {
            CheckNah((UD_Logger.Verbosity)Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckNah) + "; this method will persist for a while (into 2026).")]
        public static void CheckNah(UD_Logger.Verbosity Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = false, bool Toggle = true)
        {
            Logger.CheckNah(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }

        // Class Specific Debugs
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckNah) + "; this method will persist for a while (into 2026).")]
        public static void Vomit(int Verbosity, string Source, string Context = null, int Indent = 0, bool Toggle = true)
        {
            Vomit((UD_Logger.Verbosity)Verbosity, Source, Context, Indent, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckNah) + "; this method will persist for a while (into 2026).")]
        public static void Vomit(UD_Logger.Verbosity Verbosity, string Source, string Context = null, int Indent = 0, bool Toggle = true)
        {
            Logger.Vomit(Verbosity, Source, Context, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer version using " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Verbosity) + "; this method will persist for a while (into 2026).")]
        public static Raffle<T> Vomit<T>(
            this Raffle<T> Raffle,
            int Verbosity,
            string Source,
            string Context = null,
            bool ShowChance = false,
            bool Short = false,
            T Drawn = default,
            T Sampled = default,
            int Indent = 0,
            bool Toggle = true)
        {
            return Vomit(Raffle, (UD_Logger.Verbosity)Verbosity, Source, Context, ShowChance, Short, Drawn, Sampled, Indent, Toggle: Toggle);
        }
        public static Raffle<T> Vomit<T>(
            this Raffle<T> Raffle,
            UD_Logger.Verbosity Verbosity,
            string Source,
            string Context = null,
            bool ShowChance = false,
            bool Short = false,
            T Drawn = default,
            T Sampled = default,
            int Indent = 0,
            bool Toggle = true)
        {
            string context = Context == null ? "" : $"{Context}:";
            Logger.Entry(Verbosity, $"Vomit: {Source} {context}", Indent, Toggle: Toggle);

            if (Short && Raffle.ActiveCount < 1)
            {
                Logger.CheckNah(Verbosity, "empty", Indent: Indent + 1, Toggle: Toggle);
                return Raffle;
            }

            bool noneDrawn = Equals(Drawn, (T)default);
            bool noneSampled = Equals(Sampled, (T)default);

            T picked = default;
            if (!noneDrawn)
            {
                picked = Drawn;
            }
            else
            if (!noneSampled)
            {
                picked = Sampled;
            }
            bool nonePicked = Equals(picked, (T)default);
            foreach (Raffle<T>.Entry entry in Raffle)
            {
                bool? wasPicked = null;
                if (!nonePicked)
                {
                    wasPicked = entry.Equals((T)picked);
                }
                bool wasDrawn = !noneDrawn && entry.Equals(picked);
                int weightAdjust = wasDrawn ? 1 : 0;
                string chanceString = null;
                if (ShowChance)
                {
                    if (Raffle.ActiveCount > 0 && (Raffle.GetTotalChance(entry) * 100f) is float chance)
                    {
                        chanceString += Math.Round(chance, 2).ToString();
                    }
                    else
                    {
                        chanceString += 0.0f.ToString();
                    }
                    chanceString += "%, ";
                }
                string message = (entry.Weight + weightAdjust).ToString() + ", " + chanceString + entry.Ticket.ExtendedToString();
                Logger.LoopItem(Verbosity, message, Good: wasPicked, Indent: Indent + 1, Toggle: Toggle);
            }
            Logger.SetIndent(Indent);
            return Raffle;
        }
        public static Raffle<char> VomitBits(
            this Raffle<char> Raffle,
            int Verbosity,
            string Source,
            string Context = null,
            bool ShowChance = false,
            bool Short = false,
            char Drawn = (char)default,
            char Sampled = (char)default,
            int Indent = 0,
            bool Toggle = true)
        {
            string context = Context == null ? "" : (Context + ":");
            Entry(Verbosity, nameof(VomitBits) + ": " + Source + " " + context, Indent, Toggle: Toggle);

            if (Short && Raffle.ActiveCount < 1)
            {
                CheckNah(Verbosity, "empty", Indent: Indent + 1, Toggle: Toggle);
                return Raffle;
            }

            bool noneDrawn = Equals(Drawn, (char)default);
            bool noneSampled = Equals(Sampled, (char)default);

            char picked = (char)default;
            if (!noneDrawn)
            {
                picked = Drawn;
            }
            else
            if (!noneSampled)
            {
                picked = Sampled;
            }
            bool nonePicked = Equals(picked, (char)default);
            foreach (Raffle<char>.Entry entry in Raffle)
            {
                bool? wasPicked = null;
                if (!nonePicked)
                {
                    wasPicked = entry.Equals(picked);
                }
                bool wasDrawn = !noneDrawn && entry.Equals((char)picked);
                int weightAdjust = wasDrawn ? 1 : 0;
                string chanceString = null;
                if (ShowChance)
                {
                    if (Raffle.ActiveCount > 0 && (Raffle.GetTotalChance(entry) * 100f) is float chance)
                    {
                        chanceString = Math.Round(chance, 2).ToString();
                    }
                    else
                    {
                        chanceString = 0.0f.ToString();
                    }
                    chanceString += "%, ";
                }
                string message = BitType.TranslateBit(entry.Ticket) + "] " + (entry.Weight + weightAdjust).ToString() + ", " + chanceString;
                LoopItem(Verbosity, message, Good: wasPicked, Indent: Indent + 1, Toggle: Toggle);
            }
            LastIndent = Indent;
            return Raffle;
        }

        public static MeleeWeapon Vomit(
            this MeleeWeapon MeleeWeapon,
            int Verbosity,
            string Title = null,
            List<string> Categories = null,
            int Indent = 0,
            bool Toggle = true)
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