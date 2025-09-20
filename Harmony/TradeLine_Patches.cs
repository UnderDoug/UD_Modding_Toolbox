using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

using Qud.UI;

using XRL.UI.Framework;
using XRL.World;

using static UD_Modding_Toolbox.Utils;

namespace UD_Modding_Toolbox.Harmony
{
    [HarmonyPatch]
    public static class TradeLine_Patches
    {
        [HarmonyPatch(
            declaringType: typeof(TradeLine),
            methodName: nameof(TradeLine.setData),
            argumentTypes: new Type[] { typeof(FrameworkDataElement) },
            argumentVariations: new ArgumentType[] { ArgumentType.Normal })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> setData_UseDisplayNameEventInstead_Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator)
        {
            bool doVomit = false;
            string patchMethodName = $"{nameof(TradeLine_Patches)}.{nameof(TradeLine.setData)}";
            int metricsCheckSteps = 0;

            CodeMatcher codeMatcher = new(Instructions, Generator);

            // SB
            CodeMatch[] match_SB = new CodeMatch[]
            {
                new(OpCodes.Ldarg_0),
                new(ins => ins.LoadsField(AccessTools.Field(typeof(TradeLine), nameof(TradeLine.SB)))),
            };

            // go.DisplayName
            CodeMatch[] match_goDisplayName = new CodeMatch[]
            {
                new(OpCodes.Ldloc_2),
                new(ins => ins.Calls(AccessTools.PropertyGetter(typeof(GameObject), nameof(GameObject.DisplayName)))),
            };

            // .Append
            CodeMatch[] match_Append = new CodeMatch[]
            {
                new(ins => ins.Calls(AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[] { typeof(string) }))),
                new(OpCodes.Pop),
            };

            // GetDisplayNameEvent.GetFor(go, go.DisplayNameBase, Context: nameof(TradeLine));
            CodeInstruction[] instr_GetDisplayNameEvent_GetFor_GO_WithContext = new CodeInstruction[]
            {
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldloc_2),
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameObject), "DisplayNameBase")),
                new(OpCodes.Ldc_I4, int.MaxValue),
                new(OpCodes.Ldstr, nameof(TradeLine)),
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
            // SB.Append(go.DisplayName);
            // from the end
            if (codeMatcher.End().MatchStartBackwards(match_Append).IsInvalid)
            {
                MetricsManager.LogModError(ThisMod, $"{patchMethodName}: ({metricsCheckSteps}) {nameof(CodeMatcher.MatchStartBackwards)} failed to find instructions {nameof(match_Append)}");
                foreach (CodeMatch match in match_Append)
                {
                    MetricsManager.LogModError(ThisMod, $"    {match.name} {match.opcode}");
                }
                codeMatcher.Vomit(Generator, doVomit);
                return Instructions;
            }
            metricsCheckSteps++;

            if (codeMatcher.MatchEndBackwards(match_SB).IsInvalid)
            {
                MetricsManager.LogModError(ThisMod, $"{patchMethodName}: ({metricsCheckSteps}) {nameof(CodeMatcher.MatchEndBackwards)} failed to find instructions {nameof(match_SB)}");
                foreach (CodeMatch match in match_SB)
                {
                    MetricsManager.LogModError(ThisMod, $"    {match.name} {match.opcode}");
                }
                codeMatcher.Vomit(Generator, doVomit);
                return Instructions;
            }
            metricsCheckSteps++;

            if (codeMatcher.MatchStartForward(match_goDisplayName).IsInvalid)
            {
                MetricsManager.LogModError(ThisMod, $"{patchMethodName}: ({metricsCheckSteps}) {nameof(CodeMatcher.MatchStartForward)} failed to find instructions {nameof(match_goDisplayName)}");
                foreach (CodeMatch match in match_goDisplayName)
                {
                    MetricsManager.LogModError(ThisMod, $"    {match.name} {match.opcode}");
                }
                codeMatcher.Vomit(Generator, doVomit);
                return Instructions;
            }
            metricsCheckSteps++;

            codeMatcher.RemoveInstructions(match_goDisplayName.Length);
            codeMatcher.Insert(instr_GetDisplayNameEvent_GetFor_GO_WithContext);

            MetricsManager.LogModInfo(ThisMod, $"Successfully transpiled {patchMethodName}");
            return codeMatcher.Vomit(Generator, doVomit).InstructionEnumeration();
        }
    }
}
