using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using XRL;
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
            public T Ticket;

            [NonSerialized]
            public int Weight;

            public Entry(T Ticket, int Weight)
            {
                this.Ticket = Ticket;
                this.Weight = Math.Max(0, Weight);
            }

            public Entry(Entry Source)
                : this(Source, (int)Source)
            {
            }

            public override string ToString()
            {
                return (Ticket.ExtendedToString() ?? nameof(Ticket)) + ":" + Weight;
            }

            public void Deconstruct(out T Ticket, out int Weight)
            {
                Ticket = (T)this;
                Weight = (int)this;
            }

            public void Deconstruct(out T Ticket)
            {
                Ticket = (T)this;
            }

            public void Deconstruct(out int Weight)
            {
                Weight = (int)this;
            }

            public void Write(SerializationWriter Writer)
            {
                Writer.WriteObject(Ticket);
                Writer.WriteOptimized(Weight);
            }

            public void Read(SerializationReader Reader)
            {
                Ticket = (T)Reader.ReadObject();
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
                if (obj is T ticket)
                {
                    return Equals(ticket);
                }
                return base.Equals(obj);
            }
            public bool Equals(Entry other)
            {
                return Equals(other, true);
            }
            public bool Equals(Entry other, bool MatchWeight)
            {
                return Equals((T)other) && (!MatchWeight || Equals((int)other));
            }
            public bool Equals(T other)
            {
                return Equals(Ticket, other);
            }
            public bool Equals(int other)
            {
                return Weight.Equals(other);
            }

            public override int GetHashCode()
            {
                int ticket = Ticket.GetHashCode();
                int weight = Weight.GetHashCode();
                return ticket ^ weight;
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
                if (obj is T ticket)
                {
                    return CompareTo(ticket);
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
            public static implicit operator KeyValuePair<T, int>(Entry Entry) => new(Entry.Ticket, Entry.Weight);
            public static implicit operator Entry(KeyValuePair<T, int> Entry) => new(Entry.Key, Entry.Value);

            // T/int from Entry
            public static implicit operator T(Entry Entry) => Entry.Ticket;
            public static explicit operator int(Entry Entry) => Entry.Weight;

            // Entry from T/int
            public static implicit operator Entry(T Ticket) => new(Ticket, 1);

            // Entry +/- int
            public static Entry operator +(Entry operand1, int operand2) => new(operand1, (int)operand1 + operand2);
            public static Entry operator -(Entry operand1, int operand2) => new(operand1, (int)operand1 - operand2);

            // Entry++
            // Entry--
            public static Entry operator ++(Entry operand1) => operand1 + 1;
            public static Entry operator --(Entry operand1) => operand1 - 1;

            // Ticket +/- Entry
            public static Entry operator +(Entry operand1, T operand2)
            {
                if (Equals((T)operand1, operand2))
                {
                    operand1++;
                }
                return operand1;
            }
            public static Entry operator -(Entry operand1, T operand2)
            {
                if (Equals((T)operand1, operand2) && operand1 >= 0)
                {
                    operand1--;
                }
                return operand1;
            }

            // Entry +/- Entry
            public static Entry operator +(Entry operand1, Entry operand2)
            {
                if (operand1.Equals((T)default))
                {
                    return operand2 + (int)operand1;
                }
                else
                if (operand2.Equals((T)default))
                {
                    return operand1 + (int)operand2;
                }
                return default;
            }
            public static Entry operator -(Entry operand1, Entry operand2)
            {
                if (operand1.Equals((T)default))
                {
                    return operand2 - (int)operand1;
                }
                else
                if (operand2.Equals((T)default))
                {
                    return operand1 - (int)operand2;
                }
                return default;
            }
        }
    }
}
