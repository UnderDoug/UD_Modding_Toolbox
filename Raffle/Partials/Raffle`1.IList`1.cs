using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Wintellect.PowerCollections;
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
                Variant++;
            }
        }

        public bool IsReadOnly => Active;

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
        public bool DrawnContains(T Token)
        {
            if (DrawnEntries.Length > 0)
            {
                foreach (T drawnToken in DrawnEntries)
                {
                    if (Equals(drawnToken, Token))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CopyTo(T[] Array, int Index)
        {
            System.Array.Copy(Entries, 0, Array, Index, Length);
        }

        static int IndexOf(Raffle<T> Bag, T Token)
        {
            for (int i = 0; i < Bag.Length; i++)
            {
                if (Equals(Bag.Entries[i].Token, Token))
                {
                    return i;
                }
            }
            return -1;
        }
        public int IndexOf(T Token)
        {
            return IndexOf(this, Token);
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

        static void RemoveAt(Raffle<T> Bag, int Index)
        {
            int length = Bag.Length;
            if (Index >= length)
            {
                throw new ArgumentOutOfRangeException();
            }
            int weight = Bag.Entries[Index];
            length = --Bag.Length;
            if (Index < length)
            {
                Array.Copy(Bag.Entries, Index + 1, Bag.Entries, Index, length - Index);
            }
            Bag.Entries[length] = default;
            Bag.TotalWeight -= weight;
            Bag.Variant++;
        }
        public void RemoveAt(int Index)
        {
            if (Active)
            {
                throw new InvalidOperationException("Can't add " + nameof(Entries) + " to raffle while draw is active.");
            }
            RemoveAt(this, Index);
        }
    }
}
