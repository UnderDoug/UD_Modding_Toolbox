using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.Collections;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> 
        : IEnumerable<Raffle<T>.Entry>
        , IEnumerable<T>
        , IEnumerable<KeyValuePair<T, int>>
    {
        [Serializable]
        public class TokenEnumerator : IEnumerable<T>
        {
            [Serializable]
            public struct Enumerator
                : IEnumerator<T>
                , IEnumerator
                , IDisposable
            {
                private Raffle<T> Raffle;

                private Entry[] Entries;

                private int Version;

                private int Index;

                private int Weight;

                public T Current => Entries[Index];
                object IEnumerator.Current => Current;

                public Enumerator(Raffle<T> Raffle, Entry[] Entries)
                {
                    this.Raffle = Raffle;
                    this.Entries = new Entry[Entries.Length];
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        this.Entries[i] = new(Entries[i]);
                    }
                    Version = Raffle.Version;
                    Index = -1;
                    Weight = -1;
                }

                public Enumerator GetEnumerator()
                {
                    return this;
                }

                public bool MoveNext()
                {
                    if (Version != Raffle.Version)
                    {
                        throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    }
                    while (++Index < Entries.Length)
                    {
                        if (Entries[Index] is T token)
                        {
                            while (++Weight < Entries[Raffle.IndexOf(token)])
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
                    Array.Clear(Entries, 0, Entries.Length);
                    Raffle = null;
                }

                void IEnumerator.Reset()
                {
                    if (Version != Raffle.Version)
                    {
                        throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    }
                    Index = -1;
                    Weight = -1;
                }
            }

            protected Raffle<T> Raffle;

            protected Entry[] Entries;

            public TokenEnumerator(Raffle<T> Raffle, Entry[] Entries)
            {
                this.Raffle = Raffle;
                this.Entries = new Entry[Entries.Length];
                for (int i = 0; i < Entries.Length; i++)
                {
                    this.Entries[i] = new(Entries[i]);
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator(Raffle, Entries);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [Serializable]
        public struct Enumerator
            : IEnumerator<Entry>
            , IEnumerator<T>
            , IEnumerator<KeyValuePair<T, int>>
            , IEnumerator
            , IDisposable
            , IDictionaryEnumerator
        {
            private Raffle<T> Bag;

            private int Version;

            private int Index;

            public readonly Entry Current => new(Bag.ActiveEntries[Index].Token, Bag.ActiveEntries[Index].Weight);

            readonly T IEnumerator<T>.Current => Current;

            KeyValuePair<T, int> IEnumerator<KeyValuePair<T, int>>.Current => Current;

            object IEnumerator.Current => Current;

            DictionaryEntry IDictionaryEnumerator.Entry => new(Current.Token, Current.Weight);

            object IDictionaryEnumerator.Key => Current.Token;

            object IDictionaryEnumerator.Value => Current.Weight;

            public Enumerator(Raffle<T> Bag)
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
                Bag = null;
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


        public IEnumerator<Entry> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<KeyValuePair<T, int>> IEnumerable<KeyValuePair<T, int>>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ActiveEntries.GetEnumerator();
        }
    }
}
