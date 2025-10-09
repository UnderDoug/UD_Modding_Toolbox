using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Wintellect.PowerCollections;
using XRL.World;
using static XRL.World.Conversations.ConversationEvent;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T>
        : IList<T>
        , IReadOnlyList<T>
    {
        public int Count => Length;

        public T this[int Index]
        {
            get => ActiveEntries[Index];
            set
            {
                if (Index < 0 || Index >= Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (!Equals(value, default))
                {
                    ActiveEntries[Index].Token = value;
                    Variant++;
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
            return !(IndexOf(Token) < 0);
        }
        public bool ActiveContains(T Token)
        {
            return IndexOf(Token) is int index
                && index > -1
                && ActiveEntries[index] > 0;
        }
        public bool DrawnContains(T Token)
        {
            if (DrawnEntries.Length > 0)
            {
                foreach (Entry drawnEntry in ActiveEntries)
                {
                    if (Equals(drawnEntry, Token) && drawnEntry > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CopyTo(T[] Array, int Index)
        {
            System.Array.Copy(ActiveEntries, 0, Array, Index, Length);
        }

        public int IndexOf(T Token)
        {
            for (int i = 0; i < Length; i++)
            {
                if (Equals(ActiveEntries[i].Token, Token))
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
            int length = Length;
            if (Index < 0 || Index >= length)
            {
                throw new ArgumentOutOfRangeException();
            }
            int activeWeight = ActiveEntries[Index];
            int drawnWeight = DrawnEntries[Index];
            length = --Length;
            if (Index < length)
            {
                Array.Copy(ActiveEntries, Index + 1, ActiveEntries, Index, length - Index);
                Array.Copy(DrawnEntries, Index + 1, DrawnEntries, Index, length - Index);
            }
            ActiveEntries[length] = default;
            DrawnEntries[length] = default;
            TotalActiveWeights -= activeWeight;
            TotalDrawnWeights -= drawnWeight;
            TotalWeights = TotalActiveWeights + TotalDrawnWeights;
            Variant++;
        }
    }
}
