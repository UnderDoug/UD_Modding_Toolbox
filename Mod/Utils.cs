using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

using ConsoleLib.Console;

using HarmonyLib;
using HistoryKit;
using Kobold;

using XRL;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Text.Attributes;
using XRL.World.Text.Delegates;

using UD_Modding_Toolbox.Logging;

using Debug = UD_Modding_Toolbox.Logging.Debug;

using static UD_Modding_Toolbox.Const;
using static UD_Modding_Toolbox.Options;
using XRL.Collections;

namespace UD_Modding_Toolbox
{
    [HasGameBasedStaticCache]
    [HasVariableReplacer]
    public static class Utils
    {
        #region Debug Registration
        [UD_DebugRegistry]
        public static void Extension_DebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEach(
                Type: typeof(UD_Modding_Toolbox.Utils),
                MethodNameValues: new Dictionary<string, bool>
                {
                    { nameof(GetTilePath), false},
                    { nameof(TryGetTilePath), false},
                    { nameof(Rumble), false},
                });
        #endregion

        public static ModInfo ThisMod => ModManager.GetMod(MOD_ID);

        public static string ModName => ThisMod?.Manifest?.Title;
        public static string ModNameStripped => ThisMod?.Manifest?.Title.Strip();

        public static string ModAuthor => ThisMod?.Manifest?.Author;
        public static string ModAuthorStripped => ModAuthor?.Strip();

        public static string TellModAuthor => ModAuthor.IsNullOrEmpty() ? null : "Please let " + ModAuthor + " know on the steam workshop discussion for this mod.";
        public static string TellModAuthorStripped => ModAuthorStripped.IsNullOrEmpty() ? null : "Please let " + ModAuthorStripped + " know on the steam workshop discussion for this mod.";

        public static ModInfo HNPS_GigantismPlus => ModManager.GetMod(HNPS_GIGANTISMPLUS_MOD_ID);

        private static Random _rnd;
        public static Random Rnd
        {
            get
            {
                if (_rnd == null)
                {
                    if (The.Game == null)
                    {
                        MetricsManager.LogModWarning(ThisMod, 
                            "Attempted to retrieve " + nameof(Random) + ", but Game is not created yet." +
                            " Fall-back " + nameof(Random) + " provided.");
                        return Stat.GetSeededRandomGenerator(ModNameStripped);
                    }

                    if (The.Game.IntGameState.ContainsKey(ModNameStripped + ":Random"))
                    {
                        _rnd = new Random(The.Game.GetIntGameState(ModNameStripped + ":Random"));
                    }
                    else
                    {
                        _rnd = Stat.GetSeededRandomGenerator(ModNameStripped);
                    }
                    The.Game.SetIntGameState(ModNameStripped + ":Random", _rnd.Next());
                }
                return _rnd;
            }
        }

        [GameBasedCacheInit]
        public static void ResetRandom()
        {
            _rnd = null;
        }

        public static int RndNext(int minInclusive, int maxInclusive)
        {
            return Rnd.Next(minInclusive, maxInclusive + 1);
        }
        public static Span<T> Shuffle<T>(Span<T> Values)
        {
            if (Values == null)
            {
                throw new ArgumentNullException(nameof(Values));
            }
            int length = Values.Length;
            for (int i = 0; i < length - 1; i++)
            {
                int num = RndNext(i, length);
                if (num != i)
                {
                    (Values[num], Values[i]) = (Values[i], Values[num]);
                }
            }
            return Values;
        }
        public static T[] Shuffle<T>(T[] Values)
        {
            if (Values == null)
            {
                throw new ArgumentNullException(nameof(Values));
            }
            return Shuffle(Values.AsSpan()).ToArray();
        }
        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> Values)
        {
            if (Values == null)
            {
                throw new ArgumentNullException(nameof(Values));
            }
            foreach(T value in Shuffle(Values.ToArray()))
            {
                yield return value;
            }
        }

        public enum TextCase : int
        {
            Default = 0,
            Lower = 1,
            Capital = 2,
            Title = 3,
            Upper = 4,
        }

        public static string ConvertToCase(string Text, TextCase Case = TextCase.Default)
        {
            return Case switch
            {
                TextCase.Lower => Text.ToLower(),
                TextCase.Capital => Text.Capitalize(),
                TextCase.Title => Grammar.MakeTitleCase(Text),
                TextCase.Upper => Text.ToUpper(),
                _ => Text,
            };
        }
        public static void Error(object Message)
            => ThisMod.Error(Message);

        public static void Warn(object Message)
            => ThisMod.Warn(Message);


        public static string CallChain(params string[] Strings)
            => Strings
                ?.Aggregate(
                    seed: "",
                    func: (a, n) => a + (!a.IsNullOrEmpty() && !n.IsNullOrEmpty() ? "." : null) + n);

        public static bool HasWidget(Cell Cell)
        {
            return Cell.HasObject(GO => GO.IsWidget() && !GO.HasPart<UD_CellHighlighter>());
        }

        public static bool MigratePartFieldFromBlueprint<TPart, TField>(
            TPart Part,
            ref TField Field,
            string FieldName,
            GameObjectBlueprint Blueprint)
            where TPart : IPart
        {
            string partName = Part.GetType().Name;
            if (Blueprint.TryGetPartParameter(partName, FieldName, out TField result))
            {
                Field = result;
                return true;
            }
            if (Activator.CreateInstance(Part.GetType()) is TPart newPart
                && new Traverse(newPart) is Traverse traverse
                && traverse.Field<TField>(FieldName) is TField constructorValue)
            {
                Field = constructorValue;
                return true;
            }
            return false;
        }

        public static bool CoinToss()
        {
            return Stat.RandomCosmetic(0, 99) % 2 == 0;
        }

        public static bool RegisterGameLevelEventHandlers()
        {
            using var indent = new Indent(1);
            Debug.LogCaller(indent);

            bool gameAssigned = The.Game != null;
            if (gameAssigned)
            {
                // ExampleHandler.Register();
            }
            else
            {
                Debug.Log($"Unable to register any events", "The.Game is null", Indent: indent[1]);
            }
            Debug.YehNah($"Event Handler Registration Finished", Good: gameAssigned, Indent: indent[0]);
            return gameAssigned;
        }

        [ModSensitiveStaticCache(CreateEmptyInstance = true)]
        private static StringMap<string> TilePathCache = new();
        private static readonly List<string> TileSubfolders = new()
        {
            "",
            "Abilities",
            "Assets",
            "Blueprints",
            "Creatures",
            "Items",
            "Mutations",
            "NaturalWeapons",
            "Terrain",
            "Tiles",
            "Tiles2",
            "Walls",
            "Walls2",
            "Widgets",
        };
        public static string BuildCustomTilePath(string DisplayName)
            => Grammar.MakeTitleCase(ColorUtility.StripFormatting(DisplayName)).Replace(" ", "")
            ;

        public static string GetTilePath(string TileName, bool IsWholePath = false)
        {
            using var indent = new Indent(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(TileName),
                    Debug.Arg(nameof(IsWholePath), IsWholePath),
                });

            TilePathCache ??= new();
            if (!TilePathCache.ContainsKey(TileName))
            {
                TilePathCache[TileName] = "";
                if (IsWholePath)
                {
                    Debug.Log("No cached value, searching for whole path...", Indent: indent[1]);
                    if (SpriteManager.HasTextureInfo(TileName))
                    {
                        TilePathCache[TileName] = TileName;
                        Debug.CheckYeh($"\"{TileName}\" is a valid tile path!", Indent: indent[2]);
                    }
                    else
                        Debug.CheckNah($"\"{TileName}\" is not a valid tile path.", Indent: indent[2]);
                }
                else
                {
                    Debug.Log("No cached value, searching the following subfolders:", Indent: indent[1]);
                    Debug.Loggregrate(
                        Source: TileSubfolders,
                        Proc: n => n,
                        Empty: "none",
                        PostProc: s => $"::\"{s}\"",
                        Indent: indent[2]);

                    foreach (string subfolder in TileSubfolders)
                    {
                        string path = subfolder;

                        if (path != "")
                            path += "/";

                        path += TileName;

                        if (SpriteManager.HasTextureInfo(path))
                        {
                            TilePathCache[TileName] = path;
                            Debug.CheckYeh($"\"{path}\" is a valid tile path!", Indent: indent[2]);
                        }
                        else
                            Debug.CheckNah($"\"{path}\" is not a valid tile path.", Indent: indent[2]);
                    }
                }
            }
            return TilePathCache[TileName];
        }

        public static bool TryGetTilePath(string TileName, out string TilePath, bool IsWholePath = false)
            => !(TilePath = GetTilePath(TileName, IsWholePath)).IsNullOrEmpty();

        public static string WeaponDamageString(int DieSize, int DieCount, int Bonus)
        {
            string output = $"{DieSize}d{DieCount}";

            if (Bonus > 0)
            {
                output += $"+{Bonus}";
            }
            else if (Bonus < 0)
            {
                output += Bonus;
            }

            return output;
        }

        public static GameObjectBlueprint GetGameObjectBlueprint(string Blueprint)
        {
            GameObjectFactory.Factory.Blueprints.TryGetValue(Blueprint, out GameObjectBlueprint GameObjectBlueprint);
            return GameObjectBlueprint;
        }
        public static bool TryGetGameObjectBlueprint(string Blueprint, out GameObjectBlueprint GameObjectBlueprint)
        {
            GameObjectBlueprint = GetGameObjectBlueprint(Blueprint);
            return GameObjectBlueprint is not null;
        }
        public static string MakeAndList(IReadOnlyList<string> Words, bool Serial = true, bool IgnoreCommas = false)
        {
            List<string> replacedList = new();
            foreach (string entry in Words)
            {
                replacedList.Add(entry.Replace(",", ";;"));
            }
            string andList = Grammar.MakeAndList(replacedList, Serial);
            return andList.Replace(";;", ",");
        }
        public static string Quote(string @string)
            => $"\"{@string ?? "null"}\""
            ;

        public static BookInfo GetBook(string BookName)
        {
            BookUI.Books.TryGetValue(BookName, out BookInfo Book);
            return Book;
        }

        public static float Rumble(float Cause, float DurationFactor = 1.0f, float DurationMax = 1.0f, bool Async = false)
        {
            float duration = Math.Min(DurationMax, Cause * DurationFactor);
            CombatJuice.cameraShake(duration, Async: Async);

            using var indent = new Indent(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(duration), duration),
                    Debug.Arg(nameof(Cause), Cause),
                    Debug.Arg(nameof(DurationFactor), DurationFactor),
                    Debug.Arg(nameof(DurationMax), DurationMax),
                });

            return duration;
        }
        public static float Rumble(double Cause, float DurationFactor = 1.0f, float DurationMax = 1.0f, bool Async = true)
        {
            return Rumble((float)Cause, DurationFactor, DurationMax, Async);
        }
        public static float Rumble(int Cause, float DurationFactor = 1.0f, float DurationMax = 1.0f, bool Async = true)
        {
            return Rumble((float)Cause, DurationFactor, DurationMax, Async);
        }

        public static bool IsMaxDistance(int Distance)
        {
            return Distance >= MAX_DIST;
        }

        public static string GetBoolString(List<string> list, bool @bool)
        {
            string trueString = null;
            string falseString = null;
            string output = "";

            foreach(string @string in list)
            {
                if (@string.StartsWith("true;;"))
                {
                    trueString = @string.Replace("true;;", "");
                }
                else if (@string.StartsWith("false;;"))
                {
                    falseString = @string.Replace("false;;", "");
                }
                else
                {
                    output += @string;
                }
            }
            return output.Replace("%s", @bool ? trueString : falseString);
        }

        public static long TurnTicksBetween(long TimeTickStart, long TimeTickEnd)
        {
            if (TimeTickStart > TimeTickEnd)
            {
                // throw new ArgumentException(nameof(TimeTickEnd) + " cannot be less than " + nameof(TimeTickStart));
            }
            return TimeTickEnd - TimeTickStart;
        }
        public static double GameHoursBetween(long TimeTickStart, long TimeTickEnd)
        {
            return TurnTicksBetween(TimeTickStart, TimeTickEnd).AsGameHours();
        }
        public static double GameDaysBetween(long TimeTickStart, long TimeTickEnd)
        {
            return TurnTicksBetween(TimeTickStart, TimeTickEnd).AsGameDays();
        }
        public static double GameYearsBetween(long TimeTickStart, long TimeTickEnd)
        {
            return TurnTicksBetween(TimeTickStart, TimeTickEnd).AsGameYears();
        }

        public static long TurnTickAfterTimeTicks(long TimeTicks)
        {
            if (The.Game == null)
            {
                MetricsManager.LogModError(ThisMod, nameof(The) + "." + nameof(The.Game) + " hasn't loaded yet. " + nameof(The.CurrentTurn) + " can't be retreived.");
                return default;
            }
            return The.CurrentTurn + TimeTicks;
        }
        public static long TurnTickAfterGameHours(double Hours)
        {
            return The.CurrentTurn + Hours.GameHoursAsTurnTicks();
        }
        public static long TurnTickAfterGameDays(double Days)
        {
            return The.CurrentTurn + Days.GameDaysAsTurnTicks();
        }
        public static long TurnTickAfterGameYears(double Years)
        {
            return The.CurrentTurn + Years.GameYearsAsTurnTicks();
        }

        public static long TimeTickAtStartOfCurrentGameHour()
        {
            return Math.Max(0, The.CurrentTurn.SubtractModulo(Calendar.TurnsPerHour));
        }
        public static long TimeTickAtStartOfCurrentGameDay()
        {
            return Math.Max(0, The.CurrentTurn.SubtractModulo(Calendar.TurnsPerDay));
        }
        public static long TimeTickAtStartOfCurrentGameYear()
        {
            return Math.Max(0, The.CurrentTurn.SubtractModulo(Calendar.TurnsPerYear));
        }

        public static ModInfo GetFirstCallingModNot(ModInfo ThisMod)
        {
            try
            {
                Dictionary<Assembly, ModInfo> modAssemblies = ModManager.ActiveMods
                    ?.Where(mi => mi != ThisMod && mi.Assembly != null)
                    ?.ToDictionary(mi => mi.Assembly, mi => mi);

                if (modAssemblies.IsNullOrEmpty())
                {
                    return null;
                }
                StackTrace stackTrace = new();
                for (int i = 0; i < 12 && stackTrace?.GetFrame(i) is StackFrame stackFrameI; i++)
                {
                    if (stackFrameI?.GetMethod() is MethodBase methodBase
                        && methodBase.DeclaringType is Type declaringType
                        && modAssemblies.ContainsKey(declaringType.Assembly))
                    {
                        return modAssemblies[declaringType.Assembly];
                    }
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(GetFirstCallingModNot), x, GAME_MOD_EXCEPTION);
            }
            return null;
        }

        public static bool TryGetFirstCallingModNot([NotNullWhen(true)] ModInfo ThisMod, out ModInfo FirstCallingMod)
            => (FirstCallingMod = GetFirstCallingModNot(ThisMod)) != null
            ;

        public static string AppendTick(string String, bool AppendSpace = true)
            => String + "[" + Const.TICK + "]" + (AppendSpace ? " " : "")
            ;

        public static string AppendCross(string String, bool AppendSpace = true)
            => String + "[" + Const.CROSS + "]" + (AppendSpace ? " " : "")
            ;

        public static string AppendYehNah(string String, bool Yeh, bool AppendSpace = true)
            => Yeh
            ? AppendTick(String, AppendSpace)
            : AppendCross(String, AppendSpace)
            ;

        public static string YehNah(bool? Yeh = null)
            => "[" + (Yeh == null ? "-" : (Yeh.GetValueOrDefault() ? Const.TICK : Const.CROSS)) + "]"
            ;

        public static string DelimitedAggregator<T>(string Accumulator, T Next, string Delimiter)
            => Accumulator + (!Accumulator.IsNullOrEmpty() ? Delimiter : null) + Next
            ;

        public static string CommaDelimitedAggregator<T>(string Accumulator, T Next)
            => DelimitedAggregator(Accumulator, Next, ",")
            ;

        public static string CommaSpaceDelimitedAggregator<T>(string Accumulator, T Next)
            => DelimitedAggregator(Accumulator, Next, ", ")
            ;

        public static string NewLineDelimitedAggregator<T>(string Accumulator, T Next)
            => DelimitedAggregator(Accumulator, Next, "\n")
            ;

        public static bool EitherNull<Tx, Ty>(
            [NotNullWhen(false)] Tx X,
            [NotNullWhen(false)] Ty Y,
            out bool AreEqual)
        {
            AreEqual = (X is null) == (Y is null);
            return X is null
                || Y is null;
        }

        public static bool EitherNull<Tx, Ty>(
            [NotNullWhen(false)] Tx X,
            [NotNullWhen(false)] Ty Y,
            out int Comparison)
        {
            Comparison = 0;

            bool xNull = X is null;
            bool yNull = Y is null;

            if (!xNull
                && !yNull)
                return false;

            if (!xNull
                && yNull)
                Comparison = 1;

            if (xNull
                && !yNull)
                Comparison = -1;

            return true;
        }

        public static bool EitherNullOrEmpty<Tx, Ty>(
            [NotNullWhen(false)] Tx[] X,
            [NotNullWhen(false)] Ty[] Y,
            out bool AreEqual)
        {
            if (EitherNull(X, Y, out AreEqual))
                return AreEqual;

            AreEqual = (X.Length > 0) == (Y.Length > 0);
            return X.Length > 0
                || Y.Length > 0;
        }

        public static string GetTile(GameObjectBlueprint Blueprint)
            => Blueprint.GetPartParameter<string>(nameof(Render), nameof(Render.Tile))
            ;

        public static string GetAnatomyName(GameObjectBlueprint Blueprint)
            => Blueprint.GetPartParameter<string>(nameof(Body), nameof(Body.Anatomy))
            ;

        public static Anatomy GetAnatomy(GameObjectBlueprint Blueprint)
            => Anatomies.GetAnatomyOrFail(GetAnatomyName(Blueprint))
            ;

        public static string NewlineAggregator<T>(string Accumulator, T Next)
            => Accumulator + (!Accumulator.IsNullOrEmpty() ? '\n' : null) + Next;

        public static StringBuilder AggregateNewline<T>(StringBuilder Accumulator, T Next)
            => Accumulator
                .Append(!Accumulator.IsNullOrEmpty() ? '\n' : null)
                .Append(Next);
    }
}