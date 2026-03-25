using HarmonyLib;
using NAudio.SoundFont;
using Qud.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UD_Modding_Toolbox;
using XRL;
using XRL.Core;
using XRL.Wish;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;
using static UD_Modding_Toolbox.Const;
using Debug = UD_Modding_Toolbox.Debug;
using Options = UD_Modding_Toolbox.Options;

namespace UD_Modding_Toolbox
{
    [HasWishCommand]
    public class UD_Logger
    {
        [Serializable]
        public enum Verbosity : int
        {
            Critical = 0,   // Critical. Use sparingly, if at all, as they show up without the option. Move these to 1 when pushing to main.
            Show = 1,       // Initial debugging entries. Broad, general "did it happen?" style entries for basic trouble-shooting.
            Verbose = 2,    // Entries have more information in them, indicating how values are passed around and changed.
            Very = 3,       // Entries in more locations, or after fewer steps. These contribute to tracing program flow.
            Max = 4,        // Just like, all of it. Every step of a process, as much detail as possible.
        }

        public const int DEFAULT_INDENT_FACTOR = 4;

        protected ModInfo Mod;

        protected Type OptionClass;

        protected string _VerbosityOption;
        public int VerbosityOption => GetVerbosityOption();

        protected string _IncludeInMessageOption;
        public bool IncludeInMessage => GetIncludeInMessageOption();

        protected int LastIndent;

        public int IndentFactor;

        protected UD_Logger()
        {
            Mod = null;

            OptionClass = null;

            _VerbosityOption = null;
            _IncludeInMessageOption = null;

            LastIndent = 0;

            IndentFactor = 4;
        }

        public UD_Logger(ModInfo ThisMod, Type OptionClass, string VerbosityOptionField, string IncludeInMessageOptionField, int IndentFactor = DEFAULT_INDENT_FACTOR)
            : this()
        {
            Mod = ThisMod;

            this.OptionClass = OptionClass;

            _VerbosityOption = VerbosityOptionField;
            _IncludeInMessageOption = IncludeInMessageOptionField;

            this.IndentFactor = DEFAULT_INDENT_FACTOR;
        }

        protected int GetVerbosityOption()
        {
            if (!_VerbosityOption.IsNullOrEmpty() && OptionClass != null)
            {
                if (new Traverse (OptionClass) is Traverse optionClass
                    && optionClass.Field(_VerbosityOption) is Traverse optionField
                    && optionField.FieldExists()
                    && optionField.GetValueType() == typeof(int))
                {
                    return optionField.GetValue<int>();
                }
            }
            return Options.DebugVerbosity;
        }

        protected bool GetIncludeInMessageOption()
        {
            if (!_IncludeInMessageOption.IsNullOrEmpty() && OptionClass != null)
            {
                if (new Traverse (OptionClass) is Traverse optionClass
                    && optionClass.Field(_IncludeInMessageOption) is Traverse optionField
                    && optionField.FieldExists()
                    && optionField.GetValueType() == typeof(bool))
                {
                    return optionField.GetValue<bool>();
                }
            }
            return Options.DebugIncludeInMessage;
        }

        public void ResetIndent()
        {
            LastIndent = 0;
        }
        public void ResetIndent(out int Indent)
        {
            ResetIndent();
            GetIndent(out Indent);
        }
        public void GetIndent(out int Indent)
        {
            Indent = LastIndent;
        }
        public void SetIndent(int Indent)
        {
            LastIndent = Indent;
        }

        private void Message(string Text)
        {
            XRL.Messages.MessageQueue.AddPlayerMessage("{{Y|" + Text + "}}");
        }

        private void Log(string Text)
        {
            UnityEngine.Debug.LogError(Text);
        }

        public void Entry(Verbosity Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            this.Indent(Verbosity, Text, Indent, Toggle: Toggle);
        }

        public void Entry(string Text, int Indent = 0, bool Toggle = true)
        {
            Verbosity Verbosity = Verbosity.Critical;
            this.Indent(Verbosity, Text, Indent, Toggle: Toggle);
        }

        public void Entry(Verbosity Verbosity, string Label, string Text, int Indent = 0, bool Toggle = true)
        {
            string output = Label + ": " + Text;
            Entry(Verbosity, output, Indent, Toggle: Toggle);
        }

        public void Indent(Verbosity Verbosity, string Text, int Spaces = 0, bool Toggle = true)
        {
            if ((int)Verbosity > VerbosityOption || !Toggle)
            {
                return;
            }
            int factor = 4;
            // NBSP  \u00A0
            // Space \u0020
            string space = "\u0020";
            string indent = indent = space.ThisManyTimes(Spaces * factor);
            LastIndent = Spaces;
            string output = indent + Text;
            Log(output);
            if (IncludeInMessage)
            {
                Message(output);
            }
        }

        public void Divider(Verbosity Verbosity = Verbosity.Critical, string String = null, int Count = 60, int Indent = 0, bool Toggle = true)
        {
            if (String == null)
            {
                String = "\u003D"; // =
            }
            else
            {
                String = String[..1];
            }
            string output = String.ThisManyTimes(Count);
            Entry(Verbosity, output, Indent, Toggle: Toggle);
        }

        public void Header(Verbosity Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            string divider = "\u2550"; // ═ (box drawing, double horizontal)
            Divider(Verbosity, divider, Toggle: Toggle);
            string output = "@START: " + ClassName + "." + MethodName;
            Entry(Verbosity, output, Toggle: Toggle);
        }
        public void Footer(Verbosity Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            string divider = "\u2550"; // ═ (box drawing, double horizontal)
            string output = "///END: " + ClassName + "." + MethodName + " !//";
            Entry(Verbosity, output, Toggle: Toggle);
            Divider(Verbosity, divider, Toggle: Toggle);
        }

        public void DiveIn(Verbosity Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Divider(Verbosity, "\u003E", 25, Indent); // >
            Entry(Verbosity, Text, Indent + 1, Toggle: Toggle);
        }
        public void DiveOut(Verbosity Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Entry(Verbosity, Text, Indent + 1);
            Divider(Verbosity, "\u003C", 25, Indent, Toggle: Toggle); // <
        }

        public void Warn(Verbosity Verbosity, string ClassName, string MethodName, string Issue = null, int Indent = 0)
        {
            string noIssue = "Something didn't go as planned";
            string output = $">!< WARN | {ClassName}.{MethodName}: {Issue ?? noIssue}";
            Entry(Verbosity, output, Indent, Toggle: true);
        }

        public void LoopItem(Verbosity Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = null, bool Toggle = true)
        {
            string good = TICK;  // √
            string bad = CROSS;  // X
            string goodOrBad = string.Empty;
            if (Good != null) goodOrBad = ((bool)Good ? good : bad) + "\u005D "; // ]
            string output = Text != string.Empty ? Label + ": " + Text : Label;
            Entry(Verbosity, "\u005B" + goodOrBad + output, Indent, Toggle: Toggle);
        }
        public void CheckYeh(Verbosity Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = true, bool Toggle = true)
        {
            LoopItem(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }
        public void CheckNah(Verbosity Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = true, bool Toggle = true)
        {
            LoopItem(Verbosity, Label, Text, Indent, !Good, Toggle: Toggle);
        }

        // Class Specific Debugs
        public void Vomit(Verbosity Verbosity, string Source, string Context = null, int Indent = 0, bool Toggle = true)
        {
            string context = Context == null ? "" : $"{Context}:";
            Entry(Verbosity, $"% Vomit: {Source} {context}", Indent, Toggle: Toggle);
        }
    }
}