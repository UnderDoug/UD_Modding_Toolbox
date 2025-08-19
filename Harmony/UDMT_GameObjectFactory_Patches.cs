using HarmonyLib;

using System;

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

        private static readonly string TargetAttribute = "DisplayName";

        [HarmonyPatch(
            declaringType: typeof(GameObjectFactory),
            methodName: nameof(GameObjectFactory.LoadBakedXML),
            argumentTypes: new Type[] { typeof(ObjectBlueprintLoader.ObjectBlueprintXMLData) },
            argumentVariations: new ArgumentType[] { ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void LoadBakedXML_MutationEntryIfSupplied_Postfix(ref GameObjectFactory __instance, ref GameObjectBlueprint __result, ObjectBlueprintLoader.ObjectBlueprintXMLData node)
        {
            try
            {
                if (HNPS_GigantismPlus != null && !HNPS_GigantismPlus.IsEnabled)
                {
                    GameObjectBlueprint gameObjectBlueprint = __result;
                    if (gameObjectBlueprint.Mutations.IsNullOrEmpty())
                    {
                        gameObjectBlueprint.Mutations = new();
                    }
                    if (!node.NamedNodes("mutation").IsNullOrEmpty())
                    {
                        Debug.Entry(4,
                            $"# [{MOD_ID}] {nameof(UDMT_GameObjectFactory_Patches)}."
                            + $"{nameof(LoadBakedXML_MutationEntryIfSupplied_Postfix)}"
                            + $"(...)",
                            Indent: 0, Toggle: doDebug);
                        Debug.Entry(4, $"__result: {__result.Name}/{__result.DisplayName()}", Indent: 1, Toggle: doDebug);

                        Debug.Entry(4, $"Checking Named mutation nodes for {TargetAttribute.Quote()} attribute...", Indent: 1, Toggle: doDebug);

                        Debug.Entry(4,
                            $"> foreach ((string name, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode childNode) in node.NamedNodes(\"mutation\"))",
                            Indent: 1, Toggle: doDebug);
                        foreach ((string name, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode childNode) in node.NamedNodes("mutation"))
                        {
                            Debug.Divider(4, HONLY, Count: 25, Indent: 2, Toggle: doDebug);

                            Debug.Entry(4, $"{nameof(name)}", name, Indent: 2, Toggle: doDebug);

                            string entryName = childNode.GetAttribute(TargetAttribute);
                            if (entryName != null)
                            {
                                Debug.Entry(4, $"{nameof(entryName)}", entryName, Indent: 2, Toggle: doDebug);

                                MutationEntry mutationEntry = MutationFactory.GetMutationEntryByName(entryName);

                                if (mutationEntry != null && !gameObjectBlueprint.Mutations.ContainsKey(mutationEntry.Class))
                                {
                                    Debug.Entry(4, $"{nameof(mutationEntry)}.Class", mutationEntry.Class, Indent: 2, Toggle: doDebug);

                                    childNode.Attributes["DisplayName"] = mutationEntry.GetDisplayName();

                                    GamePartBlueprint gamePartBlueprint = new("XRL.World.Parts.Mutation", mutationEntry.Class)
                                    {
                                        Name = mutationEntry.Class,
                                        Parameters = childNode.Attributes,
                                    };
                                    if (name != mutationEntry.Class && gameObjectBlueprint.Mutations.ContainsKey(name))
                                    {
                                        Debug.CheckYeh(4, $"gameObjectBlueprint.Mutations contains {name}", Indent: 2, Toggle: doDebug);
                                        Debug.LoopItem(4, $"{name}", "removed", Indent: 3, Toggle: doDebug);
                                        gameObjectBlueprint.Mutations.Remove(name);
                                    }
                                    gameObjectBlueprint.Mutations[gamePartBlueprint.Name] = gamePartBlueprint;
                                    Debug.LoopItem(4, $"{gamePartBlueprint.Name} added to gameObjectBlueprint.Mutations", Indent: 2, Toggle: doDebug);
                                }
                            }
                        }
                        Debug.Divider(4, HONLY, Count: 25, Indent: 2, Toggle: doDebug);
                        Debug.Entry(4,
                            $"x foreach ((string name, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode childNode) in node.NamedNodes(\"mutation\")) >//",
                            Indent: 1, Toggle: doDebug);

                        if (!node.UnnamedNodes("mutation").IsNullOrEmpty())
                        {
                            Debug.Entry(4, $"Checking Unnamed mutation nodes for {TargetAttribute.Quote()} attribute...", Indent: 1, Toggle: doDebug);

                            Debug.Entry(4,
                                $"> foreach (ObjectBlueprintLoader.ObjectBlueprintXMLChildNode childNode in node.UnnamedNodes(\"mutation\"))",
                                Indent: 1, Toggle: doDebug);
                            foreach (ObjectBlueprintLoader.ObjectBlueprintXMLChildNode childNode in node.UnnamedNodes("mutation"))
                            {
                                string entryName = childNode.GetAttribute(TargetAttribute);
                                if (entryName != null)
                                {
                                    Debug.Divider(4, HONLY, Count: 25, Indent: 2, Toggle: doDebug);

                                    Debug.Entry(4, $"{nameof(entryName)}", entryName, Indent: 2, Toggle: doDebug);

                                    MutationEntry mutationEntry = MutationFactory.GetMutationEntryByName(entryName);

                                    if (mutationEntry != null && !gameObjectBlueprint.Mutations.ContainsKey(mutationEntry.Class))
                                    {
                                        Debug.Entry(4, $"{nameof(mutationEntry)}.Class", mutationEntry.Class, Indent: 2, Toggle: doDebug);

                                        GamePartBlueprint gamePartBlueprint = new("XRL.World.Parts.Mutation", mutationEntry.Class)
                                        {
                                            Name = mutationEntry.Class,
                                            Parameters = childNode.Attributes,
                                        };
                                        gameObjectBlueprint.Mutations[gamePartBlueprint.Name] = gamePartBlueprint;
                                    }
                                }
                            }
                            Debug.Divider(4, HONLY, Count: 25, Indent: 2, Toggle: doDebug);
                            Debug.Entry(4,
                                $"x foreach (ObjectBlueprintLoader.ObjectBlueprintXMLChildNode childNode in node.UnnamedNodes(\"mutation\")) >//",
                                Indent: 1, Toggle: doDebug);
                        }

                        __result = gameObjectBlueprint;
                        __instance.Blueprints[node.Name] = __result;

                        Debug.Entry(4,
                            $"x [{MOD_ID}] {nameof(UDMT_GameObjectFactory_Patches)}."
                            + $"{nameof(LoadBakedXML_MutationEntryIfSupplied_Postfix)}"
                            + $"(ref GameObjectFactory __instance, "
                            + $"ref GameObjectBlueprint __result, "
                            + $"ObjectBlueprintLoader.ObjectBlueprintXMLData node) #//",
                            Indent: 0, Toggle: doDebug);
                    }
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException($"[{MOD_ID}] {nameof(UDMT_GameObjectFactory_Patches)}", x);
            }
        }
    }
}
