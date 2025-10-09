using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.World;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> 
        : ICollection<T>
        , ICollection<KeyValuePair<T, int>>
    {
        public void Clear()
        {
            Array.Clear(ActiveEntries, 0, Count);
            Array.Clear(DrawnEntries, 0, Count);
            TotalWeights = 0;
            TotalActiveWeights = 0;
            TotalDrawnWeights = 0;
            Rnd = null;
            Size = DefaultCapacity;
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
            return Contains(Entry.Key) && this[Entry.Key] == Entry.Value;
        }
    }
}
