using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using static XRL.World.Conversations.Expression;

namespace UD_Modding_Toolbox
{
    [Serializable]
    public partial class Raffle<T> : IComposite
    {
        protected Entry[] ActiveEntries = Array.Empty<Entry>();
        protected Entry[] DrawnEntries = Array.Empty<Entry>();

        protected int TotalWeights;
        protected int TotalActiveWeights;
        protected int TotalDrawnWeights;

        protected int Size;

        protected int Length;

        protected int Variant;

        public int Capacity => Size;

        public int Version => Variant;

        public TokenEnumerator GroupedActiveTokens => new(this, ActiveEntries);
        public TokenEnumerator GroupedDrawnTokens => new(this, DrawnEntries);

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        protected Random _Rnd;

        protected Random Rnd
        {
            get => _Rnd ??= Seeded ? Stat.GetSeededRandomGenerator(Seed) : Utils.Rnd;
            set => _Rnd = value; 
        }

        public string Seed;

        public bool Seeded => !Seed.IsNullOrEmpty();

        public Raffle()
        {
            Size = DefaultCapacity;
            Length = 0;
            Variant = 0;
            TotalWeights = 0;
            TotalDrawnWeights = 0;
            _Rnd = null;
            Seed = null;
        }
        public Raffle(int Capacity)
            : this()
        {
            Size = Capacity;
        }
        public Raffle(Random Rnd)
            : this()
        {
            this.Rnd = Rnd;
        }
        public Raffle(Random Rnd, int Capacity)
            : this(Capacity)
        {
            this.Rnd = Rnd;
        }
        public Raffle(Raffle<T> Source)
            : this()
        {
            if (Source == null)
            {
                throw new ArgumentNullException(nameof(Source));
            }
            Rnd = Source.Rnd;
            EnsureCapacity(Source.Count);
            foreach ((T token, int weight) in Source)
            {
                Add(token, weight);
            }
        }
        public Raffle(Random Rnd, Raffle<T> Source)
            : this(Source)
        {
            this.Rnd = Rnd;
        }
        public Raffle(ICollection<KeyValuePair<T, int>> Source)
            : this((Raffle<T>)Source)
        {
        }
        public Raffle(Random Rnd, ICollection<KeyValuePair<T, int>> Source)
            : this(Rnd, (Raffle<T>)Source)
        {
        }
        public Raffle(Dictionary<T, int> Source)
            : this((Raffle<T>)Source)
        {
        }
        public Raffle(Random Rnd, Dictionary<T, int> Source)
            : this(Rnd, (Raffle<T>)Source)
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
            Entry[] activeEntries = new Entry[Capacity];
            Entry[] drawnEntries = new Entry[Capacity];
            Array.Copy(ActiveEntries, activeEntries, Length);
            Array.Copy(DrawnEntries, drawnEntries, Length);
            Size = Capacity;
        }

        // Weights
        public int GetTotalWeight(T Token)
        {
            return GetActiveWeight(Token) + GetDrawnWeight(Token);
        }

        // Total Weights
        public int GetActiveWeight(T Token)
        {
            if (IndexOf(Token) is int index && index > -1)
            {
                return ActiveEntries[index];
            }
            return 0;
        }
        public int GetDrawnWeight(T Token)
        {
            if (IndexOf(Token) is int index && index > -1)
            {
                return DrawnEntries[index];
            }
            return 0;
        }

        // Chances
        public float GetTotalChance(T Token)
        {
            if (Contains(Token))
            {
                return GetTotalWeight(Token) / TotalWeights;
            }
            throw new ArgumentOutOfRangeException(nameof(Token), "Parameter not found in collection.");
        }
        public bool TryGetTotalChance(T Token, out float Chance)
        {
            Chance = 0;
            if (Contains(Token))
            {
                Chance = GetTotalChance(Token);
                return true;
            }
            return false;
        }

        public float GetActiveChance(T Token)
        {
            if (Contains(Token))
            {
                return GetActiveWeight(Token) / TotalActiveWeights;
            }
            throw new ArgumentOutOfRangeException(nameof(Token), "Parameter not found in collection.");
        }
        public bool TryGetActiveChance(T Token, out float Chance)
        {
            Chance = 0;
            if (Contains(Token))
            {
                Chance = GetActiveChance(Token);
                return true;
            }
            return false;
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

        int SeededNext()
        {
            throw new NotImplementedException();
        }

        T NextToken()
        {
            int draw = Next();
            int currentCombinedWeight = 0;
            for (int i = 0; i < Length; i++)
            {
                Entry entry = ActiveEntries[i];
                if (draw < (currentCombinedWeight += entry))
                {
                    return entry;
                }
            }
            throw new InvalidOperationException("Attempted to ");
        }

        public T DrawToken()
        {
            throw new NotImplementedException();
        }

        bool DrawToken(T Token)
        {
            throw new NotImplementedException();
        }

        protected static int GetWeight(Entry[] Entries, T Token)
        {
            for (int i = 0; i < Entries.Length; i++)
            {

            }
            return 0;
        }

        public static implicit operator Dictionary<T, int>(Raffle<T> Source)
        {
            return new(Source);
        }

        public static implicit operator Raffle<T>(Dictionary<T, int> Source)
        {
            return new((ICollection<KeyValuePair<T, int>>)Source);
        }
    }
}
