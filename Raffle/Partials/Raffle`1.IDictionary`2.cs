using System;
using System.Collections.Generic;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IDictionary<T, int>
    {
        public int this[T Token]
        {
            get
            {
                int index = IndexOf(Token);
                if (index >= 0)
                {
                    return (int)ActiveEntries[index];
                }
                return index;
            }
            set
            {
                if (IndexOf(Token) is int index
                    && index >= 0)
                {
                    ActiveEntries[index].Weight = Math.Max(0, value);
                }
                else
                {
                    Add(Token, value);
                }
            }
        }

        ICollection<T> IDictionary<T, int>.Keys => GetEnumerator() as ICollection<T>;
        ICollection<int> IDictionary<T, int>.Values => GetEnumerator() as ICollection<int>;

        public void Add(T Token, int Weight)
        {
            int indent = Debug.LastIndent;
            bool doDebug = false;
            Debug.Entry(4, nameof(Add), Indent: indent + 1, Toggle: doDebug);

            if (IndexOf(Token) is int index && index >= 0)
            {
                Debug.CheckYeh(4, "Contains " + typeof(T).Name, Token.ToString(), Indent: indent + 2, Toggle: doDebug);

                if (Weight < 0 && ActiveEntries[index].Weight <= Math.Abs(Weight))
                {
                    ActiveEntries[index].Weight = 0;
                }
                else
                {
                    ActiveEntries[index].Weight += Weight;
                }
            }
            else
            {
                Debug.CheckNah(4, "Doesn't contain " + typeof(T).Name, Token.ToString(), Indent: indent + 2, Toggle: doDebug);
                Debug.Entry(4, nameof(Length), Length.ToString(), Indent: indent + 2, Toggle: doDebug);
                index = Length++;
                EnsureCapacity(Length);
                ActiveEntries[index] = new(Token, Math.Max(0, Weight));
                DrawnEntries[index] = new(Token, 0);
            }
            SyncWeightTotals();
            Variant++;

            Debug.LastIndent = indent;
        }

        public bool ContainsKey(T Token)
        {
            return Contains(Token);
        }

        public void CopyTo(KeyValuePair<T, int>[] Array, int Index)
        {
            System.Array.Copy(ActiveEntries, 0, Array, Index, Length);
        }

        public bool TryGetValue(T Token, out int Weight)
        {
            Weight = 0;
            bool anyWeight = false;
            if (TryGetActiveValue(Token, out int activeWeight))
            {
                Weight += activeWeight;
                anyWeight =  true;
            }
            if (TryGetDrawnValue(Token, out int drawnWeight))
            {
                Weight += drawnWeight;
                anyWeight = true;
            }
            return anyWeight;
        }
        public bool TryGetActiveValue(T Token, out int Weight)
        {
            Weight = 0;
            int index = IndexOf(Token);
            if (index > -1)
            {
                Weight = (int)ActiveEntries[index];
                return true;
            }
            return false;
        }
        public bool TryGetDrawnValue(T Token, out int Weight)
        {
            Weight = 0;
            int index = IndexOf(Token);
            if (index > -1)
            {
                Weight = (int)DrawnEntries[index];
                return true;
            }
            return false;
        }

        public static implicit operator Dictionary<T, int>(Raffle<T> Source)
        {
            if (Source == null)
            {
                return null;
            }
            Dictionary<T, int> dictionary = new(Source.Count);
            for (int i = 0; i < Source.Count; i++)
            {
                dictionary.Add(Source[i], (int)Source.ActiveEntries[i] + (int)Source.DrawnEntries[i]);
            }
            return dictionary;
        }

        public static implicit operator Raffle<T>(Dictionary<T, int> Source)
        {
            if (Source == null)
            {
                return null;
            }
            Raffle<T> raffle = new(Source.Count);
            foreach ((T key, int value) in Source)
            {
                raffle.Add(key, value);
            }
            return raffle;
        }
    }
}
