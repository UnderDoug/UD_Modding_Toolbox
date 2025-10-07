using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : ICollection
    {
        public int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Length; i ++)
                {
                    count += Entries[i].Weight;
                }
                return count;
            }
        }

        object ICollection.SyncRoot => this;

        bool ICollection.IsSynchronized => false;

        public void CopyTo(Array array, int index)
        {
            List<T> list = new();
            int counter = 0;
            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < Entries[i].Weight; j++)
                {
                    list[counter++] = Entries[i].Token;
                }
            }
            throw new NotImplementedException();
        }
    }
}
