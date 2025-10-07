using AiUnity.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IDictionary<T, int>
    {
        public int this[T Token]
        {
            get => Weights[IndexOf(Token)];
            set => Weights[IndexOf(Token)] = value;
        }

        ICollection<T> IDictionary<T, int>.Keys => Tokens;
        ICollection<int> IDictionary<T, int>.Values => Weights;

        public void Add(T Token, int Weight)
        {
            if (Tokens.Contains(Token))
            {
                this[Token] += Weight;
            }
            else
            {
                EnsureCapacity(Length + 1);
                Tokens[Length] = Token;
                Weights[Length] = Weight;
            }
        }

        void ICollection<KeyValuePair<T, int>>.Add(KeyValuePair<T, int> Entry)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<T, int>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<T, int>>.Contains(KeyValuePair<T, int> item)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<T, int>.ContainsKey(T key)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<T, int>>.CopyTo(KeyValuePair<T, int>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<T, int>.Remove(T key)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<T, int>>.Remove(KeyValuePair<T, int> item)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<T, int>.TryGetValue(T key, out int value)
        {
            throw new NotImplementedException();
        }
    }
}
