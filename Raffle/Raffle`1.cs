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
        protected Entry[] DrawnEntries = Array.Empty<Entry>();

        protected int TotalWeight;
        protected int TotalActiveWeight;
        protected int TotalDrawnWeight;

        protected int Size;

        protected int Length;

        protected int Variant;

        public int Capacity => Size;

        public int Version => Variant;

        public TokenEnumerator GroupedTokens => new(this);

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        protected bool Active;

        public bool IsActive => Active;

        protected Random _Rnd;

        protected Random Rnd
        {
            get => _Rnd ??= Utils.Rnd;
            set => _Rnd = value; 
        }

        public Raffle()
        {
            Size = DefaultCapacity;
            Length = 0;
            Variant = 0;
            TotalWeight = 0;
            Active = false;
            TotalDrawnWeight = 0;
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
            if (Contains(Token))
            {
                return Bag[Token];
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
            return GetActiveWeight(Token) / TotalDrawnWeight;
        }
        public float GetActiveChance(int Index)
        {
            return GetActiveChance(this[Index]);
        }
        public IEnumerable<float> GetActiveChances()
        {
            foreach (T token in Bag)
            {
                yield return GetActiveChance(token);
            }
        }
        public List<float> GetActiveChancesList()
        {
            return GetActiveChances().ToList();
        }

        public IEnumerable<KeyValuePair<T, int>> GetEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            foreach (KeyValuePair<T, int> kv in this)
            {
                if ((Filter == null || Filter(kv)))
                {
                    yield return kv;
                }
            }
        }
        public IEnumerable<KeyValuePair<T, int>> GetEntries(Predicate<T> Filter)
        {
            foreach ((T token, int weight) in this)
            {
                if ((Filter == null || Filter(token)))
                {
                    yield return new KeyValuePair<T, int>(token, weight);
                }
            }
        }

        public IEnumerable<T> GetGroupedTokens(Predicate<T> Filter = null)
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

        Raffle<T> BeginDraw(Random Rnd)
        {
            Active = true;
            this.Rnd = Rnd;
            Bag = new(Rnd, this);
            Variant++;
            Bag.Variant++;
            return Bag;
        }
        Raffle<T> EndDraw()
        {
            Active = false;
            Bag.Clear();
            Bag = null;
            Variant++;
            Bag.Variant++;
            return this;
        }

        int Next()
        {
            return Rnd.Next(Bag.ActiveTotalWeight);
        }

        T NextToken()
        {
            int draw = Next();
            int currentCombinedWeight = 0;
            foreach (Entry entry in Bag)
            {
                if (draw < (currentCombinedWeight += entry))
                {

                }
            }
            throw new InvalidOperationException("Attempted to ");
        }

        public T DrawToken()
        {
            BeginDraw(Rnd).
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
