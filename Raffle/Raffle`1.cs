using System;
using System.Collections.Generic;
using System.Text;
using XRL.World;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IComposite
    {
        protected struct Entry
        {
            public T Token;

            public int Weight;
        }

        protected Entry[] Entries;

        public int Length => Entries.Length;
    }
}
