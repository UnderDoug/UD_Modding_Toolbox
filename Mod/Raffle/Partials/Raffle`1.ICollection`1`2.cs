using System;
using System.Collections.Generic;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> 
        : ICollection<T>
        , ICollection<KeyValuePair<T, int>>
    {
        public void Clear(string Seed)
        {
            Clear();
            SetSeed(Seed);
        }
        public void Clear()
        {
            Array.Clear(ActiveEntries, 0, Count);
            Array.Clear(DrawnEntries, 0, Count);
            TotalWeights = 0;
            TotalActiveWeights = 0;
            TotalDrawnWeights = 0;
            Size = DefaultCapacity;
            Seed = null;
            Shake();
            Length = 0;
            Variant = 0;
        }

        public void Add(KeyValuePair<T, int> Entry)
        {
            Add(Entry.Key, Entry.Value);
        }

        public bool Remove(KeyValuePair<T, int> Entry)
        {
            if (Contains(Entry))
            {
                return Remove(Entry.Key);
            }
            return false;
        }

        public bool Contains(KeyValuePair<T, int> Entry)
        {
            return Contains(Entry.Key)
                && this[Entry.Key] == Entry.Value;
        }

        public static implicit operator List<KeyValuePair<T, int>>(Raffle<T> Source)
        {
            if (Source == null)
            {
                return null;
            }
            List<KeyValuePair<T, int>> collection = new();
            for (int i = 0; i < Source.Count; i++)
            {
                Entry entry = new(Source[i], (int)Source.ActiveEntries[i] + (int)Source.DrawnEntries[i]);
                collection.Add(entry);
            }
            return collection;
        }

        public static implicit operator Raffle<T>(List<KeyValuePair<T, int>> Source)
        {
            if (Source == null)
            {
                return null;
            }
            Raffle<T> collection = new();
            for (int i = 0; i < Source.Count; i++)
            {
                collection.Add(Source[i].Key, Source[i].Value);
            }
            return collection;
        }
    }
}
