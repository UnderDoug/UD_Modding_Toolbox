using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IList<T>
    {
        public T this[int Index]
        {
            get => Tokens[Index];
            set
            {
                if (!Equals(value, null))
                {
                    Tokens[Index] = value;
                    if (Weights.Length <= Tokens.Length)
                    {
                        Weights[Index] = 1;
                    }
                }
                else
                {
                    RemoveAt(Index);
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(T Token)
        {
            if (Tokens.Contains(Token))
            {
                this[Token]++;
            }
            else
            {
                Add(Token, 1);
            }
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)Tokens).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)Tokens).CopyTo(array, arrayIndex);
        }

        public int IndexOf(T Token)
        {
            for (int i = 0; i < Length; i++)
            {
                if (Equals(Tokens[i], Token))
                {
                    return i;
                }
            }
            return -1;
        }

        void IList<T>.Insert(int Index, T Token)
        {
            throw new NotImplementedException(
                "The order of the " + nameof(Tokens) + " in a " + nameof(Raffle<T>) + " is inconsequential." +
                "Consider a Dictionary<" + typeof(T).Name + ", int> or List<" + typeof(T).Name + ">.");
        }

        public bool Remove(T Token)
        {
            if (Contains(Token))
            {
                RemoveAt(IndexOf(Token));
                return true;
            }
            return false;
        }

        public void RemoveAt(int Index)
        {
            T[] tokens = new T[Count - 1];
            int[] weights = new int[Count - 1];
            int counter = 0;
            for (int i = 0; i < Count; i++)
            {
                if (i == Index)
                {
                    continue;
                }
                tokens[counter++] = Tokens[i];
                weights[counter] = Weights[i];
            }
            Tokens = tokens;
            Weights = weights;
        }
    }
}
