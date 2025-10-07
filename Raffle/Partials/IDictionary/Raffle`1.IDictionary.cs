using AiUnity.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IDictionary
    {
        object IDictionary.this[object key]
        { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException();
        }

        ICollection IDictionary.Keys => Tokens;

        ICollection IDictionary.Values => Weights;

        bool IDictionary.IsFixedSize => false;

        void IDictionary.Add(object Token, object Weight)
        {
            throw new NotImplementedException();
        }

        void IDictionary.Clear()
        {
            throw new NotImplementedException();
        }

        bool IDictionary.Contains(object Token)
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new KVPEnumerator(this);
        }

        void IDictionary.Remove(object Token)
        {
            throw new NotImplementedException();
        }
    }
}
