using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Types;

public readonly struct Num : IEquatable<Num>
{
    private readonly float _value;
    private const float Tolerance = 1e-6f;

    private Num(float value) { _value = value; }

    public static Num Abs(Num a) { return new Num(Math.Abs(a)); }

    public static Num Min(Num a, Num b) { return new Num(Math.Min(a, b)); }

    public static Num Max(Num a, Num b) { return new Num(Math.Max(a, b)); }

    public static Num Cos(Num value) { return new Num((float)Math.Cos(value)); }

    public static Num Sin(Num value) { return new Num((float)Math.Sin(value)); }

    public static Num Clamp(Num value, Num min, Num max) { return Min(Max(value, min), max); }

    // Generic conversion method for numeric types
    private static Num FromT<T>(T value) where T : IConvertible { return new Num(Convert.ToSingle(value)); }

    // Add null check for FromT
    private static Num? FromT<T>(T? value) where T : struct, IConvertible
    {
        return value == null ? null : new Num(Convert.ToSingle(value));
    }

    public static Num N(Num value) { return new Num(value); }

    public Num Radians() { return new Num(_value * (Num)Math.PI / 180); }

    public Num Degrees() { return new Num(_value * 180 / (Num)Math.PI); }

    // Implicit conversions
    public static implicit operator Num(byte value) { return FromT(value); }

    public static implicit operator Num(int value) { return FromT(value); }

    public static implicit operator Num(float value) { return FromT(value); }

    public static implicit operator Num(double value) { return FromT(value); }

    public static implicit operator Num(decimal value) { return FromT(value); }

    // Null-safe implicit conversions
    public static implicit operator Num?(float? value) { return value.HasValue ? new Num(value.Value) : null; }

    public static implicit operator Num?(int? value) { return value.HasValue ? new Num(value.Value) : null; }

    // Conversions to other types
    public static implicit operator float(Num value) { return value._value; }

    public static implicit operator double(Num value) { return value._value; }

    public static implicit operator int(Num value) { return (int)value._value; }

    public static implicit operator byte(Num value) { return (byte)value._value; }

    public static implicit operator decimal(Num value) { return (decimal)value._value; }

    // Basic arithmetic operators
    public static Num operator +(Num a, Num b) { return new Num(a._value + b._value); }

    public static Num operator -(Num a, Num b) { return new Num(a._value - b._value); }

    public static Num operator *(Num a, Num b) { return new Num(a._value * b._value); }

    public static Num operator /(Num a, Num b) { return new Num(a._value / b._value); }

    public static Num operator %(Num a, Num b) { return new Num(a._value % b._value); }

    public static Num operator -(Num a) { return new Num(-a._value); }

    public static Num operator +(Num a) { return a; }

    public static Num operator ++(Num a) { return new Num(a._value + 1); }

    public static Num operator --(Num a) { return new Num(a._value - 1); }

    // Null-safe arithmetic operators
    public static Num? operator +(Num? a, Num? b)
    {
        return a.HasValue && b.HasValue ? new Num(a.Value._value + b.Value._value) : null;
    }

    public static Num? operator -(Num? a, Num? b)
    {
        return a.HasValue && b.HasValue ? new Num(a.Value._value - b.Value._value) : null;
    }

    public static Num? operator *(Num? a, Num? b)
    {
        return a.HasValue && b.HasValue ? new Num(a.Value._value * b.Value._value) : null;
    }

    public static Num? operator /(Num? a, Num? b)
    {
        return a.HasValue && b.HasValue ? new Num(a.Value._value / b.Value._value) : null;
    }

    // Comparison operators
    public static bool operator ==(Num a, Num b) { return Math.Abs(a._value - b._value) < Tolerance; }


    public static bool operator !=(Num a, Num b) { return Math.Abs(a._value - b._value) > Tolerance; }

    public static bool operator >(Num a, Num b) { return a._value > b._value; }

    public static bool operator <(Num a, Num b) { return a._value < b._value; }

    public static bool operator >=(Num a, Num b) { return a._value >= b._value; }

    public static bool operator <=(Num a, Num b) { return a._value <= b._value; }

    // Null-safe comparison operators
    public static bool operator ==(Num? a, Num? b)
    {
        return (!a.HasValue && !b.HasValue) || (a.HasValue && b.HasValue && Math.Abs(a.Value._value - b.Value._value) < Tolerance);
    }

    public static bool operator !=(Num? a, Num? b) { return !(a == b); }

    public static bool operator >(Num? a, Num? b)
    {
        return a.HasValue && b.HasValue && a.Value._value > b.Value._value;
    }

    public static bool operator <(Num? a, Num? b)
    {
        return a.HasValue && b.HasValue && a.Value._value < b.Value._value;
    }

    // Logical operators
    public static bool operator true(Num a) { return a._value != 0; }

    public static bool operator false(Num a) { return a._value == 0; }

    public static Num operator !(Num a) { return new Num(a._value == 0 ? 1 : 0); }

    // Bitwise operators
    public static Num operator &(Num a, Num b) { return new Num((int)a._value & (int)b._value); }

    public static Num operator |(Num a, Num b) { return new Num((int)a._value | (int)b._value); }

    public static Num operator ^(Num a, Num b) { return new Num((int)a._value ^ (int)b._value); }

    public static Num operator ~(Num a) { return new Num(~(int)a._value); }

    public static Num operator <<(Num a, int b) { return new Num((int)a._value << b); }

    public static Num operator >> (Num a, int b) { return new Num((int)a._value >> b); }

    // Equality implementation
    public bool Equals(Num other) { return Math.Abs(_value - other._value) < Tolerance; }

    public override bool Equals([NotNullWhen(true)] object obj) { return obj is Num other && Equals(other); }

    public override int GetHashCode() { return _value.GetHashCode(); }

    public override string ToString() { return _value.ToString(CultureInfo.CurrentCulture); }
}