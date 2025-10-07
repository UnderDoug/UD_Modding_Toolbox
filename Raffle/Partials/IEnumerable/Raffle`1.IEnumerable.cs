using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.Collections;

namespace UD_Modding_Toolbox
{
    public partial class Raffle<T> : IEnumerable
    {
        protected int Version;

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
