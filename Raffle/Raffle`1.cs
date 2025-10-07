using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.World;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IComposite
    {
        protected T[] Tokens = Array.Empty<T>();

        protected int[] Weights = Array.Empty<int>();

        protected int Size;

        protected int Length;

        protected int Variant;

        public int Capacity => Size;

        public int Version => Variant;

        public IEnumerable<KeyValuePair<T, int>> Entries => GetEntries();

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        public Raffle()
        {
        }

        public Raffle(Dictionary<T, int> Raffle)
            : this()
        {
            Tokens = Raffle.Keys.ToArray();
            Weights = Raffle.Values.ToArray();
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
            T[] tokens = new T[Capacity];
            int[] weights = new int[Capacity];
            Array.Copy(Tokens, tokens, Length);
            Array.Copy(Weights, weights, Length);
            Tokens = tokens;
            Weights = weights;
            Size = Capacity;
        }

        public IEnumerable<KeyValuePair<T, int>> GetEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            for (int i = 0; i < Count; i++)
            {
                var kv = new KeyValuePair<T, int>(Tokens[i], Weights[i]);
                if ((Filter == null || Filter(kv)))
                {
                    yield return kv;
                }
            }
        }

        public IEnumerable<KeyValuePair<T, int>> GetEntries(Predicate<T> Filter)
        {
            for (int i = 0; i < Count; i++)
            {
                if ((Filter == null || Filter(Tokens[i])))
                {
                    yield return new KeyValuePair<T, int>(Tokens[i], Weights[i]);
                }
            }
        }

        public IEnumerable<KeyValuePair<T, int>> GetEntries()
        {
            return GetEntries((Predicate<T>)null);
        }

        public IEnumerable<T> GetTokens(Predicate<T> Filter)
        {
            int index = -1;
            int weight = -1;
            while (this[++index] is T token)
            {
                if (Filter != null && !Filter(token))
                {
                    continue;
                }
                while (++weight < this[token])
                {
                    yield return token;
                }
                weight = -1;
            }
        }
    }
}
