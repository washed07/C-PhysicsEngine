﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Types
{
    public readonly struct Num : IEquatable<Num>
    {
        public readonly float value;

        public Num(float value) => this.value = value;

        public static Num Abs(Num a) => new(Math.Abs(a));

        public static Num Min(Num a, Num b) => new(Math.Min(a, b));

        public static Num Max(Num a, Num b) => new(Math.Max(a, b));

        public static Num Cos(Num value) => new((float)Math.Cos(value));

        public static Num Sin(Num value) => new((float)Math.Sin(value));

        public static Num Clamp(Num value, Num min, Num max) => Min(Max(value, min), max);

        // Generic conversion method for numeric types
        private static Num FromT<T>(T value) where T : IConvertible
            => new(Convert.ToSingle(value));

        // Add null check for FromT
        private static Num? FromT<T>(T? value) where T : struct, IConvertible =>
            value == null ? null : new Num(Convert.ToSingle(value));

        public static Num n(Num value) => new(value);

        public Num Radians() => new(value * (Num)Math.PI / 180);
        public Num Degrees() => new(value * 180 / (Num)Math.PI);

        // Implicit conversions
        public static implicit operator Num(byte value) => FromT(value);
        public static implicit operator Num(int value) => FromT(value);
        public static implicit operator Num(float value) => FromT(value);
        public static implicit operator Num(double value) => FromT(value);
        public static implicit operator Num(decimal value) => FromT(value);

        // Null-safe implicit conversions
        public static implicit operator Num?(float? value) => value.HasValue ? new Num(value.Value) : null;
        public static implicit operator Num?(int? value) => value.HasValue ? new Num(value.Value) : null;

        // Conversions to other types
        public static implicit operator float(Num value) => value.value;
        public static implicit operator double(Num value) => value.value;
        public static implicit operator int(Num value) => (int)value.value;
        public static implicit operator byte(Num value) => (byte)value.value;
        public static implicit operator decimal(Num value) => (decimal)value.value;

        // Basic arithmetic operators
        public static Num operator +(Num a, Num b) => new(a.value + b.value);
        public static Num operator -(Num a, Num b) => new(a.value - b.value);
        public static Num operator *(Num a, Num b) => new(a.value * b.value);
        public static Num operator /(Num a, Num b) => new(a.value / b.value);
        public static Num operator %(Num a, Num b) => new(a.value % b.value);
        public static Num operator -(Num a) => new(-a.value);
        public static Num operator +(Num a) => a;
        public static Num operator ++(Num a) => new(a.value + 1);
        public static Num operator --(Num a) => new(a.value - 1);

        // Null-safe arithmetic operators
        public static Num? operator +(Num? a, Num? b) => 
            a.HasValue && b.HasValue ? new Num(a.Value.value + b.Value.value) : null;
        
        public static Num? operator -(Num? a, Num? b) => 
            a.HasValue && b.HasValue ? new Num(a.Value.value - b.Value.value) : null;
        
        public static Num? operator *(Num? a, Num? b) => 
            a.HasValue && b.HasValue ? new Num(a.Value.value * b.Value.value) : null;
        
        public static Num? operator /(Num? a, Num? b) => 
            a.HasValue && b.HasValue ? new Num(a.Value.value / b.Value.value) : null;

        // Comparison operators
        public static bool operator ==(Num a, Num b) => a.value == b.value;
        public static bool operator !=(Num a, Num b) => a.value != b.value;
        public static bool operator >(Num a, Num b) => a.value > b.value;
        public static bool operator <(Num a, Num b) => a.value < b.value;
        public static bool operator >=(Num a, Num b) => a.value >= b.value;
        public static bool operator <=(Num a, Num b) => a.value <= b.value;

        // Null-safe comparison operators
        public static bool operator ==(Num? a, Num? b) =>
            (!a.HasValue && !b.HasValue) || (a.HasValue && b.HasValue && a.Value.value == b.Value.value);
        
        public static bool operator !=(Num? a, Num? b) => !(a == b);
    
        public static bool operator >(Num? a, Num? b) =>
            a.HasValue && b.HasValue && a.Value.value > b.Value.value;
        
        public static bool operator <(Num? a, Num? b) =>
            a.HasValue && b.HasValue && a.Value.value < b.Value.value;

        // Logical operators
        public static bool operator true(Num a) => a.value != 0;
        public static bool operator false(Num a) => a.value == 0;
        public static Num operator !(Num a) => new(a.value == 0 ? 1 : 0);

        // Bitwise operators
        public static Num operator &(Num a, Num b) => new((int)a.value & (int)b.value);
        public static Num operator |(Num a, Num b) => new((int)a.value | (int)b.value);
        public static Num operator ^(Num a, Num b) => new((int)a.value ^ (int)b.value);
        public static Num operator ~(Num a) => new(~(int)a.value);
        public static Num operator <<(Num a, int b) => new((int)a.value << b);
        public static Num operator >>(Num a, int b) => new((int)a.value >> b);

        // Equality implementation
        public bool Equals(Num other) => value == other.value;
        public override bool Equals([NotNullWhen(true)] object obj) => obj is Num other && Equals(other);
        public override int GetHashCode() => value.GetHashCode();
        public override string ToString() => value.ToString();
    }
}
