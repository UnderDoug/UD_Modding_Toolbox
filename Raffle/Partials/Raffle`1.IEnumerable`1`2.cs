using System;
using System.Collections;
using System.Collections.Generic;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> 
        : IEnumerable<Raffle<T>.Entry>
        , IEnumerable<T>
        , IEnumerable<KeyValuePair<T, int>>
    {
        [Serializable]
        public class TicketEnumerator : IEnumerable<T>
        {
            [Serializable]
            public struct Enumerator
                : IEnumerator<T>
                , IEnumerator
                , IDisposable
            {
                private Raffle<T> Raffle;

                private readonly Entry[] Entries;

                private readonly int Version;

                private int Index;

                private int Weight;

                public readonly T Current => Entries[Index];
                readonly object IEnumerator.Current => Current;

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
                    int indent = Debug.LastIndent;
                    bool doDebug = false;
                    Debug.Entry(4,
                        nameof(MoveNext) + ", " +
                        nameof(Index) + ": " + Index + ", " +
                        nameof(Weight) + ": " + Weight,
                        Indent: indent + 1, Toggle: doDebug);
                    while (Weight > -1 || ++Index < Entries.Length)
                    {
                        if (++Weight < Entries[Index])
                        {
                            Debug.CheckYeh(4, Current.ToString(), Indent: indent + 2, Toggle: doDebug);
                            Debug.LastIndent = indent;
                            return true;
                        }
                        Debug.CheckNah(4, Current.ToString(), Indent: indent + 2, Toggle: doDebug);
                        Weight = -1;
                    }
                    Debug.LastIndent = indent;
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

            public TicketEnumerator(Raffle<T> Raffle, Entry[] Entries)
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
            private Raffle<T> Raffle;

            private readonly int Version;

            private int Index;

            public readonly Entry Current => new(Raffle.ActiveEntries[Index].Ticket, Raffle.ActiveEntries[Index].Weight);

            readonly T IEnumerator<T>.Current => Current;

            readonly KeyValuePair<T, int> IEnumerator<KeyValuePair<T, int>>.Current => Current;

            readonly object IEnumerator.Current => Current;

            readonly DictionaryEntry IDictionaryEnumerator.Entry => new(Current.Ticket, Current.Weight);

            readonly object IDictionaryEnumerator.Key => Current.Ticket;

            readonly object IDictionaryEnumerator.Value => Current.Weight;

            public Enumerator(Raffle<T> Raffle)
            {
                this.Raffle = Raffle;
                Version = Raffle.Version;
                Index = -1;
            }

            public bool MoveNext()
            {
                if (Version != Raffle.Version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                while (++Index < Raffle.Length)
                {
                    if (Raffle[Index] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

            public void Dispose()
            {
                Raffle = null;
            }

            void IEnumerator.Reset()
            {
                if (Version != Raffle.Version)
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
