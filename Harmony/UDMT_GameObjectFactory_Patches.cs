using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.Loaders;

using static UD_Modding_Toolbox.Const;
using static UD_Modding_Toolbox.Utils;

namespace UD_Modding_Toolbox.Harmony
{
    [HarmonyPatch]
    public static class UDMT_GameObjectFactory_Patches
    {
        public static bool doDebug = false;

        [HarmonyPatch(
            declaringType: typeof(GameObjectFactory),
            methodName: nameof(GameObjectFactory.LoadBakedXML),
            argumentTypes: new Type[] { typeof(ObjectBlueprintLoader.ObjectBlueprintXMLData) },
            argumentVariations: new ArgumentType[] { ArgumentType.Normal })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LoadBakedXML_AddMutationEntryNode_Transpiler(
            IEnumerable<CodeInstruction> Instructions, 
            ILGenerator Generator,
            MethodBase OriginalMethod)
        {
            bool doVomit = false;
            string patchMethodName = $"{nameof(UDMT_GameObjectFactory_Patches)}.{nameof(GameObjectFactory.LoadBakedXML)}";
            int metricsCheckSteps = 0;

            CodeMatcher codeMatcher = new(Instructions, Generator);

            // IL_00b4: ldloc.1
            // IL_00b5: callvirt instance IEnumerator`1<KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>>::get_Current()
            // IL_00ba: stloc.s 4
            // // GamePartBlueprint gamePartBlueprint2 = new GamePartBlueprint("XRL.World.Parts.Mutation", item2.Key);
            // IL_00bc: ldstr "XRL.World.Parts.Mutation"
            // IL_00c1: ldloca.s 4 // this is the current mutation node
            // IL_00c3: call instance KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>::get_Key()
            // IL_00c8: newobj instance void GamePartBlueprint::.ctor(string, string)
            // IL_00cd: stloc.s 5

            MethodInfo iEnumerator_Getter = AccessTools.PropertyGetter(
                typeof(IEnumerator<KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>>),
                nameof(IEnumerator<KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>>.Current));

            LocalBuilder local_at_4 = OriginalMethod.GetLocalAtIndex(4);

            MethodInfo keyValuePair_Key_Getter = AccessTools.PropertyGetter(
                typeof(KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>),
                nameof(KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>.Key));

            LocalBuilder local_at_5 = OriginalMethod.GetLocalAtIndex(5);

            CodeMatch[] match_new_GamePartBlueprint = new CodeMatch[]
            {
                new(OpCodes.Ldloc_1),
                new(OpCodes.Callvirt, iEnumerator_Getter),
                new(OpCodes.Stloc_S, local_at_4),
                new(OpCodes.Ldstr, "XRL.World.Parts.Mutation"),
                new(OpCodes.Ldloca_S, local_at_4), // this is the current mutation node
                new(OpCodes.Call, keyValuePair_Key_Getter),
                new(OpCodes.Newobj, AccessTools.Constructor(typeof(GamePartBlueprint), new Type[] { typeof(string), typeof(string) })),
                new(OpCodes.Stloc_S, local_at_5),
            };

            // find end of:
            // // GamePartBlueprint gamePartBlueprint2 = new GamePartBlueprint("XRL.World.Parts.Mutation", item2.Key);
            // from the start
            if (codeMatcher.Start().MatchEndForward(match_new_GamePartBlueprint).IsInvalid)
            {
                MetricsManager.LogModError(ThisMod,
                    $"{patchMethodName}: ({metricsCheckSteps}) " +
                    $"{nameof(CodeMatcher.MatchEndForward)} failed to find instructions " +
                    $"{nameof(match_new_GamePartBlueprint)}");

                match_new_GamePartBlueprint.Vomit(
                    EndContext: "--" + patchMethodName,
                    IncludeEnd: true,
                    Do: doVomit);

                codeMatcher.Vomit(Generator, true);
                return Instructions;
            }
            metricsCheckSteps++;

            // call string UDMT_GameObjectFactory_Patches::ProcessBakedXML_MutationEntry(string)
            CodeInstruction[] instr_ProcessBakedXML_MutationEntry = new CodeInstruction[]
            {
                CodeInstruction.Call(typeof(UDMT_GameObjectFactory_Patches), nameof(ProcessBakedXML_MutationEntry), new Type[] { typeof(string) })
            };
            codeMatcher.Advance(-1).Insert(instr_ProcessBakedXML_MutationEntry);

            // IL_00b4: ldloc.1
            // IL_00b5: callvirt instance IEnumerator`1<KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>>::get_Current()
            // IL_00ba: stloc.s 4
            // // GamePartBlueprint gamePartBlueprint2 = new GamePartBlueprint("XRL.World.Parts.Mutation", item2.Key);
            // IL_00bc: ldstr "XRL.World.Parts.Mutation"
            // IL_00c1: ldloca.s 4 // this is the current mutation node
            // IL_00c3: call instance KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>::get_Key()
            //        : call string UDMT_GameObjectFactory_Patches::ProcessBakedXML_MutationEntry(string)
            // IL_00c8: newobj instance void GamePartBlueprint::.ctor(string, string)
            // IL_00cd: stloc.s 5


            // gamePartBlueprint2.Name = item2.Value.Name;
            // IL_00cf: ldloc.s 5
            // IL_00d1: ldloca.s 4
            // IL_00d3: call instance KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>::get_Value()
            // IL_00d8: callvirt string ObjectBlueprintLoader.ObjectBlueprintXMLChildNode::get_Name()
            // IL_00dd: stfld string GamePartBlueprint::Name

            MethodInfo keyValuePair_Value_Getter = AccessTools.PropertyGetter(
                typeof(KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>),
                nameof(KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode>.Value));

            MethodInfo objectBlueprintXMLChildNode_Name_Getter = AccessTools.PropertyGetter(
                typeof(ObjectBlueprintLoader.ObjectBlueprintXMLChildNode),
                nameof(ObjectBlueprintLoader.ObjectBlueprintXMLChildNode.Name));

            CodeMatch[] match_GameObjectBlueprint_Name = new CodeMatch[]
            {
                new(OpCodes.Ldloc_S, local_at_5),
                new(OpCodes.Ldloca_S, local_at_4), // this is the current mutation node
                new(OpCodes.Call, keyValuePair_Value_Getter),
                new(OpCodes.Callvirt, objectBlueprintXMLChildNode_Name_Getter),
                new(OpCodes.Stfld, AccessTools.Field(typeof(GamePartBlueprint), nameof(GamePartBlueprint.Name))),
            };

            // find end of:
            // // gamePartBlueprint2.Name = item2.Value.Name;
            // from the start
            if (codeMatcher.Start().MatchEndForward(match_GameObjectBlueprint_Name).IsInvalid)
            {
                MetricsManager.LogModError(ThisMod,
                    $"{patchMethodName}: ({metricsCheckSteps}) " +
                    $"{nameof(CodeMatcher.MatchEndForward)} failed to find instructions " +
                    $"{nameof(match_new_GamePartBlueprint)}");

                match_GameObjectBlueprint_Name.Vomit(
                    EndContext: "--" + patchMethodName,
                    IncludeEnd: true,
                    Do: true);

                codeMatcher.Vomit(Generator, doVomit);
                return Instructions;
            }
            metricsCheckSteps++;

            codeMatcher.Insert(instr_ProcessBakedXML_MutationEntry);

            MetricsManager.LogModInfo(ThisMod, $"Successfully transpiled {patchMethodName}");
            return codeMatcher.Vomit(Generator, doVomit).InstructionEnumeration();
        }

        public static string ProcessBakedXML_MutationEntry(string MutationNodeKey)
        {
            bool doDebug = true;
            Debug.Entry(4, nameof(ProcessBakedXML_MutationEntry), MutationNodeKey, Indent: 0, Toggle: doDebug);
            if (MutationFactory.GetMutationEntryByName(MutationNodeKey) is MutationEntry mutationEntry)
            {
                return mutationEntry.Class;
            }
            return MutationNodeKey;
        }
    }
}
