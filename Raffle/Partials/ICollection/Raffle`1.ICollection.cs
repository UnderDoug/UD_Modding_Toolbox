using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : ICollection
    {
        public int Count => Tokens.Length;

        object ICollection.SyncRoot => this;

        bool ICollection.IsSynchronized => false;

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
    }
}
