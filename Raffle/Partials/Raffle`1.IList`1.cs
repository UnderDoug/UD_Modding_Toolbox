using System;
using System.Collections.Generic;

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
                    throw new ArgumentOutOfRangeException(nameof(Index), "Parameter must be less than the " + nameof(Count) + " of the collection and must not be less than 0.");
                }
                if (!Equals(value, default) && !Equals(value, null))
                {
                    ActiveEntries[Index].Ticket = value;
                    DrawnEntries[Index].Ticket = value;
                    Variant++;
                }
                else
                {
                    RemoveAt(Index);
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(T Ticket)
        {
            Add(Ticket, 1);
        }

        public bool Contains(T Ticket)
        {
            return IndexOf(Ticket) > -1;
        }
        public bool ActiveContains(T Ticket)
        {
            return IndexOf(Ticket) is int index
                && index > -1
                && ActiveEntries[index] > 0;
        }
        public bool DrawnContains(T Ticket)
        {
            if (DrawnEntries.Length > 0)
            {
                foreach (Entry drawnEntry in ActiveEntries)
                {
                    if (Equals(drawnEntry, Ticket) && drawnEntry > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CopyTo(T[] Array, int Index)
        {
            List<T> activeTickets = new();
            for (int i = 0; i < Length; i++)
            {
                if (ActiveEntries[i] > 0)
                {
                    activeTickets.Add(ActiveEntries[i]);
                }
            }
            System.Array.Copy(activeTickets.ToArray(), 0, Array, Index, activeTickets.Count);
        }

        public int IndexOf(T Ticket)
        {
            int indent = Debug.LastIndent;
            Debug.Entry(4, nameof(IndexOf), Indent: indent + 1, Toggle: doDebug);
            for (int i = 0; i < Length; i++)
            {
                Debug.LoopItem(4, i.ToString(), Indent: indent + 2, Toggle: doDebug);
                if (Equals(ActiveEntries[i].Ticket, Ticket))
                {
                    Debug.LastIndent = indent;
                    return i;
                }
            }
            Debug.LastIndent = indent;
            return -1;
        }

        void IList<T>.Insert(int Index, T Ticket)
        {
            throw new NotImplementedException(
                "The order of the " + nameof(Entry.Ticket).Pluralize() + " in a " + nameof(Raffle<T>) + " is inconsequential." +
                "Consider a Dictionary<" + typeof(T).Name + ", int> or List<" + typeof(T).Name + ">.");
        }

        public bool Remove(T Ticket)
        {
            if (Contains(Ticket))
            {
                RemoveAt(IndexOf(Ticket));
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
            int activeWeight = (int)ActiveEntries[Index];
            int drawnWeight = (int)DrawnEntries[Index];
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

        public static explicit operator Raffle<T>(List<T> List)
        {
            if (List == null)
            {
                return null;
            }
            Raffle<T> raffle = new();
            for (int i = 0; i < List.Count; i++)
            {
                raffle.Add(List[i]);
            }
            return raffle;
        }
        public static explicit operator List<T>(Raffle<T> Raffle)
        {
            if (Raffle == null)
            {
                return null;
            }
            List<T> list = new();
            for (int i = 0; i < Raffle.Count; i++)
            {
                list.Add(Raffle[i]);
            }
            return list;
        }
    }
}
