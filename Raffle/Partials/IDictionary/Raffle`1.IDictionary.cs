using AiUnity.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IDictionary
    {
        object IDictionary.this[object key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        ICollection IDictionary.Keys => throw new NotImplementedException();

        ICollection IDictionary.Values => throw new NotImplementedException();

        bool IDictionary.IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        void IDictionary.Add(object Key, object Value)
        {
            throw new NotImplementedException();
        }

        void IDictionary.Clear()
        {
            throw new NotImplementedException();
        }

        bool IDictionary.Contains(object Key)
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new KVPEnumerator(this);
        }

        void IDictionary.Remove(object Token)
        {
            if (Token is T token)
            {
                int j = 0;
                Entry[] newEntries = new Entry[Length - 1];
                for (int i = 0; i < Length; i ++)
                {
                    if (Equals(token, Entries[i].Token))
                    {
                        continue;
                    }
                    newEntries[j++] = Entries[i];
                }
                Entries = newEntries;
            }
        }
    }
}
