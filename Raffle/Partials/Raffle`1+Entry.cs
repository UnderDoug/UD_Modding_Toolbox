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
    public partial class Raffle<T>
    {
        [Serializable]
        public class Entry
            : IComposite
            , IEquatable<Entry>
            , IEquatable<int>
            , IComparable
            , IComparable<Entry>
            , IComparable<int>
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

            public Entry(Entry Source)
                : this(Source, Source)
            {
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
                Writer.WriteObject(Token);
                Writer.WriteOptimized(Weight);
            }

            public void Read(SerializationReader Reader)
            {
                Token = (T)Reader.ReadObject();
                Weight = Reader.ReadOptimizedInt32();
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

            public static bool operator ==(Entry operand1, T operand2) => operand1.Equals(operand2);
            public static bool operator !=(Entry operand1, T operand2) => !(operand1.Equals(operand2));
            public static bool operator ==(T operand1, Entry operand2) => operand2.Equals(operand1);
            public static bool operator !=(T operand1, Entry operand2) => operand2 != operand1;

            // Comparison
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

            public static bool operator <(Entry operand1, int operand2) => (int)operand1 < operand2;
            public static bool operator >(Entry operand1, int operand2) => (int)operand1 > operand2;
            public static bool operator <=(Entry operand1, int operand2) => (int)operand1 <= operand2;
            public static bool operator >=(Entry operand1, int operand2) => (int)operand1 >= operand2;

            public static bool operator <(int operand1, Entry operand2) => operand1 < (int)operand2;
            public static bool operator >(int operand1, Entry operand2) => operand1 > (int)operand2;
            public static bool operator <=(int operand1, Entry operand2) => operand1 <= (int)operand2;
            public static bool operator >=(int operand1, Entry operand2) => operand1 >= (int)operand2;

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

            // Entry from T/int
            public static implicit operator Entry(T Token)
            {
                return new(Token, 1);
            }
            public static implicit operator Entry(int Weight)
            {
                return new(default, Weight);
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

            // int +/- Entry
            public static int operator +(int operand1, Entry operand2)
            {
                operand1 += operand2.Weight;
                return operand1;
            }
            public static int operator -(int operand1, Entry operand2) =>  operand1 + -operand2;

            // uint +/- Entry
            public static uint operator +(uint operand1, Entry operand2) => (uint)(operand1 + (int)operand2);
            public static uint operator -(uint operand1, Entry operand2) => (uint)(operand1 + -(int)operand2);

            // double +/- Entry
            public static double operator +(double operand1, Entry operand2) => (double)(operand1 + (int)operand2);
            public static double operator -(double operand1, Entry operand2) => (double)(operand1 + -(int)operand2);

            // float +/- Entry
            public static float operator +(float operand1, Entry operand2) => (float)(operand1 + (int)operand2);
            public static float operator -(float operand1, Entry operand2) => (float)(operand1 + -(int)operand2);

            // long +/- Entry
            public static long operator +(long operand1, Entry operand2) => operand1 + (int)operand2;
            public static long operator -(long operand1, Entry operand2) => operand1 + -(int)operand2;

            // Token +/- Entry
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
            public static int operator +(Entry operand1, Entry operand2)
            {
                if (Equals(operand1.Token, default))
                {
                    return operand2.Weight + operand1.Weight;
                }
                else
                if (Equals(operand2.Token, default))
                {
                    return operand1.Weight + operand2.Weight;
                }
                return default;
            }
            public static int operator -(Entry operand1, Entry operand2)
            {
                if (Equals(operand1.Token, default))
                {
                    return operand2.Weight - operand1.Weight;
                }
                else
                if (Equals(operand2.Token, default))
                {
                    return operand1.Weight - operand2.Weight;
                }
                return default;
            }
        }
    }
}
