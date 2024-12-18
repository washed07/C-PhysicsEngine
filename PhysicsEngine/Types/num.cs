using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Types;

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
// ReSharper disable once InconsistentNaming
public readonly struct num : IEquatable<num>
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
    private readonly       float _value;
    public static readonly num   E         = new num((float)Math.E);
    public static readonly num   PI        = new num((float)Math.PI);
    private const          float Tolerance = 1e-6f;

    private num(float value) { _value = value; }

    public static num Abs(num a) { return new num(Math.Abs(a)); }

    public static num Min(num a, num b) { return new num(Math.Min(a, b)); }

    public static num Max(num a, num b) { return new num(Math.Max(a, b)); }

    public static num Cos(num value) { return new num((float)Math.Cos(value)); }

    public static num Sin(num value) { return new num((float)Math.Sin(value)); }

    public static num Sqrt(num value) { return new num((float)Math.Sqrt(value)); }

    public static num Clamp(num value, num min, num max) { return Min(Max(value, min), max); }

    // Generic conversion method for numeric types
    private static num FromT<T>(T value) where T : IConvertible { return new num(Convert.ToSingle(value)); }

    public num ToRadians() { return new num(_value * (num)Math.PI / 180); }

    // Add null check for FromT
    private static num? FromT<T>(T? value) where T : struct, IConvertible
    {
        return value == null ? null : new num(Convert.ToSingle(value));
    }

    public static num N(num value) { return new num(value); }

    public num Radians() { return new num(_value * (num)Math.PI / 180); }

    public num Degrees() { return new num(_value * 180 / (num)Math.PI); }

    // Implicit conversions
    public static implicit operator num(byte value) { return FromT(value); }

    public static implicit operator num(int value) { return FromT(value); }

    public static implicit operator num(float value) { return FromT(value); }

    public static implicit operator num(double value) { return FromT(value); }

    public static implicit operator num(decimal value) { return FromT(value); }

    // Null-safe implicit conversions
    public static implicit operator num?(float? value) { return value.HasValue ? new num(value.Value) : null; }

    public static implicit operator num?(int? value) { return value.HasValue ? new num(value.Value) : null; }

    // Conversions to other types
    public static implicit operator float(num value) { return value._value; }

    public static implicit operator double(num value) { return value._value; }

    public static implicit operator int(num value) { return (int)value._value; }

    public static implicit operator byte(num value) { return (byte)value._value; }

    public static implicit operator decimal(num value) { return (decimal)value._value; }

    // Basic arithmetic operators
    public static num operator +(num a, num b) { return new num(a._value + b._value); }

    public static num operator -(num a, num b) { return new num(a._value - b._value); }

    public static num operator *(num a, num b) { return new num(a._value * b._value); }

    public static num operator /(num a, num b) { return new num(a._value / b._value); }

    public static num operator %(num a, num b) { return new num(a._value % b._value); }

    public static num operator -(num a) { return new num(-a._value); }

    public static num operator +(num a) { return a; }

    public static num operator ++(num a) { return new num(a._value + 1); }

    public static num operator --(num a) { return new num(a._value - 1); }

    // Null-safe arithmetic operators
    public static num? operator +(num? a, num? b)
    {
        return a.HasValue && b.HasValue ? new num(a.Value._value + b.Value._value) : null;
    }

    public static num? operator -(num? a, num? b)
    {
        return a.HasValue && b.HasValue ? new num(a.Value._value - b.Value._value) : null;
    }

    public static num? operator *(num? a, num? b)
    {
        return a.HasValue && b.HasValue ? new num(a.Value._value * b.Value._value) : null;
    }

    public static num? operator /(num? a, num? b)
    {
        return a.HasValue && b.HasValue ? new num(a.Value._value / b.Value._value) : null;
    }

    // Comparison operators
    public static bool operator ==(num a, num b) { return Math.Abs(a._value - b._value) < Tolerance; }


    public static bool operator !=(num a, num b) { return Math.Abs(a._value - b._value) > Tolerance; }

    public static bool operator >(num a, num b) { return a._value > b._value; }

    public static bool operator <(num a, num b) { return a._value < b._value; }

    public static bool operator >=(num a, num b) { return a._value >= b._value; }

    public static bool operator <=(num a, num b) { return a._value <= b._value; }

    // Null-safe comparison operators
    public static bool operator ==(num? a, num? b)
    {
        return (!a.HasValue && !b.HasValue) ||
               (a.HasValue  && b.HasValue && Math.Abs(a.Value._value - b.Value._value) < Tolerance);
    }

    public static bool operator !=(num? a, num? b) { return !(a == b); }

    public static bool operator >(num? a, num? b)
    {
        return a.HasValue && b.HasValue && a.Value._value > b.Value._value;
    }

    public static bool operator <(num? a, num? b)
    {
        return a.HasValue && b.HasValue && a.Value._value < b.Value._value;
    }

    // Logical operators
    public static bool operator true(num a) { return a._value != 0; }

    public static bool operator false(num a) { return a._value == 0; }

    public static num operator !(num a) { return new num(a._value == 0 ? 1 : 0); }

    // Bitwise operators
    public static num operator &(num a, num b) { return new num((int)a._value & (int)b._value); }

    public static num operator |(num a, num b) { return new num((int)a._value | (int)b._value); }

    public static num operator ^(num a, num b) { return new num((int)a._value ^ (int)b._value); }

    public static num operator ~(num a) { return new num(~(int)a._value); }

    public static num operator <<(num a, int b) { return new num((int)a._value << b); }

    public static num operator >> (num a, int b) { return new num((int)a._value >> b); }

    // Equality implementation
    public bool Equals(num other) { return Math.Abs(_value - other._value) < Tolerance; }

    public override bool Equals([NotNullWhen(true)] object obj) { return obj is num other && Equals(other); }

    public override int GetHashCode() { return _value.GetHashCode(); }

    public override string ToString() { return _value.ToString(CultureInfo.CurrentCulture); }

    public int CompareTo(num other)
    {
        if (Math.Abs(this._value - other._value) < Tolerance)
        {
            return 0; // Equal within tolerance
        }

        return this._value > other._value ? 1 : -1; // Greater than or less than
    }
}