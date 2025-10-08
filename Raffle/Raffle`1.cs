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
        protected Entry[] Entries = Array.Empty<Entry>();

        protected int TotalWeight;

        protected int Size;

        protected int Length;

        protected int Variant;

        public int Capacity => Size;

        public int Version => Variant;

        public TokenEnumerator GroupedTokens => new(this);

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        protected bool Active;

        protected List<Entry> ActiveEntries;

        protected int ActiveTotalWeight;

        protected Random Rnd;

        public Raffle()
        {
            Size = DefaultCapacity;
            Length = 0;
            Variant = 0;
            TotalWeight = 0;
            Active = false;
            ActiveEntries = new();
            ActiveTotalWeight = 0;
            Rnd = Utils.Rnd;
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
        public Raffle(Dictionary<T, int> Source)
            : this()
        {
            if (Source == null)
            {
                throw new ArgumentNullException(nameof(Source));
            }
            EnsureCapacity(Source.Count);
            foreach ((T token, int weight) in Source)
            {
                Add(token, weight);
            }
        }
        public Raffle(Random Rnd, Dictionary<T, int> Source)
            : this(Source)
        {
            this.Rnd = Rnd;
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
            Entry[] entries = new Entry[Capacity];
            Array.Copy(Entries, entries, Length);
            Size = Capacity;
        }

        // Weights
        public int GetWeight(T Token)
        {
            return this[Token];
        }
        public int GetWeight(int Index)
        {
            return GetWeight(this[Index]);
        }

        //Active Weights
        public int GetActiveWeight(T Token)
        {
            if (ActiveEntries.Contains(Token))
            {
                return ActiveWeights[ActiveTokens.IndexOf(Token)];
            }
            return 0;
        }
        public int GetActiveWeight(int Index)
        {
            return GetActiveWeight(this[Index]);
        }

        // Chances
        public float GetChance(T Token)
        {
            return GetWeight(Token) / TotalWeight;
        }
        public float GetChance(int Index)
        {
            return GetChance(this[Index]);
        }
        public IEnumerable<float> GetChances()
        {
            foreach (T token in this)
            {
                yield return GetChance(token);
            }
        }
        public List<float> GetChancesList()
        {
            return GetChances().ToList();
        }

        // Active Chances
        public float GetActiveChance(T Token)
        {
            return GetActiveWeight(Token) / ActiveTotalWeight;
        }
        public float GetActiveChance(int Index)
        {
            return GetActiveChance(this[Index]);
        }
        public IEnumerable<float> GetActiveChances()
        {
            foreach (T token in ActiveTokens)
            {
                yield return GetActiveChance(token);
            }
        }
        public List<float> GetActiveChancesList()
        {
            return GetActiveChances().ToList();
        }

        public IEnumerable<KeyValuePair<T, int>> FilterEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            foreach (KeyValuePair<T, int> kv in this)
            {
                if ((Filter == null || Filter(kv)))
                {
                    yield return kv;
                }
            }
        }
        public IEnumerable<KeyValuePair<T, int>> FilterEntries(Predicate<T> Filter)
        {
            foreach ((T token, int weight) in this)
            {
                if ((Filter == null || Filter(token)))
                {
                    yield return new KeyValuePair<T, int>(token, weight);
                }
            }
        }

        public IEnumerable<T> FilterGroupedTokens(Predicate<T> Filter)
        {
            foreach (T token in GroupedTokens)
            {
                if (Filter != null && !Filter(token))
                {
                    continue;
                }
                yield return token;
            }
        }
    }
}
