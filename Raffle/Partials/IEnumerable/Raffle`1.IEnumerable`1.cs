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

            private int Weight;

            public T Current => Bag[Index];
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
                    if (Bag[Index] is T token)
                    {
                        while (++Weight < Bag[token])
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

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
