using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IList
    {
        object IList.this[int Index]
        { 
            get => this[Index]; 
            set => this[Index] = (T)value;
        }

        public bool IsFixedSize => Tokens.IsFixedSize;

        public int Add(object value)
        {
            return ((IList)Tokens).Add(value);
        }

        public bool Contains(object value)
        {
            return ((IList)Tokens).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)Tokens).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)Tokens).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)Tokens).Remove(value);
        }
    }
}
