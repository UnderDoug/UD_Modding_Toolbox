using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.Collections;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IEnumerable<T>
    {
        [Serializable]
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private Raffle<T> Bag;

            private int Version;

            private int Index;
            public T Current => Bag.Entries[Index].Token;

            private int Weight;

            object IEnumerator.Current => Current;

            public Enumerator(Raffle<T> Bag)
            {
                this.Bag = Bag;
                Version = Bag.Version;
                Index = -1;
                Weight = -1;
            }

            public Enumerator GetEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                if (Version != Bag.Version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                while (++Index < Bag.Length)
                {
                    if (Bag.Entries[Index].Token != null)
                    {
                        while (++Weight < Bag.Entries[Index].Weight)
                        {
                            return true;
                        }
                        Weight = -1;
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
                Weight = -1;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
