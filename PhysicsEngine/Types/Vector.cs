using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Types;

[StructLayout(LayoutKind.Sequential)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public struct Vector(num x, num y, num z = default(num), num w = default(num)) : IEquatable<Vector>, IComparable<Vector>
{
    public           num  x               = x, y = y, z = z, w = w;
    private readonly num? cachedMagnitude = null;

    public static readonly Vector Zero = (0, 0);

    public static readonly Vector Up = (0, -1);

    public static readonly Vector Down = (0, 1);

    public static readonly Vector Right = (1, 0);

    public static readonly Vector Left = (-1, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector WithMagnitude(num magnitude)
    {
        num currentMag = Magnitude();
        if (currentMag == 0) { return this; }

        num scale = magnitude / currentMag;
        return this * scale;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public num Magnitude()
    {
        if (cachedMagnitude.HasValue) { return cachedMagnitude.Value; }

        return Math.Sqrt(SqrMagnitude());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public num SqrMagnitude() { return x * x + y * y + z * z + w * w; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector Normalize(Vector vector)
    {
        num mag = vector.Magnitude();
        if (mag == 0) { return vector; }

        return vector / mag;
    }

    public Vector[] Select(Func<Vector, Vector> selector) { return [selector(this)]; }

    public Vector Normalize() { return Normalize(this); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector Perpendicular() { return new Vector(y, -x, z, w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator -(Vector a) { return new Vector(-a.x, -a.y, -a.z, -a.w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector Lerp(Vector a, Vector b, num t) { return a + (b - a) * t; }

    private static Vector Min(Vector a, Vector b)
    {
        return new Vector(num.Min(a.x, b.x), num.Min(a.y, b.y), num.Min(a.z, b.z), num.Min(a.w, b.w));
    }

    private static Vector Max(Vector a, Vector b)
    {
        return new Vector(num.Max(a.x, b.x), num.Max(a.y, b.y), num.Max(a.z, b.z), num.Max(a.w, b.w));
    }

    public static Vector Clamp(Vector value, Vector min, Vector max) { return Min(Max(value, min), max); }

    public static Vector Abs(Vector value)
    {
        return new Vector(num.Abs(value.x), num.Abs(value.y), num.Abs(value.z), num.Abs(value.w));
    }

    // Optimized operators using SIMD when available
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator +(Vector a, Vector b)
    {
        return new Vector(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator -(Vector a, Vector b)
    {
        return new Vector(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
    }

    public static Vector Rotate(Vector v, num angle)
    {
        num cos = num.Cos(angle);
        num sin = num.Sin(angle);
        return new Vector(v.x * cos - v.y * sin, v.x * sin + v.y * cos, v.z, v.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator *(Vector a, num b) { return new Vector(a.x * b, a.y * b, a.z * b, a.w * b); }

    public static Vector operator +(Vector a, num b) { return new Vector(a.x + b, a.y + b, a.z + b, a.w + b); }

    public static Vector operator +(num a, Vector b) { return new Vector(a + b.x, a + b.y, a + b.z, a + b.w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator *(Vector a, Vector b)
    {
        return new Vector(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator /(Vector a, num b)
    {
        num invB = 1          / b;
        return new Vector(a.x * invB, a.y * invB, a.z * invB, a.w * invB);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator /(Vector a, Vector b)
    {
        return new Vector(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator *(num a, Vector b) { return b * a; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator /(num a, Vector b) { return new Vector(a / b.x, a / b.y, a / b.z, a / b.w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static num Dot(Vector a, Vector b) { return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector Cross(Vector a, Vector b)
    {
        return new Vector(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static num Distance(Vector a, Vector b) { return (a - b).Magnitude(); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static num DistanceSqr(Vector a, Vector b)
    {
        num dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z, dw = a.w - b.w;
        return dx * dx + dy * dy + dz * dz + dw * dw;
    }

    // Efficient comparison operators using squared magnitude
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Vector a, Vector b) { return a.SqrMagnitude() > b.SqrMagnitude(); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator -(Vector a, num b) { return new Vector(a.x - b, a.y - b, a.z - b, a.w - b); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Vector a, Vector b) { return a.SqrMagnitude() < b.SqrMagnitude(); }

    public static implicit operator Vector((num x, num y, num z) tuple)
    {
        return new Vector(tuple.x, tuple.y, tuple.z);
    }

    public static implicit operator Vector((num x, num y) tuple) { return new Vector(tuple.x, tuple.y); }

    public static implicit operator Vector(Vector2 v) { return new Vector(v.X, v.Y); }

    public static implicit operator Vector(Microsoft.Xna.Framework.Vector2 v) { return new Vector(v.X, v.Y); }

    public static implicit operator Vector(Vector3 v) { return new Vector(v.X, v.Y, v.Z); }

    public static implicit operator Vector(Microsoft.Xna.Framework.Vector3 v) { return new Vector(v.X, v.Y, v.Z); }

    public static implicit operator Vector2(Vector v) { return new Vector2(v.x, v.y); }

    public static implicit operator Microsoft.Xna.Framework.Vector2(Vector v)
    {
        return new Microsoft.Xna.Framework.Vector2(v.x, v.y);
    }

    public static implicit operator Vector3(Vector v) { return new Vector3(v.x, v.y, v.z); }

    public static implicit operator Microsoft.Xna.Framework.Vector3(Vector v)
    {
        return new Microsoft.Xna.Framework.Vector3(v.x, v.y, v.z);
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector vect) { return vect == this; }

        return false;
    }

    public static bool operator ==(Vector a, Vector b) { return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w; }

    public static bool operator !=(Vector a, Vector b) { return !(a == b); }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + x.GetHashCode();
            hash = hash * 31 + y.GetHashCode();
            hash = hash * 31 + z.GetHashCode();
            return hash * 31 + w.GetHashCode();
        }
    }

    private static readonly string Format = "({0:F6}, {1:F6}, {2:F6}, {3:F6})";
    public override         string ToString() { return string.Format(Format, x, y, z, w); }

    public bool Equals(Vector other)
    {
        return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w) && Nullable.Equals
            (cachedMagnitude, other.cachedMagnitude);
    }

    public int CompareTo(Vector other) { return Magnitude().CompareTo(other.Magnitude()); }
}