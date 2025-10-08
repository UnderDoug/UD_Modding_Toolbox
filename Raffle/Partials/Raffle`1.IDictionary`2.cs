using AiUnity.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.World;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IDictionary<T, int>
    {
        public int this[T Token]
        {
            get => Entries[IndexOf(Token)];
            set => Entries[IndexOf(Token)].Weight = value;
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
                Entries[Length] = new(Token, Weight);
            }
            TotalWeight += Weight;
            Variant++;
        }

        public bool ContainsKey(T Token)
        {
            return Contains(Token);
        }

        public void CopyTo(KeyValuePair<T, int>[] Array, int Index)
        {
            System.Array.Copy(Entries, 0, Array, Index, Length);
        }

        public bool TryGetValue(T Token, out int Weight)
        {
            Weight = 0;
            if (Contains(Token))
            {
                Weight = this[Token];
                return true;
            }
            return false;
        }
    }
}
