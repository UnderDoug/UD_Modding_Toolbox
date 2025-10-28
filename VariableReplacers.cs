using System;
using System.Collections.Generic;
using System.Text;

using HistoryKit;

using XRL;
using XRL.Language;
using XRL.World;
using XRL.World.Text.Attributes;
using XRL.World.Text.Delegates;

using static UD_Modding_Toolbox.Utils;

namespace UD_Modding_Toolbox
{
    [HasVariableReplacer]
    public static class VariableReplacers
    {
        [VariableReplacer]
        public static string nbsp(DelegateContext Context)
        {
            string nbsp = "\xFF";
            string output = nbsp;
            if (!Context.Parameters.IsNullOrEmpty() && int.TryParse(Context.Parameters[0], out int count))
            {
                for (int i = 1; i < count; i++)
                {
                    output += nbsp;
                }
            }
            return output;
        }

        [VariableReplacer]
        public static string ud_spice(DelegateContext Context)
        {
            string output = "";
            if (!Context.Parameters.IsNullOrEmpty())
            {
                output = HistoricStringExpander.ExpandString($"<spice.{Context.Parameters[0]}>").StartReplace().ToString();
            }
            return output;
        }

        [VariableObjectReplacer]
        public static string ud_personTerm(DelegateContext Context)
        {
            string personTerm = "person";
            bool noSpecies = !Context.Parameters.IsNullOrEmpty() && Context.Parameters[0].ToLower() == "NoSpecies".ToLower();
            if (Context.Target is GameObject subject)
            {
                if (subject.GetGender() is Gender gender)
                {
                    personTerm = gender.PersonTerm;
                }
                else
                {
                    personTerm = subject.personTerm ?? Context.Pronouns.PersonTerm;
                }
                if (personTerm.ToLower().Contains("human") && subject.GetSpecies().ToLower() != "human")
                {
                    if (!noSpecies)
                    {
                        personTerm = !noSpecies ? subject.GetSpecies() : "person";
                    }
                    if (subject.IsPlural)
                    {
                        personTerm = Grammar.Pluralize(personTerm);
                    }
                }
            }
            return Context.Capitalize ? personTerm.ConvertCase(TextCase.Capital) : personTerm;
        }
    }
}
