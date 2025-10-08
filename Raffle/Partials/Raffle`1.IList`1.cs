using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.World;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T>
        : IList<T>
        , IReadOnlyList<T>
    {
        public int Count => Length;

        public T this[int Index]
        {
            get => Entries[Index];
            set
            {
                if (!Equals(value, default))
                {
                    Entries[Index].Token = value;
                }
                else
                {
                    RemoveAt(Index);
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(T Token)
        {
            Add(Token, 1);
        }

        public bool Contains(T Token)
        {
            foreach (T token in this)
            {
                if (Equals(token, Token))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] Array, int Index)
        {
            System.Array.Copy(Entries, 0, Array, Index, Length);
        }

        public int IndexOf(T Token)
        {
            for (int i = 0; i < Length; i++)
            {
                if (Equals(Entries[i].Token, Token))
                {
                    return i;
                }
            }
            return -1;
        }

        void IList<T>.Insert(int Index, T Token)
        {
            throw new NotImplementedException(
                "The order of the " + nameof(Entry.Token) + " in a " + nameof(Raffle<T>) + " is inconsequential." +
                "Consider a Dictionary<" + typeof(T).Name + ", int> or List<" + typeof(T).Name + ">.");
        }

        public bool Remove(T Token)
        {
            if (Contains(Token))
            {
                RemoveAt(IndexOf(Token));
                return true;
            }
            return false;
        }

        public void RemoveAt(int Index)
        {
            if (Index >= Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            int weight = Entries[Index];
            Length--;
            if (Index < Length)
            {
                Array.Copy(Entries, Index + 1, Entries, Index, Length - Index);
            }
            Entries[Length] = default;
            TotalWeight -= weight;
            Variant++;
        }
    }
}
