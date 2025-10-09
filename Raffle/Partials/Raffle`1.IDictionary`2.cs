using AiUnity.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Wintellect.PowerCollections;
using XRL.World;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IDictionary<T, int>
    {
        public int this[T Token]
        {
            get => ActiveEntries[IndexOf(Token)];
            set
            {
                int cappedValue = Math.Max(0, value);
                if (Contains(Token))
                {
                    ActiveEntries[IndexOf(Token)].Weight = cappedValue;
                }
                else
                {
                    Add(Token, cappedValue);
                }
            }
        }

        ICollection<T> IDictionary<T, int>.Keys => GetEnumerator() as ICollection<T>;
        ICollection<int> IDictionary<T, int>.Values => GetEnumerator() as ICollection<int>;

        public void Add(T Token, int Weight)
        {
            if (Contains(Token))
            {
                this[Token] += Weight;
            }
            else
            {
                EnsureCapacity(Length + 1);
                ActiveEntries[Length] = new(Token, Weight);
                DrawnEntries[Length] = new(Token, 0);
            }
            TotalActiveWeights += Weight;
            TotalWeights += TotalActiveWeights;
            Variant++;
        }

        public bool ContainsKey(T Token)
        {
            return Contains(Token);
        }

        public void CopyTo(KeyValuePair<T, int>[] Array, int Index)
        {
            System.Array.Copy(ActiveEntries, 0, Array, Index, Length);
        }

        public bool TryGetValue(T Token, out int Weight)
        {
            Weight = 0;
            bool anyWeight = false;
            if (TryGetActiveValue(Token, out int activeWeight))
            {
                Weight += activeWeight;
                anyWeight =  true;
            }
            if (TryGetDrawnValue(Token, out int drawnWeight))
            {
                Weight += drawnWeight;
                anyWeight = true;
            }
            return anyWeight;
        }
        public bool TryGetActiveValue(T Token, out int Weight)
        {
            Weight = 0;
            int index = IndexOf(Token);
            if (index > -1)
            {
                Weight = ActiveEntries[index];
                return true;
            }
            return false;
        }
        public bool TryGetDrawnValue(T Token, out int Weight)
        {
            Weight = 0;
            int index = IndexOf(Token);
            if (index > -1)
            {
                Weight = DrawnEntries[index];
                return true;
            }
            return false;
        }
    }
}
