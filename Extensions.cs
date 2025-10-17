using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

using Genkit;
using Qud.API;

using XRL;
using XRL.UI;
using XRL.Rules;
using XRL.Language;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using XRL.World.Effects;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;

using static UD_Modding_Toolbox.Const;
using static UD_Modding_Toolbox.Utils;

namespace UD_Modding_Toolbox
{
    public static class Extensions
    {
        private static bool doDebug => true;
        private static bool getDoDebug(string MethodName)
        {
            if (MethodName == nameof(CheckEquipmentSlots))
                return false;

            if (MethodName == nameof(TagIsIncludedOrNotExcluded))
                return false;

            if (MethodName == nameof(MakeIncludeExclude))
                return false;

            if (MethodName == nameof(PullInsideFromEdges))
                return false;

            if (MethodName == nameof(PullInsideFromEdge))
                return false;

            if (MethodName == nameof(GetNumberedTileVariants))
                return false;

            if (MethodName == nameof(GetMovementsPerTurn))
                return false;

            if (MethodName == nameof(DrawSeededToken))
                return false;

            return doDebug;
        }

        public static bool InheritsFrom(this Type T, Type Type, bool IncludeSelf = true)
        {
            return (IncludeSelf && T == Type)
                || Type.IsSubclassOf(T)
                || T.IsAssignableFrom(Type)
                || T.YieldInheritedTypes().Contains(Type);
        }

        public static bool IsNullOrZero([NotNullWhen(false)] this int? value)
        {
            if (value != null)
            {
                return value == 0;
            }
            return true;
        }

        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this Raffle<T> value)
        {
            if (value != null)
            {
                return value.Count == 0;
            }
            return true;
        }

        public static Raffle<T> ToRaffle<T>(this ICollection<T> Collection)
        {
            Raffle<T> raffle = new();
            foreach (T item in Collection)
            {
                raffle.Add(item);
            }
            return raffle;
        }
        public static Raffle<T> ToRaffle<T>(this ReadOnlySpan<T> Span)
        {
            Raffle<T> raffle = new();
            foreach (T item in Span)
            {
                raffle.Add(item);
            }
            return raffle;
        }
        public static Raffle<T> ToRaffle<T>(this Span<T> Span)
        {
            return ((ReadOnlySpan<T>)Span).ToRaffle();
        }
        public static Raffle<char> ToRaffle(this string Chars)
        {
            return ((ReadOnlySpan<char>)Chars).ToRaffle();
        }

        public static int GetDieCount(this DieRoll DieRoll)
        {
            if (DieRoll == null)
            {
                return 0;
            }
            if (DieRoll.LeftValue > 0)
            {
                return DieRoll.LeftValue;
            }
            else
            {
                return DieRoll.Left.GetDieCount();
            }
        }
        public static int GetDieCount(this string DieRoll)
        {
            DieRoll dieRoll = new(DieRoll);
            return dieRoll.GetDieCount();
        }

        public static DieRoll AdjustDieCount(this DieRoll DieRoll, int Amount)
        {
            if (DieRoll == null)
            {
                return null;
            }
            int type = DieRoll.Type;
            if (DieRoll.LeftValue > 0)
            {
                DieRoll.LeftValue += Amount;
                return DieRoll;
            }
            else
            {
                if (DieRoll.RightValue > 0) return new(type, DieRoll.Left.AdjustDieCount(Amount), DieRoll.RightValue);
                return new(type, DieRoll.Left.AdjustDieCount(Amount), DieRoll.Right);
            }
        } //!-- public static DieRoll AdjustDieCount(this DieRoll DieRoll, int Amount)
       
        public static string AdjustDieCount(this string DieRoll, int Amount)
        {
            DieRoll dieRoll = new(DieRoll);
            return dieRoll.AdjustDieCount(Amount).ToString();
        }
        public static bool AdjustDamageDieCount(this MeleeWeapon MeleeWeapon, int Amount)
        {
            MeleeWeapon.BaseDamage = MeleeWeapon.BaseDamage.AdjustDieCount(Amount);
            return true;
        }
        public static bool AdjustDamageDieCount(this ThrownWeapon ThrownWeapon, int Amount)
        {
            ThrownWeapon.Damage = ThrownWeapon.Damage.AdjustDieCount(Amount);
            return true;
        }
        public static bool AdjustDamageDieCount(this Projectile Projectile, int Amount)
        {
            Projectile.BaseDamage = Projectile.BaseDamage.AdjustDieCount(Amount);
            return true;
        }

        public static string BonusOrPenalty(this int Int) 
        {
            return Int >= 0 ? "bonus" : "penalty";
        }
        public static string BonusOrPenalty(this string SignedInt)
        {
            if (int.TryParse(SignedInt, out int Int))
                return Int >= 0 ? "bonus" : "penalty";
            throw new ArgumentException(
                $"{nameof(BonusOrPenalty)}(this string SignedInt): " +
                $"int.TryParse(SignedInt) failed to parse \"{SignedInt}\". " +
                $"SignedInt must be capable of conversion to int.");
        }

        public static StringBuilder AppendColdSteel(this StringBuilder sb, string value)
        {
            sb.AppendColored("coldsteel", value);
            return sb;
        }
        public static StringBuilder AppendRule(this StringBuilder sb, string value)
        {
            // different from AppendRules (plural) since this doesn't force a new-line.
            sb.AppendColored("rules", value);
            return sb;
        }
        public static StringBuilder AppendNBSP(this StringBuilder sb, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                sb.AppendColored("K", "\xFF");
            }
            return sb;
        }

        public static StringBuilder AppendColored(this StringBuilder SB, string color, char text)
        {
            return SB.AppendColored(color, text.ToString());
        }
        public static StringBuilder AppendColored(this StringBuilder SB, string color, bool text)
        {
            return SB.AppendColored(color, text.ToString());
        }
        public static StringBuilder AppendColored(this StringBuilder SB, string color, int text)
        {
            return SB.AppendColored(color, text.ToString());
        }
        public static StringBuilder AppendColored(this StringBuilder SB, string color, float text)
        {
            return SB.AppendColored(color, text.ToString());
        }
        public static StringBuilder AppendColored(this StringBuilder SB, string color, double text)
        {
            return SB.AppendColored(color, text.ToString());
        }
        public static StringBuilder AppendColored(this StringBuilder SB, string color, long text)
        {
            return SB.AppendColored(color, text.ToString());
        }
        public static StringBuilder AppendColored(this StringBuilder SB, string color, uint text)
        {
            return SB.AppendColored(color, text.ToString());
        }

        public static StringBuilder Append(this StringBuilder SB, char text)
        {
            return SB.Append(text.ToString());
        }
        public static StringBuilder Append(this StringBuilder SB, bool text)
        {
            return SB.Append(text.ToString());
        }
        public static StringBuilder Append(this StringBuilder SB, int text)
        {
            return SB.Append(text.ToString());
        }
        public static StringBuilder Append(this StringBuilder SB, float text)
        {
            return SB.Append(text.ToString());
        }
        public static StringBuilder Append(this StringBuilder SB, double text)
        {
            return SB.Append(text.ToString());
        }
        public static StringBuilder Append(this StringBuilder SB, long text)
        {
            return SB.Append(text.ToString());
        }
        public static StringBuilder Append(this StringBuilder SB, uint text)
        {
            return SB.Append(text.ToString());
        }

        public static string MaybeColor(this string Text, string Color, bool Pretty = true)
        {
            if (Pretty && Color != "") return Text.Color(Color);
            return Text;
        }

        public static string OptionalColor(this string Text, string Color, string FallbackColor = "", int Option = 3)
        {
            // 3: Most Colorful
            // 2: Vanilla Only
            // 1: Plain Text
            return Text.MaybeColor(Color, Option > 2).MaybeColor(FallbackColor, Option > 1);
        }
        public static string OptionalColorColdSteel(this string Text, int Option = 3)
        {
            return Text.OptionalColor(Color: "coldsteel", FallbackColor: "m", Option);
        }
        public static string OptionalColorNothinPersonnel(this string Text, int Option = 3)
        {
            return Text.OptionalColor(Color: "nothinpersonnel", FallbackColor: "m", Option);
        }

        // ripped from the CyberneticPropertyModifier part, converted into extension.
        // Props must equal "string:int;string:int;string:int" where
        // string   is an IntProperty
        // int      is the value
        // ;        delimits each pair.
        // Example: "ChargeRangeModifier:2;JumpRangeModifier:1"
        public static Dictionary<string, int> ParseIntProps(this string Props)
        {
            Dictionary<string, int> dictionary = new();
            string[] array = Props.Split(';');
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = array[i].Split(':');
                dictionary.Add(array2[0], Convert.ToInt32(array2[1]));
            }
            return dictionary;
        }

        // as above, but for int:int progressions (good for single value level progressions).
        // Props must equal "string:string;string:string;string:string" where
        // string   is a StringProperty
        // string      is the value
        // ;        delimits each pair.
        // Example: "StringProp:StringValue;AnotherStringProp:SecondValue"
        public static Dictionary<string, string> ParseStringProps(this string Props)
        {
            Dictionary<string, string> dictionary = new();
            string[] array = Props.Split(';');
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = array[i].Split(':');
                dictionary.Add(array2[0], array2[1]);
            }
            return dictionary;
        }

        // as above, but for int:int progressions (good for single value level progressions).
        // Progression must equal "int:int;int:int;int:int" where
        // int      is the progression "interval"
        // int      is the value being progression
        // ;        delimits each pair.
        // Example: "1:2;3:3;6:4;9:5" starts at 2, and increases 1 every 3rd "interval"
        public static Dictionary<int, int> ParseIntProgInt(this string Progression)
        {
            Dictionary<int, int> dictionary = new();
            string[] array = Progression.Split(';');
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = array[i].Split(':');
                dictionary.Add(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]));
            }
            return dictionary;
        }

        // as above, but for int:DieRoll progressions (good for level-based damage progressions).
        // Progression must equal "int:(string)DieRoll;int:(string)DieRoll;int:(string)DieRoll" where
        // int              is the progression "interval"
        // (string)DieRoll  is string formatted DieRoll being progression
        // ;                delimits each pair.
        // Example: "1:1d2;3:1d3;6:1d4;9:1d5" starts at 1d2, and increases d1 every 3rd "interval"
        public static Dictionary<int, DieRoll> ParseIntProgDieRoll(this string Progression)
        {
            Dictionary<int, DieRoll> dictionary = new();
            string[] array = Progression.Split(';');
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = array[i].Split(':');
                DieRoll dieRoll = new DieRoll(array2[1]);
                dictionary.Add(Convert.ToInt32(array2[0]), dieRoll);
            }
            return dictionary;
        }

        // Similar to above, but it takes a series of string and int properties, intermixed, and gives them to two appropriately typed dictionaries.
        public static bool ParseProps(this string Props, out Dictionary<string, string> StringProps, out Dictionary<string, int> IntProps)
        {
            Dictionary<string, string> stringDictionary = new();
            Dictionary<string, int> intDictionary = new();
            string[] props = Props.Split(';');
            for (int i = 0; i < props.Length; i++)
            {
                string[] prop = props[i].Split(':');
                if (int.TryParse(prop[1], out int result))
                {
                    intDictionary.Add(prop[0], result);
                }
                else
                {
                    stringDictionary.Add(prop[0], prop[1]);
                }
            }
            StringProps = stringDictionary;
            IntProps = intDictionary;
            if (StringProps.Count == 0 && IntProps.Count == 0)
                return false;
            return true;
        }

        public static void SetSwingSound(this GameObject Object, string Path)
        {
            if (Path != null && Path != "")
                Object.SetStringProperty("SwingSound", Path);
        }
        public static void SetBlockedSound(this GameObject Object, string Path)
        {
            if (Path != null && Path != "")
                Object.SetStringProperty("BlockedSound", Path);
        }
        public static void SetEquipmentFrameColors(this GameObject Object, string TopLeft_Left_Right_BottomRight = null)
        {
            Object.SetStringProperty("EquipmentFrameColors", TopLeft_Left_Right_BottomRight, true);
        }

        public static void CheckEquipmentSlots(this GameObject Actor)
        {
            Debug.Entry(3, $"* {nameof(CheckEquipmentSlots)}(this GameObject Actor: {Actor.DebugName})", Toggle: getDoDebug(nameof(CheckEquipmentSlots)));
            Body Body = Actor?.Body;
            if (Body != null)
            {
                List<GameObject> list = Event.NewGameObjectList();
                Debug.Entry(3, "> foreach (BodyPart bodyPart in Actor.LoopParts())", Toggle: getDoDebug(nameof(CheckEquipmentSlots)));
                foreach (BodyPart bodyPart in Body.LoopParts())
                {
                    Debug.Entry(3, "bodyPart", $"{bodyPart.Description} [{bodyPart.ID}:{bodyPart.Name}]", Indent: 1, Toggle: getDoDebug(nameof(CheckEquipmentSlots)));
                    GameObject equipped = bodyPart.Equipped;
                    if (equipped != null && !list.Contains(equipped))
                    {
                        Debug.LoopItem(3, "equipped", $"[{equipped.ID}:{equipped.ShortDisplayName}]", Indent: 2, Toggle: getDoDebug(nameof(CheckEquipmentSlots)));
                        list.Add(equipped);
                        int partCountEquippedOn = Body.GetPartCountEquippedOn(equipped);
                        int slotsRequiredFor = equipped.GetSlotsRequiredFor(Actor, bodyPart.Type, true);
                        if (partCountEquippedOn != slotsRequiredFor 
                            && bodyPart.TryUnequip(true, true, false, false) 
                            && partCountEquippedOn > slotsRequiredFor)
                        {
                            equipped.SplitFromStack();
                            bodyPart.Equip(equipped, new int?(0), true, false, false, true);
                        }
                    }
                }
                Debug.Entry(3, "x foreach (BodyPart bodyPart in Actor.LoopParts()) >//", Toggle: getDoDebug(nameof(CheckEquipmentSlots)));
            }
            else
            {
                Debug.Entry(4, $"no body on which to perform check, aborting ", Toggle: getDoDebug(nameof(CheckEquipmentSlots)));
            }
            Debug.Entry(3, $"x {nameof(CheckEquipmentSlots)}(this GameObject Actor: {Actor.DebugName}) *//", Toggle: getDoDebug(nameof(CheckEquipmentSlots)));
        }

        public static IPart RequirePart(this GameObject Object, IPart Part, bool DoRegistration = true, bool Creation = false)
        {
            if (Object.HasPart(Part.Name))
            {
                return Object.GetPart(Part.Name);
            }
            Part.ParentObject = Object;
            return Object.AddPart(Part, DoRegistration: DoRegistration, Creation: Creation);
        }
        public static IPart RequirePart(this GameObject Object, string Part, bool DoRegistration = true, bool Creation = false)
        {
            if (Object.HasPart(Part))
            {
                return Object.GetPart(Part);
            }
            GamePartBlueprint gamePartBlueprint = new(Part);
            if (gamePartBlueprint == null) return null;
            IPart part = gamePartBlueprint.Reflector?.GetInstance() ?? (Activator.CreateInstance(gamePartBlueprint.T) as IPart);
            part.ParentObject = Object;
            gamePartBlueprint.InitializePartInstance(part);
            return Object.AddPart(part, DoRegistration: DoRegistration, Creation: Creation);
        }

        public static IPart ConvertToPart(this string Part)
        {
            GamePartBlueprint gamePartBlueprint = new(Part);
            IPart part = gamePartBlueprint.Reflector?.GetInstance() ?? (Activator.CreateInstance(gamePartBlueprint.T) as IPart);
            return part;
        }

        public static IModification ConvertToModification(this string ModPartName)
        {
            IModification ModPart;
            Type type = ModManager.ResolveType("XRL.World.Parts." + ModPartName);
            if (type == null)
            {
                MetricsManager.LogError("ConvertToModification", "Couldn'type resolve unknown mod part: " + ModPartName);
                return null;
            }
            ModPart = Activator.CreateInstance(type) as IModification;
            if (ModPart == null)
            {
                if (Activator.CreateInstance(type) is not IPart)
                {
                    MetricsManager.LogError("failed to load " + type);
                }
                else
                {
                    MetricsManager.LogError(type?.ToString() + " is not an IModification");
                }
                return null;
            }
            return ModPart;
        }

        public static bool IsDefaultEquipmentOf(this GameObject Object, BodyPart BodyPart)
        {
            return BodyPart.DefaultBehavior == Object;
        }

        public static BodyPart EquippingPart(this GameObject Object)
        {
            Body body = Object?.Equipped?.Body;
            if (body != null)
            {
                foreach (BodyPart part in body.LoopParts())
                {
                    if (part.DefaultBehavior == Object)
                        return part;
                    if (part.Equipped == Object)
                        return part;
                    if (part.Cybernetics == Object)
                        return part;
                }
            }
            return null;
        }

        public static bool InheritsFrom(this GameObject Object, string Blueprint)
        {
            return Object.Blueprint == Blueprint || Object.GetBlueprint().InheritsFrom(Blueprint);
        }

        // partially repurposed from https://stackoverflow.com/a/32184652
        public static bool SetPropertyValue(this object @object, string PropertyName, object Value)
        {
            PropertyInfo property = @object?.GetType()?.GetProperty(PropertyName);
            if (property == null) return false;
            Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            object safeValue = (Value == null) ? null : Convert.ChangeType(Value, type);

            property.SetValue(@object, safeValue, null);
            return property?.GetValue(@object, null) != null;
        }
        // partially repurposed from https://stackoverflow.com/a/1965659
        public static bool SetFieldValue(this object @object, string FieldName, object Value)
        {
            FieldInfo field = @object?.GetType()?.GetField(FieldName);
            if (field == null) return false;
            Type type = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;
            object safeValue = (Value == null) ? null : Convert.ChangeType(Value, type);

            field.SetValue(@object, safeValue);
            return field?.GetValue(@object) != null;
        }
        public static bool SetPropertyOrFieldValue(this object @object, string PropertyOrField, object Value)
        {
            return @object.SetPropertyValue(PropertyOrField, Value) || @object.SetFieldValue(PropertyOrField, Value);
        }

        public static string NearDemonstrative(this GameObject Object)
        {
            return Object.IsPlural ? "these" : "this";
        }
        public static string FarDemonstrative(this GameObject Object)
        {
            return Object.IsPlural ? "those" : "that";
        }

        public static string GetProcessedItem(this List<string> item, bool second, List<List<string>> items, GameObject obj)
        {
            if (item[0] == "")
            {
                if (second && item == items[0])
                {
                    return obj.It + " " + item[1];
                }
                return item[1];
            }
            if (item[0] == null)
            {
                if (second && item == items[0])
                {
                    return obj.Itis + " " + item[1];
                }
                if (item != items[0])
                {
                    bool flag = true;
                    foreach (List<string> item2 in items)
                    {
                        if (item2[0] != null)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        return item[1];
                    }
                }
                return obj.GetVerb("are", PrependSpace: false) + " " + item[1];
            }
            if (second && item == items[0])
            {
                return obj.It + obj.GetVerb(item[0]) + " " + item[1];
            }
            return obj.GetVerb(item[0], PrependSpace: false) + " " + item[1];
        }

        public static string GetObjectNoun(this GameObject Object)
        {
            if (Object == null)
                return null;

            if (!Object.Understood())
                return "artifact";

            if (Object.InheritsFrom("FoldingChair") 
                && Object.HasPart<ModGigantic>())
                return "folding chair";

            if (Object.IsCreature)
                return "creature";

            if (Object.HasPart<CyberneticsBaseItem>())
                return "implant";

            if (Object.InheritsFrom("Tonic"))
                return "tonic";

            if (Object.HasPart<Medication>())
                return "medication";

            if (Object.InheritsFrom("Energy Cell"))
                return "energy cell";

            if (Object.InheritsFrom("LightSource"))
                return "light source";

            if (Object.InheritsFrom("Tool"))
                return "tool";

            if (Object.InheritsFrom("BaseRecoiler"))
                return "recoiler";

            if (Object.InheritsFrom("BaseNugget"))
                return "nugget";

            if (Object.InheritsFrom("Gemstone"))
                return "gemstone";

            if (Object.InheritsFrom("Random Figurine"))
                return "figurine";

            if (Object.HasPart<Applicator>())
                return "applicator";

            if (Object.HasPart<Tombstone>())
                return "tombstone";

            if (Object.TryGetPart(out MissileWeapon missileWeapon))
            {
                if (missileWeapon.Skill.Contains("Shotgun") 
                    || Object.InheritsFrom("BaseShotgun") 
                    || (Object.TryGetPart(out MagazineAmmoLoader loader) && loader.AmmoPart is "AmmoShotgunShell"))
                    return "shotgun";

                if (Object.InheritsFrom("BaseHeavyWeapon"))
                    return "heavy weapon";

                if (Object.InheritsFrom("BaseBow"))
                    return "bow";

                if (Object.InheritsFrom("BaseRifle"))
                    return "rifle";

                if (Object.InheritsFrom("BasePistol"))
                    return "pistol";

                return "missile weapon";
            }

            if (Object.TryGetPart(out ThrownWeapon thrownWeapon) && !thrownWeapon.IsImprovised())
            {
                if (Object.InheritsFrom("BaseBoulder"))
                    return "boulder";

                if (Object.InheritsFrom("BaseStone"))
                    return "stone";

                if (Object.InheritsFrom("Grenade"))
                    return "grenade";

                if (Object.InheritsFrom("BaseDagger"))
                    return "dagger";

                return "thrown weapon";
            }

            if (Object.TryGetPart(out MeleeWeapon meleeWeapon) && !meleeWeapon.IsImprovised())
            {
                if (Object.InheritsFrom("BaseCudgel"))
                    return "cudgel";

                if (Object.InheritsFrom("BaseAxe"))
                    return "axe";

                if (Object.InheritsFrom("BaseLongBlade"))
                    return "long blade";

                if (Object.InheritsFrom("BaseDagger"))
                    return "short blade";

                return "weapon";
            }

            if (Object.TryGetPart(out Armor armor))
            {
                if (!Object.IsPluralIfKnown)
                {
                    switch (armor.WornOn)
                    {
                        case "Back":
                            {
                                if (Object.Blueprint is "Mechanical Wings")
                                    return "wing";

                                if (Object.Blueprint is "Gas Tumbler")
                                    return "tumbler";

                                if (armor.CarryBonus > 0
                                    || Object.Blueprint is "Gyrocopter Backpack"
                                    || Object.Blueprint is "SkybearJetpack")
                                    return "pack";

                                return "cloak";
                            }
                        case "Head":
                            {
                                if (armor.AV < 3)
                                    return "hat";

                                return "helmet";
                            }
                        case "Face":
                            {
                                if (Object.InheritsFrom("BaseMask"))
                                    return "mask";

                                break;
                            }
                        case "Body":
                            {
                                if (Object.Blueprint.Contains("Plate"))
                                    return "plate";

                                if (armor.AV > 2)
                                    return "suit";

                                return "vest";
                            }
                        case "Arm":
                            {
                                if (Object.InheritsFrom("BaseUtilityBracelet"))
                                    return "utility bracelet";

                                if (Object.InheritsFrom("BaseBracelet"))
                                    return "bracelet";

                                if (Object.InheritsFrom("BaseArmlet"))
                                    return "armlet";

                                break;
                            }
                        default:
                            return "armor";
                    };
                }
                switch (armor.WornOn)
                {
                    case "Back":
                        {
                            if (Object.Blueprint is "Mechanical Wings")
                                return "wing";

                            if (Object.Blueprint is "Gas Tumbler")
                                return "tumbler";

                            break;
                        }
                    case "Face":
                        {
                            if (Object.HasPart<XRL.World.Parts.Spectacles>())
                                return "spectacle";

                            if (Object.InheritsFrom("BaseEyewear"))
                                return "goggle";

                            if (Object.InheritsFrom("BaseFaceJewelry"))
                                return "jewelry";

                            if (Object.Blueprint is "VISAGE")
                                return "scanner";

                            break;
                        }
                    case "Hands":
                        {
                            if (Object.HasPart<Metal>())
                                return "gauntlet";

                            return "glove";
                        }
                    case "Feet":
                        {
                            if (Object.InheritsFrom("BaseBoot"))
                            {
                                if (Object.HasPart<Metal>())
                                    return "sabaton";
                                return "boot";
                            }

                            return "shoe";
                        }
                    default:
                        break;
                }
            }
            if (Object.InheritsFrom("Furniture"))
            {
                string bodyType = Object.GetPropertyOrTag("BodyType");
                string @class = Object.GetPropertyOrTag("Class");
                if (!@class.IsNullOrEmpty())
                {
                    if (!bodyType.IsNullOrEmpty())
                    {
                        if (bodyType is "Pillow")
                        {
                            return "seat";
                        }
                    }
                    return @class;
                }
                if (!bodyType.IsNullOrEmpty())
                {
                    return bodyType.SplitCamelCase().ToLower();
                }
                if (Object.InheritsFrom("Statue") || Object.InheritsFrom("Random Statue"))
                {
                    return "statue";
                }
                if (Object.InheritsFrom("Eater Hologram"))
                {
                    return "hologram";
                }
                if (Object.InheritsFrom("Switch"))
                {
                    return "switch";
                }
                if (Object.InheritsFrom("Sign"))
                {
                    return "sign";
                }
                if (Object.InheritsFrom("BaseBookshelf"))
                {
                    return "bookshelf";
                }
                if (Object.HasPart<MergeConduit>())
                {
                    return "power conduit";
                }
                if (Object.HasPart<Container>() || Object.HasPart<LiquidVolume>())
                {
                    return "container";
                }
                if (Object.TryGetPart(out LightSource lightSource) && lightSource.Radius > 0)
                {
                    return "light source";
                }
            }
            if (Object.HasPart<Chair>())
            {
                return "chair";
            }
            if (Object.HasPart<Bed>())
            {
                return "bed";
            }
            if (Object.HasPart<Container>() || Object.HasPart<LiquidVolume>())
            {
                return "container";
            }
            switch (Scanning.GetScanTypeFor(Object))
            {
                case Scanning.Scan.Tech:
                    return "artifact";
                case Scanning.Scan.Bio:
                    return "organism";
                default:
                    if (Object.HasPart<XRL.World.Parts.Shield>() && !Object.IsPluralIfKnown)
                    {
                        return "shield";
                    }
                    if (!Object.Takeable)
                    {
                        return Object.Render?.DisplayName ?? "object";
                    }
                    return Object.Render?.DisplayName ?? "item";
            }
        }

        public static bool IsImprovised(this ThrownWeapon ThrownWeapon)
        {
            bool hasImprovisedProp =
                ThrownWeapon.ParentObject.HasTagOrStringProperty("IsImprovisedThrown")
             && ThrownWeapon.ParentObject.GetStringProperty("IsImprovisedThrown") != "false";

            ThrownWeapon @default = new();
            return ThrownWeapon.SameAs(@default)
                || ThrownWeapon.ParentObject.GetIntProperty("IsImprovisedThrown") > 0
                || hasImprovisedProp;
        }

        public static bool IsImprovised(this MeleeWeapon MeleeWeapon)
        {
            bool isImprovisedButGigantic = MeleeWeapon.ParentObject.HasPart<ModGigantic>()
             && MeleeWeapon.MaxStrengthBonus == 0 
             && MeleeWeapon.PenBonus == 0 
             && MeleeWeapon.HitBonus == 0 
             && (MeleeWeapon.BaseDamage is "1d2" || MeleeWeapon.BaseDamage is "1d2+3") 
             && MeleeWeapon.Ego == 0 
             && MeleeWeapon.Skill is "Cudgel" 
             && MeleeWeapon.Stat is "Strength"
             && MeleeWeapon.Slot is "Hand"
             && MeleeWeapon.Attributes.IsNullOrEmpty();

            bool hasImprovisedProp = 
                MeleeWeapon.ParentObject.HasTagOrStringProperty("IsImprovisedMelee") 
             && MeleeWeapon.ParentObject.GetStringProperty("IsImprovisedMelee") is not "false";

            MeleeWeapon @default = new();

            return MeleeWeapon.SameAs(@default) 
                || MeleeWeapon.IsImprovisedWeapon() 
                || isImprovisedButGigantic 
                || MeleeWeapon.ParentObject.GetIntProperty("IsImprovisedMelee") > 0
                || hasImprovisedProp;
        }

        public static List<GameObject> GetNaturalEquipment(this Body Body)
        {
            static bool filter(GameObject GO) { return GO.HasPart<NaturalEquipment>() || GO.HasTag("NaturalGear"); }
            return Body.GetEquippedObjects(filter);
        }

        public static string ToLiteral(this string String, bool Quotes = false)
        {
            if (String.IsNullOrEmpty())
            {
                return null;
            }
            string output = Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(String, false);
            if (Quotes)
            {
                output = output.Quote();
            }
            return output;
        }
        public static bool IsEndOfSection(this OpCode OpCode)
        {
            string ciOpcode = OpCode.ToString();
            return ciOpcode.StartsWith("pop")
                || ciOpcode.StartsWith("br")
                || ciOpcode.StartsWith("be")
                || ciOpcode.StartsWith("bg")
                || ciOpcode.StartsWith("bl")
                || ciOpcode.StartsWith("leave")
                || ciOpcode.StartsWith("ret")
                || ciOpcode.StartsWith("st")
                || ciOpcode.StartsWith("throw");
        }

        public static LocalBuilder GetLocalAtIndex(this MethodBase MethodBase, int Index)
        {
            return MethodBase.GetMethodBody().LocalVariables[Index] as LocalBuilder;
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.DrawCosmetic) + " instead.")]
        public static T DrawRandomToken<T>(
            this List<T> Bag,
            T ExceptForToken = default,
            List<T> ExceptForTokens = null)
        {
            return Bag.DrawSeededToken(
                Seed: (string)null,
                Stepper: null,
                Context: null,
                ExceptForToken: ExceptForToken,
                ExceptForTokens: ExceptForTokens);
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.DrawCosmetic) + " instead.")]
        public static T DrawRandomToken<T>(
            this List<T> Bag,
            Predicate<T> Filter = null)
        {
            return Bag.DrawSeededToken(
                Seed: (string)null,
                Stepper: null,
                Context: null,
                Filter: Filter);
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.Draw) + " instead.")]
        public static T DrawSeededToken<T>(
            this List<T> Bag,
            string Seed,
            int? Stepper = null,
            string Context = null,
            T ExceptForToken = default,
            List<T> ExceptForTokens = null)
        {
            if (ExceptForToken == null && ExceptForTokens == null)
            {
                return Bag.DrawSeededToken(
                    Seed: Seed,
                    Stepper: Stepper,
                    Context: Context,
                    Filter: null);
            }

            bool Filter(T t)
            {
                if ((ExceptForToken != null && Equals(t, ExceptForToken))
                    || (!ExceptForTokens.IsNullOrEmpty() && ExceptForTokens.Contains(t)))
                {
                    return false;
                }
                return true;
            }
            return Bag.DrawSeededToken(
                Seed: Seed,
                Stepper: Stepper,
                Context: Context,
                Filter: Filter);
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.Draw) + " instead.")]
        public static T DrawSeededToken<T>(
            this List<T> Bag,
            string Seed,
            int? Stepper = null,
            string Context = null,
            Predicate<T> Filter = null)
        {
            if (Bag.IsNullOrEmpty())
            {
                return default;
            }
            List<T> drawBag = (from T t in Bag where Filter == null || Filter(t) select t).ToList();
            if (drawBag.IsNullOrEmpty())
            {
                return default;
            }

            T token = default;

            if (!Seed.IsNullOrEmpty())
            {
                string stepper = Stepper == null ? null : ("-" + Stepper);
                string context = Context.IsNullOrEmpty() ? null : ("-" + Context);

                string seed = Seed + context + stepper;
                int low = 0;
                int high = (drawBag.Count - 1) * 7;
                int roll = Stat.SeededRandom(seed, low, high) % (drawBag.Count - 1);

                int indent = Debug.LastIndent;
                bool doDebug = getDoDebug(nameof(DrawSeededToken));
                Debug.Divider(4, HONLY, Count: 25, Indent: indent + 1, Toggle: doDebug);
                Debug.Entry(4, $"{nameof(Seed)}: {Seed}", Indent: indent + 1, Toggle: doDebug);
                Debug.Entry(4, $"{nameof(Stepper)}: {Stepper}", Indent: indent + 1, Toggle: doDebug);
                Debug.Entry(4, $"{nameof(Context)}: {Context}", Indent: indent + 1, Toggle: doDebug);
                Debug.Entry(4, $"{nameof(seed)}: {seed}", Indent: indent + 1, Toggle: doDebug);
                Debug.Entry(4, $"{nameof(low)}: {low}", Indent: indent + 1, Toggle: doDebug);
                Debug.Entry(4, $"{nameof(drawBag.Count)} - 1: {drawBag.Count - 1}", Indent: indent + 1, Toggle: doDebug);
                Debug.Entry(4, $"{nameof(high)}: {high}", Indent: indent + 1, Toggle: doDebug);
                Debug.Entry(4, $"{nameof(roll)}: {roll}", Indent: indent + 1, Toggle: doDebug);
                Debug.Divider(4, HONLY, Count: 25, Indent: indent + 1, Toggle: doDebug);
                Debug.LastIndent = indent;

                token = drawBag.ElementAt(roll);
            }
            token ??= drawBag.GetRandomElement();
            Bag.Remove(token);
            return token;
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.Draw) + " instead.")]
        public static T DrawSeededToken<T>(
            this List<T> Bag,
            Guid Seed,
            int? Stepper = null,
            string Context = null,
            T ExceptForToken = default,
            List<T> ExceptForTokens = null)
        {
            return Bag.DrawSeededToken(
                Seed: Seed.ToString(),
                Stepper: Stepper,
                Context: Context,
                ExceptForToken: ExceptForToken,
                ExceptForTokens: ExceptForTokens);
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.Draw) + " instead.")]
        public static T DrawToken<T>(this List<T> Bag, T Token)
        {
            T token = (!Bag.IsNullOrEmpty() || Bag.Remove(Token)) ? Token : default;
            return token;
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.DrawCosmetic) + " instead.")]
        public static T DrawRandomToken<T>(
            this Dictionary<string, List<T>> Bag,
            string FromPocket = null,
            T ExceptForToken = default,
            List<T> ExceptForTokens = null)
        {
            return Bag.DrawSeededToken(
                Seed: (string)null,
                Stepper: null,
                Context: null,
                FromPocket: FromPocket,
                ExceptForToken: ExceptForToken,
                ExceptForTokens: ExceptForTokens);
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.Draw) + " instead.")]
        public static T DrawSeededToken<T>(
            this Dictionary<string,List<T>> Bag,
            string Seed,
            int? Stepper = null,
            string Context = null,
            string FromPocket = null,
            T ExceptForToken = default,
            List<T> ExceptForTokens = null)
        {
            List<T> drawBag = new();
            ExceptForTokens ??= new();
            bool haveTargetPocket = !FromPocket.IsNullOrEmpty() && Bag.ContainsKey(FromPocket);
            if (haveTargetPocket)
            {
                drawBag = Bag[FromPocket];
            }
            else
            {
                foreach ((_, List<T> pocket) in Bag)
                {
                    foreach (T element in pocket)
                    {
                        drawBag.TryAdd(element);
                    }
                }
            }

            T token = drawBag.DrawSeededToken(Seed, Stepper, Context, ExceptForToken, ExceptForTokens);

            if (haveTargetPocket)
            {
                if (Bag[FromPocket].Contains(token))
                {
                    Bag[FromPocket].Remove(token);
                }
            }
            else
            {
                foreach ((_, List<T> pocket) in Bag)
                {
                    if (pocket.Contains(token))
                    {
                        pocket.Remove(token);
                    }
                }
            }
            return token;
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.Draw) + " instead.")]
        public static T DrawSeededToken<T>(
            this Dictionary<string, List<T>> Bag,
            Guid Seed,
            int? Stepper = null,
            string Context = null,
            string FromPocket = null,
            T ExceptForToken = null,
            List<T> ExceptForTokens = null)
            where T : class
        {
            return Bag.DrawSeededToken(
                Seed: Seed.ToString(),
                Stepper: Stepper,
                Context: Context,
                FromPocket: FromPocket,
                ExceptForToken: ExceptForToken,
                ExceptForTokens: ExceptForTokens);
        }

        [Obsolete("Cast to " + nameof(Raffle<object>) + " and use " + nameof(Raffle<object>.Draw) + " instead.")]
        public static T DrawToken<T>(
            this Dictionary<string, List<T>> Bag,
            T Token,
            string FromPocket = null)
            where T : class
        {
            List<T> drawBag = new();
            bool haveTargetPocket = !FromPocket.IsNullOrEmpty() && Bag.ContainsKey(FromPocket);
            if (haveTargetPocket)
            {
                drawBag = Bag[FromPocket];
            }
            else
            {
                foreach ((_, List<T> pocket) in Bag)
                {
                    foreach (T element in pocket)
                    {
                        drawBag.TryAdd(element);
                    }
                }
            }

            T token = (!drawBag.IsNullOrEmpty() && drawBag.Contains(Token)) ? Token : null;

            if (haveTargetPocket)
            {
                if (Bag[FromPocket].Contains(token))
                {
                    Bag[FromPocket].Remove(token);
                }
            }
            else
            {
                foreach ((_, List<T> pocket) in Bag)
                {
                    if (pocket.Contains(token))
                    {
                        pocket.Remove(token);
                    }
                }
            }
            return token;
        }

        public static bool Contains<T>(
            this Dictionary<string, List<T>> Bag,
            T Token,
            out string Key,
            string InPocket = null)
            where T : class
        {
            bool haveToken = false;
            Key = null;
            if (!Bag.IsNullOrEmpty())
            {
                bool haveTargetPocket = !InPocket.IsNullOrEmpty() && Bag.ContainsKey(InPocket);
                foreach ((string key, List<T> pocket) in Bag)
                {
                    if (haveTargetPocket && InPocket != key)
                    {
                        continue;
                    }
                    foreach (T token in pocket)
                    {
                        if (token == Token)
                        {
                            Key = key;
                            haveToken = true;
                            break;
                        }
                    }
                }
            }
            return haveToken;
        }
        public static bool Contains<T>(this Dictionary<string, List<T>> Bag, T Token, string InPocket = null)
            where T : class
        {
            return Bag.Contains(Token, out _, InPocket);
        }
        public static IEnumerable<T> GetContents<T>(this Dictionary<string, List<T>> Bag, string FromPocket = null)
            where T : class
        {
            if (!Bag.IsNullOrEmpty())
            {
                bool haveTargetPocket = !FromPocket.IsNullOrEmpty() && Bag.ContainsKey(FromPocket);
                foreach ((string key, List<T> pocket) in Bag)
                {
                    if (haveTargetPocket && FromPocket != key)
                    {
                        continue;
                    }
                    foreach (T token in pocket)
                    {
                        yield return token;
                    }
                }
            }
            yield break;
        }
        public static IEnumerable<string> GetContentsAsString<T>(
            this Dictionary<string, List<T>> Bag,
            string FromPocket = null,
            bool WithKey = false)
            where T : class
        {
            if (!Bag.IsNullOrEmpty())
            {
                bool haveTargetPocket = !FromPocket.IsNullOrEmpty() && Bag.ContainsKey(FromPocket);
                foreach ((string key, List<T> pocket) in Bag)
                {
                    if (haveTargetPocket && FromPocket != key)
                    {
                        continue;
                    }
                    if (WithKey)
                    {
                        yield return "@" + key;
                    }
                    foreach (T token in pocket)
                    {
                        if (token is string elementString)
                        {
                            yield return elementString;
                        }
                        else yield break;
                    }
                }
            }
            yield break;
        }

        public static GameObjectBlueprint GetGameObjectBlueprint(this GameObject GameObject)
        {
            return Utils.GetGameObjectBlueprint(GameObject?.Blueprint);
        }
        public static bool TryGetGameObjectBlueprint(this GameObject GameObject, out GameObjectBlueprint GameObjectBlueprint)
        {
            GameObjectBlueprint = GameObject.GetGameObjectBlueprint();
            return GameObjectBlueprint is not null;
        }

        public static string DebugName(this BodyPart BodyPart)
        {
            return $"[{BodyPart.ID}:{BodyPart.Name}]::{BodyPart.Description}";
        }

        public static int Between(this int @int, int Min, int Max)
        {
            return Math.Min(Math.Max(@int, Min), Max);
        }
        public static double Between(this double @double, double Min, double Max)
        {
            return Math.Min(Math.Max(@double, Min), Max);
        }
        public static float Between(this float @float, float Min, float Max)
        {
            return Math.Min(Math.Max(@float, Min), Max);
        }

        public static int RapidAdvancementCeiling(this int @int, int MinAdvances = 0)
        {
            MinAdvances = MinAdvances > 0 ? (int)Math.Ceiling(MinAdvances / 3.0) : 0;
            return (int)Math.Max(MinAdvances, Math.Ceiling(@int / 3.0)) * 3;
        }

        public static int RapidAdvancementFloor(this int @int, int MinAdvances = 0)
        {
            MinAdvances = MinAdvances > 0 ? (int)Math.Ceiling(MinAdvances / 3.0) : 0;
            return (int)Math.Max(MinAdvances, Math.Floor(@int / 3.0)) * 3;
        }

        public static int RapidAdvancementRound(this int @int, int MinAdvances = 0)
        {
            MinAdvances = MinAdvances > 0 ? (int)Math.Ceiling(MinAdvances / 3.0) : 0;
            return (int)Math.Max(MinAdvances, Math.Round(@int / 3.0)) * 3;
        }

        public static string Are(this GameObject Object)
        {
            return Object.IsPlural || (Object.IsPlayer() && Grammar.AllowSecondPerson) ? "Are" : "Is";
        }
        public static string are(this GameObject Object)
        {
            return Object.IsPlural || (Object.IsPlayer() && Grammar.AllowSecondPerson) ? "are" : "is";
        }
        public static string SplitCamelCase(this string @string)
        {
            return Regex.Replace(
                Regex.Replace(
                    @string,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        public static T Sample<T>(this Dictionary<T, int> WeightedList)
            where T : class
        {
            T Output = default;
            int weightMax = 0;
            foreach ((_, int entryWeight) in WeightedList)
            {
                weightMax += entryWeight;
            }
            int ticket = Stat.Roll(1, weightMax);
            int weightCurrent = 0;
            foreach ((T entryT, int entryWeight) in WeightedList)
            {
                weightCurrent += entryWeight;
                if (ticket <= weightCurrent)
                {
                    Output = entryT;
                    break;
                }
            }
            return Output;
        }
        public static T Draw<T>(this Dictionary<T, int> WeightedList)
            where T : class
        {
            T Output = WeightedList.Sample();
            if(--WeightedList[Output] == 0)
                WeightedList.Remove(Output);
            return Output;
        }
        public static void AddTicket<T>(this Dictionary<T, int> WeightedList, T Ticket)
            where T : class
        {
            if (WeightedList.ContainsKey(Ticket))
            {
                WeightedList[Ticket]++;
            }
            else
            {
                WeightedList.Add(Ticket, 1);
            }
        }

        public static bool TagIsIncludedOrNotExcluded(this GameObjectBlueprint Blueprint, string TagName, Dictionary<string, bool> IncludeExclude)
        {
            Debug.Entry(4, 
                $"* ({Blueprint.Name})."
                + $"{nameof(TagIsIncludedOrNotExcluded)}" 
                + $"(string TagName: {TagName}, Dictionary<string, bool> IncludeExclude)",
                Indent: 0, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));

            if (!IncludeExclude.IsNullOrEmpty())
            {

                Debug.CheckYeh(4, $"IncludeExclude Not Empty", Indent: 1, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));

                List<string> includeTags = new();
                List<string> excludeTags = new();

                Debug.Entry(4, $"IncludeExclude values:", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                foreach ((string entryValue, bool entryInclude) in IncludeExclude)
                {
                    if (entryInclude) includeTags.Add(entryValue);
                    else excludeTags.Add(entryValue);
                    Debug.LoopItem(4, $"{(entryInclude ? "include" : "exclude")}: {entryValue}", 
                        Indent: 2, Good: entryInclude, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                }

                bool noBlueprintTagValue = !Blueprint.TryGetTag(TagName, out string blueprintTagValue);
                bool noBlueprintPart = true;
                foreach ((string part,_) in Blueprint.Parts)
                {
                    if (includeTags.Contains(part.ToLower()))
                    {
                        noBlueprintPart = false;
                        break;
                    }
                }
                bool noBlueprintTagOrPart = noBlueprintTagValue && noBlueprintPart;
                if (!includeTags.IsNullOrEmpty() && noBlueprintTagOrPart)
                {
                    Debug.CheckNah(4, $"includeTags not empty and {Blueprint.Name} doesn't have \"{TagName}\" tag or equivalent Part", 
                        Indent: 1, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                    return false; 
                }
                else if (noBlueprintTagOrPart)
                {
                    Debug.CheckYeh(4, 
                        $"includeTags is empty and {Blueprint.Name} doesn't have \"{TagName}\" tag or equivalent Part, " + 
                        $"exclusions are irrelevant",
                        Indent: 1, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                    return true;
                }

                Debug.CheckYeh(4, $"{TagName}: \"{blueprintTagValue}\"", Indent: 1, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                List<string> blueprintTagValues = new();
                if (blueprintTagValue != null)
                {
                    if (blueprintTagValue.Contains(","))
                    {
                        Debug.Entry(4, $"blueprintTagValue contains \",\"", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                        string[] tagsArray = blueprintTagValue.Split(',');
                        foreach (string value in tagsArray)
                        {
                            Debug.LoopItem(4, $" \"{value}\" added to values list", Indent: 3, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                            blueprintTagValues.Add(value);
                        }
                    }
                    else
                    {
                        Debug.Entry(4, $"blueprintTagValue doesn't contain \",\"", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                        Debug.LoopItem(4, $" \"{blueprintTagValue}\" added to values list", Indent: 3, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                        blueprintTagValues.Add(blueprintTagValue);
                    }
                }
                foreach ((string part, _) in Blueprint.Parts)
                {
                    blueprintTagValues.Add(part.ToLower());
                }

                List<string> inclusionMatches = new();
                List<string> exclusionMatches = new();

                Debug.Entry(4, $"blueprintTagValues Matches:", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                foreach (string tagValue in blueprintTagValues)
                {
                    if (includeTags.Contains(tagValue))
                    {
                        Debug.CheckYeh(4, $"includeTags contains {tagValue}", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                        inclusionMatches.Add(tagValue);
                    }
                    if (excludeTags.Contains(tagValue))
                    {
                        Debug.CheckNah(4, $"excludeTags contains {tagValue}", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                        exclusionMatches.Add(tagValue);
                    }
                }

                Debug.Entry(4, $"inclusionMatches:", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                foreach (string entry in inclusionMatches)
                {
                    Debug.CheckYeh(4, $"{entry}", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                }
                if (inclusionMatches.IsNullOrEmpty())
                    Debug.CheckNah(4, $"Empty", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));

                Debug.Entry(4, $"exclusionMatches:", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                foreach (string entry in exclusionMatches)
                {
                    Debug.CheckNah(4, $"{entry}", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
                }
                if (exclusionMatches.IsNullOrEmpty())
                    Debug.CheckYeh(4, $"Empty", Indent: 2, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));

                if (!includeTags.IsNullOrEmpty() && inclusionMatches.IsNullOrEmpty())
                    return false;
                Debug.CheckYeh(4, $"includeTags was Empty, or there was at least one inclusionMatch", Indent: 1, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));

                if (!excludeTags.IsNullOrEmpty() && !exclusionMatches.IsNullOrEmpty())
                    return false;
                Debug.CheckYeh(4, $"excludeTags was Empty, or there were no exclusionMatchs", Indent: 1, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
            }
            Debug.CheckYeh(4, $"IncludeExclude was empty or filter allowed blueprint through", Indent: 1, Toggle: getDoDebug(nameof(TagIsIncludedOrNotExcluded)));
            return true;
        }

        public static void MakeIncludeExclude(this string valueString, Dictionary<string, bool> @return)
        {
            Debug.Entry(4,
                $"* {valueString}."
                + $"{nameof(MakeIncludeExclude)}"
                + $"(Dictionary<string, bool> @return)",
                Indent: 0, Toggle: getDoDebug(nameof(MakeIncludeExclude)));

            if (!valueString.IsNullOrEmpty())
            {
                Debug.Entry(4, $"value not empty or null", Indent: 1, Toggle: getDoDebug(nameof(MakeIncludeExclude)));
                if (valueString.Contains(","))
                {
                    Debug.Entry(4, $"valueString contains \",\"", Indent: 1, Toggle: getDoDebug(nameof(MakeIncludeExclude)));
                    string[] classesArray = valueString.Split(',');
                    foreach (string entry in classesArray)
                    {
                        bool isNot = entry.StartsWith("!");
                        string value = isNot ? entry.Substring(1) : entry;
                        Debug.LoopItem(4, $"{value}: {(!isNot ? "include" : "exclude")}", Indent: 2, Good: !isNot, Toggle: getDoDebug(nameof(MakeIncludeExclude)));
                        @return.Add(value.ToLower(), !isNot);
                    }
                }
                else
                {
                    Debug.Entry(4, $"valueString doesn't contain \",\"", Indent: 1, Toggle: getDoDebug(nameof(MakeIncludeExclude)));

                    bool isNot = valueString.StartsWith("!");
                    string value = isNot ? valueString.Substring(1) : valueString;
                    Debug.LoopItem(4, $"{value}: {(!isNot ? "include" : "exclude")}", Indent: 2, Good: !isNot, Toggle: getDoDebug(nameof(MakeIncludeExclude)));
                    @return.Add(value.ToLower(), !isNot);
                }
            }
        }
        public static string GetOwner(this GameObjectBlueprint Blueprint)
        {
            return Blueprint?.GetPartParameter<string>("Physics", "Owner");
        }
        public static bool HasOwner(this GameObjectBlueprint Blueprint)
        {
            return !Blueprint.GetOwner().IsNullOrEmpty();
        }

        public static string Quote(this string @string)
        {
            return Utils.Quote($"{@string}");
        }

        public static Dictionary<string,List<Cell>> GetHutRegion(this Zone Z, Rect2D R, bool Round = false)
        {
            string Inner = "Inner";
            string Outer = "Outer";
            string Corners = "Corners";
            string NorthEdge = "NorthEdge";
            string SouthEdge = "SouthEdge";
            string EastEdge = "EastEdge";
            string WestEdge = "WestEdge";
            string Door = "Door";
            Dictionary<string, List<Cell>> Region = new()
            {
                { Inner, new() },
                { Outer, new() },
                { Corners, new() },
                { NorthEdge, new() },
                { SouthEdge, new() },
                { EastEdge, new() },
                { WestEdge, new() },
                { Door, new() },
            };
            Rect2D r = R.ReduceBy(1, 1);
            Cell cell;
            for (int i = r.y1; i <= r.y2; i++)
            {
                for (int j = r.x1; j <= r.x2; j++)
                {
                    if ((cell = Z.GetCell(j, i)) == null) continue;
                    Region[Inner].Add(cell);
                }
            }
            if (Round)
            {
                for (int k = R.x1 + 1; k <= R.x2 - 1; k++)
                {
                    if ((cell = Z.GetCell(k, R.y1)) != null)
                        Region[Outer].Add(cell);
                    if ((cell = Z.GetCell(k, R.y2)) != null)
                        Region[Outer].Add(cell);
                }
                for (int l = R.y1 + 1; l <= R.y2 - 1; l++)
                {
                    if ((cell = Z.GetCell(R.x1, l)) != null)
                        Region[Outer].Add(cell);
                    if ((cell = Z.GetCell(R.x2, l)) != null)
                        Region[Outer].Add(cell);
                }

                if ((cell = Z.GetCell(R.x1 + 1, R.y1 + 1)) != null)
                    Region[Outer].Add(cell);
                if ((cell = Z.GetCell(R.x2 - 1, R.y1 + 1)) != null)
                    Region[Outer].Add(cell);
                if ((cell = Z.GetCell(R.x1 + 1, R.y2 - 1)) != null)
                    Region[Outer].Add(cell);
                if ((cell = Z.GetCell(R.x2 - 1, R.y2 - 1)) != null)
                    Region[Outer].Add(cell);
            }
            else
            {
                for (int m = R.x1; m <= R.x2; m++)
                {
                    if ((cell = Z.GetCell(m, R.y1)) != null)
                        Region[Outer].Add(cell);
                    if ((cell = Z.GetCell(m, R.y2)) != null)
                        Region[Outer].Add(cell);
                }
                for (int n = R.y1; n <= R.y2; n++)
                {
                    if ((cell = Z.GetCell(R.x1, n)) != null)
                        Region[Outer].Add(cell);
                    if ((cell = Z.GetCell(R.x2, n)) != null)
                        Region[Outer].Add(cell);
                }
            }
            foreach (Cell outerCell in Region[Outer])
            {
                if (outerCell.Y == R.y1) Region[NorthEdge].Add(outerCell);
                if (outerCell.Y == R.y2) Region[SouthEdge].Add(outerCell);
                if (outerCell.X == R.x2) Region[EastEdge].Add(outerCell);
                if (outerCell.X == R.x1) Region[WestEdge].Add(outerCell);
                if ((outerCell.X == R.x1 || outerCell.X == R.x2) && (outerCell.Y == R.y1 || outerCell.Y == R.y2))
                    Region[Corners].Add(outerCell);
            }
            if (R.Door != null)
            {
                Cell door = Z.GetCell(R.Door);
                string doorSide = R.GetCellSide(R.Door) switch
                {
                    "N" => NorthEdge,
                    "S" => SouthEdge,
                    "E" => EastEdge,
                    "W" => WestEdge,
                    _ => null,
                };
                if (doorSide != null)
                {
                    Cell newDoor = Region[doorSide].GetRandomElement();
                    
                    Dictionary<string,List<Cell>> Edges = new()
                    {
                        { NorthEdge, Region[NorthEdge] },
                        { SouthEdge, Region[SouthEdge] },
                        { EastEdge, Region[EastEdge] },
                        { WestEdge, Region[WestEdge] },
                    };
                    Point2D newDoor2D = newDoor.PullInsideFromEdges(Edges, doorSide);
                    R.Door.x = newDoor2D.x;
                    R.Door.y = newDoor2D.y;
                }
                door = Z.GetCell(R.Door);
                Region[Door].Add(door);
            }
            return Region;
        }

        public static List<Cell> GetOrdinalAdjacentCells(this Cell @this, bool bLocalOnly = false, bool BuiltOnly = true, bool IncludeThis = false)
        {
            List<Cell> exclusionList = new(Cell.DirectionListCardinalOnly.Length);
            string[] directionListCardinalOnly = Cell.DirectionListCardinalOnly;
            foreach (string direction in directionListCardinalOnly)
            {
                Cell cell = (bLocalOnly ? @this.GetLocalCellFromDirection(direction) : @this.GetCellFromDirection(direction));
                if (cell != null)
                {
                    exclusionList.Add(cell);
                }
            }

            List<Cell> cellsList = (bLocalOnly ? @this.GetLocalAdjacentCells() : @this.GetAdjacentCells());
            foreach (Cell excludeCell in exclusionList)
            {
                if (cellsList.Contains(excludeCell)) cellsList.Remove(excludeCell);
            }

            if (IncludeThis)
            {
                cellsList.Add(@this);
            }

            return cellsList;
        }

        public static Point2D PullInsideFromEdges(this Cell Cell, Dictionary<string,List<Cell>> Edges, string DoorSide = "")
        {
            Debug.Entry(4,
                $"@ {typeof(Extensions).Name}."
                + $"{nameof(PullInsideFromEdges)}"
                + $"(Cell: {Cell}, List<Cell> Edge, " 
                + $"string DoorSide: {(DoorSide.IsNullOrEmpty() ? $"".Quote() : DoorSide.Quote())})",
                Indent: 0, Toggle: getDoDebug(nameof(PullInsideFromEdges)));

            Point2D output = new(Cell.X, Cell.Y);
            foreach ((string Side, List<Cell> Edge) in Edges)
            {
                Debug.Entry(4, $"Side: {Side}", Indent: 1, Toggle: getDoDebug(nameof(PullInsideFromEdges)));
                if (DoorSide == "" || Side == DoorSide)
                    output = Cell.PullInsideFromEdge(Edge);
            }
            Debug.Entry(4,
                $"x {typeof(Extensions).Name}."
                + $"{nameof(PullInsideFromEdges)}"
                + $"(Cell: [{Cell}], List<Cell> Edge, "
                + $"string DoorSide: {(DoorSide.IsNullOrEmpty() ? $"".Quote() : DoorSide.Quote())}) @//",
                Indent: 0, Toggle: getDoDebug(nameof(PullInsideFromEdges)));
            return output;
        }

        public static Point2D PullInsideFromEdge(this Cell Cell, List<Cell> Edge)
        {
            Debug.Entry(4,
                $"* {typeof(Extensions).Name}."
                + $"{nameof(PullInsideFromEdge)}"
                + $"(Cell: {Cell}, List<Cell> Edge)",
                Indent: 1, Toggle: getDoDebug(nameof(PullInsideFromEdge)));

            Point2D output = new(Cell.X, Cell.Y);
            if (Edge != null && Edge.Contains(Cell))
            {
                List<int> Xs = new();
                List<int> Ys = new();
                string XsString = string.Empty;
                string YsString = string.Empty;
                foreach (Cell cell in Edge)
                {
                    if (!Xs.Contains(cell.X)) Xs.Add(cell.X);
                    if (!Ys.Contains(cell.Y)) Ys.Add(cell.Y);
                }
                foreach (int X in Xs)
                {
                    XsString += XsString == "" ? $"{X}" : $",{X}";
                }
                foreach (int Y in Ys)
                {
                    YsString += YsString == "" ? $"{Y}" : $",{Y}";
                }
                Debug.Entry(4, $"Xs: {XsString}", Indent: 2, Toggle: getDoDebug(nameof(PullInsideFromEdge)));
                Debug.Entry(4, $"Ys: {YsString}", Indent: 2, Toggle: getDoDebug(nameof(PullInsideFromEdge)));
                if (Xs.Count > 1 &&  Ys.Count > 1)
                {
                    Debug.Entry(2, 
                        $"WARN [GigantismPlus]: " + 
                        $"{typeof(Extensions).Name}." + 
                        $"{nameof(PullInsideFromEdge)}() " + 
                        $"List<Cell> Edge must be a straight line.", 
                        Indent: 0, Toggle: getDoDebug(nameof(PullInsideFromEdge)));
                    return output;
                }
                bool edgeIsLat = Xs.Count > 1;
                Debug.Entry(4, $"edgeIsLat: {edgeIsLat}", Indent: 2, Toggle: getDoDebug(nameof(PullInsideFromEdge)));
                int max = int.MinValue;
                int min = int.MaxValue;
                if (edgeIsLat)
                {
                    foreach(int x in Xs)
                    {
                        max = Math.Max(max, x);
                        min = Math.Min(min, x);
                    }
                    if (output.x <= min)
                    {
                        Debug.Entry(4, $"output.x ({output.x}) >= min ({min}): output.x = min + 1", Indent: 2, Toggle: getDoDebug(nameof(PullInsideFromEdge)));
                        output.x = min + 1;
                    }
                    if (output.x >= max)
                    {
                        Debug.Entry(4, $"output.x ({output.x}) <= max ({max}): output.x = max - 1", Indent: 2, Toggle: getDoDebug(nameof(PullInsideFromEdge)));
                        output.x = max - 1;
                    }
                }
                else
                {
                    foreach (int y in Ys)
                    {
                        max = Math.Max(max, y);
                        min = Math.Min(min, y);
                    }
                    if (output.y <= min)
                    {
                        Debug.Entry(4, $"output.y ({output.y}) >= min ({min}): output.y = min + 1", Indent: 2, Toggle: getDoDebug(nameof(PullInsideFromEdge)));
                        output.y = min + 1;
                    }
                    if (output.y >= max)
                    {
                        Debug.Entry(4, $"output.y ({output.y}) <= max ({max}): output.y = max - 1", Indent: 2, Toggle: getDoDebug(nameof(PullInsideFromEdge)));
                        output.y = max - 1;
                    }
                }
            }
            Debug.Entry(4,
                $"x {typeof(Extensions).Name}."
                + $"{nameof(PullInsideFromEdge)}"
                + $"(Cell: {Cell}, List<Cell> Edge) *//",
                Indent: 1, Toggle: getDoDebug(nameof(PullInsideFromEdge)));

            return output;
        }

        public static List<string> GetNumberedTileVariants(this string Source)
        {
            bool doDebug = getDoDebug(nameof(GetNumberedTileVariants));
            Debug.Entry(4,
                $"* {typeof(Extensions).Name}."
                + $"{nameof(GetNumberedTileVariants)}"
                + $"(string Source: {Source})",
                Indent: 0, Toggle: doDebug);
            List<string> output = new();
            
            string[] sourcePieces = Source.Split("~");
            string pathBefore = sourcePieces[0];
            string pathAfter = sourcePieces[2];

            Debug.Entry(4, $"pathBefore: {pathBefore}, pathAfter: {pathAfter}", Indent: 1, Toggle: doDebug);
            Debug.Entry(4, $"sourcePieces[1] {sourcePieces[1]}", Indent: 1, Toggle: doDebug);
            
            string[] pathRange = sourcePieces[1].Split('-');
            int first = int.Parse(pathRange[0]);
            int last = int.Parse(pathRange[1]);

            Debug.Entry(4, $"first {first} - last: {last}", Indent: 1, Toggle: doDebug);

            for (int i = first; i <= last; i++)
            {
                Debug.Entry(4, $"i: {i}", Indent: 2, Toggle: doDebug);

                int padding = Math.Max(2, last.ToString().Length);
                string number = $"{i}".PadLeft(padding, '0');
                string path = $"{pathBefore}{number}{pathAfter}";
                Debug.Entry(4, $"path: {path}", Indent: 2, Toggle: doDebug);
                if (!output.Contains(path)) output.Add(path);
            }

            return output;
        }

        public static List<string> CommaExpansion(this string String)
        {
            string[] stringPieces = String.Split(",");
            List<string> output = new();
            for (int i = 0; i < stringPieces.Count(); i++)
            {
                if (!output.Contains(stringPieces[i])) output.Add(stringPieces[i]);
            }
            return output;
        }

        public static bool SeededRandomBool(this Guid Seed, int ChanceIn = 2)
        {
            int High = ChanceIn * 7;
            return Stat.SeededRandom(Seed.ToString(), 0, High) % ChanceIn == 0;
        }

        public static string Join<T>(this List<T> List, string Delimiter = ",")
            where T : IConvertible
        {
            string output = "";
            if (!List.IsNullOrEmpty())
            {
                foreach (T item in List)
                {
                    if (!output.IsNullOrEmpty())
                    {
                        output += Delimiter;
                    }
                    output += item;
                }
            }
            return output;
        }

        public static bool TrySplit(this string String, string Delimiter, out string[] split)
        {
            if (!String.IsNullOrEmpty() && String.Contains(Delimiter))
            {
                split = String.Split(Delimiter);
                return true;
            }
            split = null;
            return false;
        }

        public static bool TrySplitToList(this string String, string Delimiter, out List<string> List)
        {
            List<string> strings = new();
            if (!String.IsNullOrEmpty() && String.TrySplit(Delimiter, out string[] split))
            {
                foreach (string item in split)
                {
                    if (!strings.Contains(item)) strings.Add(item);
                }
                List = strings;
                return true;
            }
            List = new();
            return false;
        }

        public static bool ContainsCaseInsensitive(this string String, string Value)
        {
            return String.ToLower().Contains(Value.ToLower());
        }

        public static bool IsGivingStewful(this string NamePiece)
        {
            return NamePiece.ContainsCaseInsensitive("Stew")
                || NamePiece.ContainsCaseInsensitive("Cook");
        }
        public static bool IsGivingStewless(this string NamePiece)
        {
            return NamePiece.ContainsCaseInsensitive("Hanker")
                || NamePiece.ContainsCaseInsensitive("Gains Seeker");
        }
        public static bool IsGivingThoughtful(this string NamePiece)
        {
            return NamePiece.ContainsCaseInsensitive("Thought")
                || NamePiece.ContainsCaseInsensitive("Think")
                || NamePiece.ContainsCaseInsensitive("Book")
                || NamePiece.ContainsCaseInsensitive("Listen")
                || NamePiece.ContainsCaseInsensitive("Philosoph");
        }
        public static bool IsGivingTough(this string NamePiece)
        {
            return NamePiece.ContainsCaseInsensitive("Stone")
                || NamePiece.ContainsCaseInsensitive("Pillar")
                || NamePiece.ContainsCaseInsensitive("Solid")
                || NamePiece.ContainsCaseInsensitive("Sturdy")
                || NamePiece.ContainsCaseInsensitive("Mov") // Immovable, Unmoving
                || NamePiece.ContainsCaseInsensitive("Stop")
                || NamePiece.ContainsCaseInsensitive("Mountain")
                || NamePiece.ContainsCaseInsensitive("Thic") // Thick, Thicc
                || NamePiece.ContainsCaseInsensitive("Hefty");
        }
        public static bool IsGivingStrong(this string NamePiece)
        {
            return NamePiece.ContainsCaseInsensitive("Gain")
                || NamePiece.ContainsCaseInsensitive("Swole")
                || NamePiece.ContainsCaseInsensitive("Mighty")
                || NamePiece.ContainsCaseInsensitive("Mountain")
                || NamePiece.ContainsCaseInsensitive("Slap");
        }
        public static bool IsGivingResilient(this string NamePiece)
        {
            return NamePiece.ContainsCaseInsensitive("Waits")
                || NamePiece.ContainsCaseInsensitive("Sits")
                || NamePiece.ContainsCaseInsensitive("Silent")
                || NamePiece.ContainsCaseInsensitive("Still")
                || NamePiece.ContainsCaseInsensitive("Tall")
                || NamePiece.ContainsCaseInsensitive("Mountain")
                || NamePiece.ContainsCaseInsensitive("Keeps Going");
        }
        public static bool IsGivingPopular(this string NamePiece)
        {
            return NamePiece.ContainsCaseInsensitive("Altruis");
        }
        public static bool IsGivingWrassler(this string NamePiece)
        {
            return NamePiece.ContainsCaseInsensitive("Folding Chair");
        }
        public static bool IsGivingTrulyImmense(this string NamePiece)
        {
            return NamePiece.ContainsCaseInsensitive("Belly")
                || NamePiece.ContainsCaseInsensitive("Heft")
                || NamePiece.ContainsCaseInsensitive("Considerable Size")
                || NamePiece.ContainsCaseInsensitive("Bellows")
                || NamePiece.ContainsCaseInsensitive("Rumbles")
                || NamePiece.ContainsCaseInsensitive("Rotund")
                || NamePiece.ContainsCaseInsensitive("Giant")
                || NamePiece.ContainsCaseInsensitive("Immense")
                || NamePiece.ContainsCaseInsensitive("Enormous")
                || NamePiece.ContainsCaseInsensitive("Huge")
                || NamePiece.ContainsCaseInsensitive("Yuge")
                || NamePiece.ContainsCaseInsensitive("Somewhat Big")
                || NamePiece.ContainsCaseInsensitive("Really Big");
        }

        public static List<BaseSkill> AddSkills(this GameObject Creature, List<string> Skills)
        {
            List<BaseSkill> output = new();
            foreach (string skill in Skills)
            {
                if (Creature.HasSkill(skill))
                {
                    continue;
                }
                output.TryAdd(Creature.AddSkill(skill));
            }
            return output;
        }

        public static bool TryAdd<T>(this List<T> List, T Item)
        {
            if (!List.Contains(Item))
            {
                List.Add(Item);
                return true;
            }
            return false;
        }

        public static bool TryGetBook(this string BookName, out BookInfo Book)
        {
            return BookUI.Books.TryGetValue(BookName, out Book);
        }
        public static bool TryGetListBookPages(this string BookName, out List<string> Pages)
        {
            List<string> pages = new();
            if (BookName.TryGetBook(out BookInfo Book) && !Book.Pages.IsNullOrEmpty())
            {
                foreach (BookPage page in Book.Pages)
                {
                    pages.TryAdd(page.FullText.Trim());
                }
            }
            Pages = pages;
            return !Pages.IsNullOrEmpty();
        }
        public static List<string> BookPagesAsList(this string BookName)
        {
            BookName.TryGetListBookPages(out List<string> pages);
            return pages;
        }

        public static List<Location2D> ToLocation2DList(this List<Cell> CellList)
        {
            if (CellList.IsNullOrEmpty()) return null;
            List<Location2D> Location2DList = new();
            foreach (Cell cell in CellList)
            {
                Location2DList.TryAdd(cell.Location);
            }
            return Location2DList;
        }
        public static List<string> ToStringList(this List<Location2D> Location2DList)
        {
            if (Location2DList.IsNullOrEmpty()) return null;
            List<string> stringList = new();
            foreach (Location2D location in Location2DList)
            {
                stringList.TryAdd(location.ToString());
            }
            return stringList;
        }
        public static List<string> ToStringList(this List<Cell> CellList)
        {
            return CellList.ToLocation2DList().ToStringList();
        }

        public static List<Cell> ToCellList(this List<string> StringList, Zone Z)
        {
            List<Cell> cellList = new();
            foreach (string @string in StringList)
            {
                string[] coord = @string.Split(",");
                cellList.TryAdd(Z.GetCell(int.Parse(coord[0]), int.Parse(coord[1])));
            }
            return cellList;
        }
        public static List<string> ToStringCoordList(this string String)
        {
            List<string> stringList = new();
            if (!String.Contains(";"))
            {
                stringList.TryAdd(String);
            }
            else
            {
                string[] coords = String.Split(";");
                foreach (string coord in coords)
                {
                    stringList.TryAdd(coord);
                }
            }
            return stringList;
        }

        public static bool OverlapsWith<T>(this List<T> List1, List<T> List2)
        {
            bool overlaps = false;
            if (!List1.IsNullOrEmpty() && !List2.IsNullOrEmpty())
            {
                foreach (T item in List1)
                {
                    if (List2.Contains(item))
                    {
                        overlaps = true;
                        break;
                    }
                }
            }
            return overlaps;
        }

        public static Cell GetCellOppositePivotCell(this Cell Origin, Cell Pivot)
        {
            if (Origin == null || Origin == Pivot || !Origin.GetAdjacentCells().Contains(Pivot))
            {
                return null;
            }
            return Pivot.GetCellFromDirection(Origin.GetDirectionFromCell(Pivot));
        }

        public static string YehNah(this bool Condition, bool Flip = false)
        {
            // Input |  Flip
            // true  != false   == true
            // true  != true    == false
            // false != false   == false
            // false != true    == true
            return Condition != Flip ? TICK.Color("G") : CROSS.Color("R");
        }

        public static string ThisManyTimes(this string @string, int Times = 1)
        {
            if (Times < 1)
            {
                return null;
            }
            string output = @string;

            for (int i = 0; i < Times; i++)
            {
                output += @string;
            }

            return output;
        }
        public static string ThisManyTimes(this char @char, int Times = 1)
        {
            return @char.ToString().ThisManyTimes(Times);
        }

        public static string PickDirectionS(this GameObject who, string Label = null, bool NullIfSame = false)
        {
            return who.CurrentCell.GetDirectionFromCell(who.Physics.PickDirection(Label, who), NullIfSame);
        }

        public static bool IsInOrthogonalDirectionWith(this Cell OriginCell, Cell TargetCell)
        {
            return OriginCell != null && TargetCell != null 
                && (OriginCell.X == TargetCell.X 
                    || OriginCell.Y == TargetCell.Y 
                    || Math.Abs(OriginCell.X - TargetCell.X) == Math.Abs(OriginCell.Y - TargetCell.Y));
        }
        public static bool IsInOrthogonalDirectionWith(this GameObject Origin, GameObject Target)
        {
            Cell OriginCell = Origin.CurrentCell;
            Cell TargetCell = Target.CurrentCell;
            return IsInOrthogonalDirectionWith(OriginCell, TargetCell);
        }
        public static bool IsInOrthogonalDirectionWith(this Cell OriginCell, GameObject Target)
        {
            Cell TargetCell = Target.CurrentCell;
            return IsInOrthogonalDirectionWith(OriginCell, TargetCell);
        }
        public static bool IsInOrthogonalDirectionWith(this GameObject Origin, Cell TargetCell)
        {
            Cell OriginCell = Origin.CurrentCell;
            return IsInOrthogonalDirectionWith(OriginCell, TargetCell);
        }

        public static int CosmeticDistanceTo(this Cell Origin, Cell Target)
        {
            if (Origin.ParentZone != Target.ParentZone)
                return 0;
            return Origin.CosmeticDistanceTo(Target.X, Target.Y);
        }
        public static int CosmeticDistanceTo(this GameObject Origin, GameObject Target)
        {
            if (!GameObject.Validate(Origin) || !GameObject.Validate(Target))
                return 0;

            Cell OriginCell = Origin.CurrentCell;
            Cell TargetCell = Target.CurrentCell;

            if (OriginCell.ParentZone != TargetCell.ParentZone)
                return 0;

            return OriginCell.CosmeticDistanceTo(TargetCell);
        }

        public static List<MutationEntry> GetStartingMutationEntries(this GameObject Mutant)
        {
            List<MutationEntry> list = new();
            if (Mutant != null)
            {
                if (!Mutant.GetBlueprint().Mutations.IsNullOrEmpty())
                {
                    foreach ((string mutation, GamePartBlueprint mutationBlueprint) in Mutant.GetBlueprint().Mutations)
                    {
                        BaseMutation baseMutation = Mutations.GetGenericMutation(mutation);
                        if (baseMutation != null)
                        {
                            list.TryAdd(baseMutation.GetMutationEntry());
                        }
                        
                        IPart part = mutationBlueprint.Reflector?.GetInstance() 
                            ?? (Activator.CreateInstance(mutationBlueprint.T) as IPart);
                        if (part is BaseMutation mutationPart)
                        {
                            list.TryAdd(mutationPart.GetMutationEntry());
                        }
                    }
                }
                if (Mutant.IsOriginalPlayerBody() && !EmbarkBuilderConfiguration.activeModules.IsNullOrEmpty())
                {
                    foreach (AbstractEmbarkBuilderModule activeModule in EmbarkBuilderConfiguration.activeModules)
                    {
                        if (activeModule.type == "QudMutationsModule")
                        {
                            QudMutationsModule mutationModule = activeModule as QudMutationsModule;
                            if (mutationModule?.data?.selections != null && !mutationModule.data.selections.IsNullOrEmpty())
                            {
                                foreach (QudMutationModuleDataRow selection in mutationModule.data.selections)
                                {
                                    list.TryAdd(selection.Entry);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return list;
        }
        public static bool HasStartingMutations(this GameObject Mutant)
        {
            return !Mutant.GetStartingMutationEntries().IsNullOrEmpty();
        }
        public static List<BaseMutation> GetStartingBaseMutations(this GameObject Mutant)
        {
            List<BaseMutation> list = new();
            if (Mutant != null && Mutant.HasStartingMutations())
            {
                foreach (MutationEntry entry in Mutant.GetStartingMutationEntries())
                {
                    list.TryAdd(entry.Mutation);
                }
            }
            return list;
        }
        public static List<string> GetStartingMutations(this GameObject Mutant)
        {
            List<string> list = new();
            if (Mutant != null && Mutant.HasStartingMutations())
            {
                foreach (MutationEntry entry in Mutant.GetStartingMutationEntries())
                {
                    list.TryAdd(entry.Name);
                }
            }
            return list;
        }
        public static List<string> GetStartingMutationClasses(this GameObject Mutant)
        {
            List<string> list = new();
            if (Mutant != null && Mutant.HasStartingMutations())
            {
                foreach (MutationEntry entry in Mutant.GetStartingMutationEntries())
                {
                    list.TryAdd(entry.Class);
                }
            }
            return list;
        }

        public static string GetStartingPregen(this GameObject Player)
        {
            string pregen = null;
            if (Player != null)
            {
                if (Player.IsOriginalPlayerBody() && !EmbarkBuilderConfiguration.activeModules.IsNullOrEmpty())
                {
                    foreach (AbstractEmbarkBuilderModule activeModule in EmbarkBuilderConfiguration.activeModules)
                    {
                        if (activeModule.type == "QudPregenModule")
                        {
                            QudPregenModule pregenModule = activeModule as QudPregenModule;
                            string pregenData = pregenModule?.data?.Pregen;
                            if (pregenModule != null && pregenData != null && pregenModule.pregens.ContainsKey(pregenData))
                            {
                                pregen = pregenModule.pregens[pregenData].Name;
                            }
                            break;
                        }
                    }
                }
            }
            return pregen;
        }

        public static GameObject Think(this GameObject Thinker, string Hrm)
        {
            if (GameObject.Validate(Thinker) && Thinker.Brain != null && !Thinker.IsPlayerControlled())
            {
                Thinker.Brain.Think(Hrm);
            }
            return Thinker;
        }

        public static bool TryGetStat(this GameObject Actor, string Stat, out Statistic Statistic)
        {
            Statistic = null;
            if (Actor != null && Actor.HasStat(Stat))
            {
                Statistic = Actor.GetStat(Stat);
                return true;
            }
            return false;
        }

        public static double GetMovementsPerTurn(this GameObject Mover, bool IgnoreSprint = false)
        {
            if (Mover != null && Mover.TryGetStat("MoveSpeed", out Statistic MS) && Mover.TryGetStat("Speed", out Statistic QN))
            {
                int indent = Debug.LastIndent;
                Debug.Entry(4, $"* {nameof(GameObject)}.{nameof(GetMovementsPerTurn)}(IgnoreSprint: {IgnoreSprint})", 
                    Indent: indent, Toggle: getDoDebug(nameof(GetMovementsPerTurn)));

                int EMS = 100 - MS.Value + 100;
                Debug.Entry(4, $"{nameof(EMS)}", $"{EMS}",
                    Indent: indent + 1, Toggle: getDoDebug(nameof(GetMovementsPerTurn)));
                if (IgnoreSprint && Mover.TryGetEffect(out Running running))
                {
                    EMS -= running.MovespeedBonus;
                    Debug.Entry(4, $"{nameof(running.MovespeedBonus)}", $"{running.MovespeedBonus}",
                        Indent: indent + 2, Toggle: getDoDebug(nameof(GetMovementsPerTurn)));
                    Debug.Entry(4, $"{nameof(EMS)}", $"{EMS}",
                        Indent: indent + 2, Toggle: getDoDebug(nameof(GetMovementsPerTurn)));
                }
                int EQN = QN.Value;
                Debug.Entry(4, $"{nameof(EQN)}", $"{EQN}",
                    Indent: indent + 1, Toggle: getDoDebug(nameof(GetMovementsPerTurn)));

                Debug.Entry(4, $"x {nameof(GameObject)}.{nameof(GetMovementsPerTurn)}(IgnoreSprint: {IgnoreSprint}) *//",
                    Indent: indent, Toggle: getDoDebug(nameof(GetMovementsPerTurn)));
                return (EQN * EMS) / 10000.0;
            }
            return default;
        }

        public static string Pens(this string String)
        {
            return "\u001a".Color("c") + String;
        }
        public static string Damage(this string String)
        {
            return "\u0003".Color("r") + String;
        }

        public static bool IsHostileTowardsInRadius(this GameObject GO, GameObject Actor, int Radius)
        {
            return GO != null && Actor != null
                && GO.IsHostileTowards(Actor)
                && GO.CurrentCell.CosmeticDistanceto(Actor.CurrentCell.Location) < Radius + 1;
        }

        public static int GetStatisticPercent(this Statistic Statistic)
        {
            return Statistic == null ? -1 : (int)Math.Round(Statistic.Value / Statistic.BaseValue * 100.0);
        }
        public static bool TryGetStatisticPercent(this Statistic Statistic, out int Percent)
        {
            Percent = GetStatisticPercent(Statistic);
            return !(Percent < 0);
        }

        public static int GetHitpointPercent(this GameObject GO)
        {
            return GetStatisticPercent(GO?.GetStat("Hitpoints"));
        }
        public static bool TryGetHitpointPercent(this GameObject GO, out int Percent)
        {
            return TryGetStatisticPercent(GO?.GetStat("Hitpoints"), out Percent);
        }

        public static List<GameObjectBlueprint> SafelyGetBlueprintsInheritingFrom(this GameObjectFactory Factory, string Name, bool ExcludeBase = true)
        {
            List<GameObjectBlueprint> outputList = new();
            foreach (GameObjectBlueprint blueprint in Factory.BlueprintList)
            {
                if (blueprint.InheritsFromSafe(Name) && (!ExcludeBase || !blueprint.IsBaseBlueprint()))
                {
                    outputList.Add(blueprint);
                }
            }
            return outputList;
        }
        public static List<string> InheritanceRoots => new()
        {
            nameof(Object),
            "SultanMuralController",
        };
        public static bool InheritsFromSafe(this GameObjectBlueprint GameObjectBlueprint, string what)
        {
            string parentBlueprint = GameObjectBlueprint.Inherits;
            while (!parentBlueprint.IsNullOrEmpty())
            {
                if (parentBlueprint == what)
                {
                    return true;
                }
                string inherits = parentBlueprint;
                parentBlueprint = GameObjectFactory.Factory?.GetBlueprintIfExists(parentBlueprint)?.Inherits;
                if (parentBlueprint.IsNullOrEmpty() 
                    && !InheritanceRoots.Contains(inherits))
                {
                    MetricsManager.LogModWarning(ThisMod,
                        $"{nameof(Extensions)}.{nameof(InheritsFromSafe)}({what.Quote()}):" +
                        $" bluprint ancestor {inherits.Quote()} does not exist in blueprint list." +
                        $" The first mention of this blueprint in this log should reveal the mod with this inheritance issue.");
                }
            }
            return false;
        }

        public static long SubtractModulo(this long operand1, long operand2)
        {
            return operand1 - (operand1 % operand2);
        }
        public static int SubtractModulo(this int operand1, int operand2)
        {
            return operand1 - (operand1 % operand2);
        }

        public static double AsGameHours(this long TimeTick)
        {
            return TimeTick / Calendar.TurnsPerHour;
        }
        public static double AsGameDays(this long TimeTick)
        {
            return TimeTick / Calendar.TurnsPerDay;
        }
        public static double AsGameYears(this long TimeTick)
        {
            return TimeTick / Calendar.TurnsPerYear;
        }

        public static long TurnTicksUntil(this long TimeTickEnd)
        {
            return Math.Max(0, TurnTicksBetween(The.CurrentTurn, TimeTickEnd));
        }

        public static long GameHoursAsTurnTicks(this double Hours)
        {
            return (long)(Hours * Calendar.TurnsPerHour);
        }
        public static long GameDaysAsTurnTicks(this double Days)
        {
            return (long)(Days * Calendar.TurnsPerDay);
        }
        public static long GameYearsAsTurnTicks(this double Years)
        {
            return (long)(Years * Calendar.TurnsPerYear);
        }

        public static double GameHoursSince(this long TimeTick)
        {
            return GameHoursBetween(TimeTick, The.CurrentTurn);
        }
        public static double GameDaysSince(this long TimeTick)
        {
            return GameDaysBetween(TimeTick, The.CurrentTurn);
        }
        public static double GameYearsSince(this long TimeTick)
        {
            return GameYearsBetween(TimeTick, The.CurrentTurn);
        }

        public static int GameHoursAgo(this long TimeTick)
        {
            return (int)GameHoursSince(TimeTick);
        }
        public static int GameDaysAgo(this long TimeTick)
        {
            return (int)GameDaysSince(TimeTick);
        }
        public static int GameYearsAgo(this long TimeTick)
        {
            return (int)GameYearsSince(TimeTick);
        }

        public static string TimeAgo(this long TimeTick)
        {
            int yearsAgo = (int)Math.Floor(GameYearsSince(TimeTick));
            int monthsAgo = (int)Math.Floor(GameYearsSince(TimeTick) / 13);
            int daysAgo = GameDaysAgo(TimeTick);
            int hoursAgo = GameHoursAgo(TimeTick);
            string ago = " ago";
            string unit = "some time";
            if (yearsAgo > 0)
            {
                unit = yearsAgo.Things("year");
            }
            else
            if (monthsAgo > 0)
            {
                unit = monthsAgo.Things("month");
            }
            else
            if (daysAgo > 0)
            {
                unit = daysAgo.Things("day");
            }
            else
            if (hoursAgo > 0)
            {
                unit = hoursAgo.Things("hour");
            }
            else
            {
                return "just now";
            }
            return unit + ago;
        }

        public static string ExtendedToString<T>(this T Thing)
        {
            foreach (MethodInfo methodInfo in typeof(Extensions).GetMethods())
            {
                if (methodInfo.GetParameters().Length == 1
                    && Thing.GetType().InheritsFrom(methodInfo.GetParameters()[0].ParameterType)
                    && methodInfo.ReturnType == typeof(string)
                    && methodInfo.Name == nameof(ToString)
                    && methodInfo.Invoke(Thing, new object[] { Thing }) is string thingString)
                {
                    return thingString;
                }
            }
            foreach (MethodInfo methodInfo in ModManager.GetMethodsWithAttribute(typeof(ExtensionAttribute)))
            {
                if (Thing.GetType().InheritsFrom(methodInfo.GetParameters()[0].ParameterType)
                    && methodInfo.ReturnType == typeof(string)
                    && methodInfo.Name == nameof(ToString)
                    && methodInfo.Invoke(Thing, new object[] { Thing }) is string thingString)
                {
                    return thingString;
                }
            }
            return Thing.ToString();
        }

        public static string ToString(this IBaseJournalEntry JournalEntry)
        {
            string output = JournalEntry.GetShortText().Strip();
            if (!output.IsNullOrEmpty() && output.Length > 30)
            {
                output = output[..29];
            }
            return output + "...";
        }

        public static string ToStringWithGenerics(this Type Type)
        {
            if (Type == null)
            {
                return null;
            }
            List<Type> typeGenerics = new(Type.GetGenericArguments());
            string typeString = Type.Name;
            string genericsString = null;
            if (!typeGenerics.IsNullOrEmpty())
            {
                List<string> genericsStringList = new();
                foreach (Type genericType in typeGenerics)
                {
                    genericsStringList.Add(genericType.ToStringWithGenerics());
                }
                genericsString = genericsStringList.Join(", ");
            }
            if (!genericsString.IsNullOrEmpty())
            {
                typeString = typeString.Split('`')[0];
                genericsString = "<" + genericsString + ">";
            }
            return typeString + genericsString;
        }
    }
}
