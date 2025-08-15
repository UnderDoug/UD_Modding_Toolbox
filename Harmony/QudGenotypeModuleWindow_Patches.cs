using HarmonyLib;

using System;
using System.Collections.Generic;

using XRL;
using XRL.CharacterBuilds.Qud.UI;
using XRL.UI.Framework;

using static UD_Modding_Toolbox.Const;
using static UD_Modding_Toolbox.Utils;

namespace UD_Modding_Toolbox.Harmony
{
    [HarmonyPatch]
    public static class QudGenotypeModuleWindow_Patches
    {
        public static bool doDebug = true;

        [HarmonyPatch(
            declaringType: typeof(QudGenotypeModuleWindow),
            methodName: nameof(QudGenotypeModuleWindow.GetSelections))]
        [HarmonyPostfix]
        public static void GetSelections_NoPricklePigs_Postfix(ref QudGenotypeModuleWindow __instance, ref IEnumerable<ChoiceWithColorIcon> __result)
        {
            try
            {
                List<ChoiceWithColorIcon> choiceWithColorIconCopy = new();
                foreach (ChoiceWithColorIcon entry in __result)
                {
                    // CharacterBuilderModules
                    GenotypeEntry genotypeEntry = GenotypeFactory.GetGenotypeEntry(entry.Id);
                    if (genotypeEntry != null && genotypeEntry.CharacterBuilderModules != "NonePlease")
                    {
                        choiceWithColorIconCopy.Add(entry);
                    }
                }
                __result = choiceWithColorIconCopy;
            }
            catch (Exception x)
            {
                MetricsManager.LogException(
                    $"[{MOD_ID}] {nameof(QudGenotypeModuleWindow_Patches)}" +
                    $"{nameof(GetSelections_NoPricklePigs_Postfix)}()",
                    x);
            }
        }
    }
}
