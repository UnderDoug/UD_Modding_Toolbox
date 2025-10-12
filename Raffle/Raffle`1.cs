using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.World;
using static XRL.World.Conversations.Expression;

namespace UD_Modding_Toolbox
{
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

        public TokenEnumerator GroupedActiveTokens => new(this, ActiveEntries);
        public TokenEnumerator GroupedDrawnTokens => new(this, DrawnEntries);

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        protected Random Rnd => Stat.GetSeededRandomGenerator(_NextSeed);
        protected string _NextSeed => _Seed + "::" + ActiveCount;

        protected string _Seed;

        public string Seed;

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
            Shake();
            Seed = null;
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
            SetSeed(Seed);
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
        public void Shake()
        {
            if (!Seeded)
            {
                _Seed = Utils.Rnd.Next().ToString();
            }
        }

        public virtual bool HasTokens()
        {
            return ActiveCount > 0;
        }

        public virtual bool CanDraw()
        {
            return HasTokens();
        }

        // Weights
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
        public int GetTotalWeight(T Token)
        {
            return GetActiveWeight(Token) + GetDrawnWeight(Token);
        }

        public int GetActiveWeight(T Token)
        {
            return GetWeight(ActiveEntries, Token); ;
        }
        public int GetDrawnWeight(T Token)
        {
            return GetWeight(DrawnEntries, Token);
        }

        // Chances
        public float GetTotalChance(T Token, int WeightAdjust, int CountAdjust)
        {
            if (Contains(Token))
            {
                return (float)(GetTotalWeight(Token) + WeightAdjust) / (ActiveCount + CountAdjust);
            }
            return 0;
        }
        public float GetTotalChance(T Token, int WeightAdjust)
        {
            return GetTotalChance(Token, WeightAdjust, 0);
        }
        public float GetTotalChance(T Token)
        {
            return GetTotalChance(Token, 0);
        }

        public float GetActiveChance(T Token, int WeightAdjust, int CountAdjust)
        {
            if (Contains(Token))
            {
                return (float)(GetActiveWeight(Token) + WeightAdjust) / (ActiveCount + CountAdjust);
            }
            return 0;
        }
        public float GetActiveChance(T Token, int WeightAdjust)
        {
            return GetActiveChance(Token, WeightAdjust, 0);
        }
        public float GetActiveChance(T Token)
        {
            return GetActiveChance(Token, 0);
        }

        public IEnumerable<float> GetTotalChances()
        {
            foreach (T token in this)
            {
                yield return GetTotalChance(token);
            }
        }
        public List<float> GetTotalChancesList()
        {
            return GetTotalChances().ToList();
        }

        public IEnumerable<float> GetActiveChances()
        {
            foreach (T token in this)
            {
                yield return GetActiveChance(token);
            }
        }
        public List<float> GetActiveChancesList()
        {
            return GetActiveChances().ToList();
        }

        public IEnumerable<KeyValuePair<T, int>> GetActiveEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            foreach (KeyValuePair<T, int> kv in ActiveEntries)
            {
                if ((Filter == null || Filter(kv)))
                {
                    yield return kv;
                }
            }
        }
        public IEnumerable<KeyValuePair<T, int>> GetActiveEntries(Predicate<T> Filter)
        {
            foreach ((T token, int weight) in ActiveEntries)
            {
                if ((Filter == null || Filter(token)))
                {
                    yield return new KeyValuePair<T, int>(token, weight);
                }
            }
        }

        public IEnumerable<KeyValuePair<T, int>> GetDrawnEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            foreach (KeyValuePair<T, int> kv in DrawnEntries)
            {
                if ((Filter == null || Filter(kv)))
                {
                    yield return kv;
                }
            }
        }
        public IEnumerable<KeyValuePair<T, int>> GetDrawnEntries(Predicate<T> Filter)
        {
            foreach ((T token, int weight) in DrawnEntries)
            {
                if ((Filter == null || Filter(token)))
                {
                    yield return new KeyValuePair<T, int>(token, weight);
                }
            }
        }

        public IEnumerable<KeyValuePair<T, int>> GetKeyValuePairEntries()
        {
            for (int i = 0; i < Length; i++)
            {
                Entry entry = new(ActiveEntries[i], 0);
                entry += DrawnEntries[i].Weight;
                yield return entry;
            }
        }

        public IEnumerable<KeyValuePair<T, int>> GetKeyValuePairEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            foreach (Entry entry in GetKeyValuePairEntries())
            {
                if (Filter == null || Filter(entry))
                {
                    yield return new Entry(entry);
                }
            }
        }
        public IEnumerable<KeyValuePair<T, int>> GetKeyValuePairEntries(Predicate<T> Filter)
        {
            foreach (Entry entry in GetKeyValuePairEntries())
            {
                if (Filter == null || Filter(entry))
                {
                    yield return new Entry(entry);
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
        bool NextToken(out T Token, Random Rnd)
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
        bool NextToken(out T Token)
        {
            return NextToken(out Token, Rnd);
        }
        bool DrawToken(T Token, int Weight)
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
        bool DrawToken(T Token)
        {
            return DrawToken(Token, 1);
        }
        bool DrawEntry(T Token)
        {
            return DrawToken(Token, this[Token]);
        }

        T Draw(Random Rnd, bool RefillIfEmpty)
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

        IEnumerable<T> DrawN(int Number, Random Rnd, bool RefillIfEmpty)
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

        IEnumerable<T> DrawUptoN(int Number, Random Rnd, bool RefillFIrst)
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

        IEnumerable<T> DrawAll(Random Rnd, bool RefillFirst)
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

        IEnumerable<T> DrawRemaining(Random Rnd)
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

        bool TryDraw(out T Token, Random Rnd)
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

        T DrawUnique(Random Rnd, bool RefillIfEmpty)
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

        T Sample(Random Rnd)
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

        bool TrySample(out T Token, Random Rnd)
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
