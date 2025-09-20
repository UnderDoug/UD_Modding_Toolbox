using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using Qud.API;

using XRL.World;

using static UD_Modding_Toolbox.Utils;

namespace UD_Modding_Toolbox.Harmony
{
    [HarmonyPatch]
    public static class EquipmentAPI_Patches
    {
        [HarmonyPatch(
            declaringType: typeof(EquipmentAPI),
            methodName: nameof(EquipmentAPI.ShowInventoryActionMenu),
            argumentTypes: new Type[] 
            { 
                typeof(Dictionary<string, InventoryAction>),
                typeof(GameObject),
                typeof(GameObject),
                typeof(bool),
                typeof(bool),
                typeof(string),
                typeof(IComparer<InventoryAction>),
                typeof(bool),
            },
            argumentVariations: new ArgumentType[] 
            { 
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
            })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ShowInventoryActionMenu_UseDisplayNameEventInstead_Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator)
        {
            bool doVomit = true;
            string patchMethodName = $"{nameof(EquipmentAPI_Patches)}.{nameof(EquipmentAPI.ShowInventoryActionMenu)}";
            int metricsCheckSteps = 0;

            CodeMatcher codeMatcher = new(Instructions, Generator);

            if (true || doVomit)
            {
                MetricsManager.LogModInfo(ThisMod, $"Skipped {patchMethodName} transpiler");
                return codeMatcher.InstructionEnumeration();
            }

            // (isConfused ? GO.DisplayName : null);
            CodeMatch[] match_GoDisplayName = new CodeMatch[]
            {
                // (isConfused
                new(OpCodes.Pop),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Brtrue_S),

                // : null
                new(OpCodes.Ldnull),
                new(OpCodes.Br_S),

                // ? GO.DisplayName)
                new(OpCodes.Ldarg_2),
                new(ins => ins.Calls(AccessTools.PropertyGetter(typeof(GameObject), nameof(GameObject.DisplayName)))),
            };

            // GetDisplayNameEvent.GetFor(GO, GO.DisplayNameBase, Context: nameof(EquipmentAPI.ShowInventoryActionMenu))
            CodeInstruction[] instr_GetDisplayNameEvent_GetFor_GO_WithContext = new CodeInstruction[]
            {
                // new(OpCodes.Ldarg_2),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameObject), "DisplayNameBase")),
                new(OpCodes.Ldc_I4, int.MaxValue),
                new(OpCodes.Ldstr, nameof(EquipmentAPI.ShowInventoryActionMenu)),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Call, AccessTools.Method(typeof(GetDisplayNameEvent), nameof(GetDisplayNameEvent.GetFor), new Type[] { typeof(GameObject), typeof(string), typeof(int), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })),
            };

            // find start of:
            // (isConfused ? GO.DisplayName : null)
            // from the end
            if (codeMatcher.End().MatchStartBackwards(match_GoDisplayName).IsInvalid)
            {
                MetricsManager.LogModError(ThisMod, $"{patchMethodName}: ({metricsCheckSteps}) {nameof(CodeMatcher.MatchStartBackwards)} failed to find instructions {nameof(match_GoDisplayName)}");
                foreach (CodeMatch match in match_GoDisplayName)
                {
                    MetricsManager.LogModError(ThisMod, $"    {match.name} {match.opcode}");
                }
                codeMatcher.Vomit(Generator, doVomit);
                return Instructions;
            }
            metricsCheckSteps++;

            // remove:
            // (isConfused ? GO.DisplayName : null)
            codeMatcher.RemoveInstructions(match_GoDisplayName.Length - 2);
            codeMatcher.Advance(1).RemoveInstruction();

            // insert:
            // GetDisplayNameEvent.GetFor(GO, GO.DisplayNameBase, Context: nameof(EquipmentAPI.ShowInventoryActionMenu))
            codeMatcher.Insert(instr_GetDisplayNameEvent_GetFor_GO_WithContext);

            MetricsManager.LogModInfo(ThisMod, $"Successfully transpiled {patchMethodName}");
            return codeMatcher.Vomit(Generator, doVomit).InstructionEnumeration();
        }
    }
}
