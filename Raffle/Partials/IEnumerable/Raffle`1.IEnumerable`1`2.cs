using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.Collections;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IEnumerable<KeyValuePair<T, int>>
    {
        [Serializable]
        public struct KVPEnumerator : IEnumerator<KeyValuePair<T, int>>, IEnumerator, IDisposable, IDictionaryEnumerator
        {
            private Raffle<T> Bag;

            private int Version;

            private int Index;

            public KeyValuePair<T, int> Current => new(Bag[Index], Bag.Weights[Index]);

            object IEnumerator.Current => Current;

            DictionaryEntry IDictionaryEnumerator.Entry => new(Current.Key, Current.Value);

            object IDictionaryEnumerator.Key => Current.Key;

            object IDictionaryEnumerator.Value => Current.Value;

            public KVPEnumerator(Raffle<T> Bag)
            {
                this.Bag = Bag;
                Version = Bag.Version;
                Index = -1;
            }

            public bool MoveNext()
            {
                if (Version != Bag.Version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                while (++Index < Bag.Length)
                {
                    if (Bag[Index] is T token && Bag[token] > 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            public void Dispose()
            {
            }

            void IEnumerator.Reset()
            {
                if (Version != Bag.Version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                Index = -1;
            }
        }

        IEnumerator<KeyValuePair<T, int>> IEnumerable<KeyValuePair<T, int>>.GetEnumerator()
        {
            return new KVPEnumerator(this);
        }
    }
}
