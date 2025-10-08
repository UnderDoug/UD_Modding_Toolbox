using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.World;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : ICollection<T>, ICollection<KeyValuePair<T, int>>
    {
        public void Clear()
        {
            Array.Clear(Tokens, 0, Tokens.Length);
            Array.Clear(Weights, 0, Weights.Length);
            TotalWeight = 0;
            Bag = new();
            Rnd = Utils.Rnd;
            Size = DefaultCapacity;
            Length = 0;
            Variant = 0;
        }

        void ICollection<KeyValuePair<T, int>>.Add(KeyValuePair<T, int> Entry)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<T, int>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<T, int>>.Contains(KeyValuePair<T, int> item)
        {
            throw new NotImplementedException();
        }
    }
}
