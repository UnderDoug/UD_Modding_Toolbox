using HarmonyLib;

using System;

using XRL;
using XRL.CharacterBuilds.Qud;

using static UD_Modding_Toolbox.Const;
using static UD_Modding_Toolbox.Utils;

namespace UD_Blink_Mutation.Harmony
{
    [HarmonyPatch]
    public static class UDMT_QudGenotypeModule_Patches
    {
        public static bool doDebug = true;

        [HarmonyPatch(
            declaringType: typeof(QudGenotypeModule),
            methodName: nameof(QudGenotypeModule.handleUIEvent),
            argumentTypes: new Type[] { typeof(string), typeof(object) },
            argumentVariations: new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool handleUIEvent_BlockSpecificValue_Prefix(ref QudGenotypeModule __instance, ref object __result)
        {
            try
            {
                GenotypeEntry genotypeEntry = __instance.data?.Entry;
                if (genotypeEntry != null && genotypeEntry.CharacterBuilderModules == "NonePlease")
                {
                    __instance.data.Genotype = "Mutated Human";
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(
                    $"[{MOD_ID}] {nameof(UDMT_QudGenotypeModule_Patches)}." +
                    $"{nameof(handleUIEvent_BlockSpecificValue_Prefix)}()", 
                    x);
            }
            return true;
        }

        [HarmonyPatch(
            declaringType: typeof(QudGenotypeModule),
            methodName: nameof(QudGenotypeModule.getSelected))]
        [HarmonyPostfix]
        public static void getSelected_BlockSpecificValue_Prefix(ref QudGenotypeModule __instance, ref string __result)
        {
            try
            {
                GenotypeEntry genotypeEntry = GenotypeFactory.GetGenotypeEntry(__result);
                if (genotypeEntry != null && genotypeEntry.CharacterBuilderModules == "NonePlease")
                {
                    __result = "Mutated Human";
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(
                    $"[{MOD_ID}] {nameof(UDMT_QudGenotypeModule_Patches)}." +
                    $"{nameof(getSelected_BlockSpecificValue_Prefix)}()", 
                    x);
            }
        }
    }
}
