using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.World;
using static XRL.World.Conversations.Expression;

namespace UD_Modding_Toolbox
{
    /// <summary>
    /// Represents a container from which <see cref="Entry.Token"/>s can be drawn randomly, <see cref="Entry.Weight"/>ed according to an <see cref="int"/> value stored alongside each <see cref="Entry.Token"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="Raffle{T}"/> can be thought of like a hat from which you might draw names. Each name is a <see cref="string"/> <see cref="Entry.Token"/>, and placing duplicate <see cref="Entry.Token"/>s in results in increasing the likelihood of that token being drawn.<br/><br/>
    /// With given <see cref="string"/> <see cref="Entry.Token"/>s "cat", "dog", "bird", and "cat" again, <see cref="Draw()"/> is twice as likely to return "cat" as it is either "dog" or "bird" since "cat" has twice as much <see cref="Entry.Weight"/>s as either, but still only has a 50/50 chance overall, since it represents half the total number of <see cref="Entry.Token"/>s by <see cref="Entry.Weight"/> (4).
    /// </remarks>
    /// <typeparam name="T">Unrestricted, but reference types are less likely to run into issues I haven't anticipated.</typeparam>
    [Serializable]
    public partial class Raffle<T> : IComposite
    {
        private static bool doDebug => false;

        protected Entry[] ActiveEntries = Array.Empty<Entry>();
        protected Entry[] DrawnEntries = Array.Empty<Entry>();

        protected int TotalWeights;
        protected int TotalActiveWeights;
        protected int TotalDrawnWeights;

        public int TotalCount => SyncWeightTotals() ? TotalWeights : 0;
        public int ActiveCount => SyncWeightTotals() ? TotalActiveWeights : 0;
        public int DrawnCount => SyncWeightTotals() ? TotalDrawnWeights : 0;

        protected int Size;

        protected int Length;

        protected int Variant;

        public int Capacity => Size;

        public int Version => Variant;

        /// <summary>
        /// Enumerator for each <see cref="Entry.Token"/> currently capable of being returned by <see cref="Draw()"/>, as though each <see cref="Entry.Token"/> was returned by <see cref="Draw()"/> sequentially.
        /// </summary>
        /// <remarks>
        /// A <see cref="Raffle{T}"/> with entries { {"cat", 2}, {"dog", 1}, {"bird", 2} } will enumerate <see cref="Entry.Token"/>s "cat", "cat", "dog", "bird", "bird".
        /// </remarks>
        public TokenEnumerator GroupedActiveTokens => new(this, ActiveEntries);

        /// <summary>
        /// Enumerator for each <see cref="Entry.Token"/> that has already been returned by <see cref="Draw()"/>, as though drawing each <see cref="Entry.Token"/> sequentially.
        /// </summary>
        /// <remarks>
        /// A <see cref="Raffle{T}"/> with entries { {"cat", 2}, {"dog", 1}, {"bird", 2} } that has already drawn <see cref="Entry.Token"/>s "bird" and "cat" will enumerate <see cref="Entry.Token"/>s "cat", "bird" (this is the order the <see cref="Entry"/>s were added to the <see cref="Raffle{T}"/>).
        /// </remarks>
        public TokenEnumerator GroupedDrawnTokens => new(this, DrawnEntries);

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        protected Random Rnd => Stat.GetSeededRandomGenerator(_NextSeed);
        protected string _NextSeed => _Seed + "::" + ActiveCount;

        protected string _Seed;

        /// <summary>
        /// The deterministic results of a <see cref="Raffle{T}"/> can be controlled by providing a <see cref="string"/> seed to the constructor.
        /// </summary>
        /// <remarks>
        /// For a given seed, provided the contents of the <see cref="Raffle{T}"/> aren't altered between <see cref="Draw()"/> calls, the order that <see cref="Entry.Token"/>s are returned by <see cref="Draw()"/> will be the same every time.
        /// </remarks>
        public string Seed { get; protected set; }

        /// <returns>
        /// <see langword="true"/> if a <see cref="Seed"/> is currently in effect;<br/>
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Seeded => !Seed.IsNullOrEmpty();

        public Raffle()
        {
            Size = 0;
            EnsureCapacity(DefaultCapacity);
            Length = 0;
            Variant = 0;
            TotalWeights = 0;
            TotalActiveWeights = 0;
            TotalDrawnWeights = 0;
            Seed = null;
            Shake();
        }
        public Raffle(int Capacity)
            : this()
        {
            EnsureCapacity(Capacity);
        }
        public Raffle(string Seed)
            : this()
        {
            SetSeed(Seed);
        }
        public Raffle(string Seed, int Capacity)
            : this(Capacity)
        {
            SetSeed(Seed);
        }
        public Raffle(Raffle<T> Source)
            : this()
        {
            if (Source == null)
            {
                throw new ArgumentNullException(nameof(Source));
            }
            SetSeed(Source.Seed);
            EnsureCapacity(Source.Count);
            foreach (Entry entry in Source)
            {
                Add(entry.Token, entry.Weight);
            }
        }
        public Raffle(string Seed, Raffle<T> Source)
            : this(Source)
        {
            SetSeed(Seed);
        }
        public Raffle(ICollection<KeyValuePair<T, int>> Source)
            : this((Raffle<T>)Source)
        {
        }
        public Raffle(string Seed, ICollection<KeyValuePair<T, int>> Source)
            : this(Seed, (Raffle<T>)Source)
        {
        }
        public Raffle(Dictionary<T, int> Source)
            : this((Raffle<T>)Source)
        {
        }
        public Raffle(string Seed, Dictionary<T, int> Source)
            : this(Seed, (Raffle<T>)Source)
        {
        }
        public Raffle(List<T> Source)
            : this((Raffle<T>)Source)
        {
        }
        public Raffle(string Seed, List<T> Source)
            : this(Seed, (Raffle<T>)Source)
        {
        }

        public void EnsureCapacity(int Capacity)
        {
            if (Size < Capacity)
            {
                Resize(Capacity);
            }
        }

        protected void Resize(int Capacity)
        {
            if (Capacity == 0)
            {
                Capacity = DefaultCapacity;
            }
            Array.Resize(array: ref ActiveEntries, Capacity);
            Array.Resize(array: ref DrawnEntries, Capacity);
            Size = Capacity;
        }

        protected bool SyncWeightTotals()
        {
            TotalActiveWeights = 0;
            TotalDrawnWeights = 0;
            for (int i = 0; i < Count; i++)
            {
                TotalActiveWeights += (int)ActiveEntries[i];
                TotalDrawnWeights += (int)DrawnEntries[i];
            }
            TotalWeights = TotalActiveWeights + TotalDrawnWeights;
            return true;
        }

        protected Random SetSeed(string Seed)
        {
            _Seed = this.Seed = Seed ?? "none";
            return Rnd;
        }

        /// <summary>
        /// Generates a new internal <see cref="_Seed"/> for a <see cref="Raffle{T}"/> that is not <see cref="Seeded"/>. Throws an <see cref="InvalidOperationException()"/> if called by a <see cref="Seeded"/> one.
        /// </summary>
        /// <remarks>
        /// The results will continue to be deterministic, but in a different random order from before <see cref="Shake()"/> was called.
        /// </remarks>
        public void Shake()
        {
            if (!Seeded)
            {
                _Seed = Utils.Rnd.Next().ToString();
            }
            else
            {
                throw new InvalidOperationException(
                    "A " + nameof(Seeded) + " " + nameof(Raffle<T>) + " can't be shaken because the results are supposed to be deterministic. " +
                    "Consider copying the " + nameof(Raffle<T>) + " to a new instance without a seed.");
            }
        }

        /// <summary>
        /// Chacks that <see cref="Shake()"/> is a valid operation calling it and returning <see langword="true"/> if so; otherwise, returning <see langword="false"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="Shake()"/> is successful;<br/>
        /// <see langword="false"/> otherwise.
        /// </returns>
        public virtual bool TryShake()
        {
            if (Seeded)
            {
                Shake();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether any <see cref="ActiveEntries"/> have any <see cref="Entry.Token"/>s avaiable to <see cref="Draw()"/> (<see cref="Entry.Weight"/> &gt; 0), returning <see langword="true"/> if so; otherwise, returning <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// A derived type could <see langword="override"/> this method to narrow or widen what it means for a <see cref="Raffle{T}"/> to "have <see cref="Entry.Token"/>s".
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the <see cref="ActiveCount"/> is greater than 0;<br/>
        /// <see langword="false"/> otherwise.
        /// </returns>
        public virtual bool HasTokens()
        {
            return ActiveCount > 0;
        }

        /// <summary>
        /// Checks whether the <see cref="Raffle{T}"/> is currently in a state in which calling <see cref="Draw()"/> would successfully return a <see cref="Entry.Token"/> at least one time, returning <see langword="true"/> if so; otherwise, returning <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// A derived type could <see langword="override"/> this method to narrow or widen what it means for a <see cref="Raffle{T}"/> to be capable of successfully calling <see cref="Draw()"/>.
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if <see cref="HasTokens()"/> returns <see langword="true"/>;<br/>
        /// <see langword="false"/> otherwise.
        /// </returns>
        public virtual bool CanDraw()
        {
            return HasTokens();
        }

        protected static int GetWeight(Entry[] Entries, T Token)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (!Equals(Entries[i], null) && Equals(Entries[i].Token, Token))
                {
                    return (int)Entries[i];
                }
            }
            return 0;
        }

        /// <summary>
        /// Retreives the <see cref="ActiveEntries"/> <see cref="Entry.Weight"/> value for the passed <see cref="Entry.Token"/>.
        /// </summary>
        /// <param name="Token">The <see cref="Entry.Token"/> <see cref="ActiveEntries"/> is checked for to retreive a <see cref="Entry.Weight"/> value.</param>
        /// <returns>
        /// The <see cref="Entry.Weight"/> of the supplied <paramref name="Token"/>; otherwise, <br/>
        /// 0 if the <see cref="Raffle{T}"/> doesn't contain it at all.
        /// </returns>
        public int GetActiveWeight(T Token)
        {
            return GetWeight(ActiveEntries, Token); ;
        }

        /// <summary>
        /// Retreives the <see cref="DrawnEntries"/> <see cref="Entry.Weight"/> value for the passed <paramref name="Token"/>.
        /// </summary>
        /// <param name="Token">The <see cref="Entry.Token"/> <see cref="DrawnEntries"/> is checked for to retreive a <see cref="Entry.Weight"/> value.</param>
        /// <returns>
        /// The <see cref="Entry.Weight"/> of the supplied <paramref name="Token"/>; otherwise, <br/>
        /// 0 if the <see cref="Raffle{T}"/> doesn't contain it at all.
        /// </returns>
        public int GetDrawnWeight(T Token)
        {
            return GetWeight(DrawnEntries, Token);
        }

        /// <summary>
        /// Retreives the combined <see cref="ActiveEntries"/> and <see cref="DrawEntries"/> <see cref="Entry.Weight"/> value for the passed <see cref="Entry.Token"/>.
        /// </summary>
        /// <param name="Token">The <see cref="Entry.Token"/> <see cref="ActiveEntries"/> and <see cref="DrawEntries"/> are checked for to retreive a <see cref="Entry.Weight"/> value.</param>
        /// <returns>
        /// The combined <see cref="Entry.Weight"/> of the supplied <paramref name="Token"/>; otherwise, <br/>
        /// 0 if the <see cref="Raffle{T}"/> doesn't contain it at all.
        /// </returns>
        public int GetTotalWeight(T Token)
        {
            return GetActiveWeight(Token) + GetDrawnWeight(Token);
        }

        /// <summary>
        /// Calculates the chance (where 1 is 100%) that the passed <paramref name="Token"/> would be returned by the first call of <see cref="Draw()"/>, adjusting the <see cref="Entry.Weight"/> value by <paramref name="WeightAdjust"/> for the purposes of this calculation.
        /// </summary>
        /// <remarks>
        /// This method treats the <see cref="Raffle{T}"/> as though no <see cref="Entry.Token"/>s have been returned by <see cref="Draw()"/> and <see cref="DrawnCount"/> is 0.
        /// </remarks>
        /// <param name="Token">The <see cref="Entry.Token"/> whose chance to be returned by <see cref="Draw()"/> will be calculated based on its <see cref="Entry.Weight"/>.</param>
        /// <param name="WeightAdjust">An amount to add to the passed <paramref name="Token"/>'s <see cref="Entry.Weight"/> before calculating its chance to be returned by <see cref="Draw()"/></param>
        /// <returns>
        /// A value up to 1, representing the passed <paramref name="Token"/>'s chance to be returned by the first <see cref="Draw()"/> call, as though its <see cref="Entry.Weight"/> was <paramref name="WeightAdjust"/> higher.
        /// </returns>
        public float GetTotalChance(T Token, int WeightAdjust)
        {
            if (Contains(Token))
            {
                return (float)Math.Max(0, GetTotalWeight(Token) + WeightAdjust) / TotalCount;
            }
            return 0;
        }

        /// <summary>
        /// Calculates the chance (where 1 is 100%) that the passed <paramref name="Token"/> would be returned by the first call of <see cref="Draw()"/>.
        /// </summary>
        /// <remarks>
        /// This method treats the <see cref="Raffle{T}"/> as though no <see cref="Entry.Token"/>s have been returned by <see cref="Draw()"/> and <see cref="DrawnCount"/> is 0.
        /// </remarks>
        /// <param name="Token">The <see cref="Entry.Token"/> whose chance to be returned by <see cref="Draw()"/> will be calculated based on its <see cref="Entry.Weight"/>.</param>
        /// <returns>
        /// A value up to 1, representing the passed <paramref name="Token"/>'s chance to be returned by the first <see cref="Draw()"/> call.
        /// </returns>
        public float GetTotalChance(T Token)
        {
            return GetTotalChance(Token, 0);
        }

        /// <summary>
        /// Calculates the chance (where 1 is 100%) that the passed <paramref name="Token"/> will be the next one returned by <see cref="Draw()"/>, adjusting the <see cref="Entry.Weight"/> value by <paramref name="WeightAdjust"/> and the <see cref="ActiveCount"/> by <paramref name="CountAdjust"/> for the purposes of this calculation.
        /// </summary>
        /// <param name="Token">The <see cref="Entry.Token"/> whose chance to be returned by <see cref="Draw()"/> will be calculated based on its <see cref="Entry.Weight"/> after adding <paramref name="WeightAdjust"/> to it.</param>
        /// <param name="WeightAdjust">The amount to add to the passed <paramref name="Token"/>'s <see cref="Entry.Weight"/> before calculating its chance to be returned by <see cref="Draw()"/>.</param>
        /// <param name="CountAdjust">
        ///     The amount to add to <see cref="ActiveCount"/> before calculating the passed <paramref name="Token"/>'s chance to be returned by <see cref="Draw()"/><br/><br/>
        ///     To avoid a <see cref="DivideByZeroException"/>, the value passed to this parameter shouldn't result in <see cref="ActiveCount"/> being below 1.
        /// </param>
        /// <returns>
        /// A value up to 1, representing the passed <paramref name="Token"/>'s chance to be returned by the next <see cref="Draw()"/> call, as though its <see cref="Entry.Weight"/> was <paramref name="WeightAdjust"/> higher and the <see cref="ActiveCount"/> was <paramref name="CountAdjust"/> higher.
        /// </returns>
        /// <exception cref="DivideByZeroException"><see cref="ActiveCount"/> is &lt; 1 and <paramref name="CountAdjust"/> is insufficient to bring it above 0, or <paramref name="CountAdjust"/> brings <see cref="ActiveCount"/> below 1.</exception>
        public float GetActiveChance(T Token, int WeightAdjust, int CountAdjust)
        {
            if (ActiveCount < 1 && CountAdjust < 1)
            {
                throw new DivideByZeroException(nameof(ActiveCount) + " is less than 1 and the passed " + nameof(CountAdjust) + " is insufficient to bring it above 0");
            }
            if (ActiveCount + CountAdjust < 1)
            {
                throw new DivideByZeroException(nameof(CountAdjust) + " must not reduce " + nameof(ActiveCount) + " below 1");
            }
            if (Contains(Token))
            {
                return (float)Math.Max(0, GetActiveWeight(Token) + WeightAdjust) / ActiveCount + CountAdjust;
            }
            return 0;
        }

        /// <summary>
        /// Calculates the chance (where 1 is 100%) that the passed <paramref name="Token"/> will be the next one returned by <see cref="Draw()"/>, adjusting the <see cref="Entry.Weight"/> value by <paramref name="WeightAdjust"/> for the purposes of this calculation.
        /// </summary>
        /// <param name="Token">The <see cref="Entry.Token"/> whose chance to be returned by <see cref="Draw()"/> will be calculated based on its <see cref="Entry.Weight"/> after adding <paramref name="WeightAdjust"/> to it.</param>
        /// <param name="WeightAdjust">The amount to add to the passed <paramref name="Token"/>'s <see cref="Entry.Weight"/> before calculating its chance to be returned by <see cref="Draw()"/>.</param>
        /// <returns>
        /// A value up to 1, representing the passed <paramref name="Token"/>'s chance to be returned by the next <see cref="Draw()"/> call, as though its <see cref="Entry.Weight"/> was <paramref name="WeightAdjust"/> higher.
        /// </returns>
        /// <exception cref="DivideByZeroException"><see cref="ActiveCount"/> is &lt; 1.</exception>
        public float GetActiveChance(T Token, int WeightAdjust)
        {
            return GetActiveChance(Token, WeightAdjust, 0);
        }

        /// <summary>
        /// Calculates the chance (where 1 is 100%) that the passed <paramref name="Token"/> will be the next one returned by <see cref="Draw()"/>.
        /// </summary>
        /// <param name="Token">The <see cref="Entry.Token"/> whose chance to be returned by <see cref="Draw()"/> will be calculated based on its <see cref="Entry.Weight"/>.</param>
        /// <returns>
        /// A value up to 1, representing the passed <paramref name="Token"/>'s chance to be returned by the next <see cref="Draw()"/> call.
        /// </returns>
        /// <exception cref="DivideByZeroException"><see cref="ActiveCount"/> is &lt; 1.</exception>
        public float GetActiveChance(T Token)
        {
            return GetActiveChance(Token, 0);
        }

        /// <summary>
        /// Enumerates the <see cref="Raffle{T}"/> yielding a <see cref="KeyValuePair{T,float}"/> containing each <see cref="Entry.Token"/> and its chance to be returned by the first <see cref="Draw()"/> call, per <see cref="GetTotalChance(T)"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{KeyValuePair{T,float}}"/> of <see cref="KeyValuePair{T,float}"/> containing each <see cref="Entry.Token"/> and its chance to be returned by the first <see cref="Draw()"/> call, per <see cref="GetTotalChance(T)"/></returns>
        public IEnumerable<KeyValuePair<T,float>> GetTotalChances()
        {
            foreach (T token in this)
            {
                yield return new(token, GetTotalChance(token));
            }
        }

        /// <summary>
        /// Produces a <see cref="List{KeyValuePair{T,float}}"/> of <see cref="KeyValuePair{T,float}"/> for each <see cref="Entry.Token"/> and its chance to be returned by the first <see cref="Draw()"/> call, per <see cref="GetTotalChance(T)"/>.
        /// </summary>
        /// <returns>A <see cref="List{KeyValuePair{T,float}}"/> of <see cref="KeyValuePair{T,float}"/> for each <see cref="Entry.Token"/> and its chance to be returned by the first <see cref="Draw()"/> call, per <see cref="GetTotalChance(T)"/></returns>
        public List<KeyValuePair<T, float>> GetTotalChancesList()
        {
            return GetTotalChances().ToList();
        }

        /// <summary>
        /// Enumerates the <see cref="Raffle{T}"/> yielding a <see cref="KeyValuePair{T,float}"/> containing each <see cref="Entry.Token"/> and its chance to be returned by the next <see cref="Draw()"/> call, per <see cref="GetActiveChance(T)"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{KeyValuePair{T,float}}"/> of <see cref="KeyValuePair{T,float}"/> containing each <see cref="Entry.Token"/> and its chance to be returned by the next <see cref="Draw()"/> call, per <see cref="GetActiveChance(T)"/></returns>
        public IEnumerable<KeyValuePair<T, float>> GetActiveChances()
        {
            foreach (T token in this)
            {
                yield return new(token, GetActiveChance(token));
            }
        }

        /// <summary>
        /// Produces a <see cref="List{KeyValuePair{T,float}}"/> of <see cref="KeyValuePair{T,float}"/> for each <see cref="Entry.Token"/> and its chance to be returned by the next <see cref="Draw()"/> call, per <see cref="GetActiveChance(T)"/>.
        /// </summary>
        /// <returns>A <see cref="List{KeyValuePair{T,float}}"/> of <see cref="KeyValuePair{T,float}"/> for each <see cref="Entry.Token"/> and its chance to be returned by the next <see cref="Draw()"/> call, per <see cref="GetActiveChance(T)"/></returns>
        public List<KeyValuePair<T, float>> GetActiveChancesList()
        {
            return GetActiveChances().ToList();
        }

        public IEnumerable<KeyValuePair<T, int>> GetActiveEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            foreach (KeyValuePair<T, int> entry in ActiveEntries)
            {
                if (Filter == null || Filter(entry))
                {
                    yield return new(entry.Key, entry.Value);
                }
            }
        }
        public IEnumerable<KeyValuePair<T, int>> GetActiveEntries(Predicate<T> Filter)
        {
            foreach (KeyValuePair<T, int> entry in ActiveEntries)
            {
                if (Filter == null || Filter(entry.Key))
                {
                    yield return new(entry.Key, entry.Value);
                }
            }
        }

        public IEnumerable<KeyValuePair<T, int>> GetDrawnEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            foreach (KeyValuePair<T, int> entry in DrawnEntries)
            {
                if (Filter == null || Filter(entry))
                {
                    yield return new(entry.Key, entry.Value);
                }
            }
        }
        public IEnumerable<KeyValuePair<T, int>> GetDrawnEntries(Predicate<T> Filter)
        {
            foreach (KeyValuePair<T, int> entry in DrawnEntries)
            {
                if (Filter == null || Filter(entry.Key))
                {
                    yield return new(entry.Key, entry.Value);
                }
            }
        }

        public IEnumerable<KeyValuePair<T, int>> GetKeyValuePairEntries()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return new (ActiveEntries[i], GetTotalWeight(ActiveEntries[i]));
            }
        }

        public IEnumerable<KeyValuePair<T, int>> GetKeyValuePairEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            foreach (KeyValuePair<T, int> entry in GetKeyValuePairEntries())
            {
                if (Filter == null || Filter(entry))
                {
                    yield return new(entry.Key, entry.Value);
                }
            }
        }
        public IEnumerable<KeyValuePair<T, int>> GetKeyValuePairEntries(Predicate<T> Filter)
        {
            foreach (KeyValuePair<T, int> entry in GetKeyValuePairEntries())
            {
                if (Filter == null || Filter(entry.Key))
                {
                    yield return new(entry.Key, entry.Value);
                }
            }
        }

        public IEnumerable<T> GetGroupedActiveTokens(Predicate<T> Filter = null)
        {
            foreach (T token in GroupedActiveTokens)
            {
                if (Filter != null && !Filter(token))
                {
                    continue;
                }
                yield return token;
            }
        }

        public IEnumerable<T> GetGroupedDrawnTokens(Predicate<T> Filter = null)
        {
            foreach (T token in GroupedDrawnTokens)
            {
                if (Filter != null && !Filter(token))
                {
                    continue;
                }
                yield return token;
            }
        }

        protected int Next(Random Rnd)
        {
            if (!HasTokens())
            {
                return -1;
            }
            return Rnd.Next(ActiveCount);
        }
        protected int Next()
        {
            return Next(Rnd);
        }

        protected bool NextToken(out T Token, Random Rnd)
        {
            Token = default;
            int targetWeight = Next(Rnd);
            if (!HasTokens() || targetWeight < 0)
            {
                return false;
            }
            int currentCombinedWeight = 0;
            for (int i = 0; i < Length; i++)
            {
                Entry entry = ActiveEntries[i];
                if ((int)entry < 1)
                {
                    continue;
                }
                if (targetWeight < (currentCombinedWeight += (int)entry))
                {
                    Token = ActiveEntries[i];
                    return true;
                }
            }
            throw new IndexOutOfRangeException(nameof(targetWeight) + " was too big for total combined weight of " + currentCombinedWeight);
        }
        protected bool DrawToken(T Token, int Weight)
        {
            if (IndexOf(Token) is int index
                && index > -1
                && ActiveEntries[index] > 0)
            {
                int weight = Math.Min(Weight, (int)ActiveEntries[index]);
                ActiveEntries[index] -= weight;
                DrawnEntries[index] += weight;

                SyncWeightTotals();

                return true;
            }
            return false;
        }
        protected bool DrawToken(T Token)
        {
            return DrawToken(Token, 1);
        }
        protected bool DrawEntry(T Token)
        {
            return DrawToken(Token, this[Token]);
        }

        protected T Draw(Random Rnd, bool RefillIfEmpty)
        {
            if (NextToken(out T token, Rnd)
                && DrawToken(token))
            {
                if (RefillIfEmpty && !HasTokens())
                {
                    Refill();
                }
                return token;
            }
            return default;
        }
        public T Draw(bool RefillIfEmpty)
        {
            return Draw(Rnd, RefillIfEmpty);
        }
        public T Draw()
        {
            return Draw(true);
        }
        public T DrawCosmetic(bool RefillIfEmpty)
        {
            return Draw(Stat.Rnd2, RefillIfEmpty);
        }
        public T DrawCosmetic()
        {
            return DrawCosmetic(true);
        }

        protected IEnumerable<T> DrawN(int Number, Random Rnd, bool RefillIfEmpty)
        {
            if ((RefillIfEmpty || ActiveCount > Number) && Number > 0)
            {
                for (int i = 0; i < Number; i++)
                {
                    yield return Draw(Rnd, RefillIfEmpty);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(Number), 
                    message: "Paramater must be greater than zero, " +
                        "and not exceed " + nameof(ActiveCount) + " if " + 
                        nameof(RefillIfEmpty) + " is false");
            }
        }

        public IEnumerable<T> DrawN(int Number, bool RefillIfEmpty)
        {
            return DrawN(Number, Rnd, RefillIfEmpty);
        }

        public IEnumerable<T> DrawN(int Number)
        {
            return DrawN(Number, true);
        }

        public IEnumerable<T> DrawNConsmetic(int Number, bool RefillIfEmpty)
        {
            return DrawN(Number, Stat.Rnd2, RefillIfEmpty);
        }
        public IEnumerable<T> DrawNConsmetic(int Number)
        {
            return DrawNConsmetic(Number, true);
        }

        protected IEnumerable<T> DrawUptoN(int Number, Random Rnd, bool RefillFIrst)
        {
            if (RefillFIrst && !HasTokens())
            {
                Refill();
            }
            if (Number > 0)
            {
                for (int i = 0; i < Number; i++)
                {
                    if (TryDraw(out T token, Rnd))
                    {
                        yield return token;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(Number), 
                    message: "Paramater must be greater than zero");
            }
        }

        public IEnumerable<T> DrawUptoN(int Number, bool RefillFIrst)
        {
            return DrawUptoN(Number, Rnd, RefillFIrst);
        }
        public IEnumerable<T> DrawUptoN(int Number)
        {
            return DrawUptoN(Number, true);
        }

        public IEnumerable<T> DrawUptoNCosmetic(int Number, bool RefillFIrst)
        {
            return DrawUptoN(Number, Stat.Rnd2, RefillFIrst);
        }
        public IEnumerable<T> DrawUptoNCosmetic(int Number)
        {
            return DrawUptoNCosmetic(Number, true);
        }

        protected IEnumerable<T> DrawAll(Random Rnd, bool RefillFirst)
        {
            if (RefillFirst)
            {
                Refill();
            }
            if (ActiveCount > 0)
            {
                while (CanDraw())
                {
                    yield return Draw(Rnd, false);
                }
            }
            else
            {
                throw new InvalidOperationException("Can't " + nameof(DrawAll) + " from an empty " + nameof(Raffle<T>));
            }
        }
        public IEnumerable<T> DrawAll(bool RefillFirst)
        {
            return DrawAll(Rnd, RefillFirst);
        }
        public IEnumerable<T> DrawAll()
        {
            return DrawAll(Rnd, true);
        }

        public IEnumerable<T> DrawAllCosmetic(bool RefillFirst)
        {
            return DrawAll(Stat.Rnd2, RefillFirst);
        }
        public IEnumerable<T> DrawAllCosmetic()
        {
            return DrawAllCosmetic(true);
        }

        protected IEnumerable<T> DrawRemaining(Random Rnd)
        {
            return DrawAll(Rnd, false);
        }
        public IEnumerable<T> DrawRemaining()
        {
            return DrawRemaining(Rnd);
        }
        public IEnumerable<T> DrawRemainingCosmetic()
        {
            return DrawRemaining(Stat.Rnd2);
        }

        protected bool TryDraw(out T Token, Random Rnd)
        {
            Token = Draw(Rnd, false);
            if (!Equals(Token, null) && !Equals(Token, default))
            {
                return true;
            }
            return false;
        }
        public bool TryDraw(out T Token)
        {
            return TryDraw(out Token, Rnd);
        }

        protected T DrawUnique(Random Rnd, bool RefillIfEmpty)
        {
            if (NextToken(out T token, Rnd)
                && DrawEntry(token))
            {
                if (RefillIfEmpty && !HasTokens())
                {
                    Refill();
                }
                return token;
            }
            return default;
        }
        public T DrawUnique(bool RefillIfEmpty)
        {
            return DrawUnique(Rnd, RefillIfEmpty);
        }
        public T DrawUnique()
        {
            return DrawUnique(true);
        }

        public T DrawUniqueCosmetic(bool RefillIfEmpty)
        {
            return DrawUnique(Stat.Rnd2, RefillIfEmpty);
        }
        public T DrawUniqueCosmetic()
        {
            return DrawUniqueCosmetic(true);
        }

        protected T Sample(Random Rnd)
        {
            if (NextToken(out T token, Rnd))
            {
                return token;
            }
            return default;
        }
        public T Sample()
        {
            return Sample(Rnd);
        }
        public T SampleCosmetic()
        {
            return Sample(Stat.Rnd2);
        }

        protected bool TrySample(out T Token, Random Rnd)
        {
            Token = Sample(Rnd);
            if (!Equals(Token, null) && !Equals(Token, default))
            {
                return true;
            }
            return false;
        }
        public bool TrySample(out T Token)
        {
            return TrySample(out Token, Rnd);
        }
        public bool TrySampleCosmetic(out T Token)
        {
            return TrySample(out Token, Stat.Rnd2);
        }

        public Raffle<T> Refill(string Seed = null)
        {
            for (int i = 0; i < Length; i++)
            {
                T token = (T)ActiveEntries[i];
                int combinedWeight = (int)ActiveEntries[i] + (int)DrawnEntries[i];

                ActiveEntries[i] = new(token, combinedWeight);
                DrawnEntries[i] = new(token, 0);
            }
            SyncWeightTotals();
            if (!Seeded)
            {
                Shake();
            }
            else
            if (Seed != null)
            {
                SetSeed(Seed);
            }
            Variant++;
            return this;
        }

        public static Raffle<T> operator +(Raffle<T> operand1, Raffle<T> operand2)
        {
            if (operand1.IsNullOrEmpty() || operand2.IsNullOrEmpty())
            {
                throw new ArgumentNullException();
            }
            for (int i = 0; i < operand2.Length; i++)
            {
                T token = operand2[i];
                int weight = operand2[token];
                operand1.Add(operand2[i], weight);
            }
            return operand1;
        }

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Size);
            Writer.WriteOptimized(Length);
            Writer.WriteOptimized(Variant);
            Writer.WriteOptimized(TotalCount);
            Writer.WriteOptimized(ActiveCount);
            Writer.WriteOptimized(DrawnCount);
            Writer.WriteOptimized(_Seed);
            Writer.Write(ActiveEntries.ToList());
            Writer.Write(DrawnEntries.ToList());
        }

        public virtual void Read(SerializationReader Reader)
        {
            Size = Reader.ReadOptimizedInt32();
            Length = Reader.ReadOptimizedInt32();
            Variant = Reader.ReadOptimizedInt32();
            TotalWeights = Reader.ReadOptimizedInt32();
            TotalActiveWeights = Reader.ReadOptimizedInt32();
            TotalDrawnWeights = Reader.ReadOptimizedInt32();
            _Seed = Reader.ReadOptimizedString();
            ActiveEntries = Reader.ReadList<Entry>().ToArray();
            DrawnEntries = Reader.ReadList<Entry>().ToArray();
        }
    }
}
