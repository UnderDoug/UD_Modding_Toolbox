using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL;
using XRL.Rules;
using XRL.World;

namespace UD_Modding_Toolbox
{
    [Serializable]
    public partial class Raffle<T> : IComposite
    {
        [Serializable]
        public class Entry : IComposite, IEquatable<Entry>, IEquatable<int>, IComparable, IComparable<Entry>, IComparable<int>
        {
            [NonSerialized]
            public T Token;

            [NonSerialized]
            public int Weight;

            public Entry(T Token, int Weight)
            {
                this.Token = Token;
                this.Weight = Weight;
            }

            public void Deconstruct(out T Token, out int Weight)
            {
                Token = this.Token;
                Weight = this.Weight;
            }

            public void Deconstruct(out T Token)
            {
                Token = this.Token;
            }

            public void Deconstruct(out int Weight)
            {
                Weight = this.Weight;
            }

            public void Write(SerializationWriter Writer)
            {
                Writer.WriteObject(new KeyValuePair<T, int>(Token, Weight));
            }

            public void Read(SerializationReader Reader)
            {
                var entry = (KeyValuePair<T, int>)Reader.ReadObject();
                Token = entry.Key;
                Weight = entry.Value;
            }

            public int CompareTo(object obj)
            {
                if (obj is Entry entry)
                {
                    return CompareTo(entry);
                }
                if (obj is int @int)
                {
                    return CompareTo(@int);
                }
                if (obj is string @string && int.TryParse(@string, out int parsedString))
                {
                    return CompareTo(parsedString);
                }
                if (obj is T token)
                {
                    return CompareTo(token);
                }
                return 0;
            }

            public int CompareTo(Entry other)
            {
                return Weight.CompareTo((int)other);
            }

            public int CompareTo(int other)
            {
                return Weight.CompareTo(other);
            }

            // Equivalency
            public override bool Equals(object obj)
            {
                if (obj is Entry entry)
                {
                    return Equals(entry);
                }
                if (obj is int @int)
                {
                    return Equals(@int);
                }
                if (obj is string @string && int.TryParse(@string, out int parsedString))
                {
                    return Equals(parsedString);
                }
                if (obj is T token)
                {
                    return Equals(token);
                }
                return base.Equals(obj);
            }
            public bool Equals(Entry other)
            {
                return Equals(Token, other.Token) && Equals((int)other);
            }
            public bool Equals(int other)
            {
                return Weight.Equals(other);
            }

            public override int GetHashCode()
            {
                int token = Token.GetHashCode();
                int weight = Weight.GetHashCode();
                return token ^ weight;
            }

            public static bool operator ==(Entry operand1, int operand2) => (int)operand1 == operand2;
            public static bool operator !=(Entry operand1, int operand2) => !((int)operand1 == operand2);
            public static bool operator ==(int operand1, Entry operand2) => (int)operand2 == operand1;
            public static bool operator !=(int operand1, Entry operand2) => !((int)operand2 == operand1);

            public static bool operator ==(Entry operand1, int operand2) => (int)operand1 == operand2;
            public static bool operator !=(Entry operand1, int operand2) => !((int)operand1 == operand2);
            public static bool operator ==(int operand1, Entry operand2) => (int)operand2 == operand1;
            public static bool operator !=(int operand1, Entry operand2) => !((int)operand2 == operand1);


            // Entry to/from KeyValuePair<T, int>
            public static implicit operator KeyValuePair<T, int>(Entry Entry)
            {
                return new(Entry.Token, Entry.Weight);
            }
            public static implicit operator Entry(KeyValuePair<T, int> Entry)
            {
                return new(Entry.Key, Entry.Value);
            }

            // T/int from Entry
            public static implicit operator T(Entry Entry)
            {
                return Entry.Token;
            }
            public static implicit operator int(Entry Entry)
            {
                return Entry.Weight;
            }


            // Entry +/- int
            public static Entry operator +(Entry operand1, int operand2)
            {
                operand1.Weight += operand2;
                return operand1;
            }
            public static Entry operator -(Entry operand1, int operand2) => operand1 + -operand2;

            // Entry +/- uint
            public static Entry operator +(Entry operand1, uint operand2) => operand1 + (int)operand2;
            public static Entry operator -(Entry operand1, uint operand2) => operand1 + -(int)operand2;

            // Entry +/- double
            public static Entry operator +(Entry operand1, double operand2) => operand1 + (int)operand2;
            public static Entry operator -(Entry operand1, double operand2) => operand1 + -(int)operand2;

            // Entry +/- float
            public static Entry operator +(Entry operand1, float operand2) => operand1 + (int)operand2;
            public static Entry operator -(Entry operand1, float operand2) => operand1 + -(int)operand2;

            // Entry +/- long
            public static Entry operator +(Entry operand1, long operand2) => operand1 + (int)operand2;
            public static Entry operator -(Entry operand1, long operand2) => operand1 + -(int)operand2;

            // Entry +/- Token
            public static Entry operator +(Entry operand1, T operand2)
            {
                if (Equals(operand1.Token, operand2))
                {
                    operand1.Weight++;
                }
                return operand1;
            }
            public static Entry operator -(Entry operand1, T operand2)
            {
                if (Equals(operand1.Token, operand2))
                {
                    operand1.Weight = Math.Max(0, --operand1.Weight);
                }
                return operand1;
            }

            // Entry +/- Entry
            public static int operator +(Entry operand1, Entry operand2) => operand1.Weight + operand2.Weight;
            public static int operator -(Entry operand1, Entry operand2) => operand1.Weight - operand2.Weight;
        }

        protected Entry[] Entries = Array.Empty<Entry>();

        protected T[] Tokens = Array.Empty<T>();

        protected int[] Weights = Array.Empty<int>();

        protected int TotalWeight;

        protected int Size;

        protected int Length;

        protected int Variant;

        public int Capacity => Size;

        public int Version => Variant;

        // public EntryEnumerator Entries => new(this);

        public TokenEnumerator GroupedTokens => new(this);

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        protected Queue<T> Bag;

        protected bool Active;

        protected List<Entry> ActiveEntries;

        protected List<T> ActiveTokens;

        protected List<int> ActiveWeights;

        protected int ActiveTotalWeight;

        protected Random Rnd;

        public Raffle()
        {
            Size = DefaultCapacity;
            Length = 0;
            Variant = 0;
            TotalWeight = 0;
            Bag = new();
            Active = false;
            ActiveEntries = new();
            ActiveTokens = new();
            ActiveWeights = new();
            ActiveTotalWeight = 0;
            Rnd = Utils.Rnd;
        }
        public Raffle(int Capacity)
            : this()
        {
            Size = Capacity;
        }
        public Raffle(Random Rnd)
            : this()
        {
            this.Rnd = Rnd;
        }
        public Raffle(Random Rnd, int Capacity)
            : this(Capacity)
        {
            this.Rnd = Rnd;
        }
        public Raffle(Dictionary<T, int> Source)
            : this()
        {
            if (Source == null)
            {
                throw new ArgumentNullException(nameof(Source));
            }
            EnsureCapacity(Source.Count);
            foreach ((T token, int weight) in Source)
            {
                Add(token, weight);
            }
        }
        public Raffle(Random Rnd, Dictionary<T, int> Source)
            : this(Source)
        {
            this.Rnd = Rnd;
        }

        public void EnsureCapacity(int Capacity)
        {
            if (Size < Capacity)
            {
                Resize(Capacity);
            }
        }

        protected void Resize(int Capacity)
        {
            if (Capacity == 0)
            {
                Capacity = DefaultCapacity;
            }
            T[] tokens = new T[Capacity];
            int[] weights = new int[Capacity];
            Array.Copy(Tokens, tokens, Length);
            Array.Copy(Weights, weights, Length);
            Tokens = tokens;
            Weights = weights;
            Size = Capacity;
        }

        // Weights
        public int GetWeight(T Token)
        {
            return this[Token];
        }
        public int GetWeight(int Index)
        {
            return GetWeight(this[Index]);
        }

        //Active Weights
        public int GetActiveWeight(T Token)
        {
            if (ActiveTokens.Contains(Token))
            {
                return ActiveWeights[ActiveTokens.IndexOf(Token)];
            }
            return 0;
        }
        public int GetActiveWeight(int Index)
        {
            return GetActiveWeight(this[Index]);
        }

        // Chances
        public float GetChance(T Token)
        {
            return GetWeight(Token) / TotalWeight;
        }
        public float GetChance(int Index)
        {
            return GetChance(this[Index]);
        }
        public IEnumerable<float> GetChances()
        {
            foreach (T token in Tokens)
            {
                yield return GetChance(token);
            }
        }
        public List<float> GetChancesList()
        {
            return GetChances().ToList();
        }

        // Active Chances
        public float GetActiveChance(T Token)
        {
            return GetActiveWeight(Token) / ActiveTotalWeight;
        }
        public float GetActiveChance(int Index)
        {
            return GetActiveChance(this[Index]);
        }
        public IEnumerable<float> GetActiveChances()
        {
            foreach (T token in ActiveTokens)
            {
                yield return GetActiveChance(token);
            }
        }
        public List<float> GetActiveChancesList()
        {
            return GetActiveChances().ToList();
        }

        public IEnumerable<KeyValuePair<T, int>> FilterEntries(Predicate<KeyValuePair<T, int>> Filter)
        {
            foreach (KeyValuePair<T, int> kv in this)
            {
                if ((Filter == null || Filter(kv)))
                {
                    yield return kv;
                }
            }
        }
        public IEnumerable<KeyValuePair<T, int>> FilterEntries(Predicate<T> Filter)
        {
            foreach ((T token, int weight) in this)
            {
                if ((Filter == null || Filter(token)))
                {
                    yield return new KeyValuePair<T, int>(token, weight);
                }
            }
        }

        public IEnumerable<T> FilterGroupedTokens(Predicate<T> Filter)
        {
            foreach (T token in GroupedTokens)
            {
                if (Filter != null && !Filter(token))
                {
                    continue;
                }
                yield return token;
            }
        }
    }
}
