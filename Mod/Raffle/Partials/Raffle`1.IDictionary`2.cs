using System;
using System.Collections.Generic;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IDictionary<T, int>
    {
        public int this[T Ticket]
        {
            get
            {
                int index = IndexOf(Ticket);
                if (index >= 0)
                {
                    return (int)ActiveEntries[index];
                }
                return index;
            }
            set
            {
                if (IndexOf(Ticket) is int index
                    && index >= 0)
                {
                    ActiveEntries[index].Weight = Math.Max(0, value);
                }
                else
                {
                    Add(Ticket, value);
                }
            }
        }

        ICollection<T> IDictionary<T, int>.Keys => GetEnumerator() as ICollection<T>;
        ICollection<int> IDictionary<T, int>.Values => GetEnumerator() as ICollection<int>;

        public void Add(T Ticket, int Weight)
        {
            if (IndexOf(Ticket) is int index && index >= 0)
            {
                if (Weight < 0 && ActiveEntries[index].Weight <= Math.Abs(Weight))
                {
                    ActiveEntries[index].Weight = 0;
                }
                else
                {
                    ActiveEntries[index].Weight += Weight;
                }
            }
            else
            {
                index = Length++;
                EnsureCapacity(Length);
                ActiveEntries[index] = new(Ticket, Math.Max(0, Weight));
                DrawnEntries[index] = new(Ticket, 0);
                Variant++;
            }
            SyncWeightTotals();
        }

        public void AddRange(Raffle<T> Raffle)
        {
            foreach (Entry entry in Raffle)
            {
                Add((T)entry, (int)entry);
            }
        }
        public void AddRange(IEnumerable<KeyValuePair<T, int>> Entries)
        {
            AddRange((Raffle<T>)Entries);
        }
        public void AddRange(Dictionary<T, int> Entries)
        {
            AddRange((IEnumerable<KeyValuePair<T, int>>)Entries);
        }

        public void AddRange(IEnumerable<T> Tickets)
        {
            foreach (T ticket in Tickets)
            {
                Add(ticket);
            }
        }

        public bool ContainsKey(T Ticket)
        {
            return Contains(Ticket);
        }

        public void CopyTo(KeyValuePair<T, int>[] Array, int Index)
        {
            System.Array.Copy(ActiveEntries, 0, Array, Index, Length);
        }

        public bool TryGetValue(T Ticket, out int Weight)
        {
            Weight = 0;
            bool anyWeight = false;
            if (TryGetActiveValue(Ticket, out int activeWeight))
            {
                Weight += activeWeight;
                anyWeight =  true;
            }
            if (TryGetDrawnValue(Ticket, out int drawnWeight))
            {
                Weight += drawnWeight;
                anyWeight = true;
            }
            return anyWeight;
        }
        public bool TryGetActiveValue(T Ticket, out int Weight)
        {
            Weight = 0;
            int index = IndexOf(Ticket);
            if (index > -1)
            {
                Weight = (int)ActiveEntries[index];
                return true;
            }
            return false;
        }
        public bool TryGetDrawnValue(T Ticket, out int Weight)
        {
            Weight = 0;
            int index = IndexOf(Ticket);
            if (index > -1)
            {
                Weight = (int)DrawnEntries[index];
                return true;
            }
            return false;
        }

        public static implicit operator Dictionary<T, int>(Raffle<T> Source)
        {
            if (Source == null)
            {
                return null;
            }
            Dictionary<T, int> dictionary = new(Source.Count);
            for (int i = 0; i < Source.Count; i++)
            {
                dictionary.Add(Source[i], (int)Source.ActiveEntries[i] + (int)Source.DrawnEntries[i]);
            }
            return dictionary;
        }

        public static implicit operator Raffle<T>(Dictionary<T, int> Source)
        {
            if (Source == null)
            {
                return null;
            }
            Raffle<T> raffle = new(Source.Count);
            foreach ((T key, int value) in Source)
            {
                raffle.Add(key, value);
            }
            return raffle;
        }
    }
}
