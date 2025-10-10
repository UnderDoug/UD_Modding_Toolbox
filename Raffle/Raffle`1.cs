using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.Rules;
using XRL.World;

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

        public int ActiveCount => TotalActiveWeights;
        public int DrawnCount => TotalDrawnWeights;

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
        protected string _NextSeed => _Seed + "::" + TotalActiveWeights;

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
            TotalDrawnWeights = 0;
            Shake();
            Seed = null;
        }
        public Raffle(int Capacity)
            : this()
        {
            Size = Capacity;
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
            int indent = Debug.LastIndent;
            Debug.Entry(4, nameof(EnsureCapacity), Capacity.ToString() + " : " + Size.ToString(), Indent: indent + 1, Toggle: doDebug);
            if (Size < Capacity)
            {
                Resize(Capacity);
            }
            Debug.LastIndent = indent;
        }

        protected void Resize(int Capacity)
        {
            int indent = Debug.LastIndent;
            Debug.Entry(4, nameof(Resize), Capacity.ToString(), Indent: indent + 1, Toggle: doDebug);
            if (Capacity == 0)
            {
                Capacity = DefaultCapacity;
            }
            // Entry[] activeEntries = new Entry[Capacity];
            // Entry[] drawnEntries = new Entry[Capacity];
            for (int i = 0; i < Length; i++)
            {
                // activeEntries[i] = ActiveEntries[i];
                // drawnEntries[i] = DrawnEntries[i];
            }
            // Array.Copy(ActiveEntries, 0, activeEntries, 0, Length);
            // Array.Copy(DrawnEntries, 0, drawnEntries, 0, Length);
            // ActiveEntries = activeEntries;
            // DrawnEntries = drawnEntries;
            Array.Resize(array: ref ActiveEntries, Capacity);
            Array.Resize(array: ref DrawnEntries, Capacity);
            Size = Capacity;
            Debug.LastIndent = indent;
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
            return TotalActiveWeights > 0;
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
                    return Entries[i];
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
        protected static float GetChance(Entry[] Entries, T Token, int TotalWeights)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (!Equals(Entries[i], null) && Equals(Entries[i].Token, Token) && Entries[i] > 0)
                {
                    return Entries[i] / TotalWeights;
                }
            }
            return 0;
        }
        public float GetTotalChance(T Token)
        {
            if (Contains(Token))
            {
                return GetTotalWeight(Token) / TotalWeights;
            }
            return 0;
        }

        public float GetActiveChance(T Token)
        {
            if (Contains(Token))
            {
                return GetActiveWeight(Token) / TotalActiveWeights;
            }
            return 0;
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

        int Next()
        {
            return Rnd.Next(TotalActiveWeights);
        }
        T NextToken()
        {
            int targetWeight = Next();
            int currentCombinedWeight = 0;
            for (int i = 0; i < Length; i++)
            {
                if (targetWeight < (currentCombinedWeight += ActiveEntries[i]))
                {
                    return ActiveEntries[i];
                }
            }
            return default;
        }
        bool DrawToken(T Token)
        {
            if (IndexOf(Token) is int index
                && index > -1
                && ActiveEntries[index] > 0)
            {
                ActiveEntries[index]--;
                TotalActiveWeights--;

                DrawnEntries[index]++;
                TotalDrawnWeights++;

                return true;
            }
            return false;
        }

        public T Draw(bool RefillIfEmpty)
        {
            if (NextToken() is T token
                && DrawToken(token))
            {
                if (RefillIfEmpty && ActiveCount < 1)
                {
                    Refill();
                }
                return token;
            }
            return default;
        }

        public T Draw()
        {
            return Draw(true);
        }

        public IEnumerable<T> DrawN(int Number, bool RefillIfEmpty)
        {
            if ((RefillIfEmpty || ActiveCount > Number) && Number > 0)
            {
                for (int i = 0; i < Number; i++)
                {
                    yield return Draw(RefillIfEmpty);
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
        public IEnumerable<T> DrawN(int Number)
        {
            return DrawN(Number, true);
        }

        public IEnumerable<T> DrawUptoN(int Number, bool RefillFIrst)
        {
            if (RefillFIrst && TotalDrawnWeights > 0)
            {
                Refill();
            }
            if (Number > 0)
            {
                for (int i = 0; i < Number; i++)
                {
                    if (TryDraw(out T token))
                    {
                        yield return token;
                    }
                    else
                    {
                        yield break;
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
        public IEnumerable<T> DrawUptoN(int Number)
        {
            return DrawUptoN(Number, false);
        }

        public IEnumerable<T> DrawAll(bool RefillFirst)
        {
            if (RefillFirst)
            {
                Refill();
            }
            if (ActiveCount > 0)
            {
                while (CanDraw())
                {
                    yield return Draw(false);
                }
            }
            else
            {
                throw new InvalidOperationException("Can't " + nameof(DrawAll) + " from an empty " + nameof(Raffle<T>));
            }
        }

        public IEnumerable<T> DrawAll()
        {
            return DrawAll(false);
        }

        public bool TryDraw(out T Token)
        {
            Token = Draw(false);
            if (!Equals(Token, null) && !Equals(Token, default))
            {
                return true;
            }
            return false;
        }

        public T Sample()
        {
            if (NextToken() is T token)
            {
                return token;
            }
            return default;
        }

        public bool TrySample(out T Token)
        {
            Token = Sample();
            if (!Equals(Token, null) && !Equals(Token, default))
            {
                return true;
            }
            return false;
        }

        public Raffle<T> Refill(string Seed = null)
        {
            for (int i = 0; i < Length; i++)
            {
                int drawnWeight = DrawnEntries[i];

                ActiveEntries[i] += drawnWeight;
                TotalActiveWeights += drawnWeight;

                DrawnEntries[i] -= drawnWeight;
                TotalDrawnWeights -= drawnWeight;
            }
            if (!Seeded)
            {
                Shake();
            }
            else
            if (Seed != null)
            {
                SetSeed(Seed);
            }
            return this;
        }

        public static implicit operator Dictionary<T, int>(Raffle<T> Source)
        {
            return new(Source);
        }

        public static implicit operator Raffle<T>(Dictionary<T, int> Source)
        {
            return new((ICollection<KeyValuePair<T, int>>)Source);
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
                operand1.Add(token, weight);
            }
            return operand1;
        }
    }
}
