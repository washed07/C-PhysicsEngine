using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Types;

[StructLayout(LayoutKind.Sequential)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public struct Vect(Num x, Num y, Num z = default(Num), Num w = default(Num)) : IEquatable<Vect>
{
    public           Num  x               = x, y = y, z = z, w = w;
    private readonly Num? cachedMagnitude = null;

    public static readonly Vect Zero = (0, 0);

    public static readonly Vect Up = (0, -1);

    public static readonly Vect Down = (0, 1);

    public static readonly Vect Right = (1, 0);

    public static readonly Vect Left = (-1, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vect WithMagnitude(Num magnitude)
    {
        Num currentMag = Magnitude();
        if (currentMag == 0) { return this; }

        Num scale = magnitude / currentMag;
        return this * scale;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Num Magnitude()
    {
        if (cachedMagnitude.HasValue) { return cachedMagnitude.Value; }

        return Math.Sqrt(SqrMagnitude());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Num SqrMagnitude() { return x * x + y * y + z * z + w * w; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect Normalize(Vect vector)
    {
        Num mag = vector.Magnitude();
        if (mag == 0) { return vector; }

        return vector / mag;
    }
    
    public Vect Normalize() { return Normalize(this); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vect Perpendicular() { return new Vect(y, -x, z, w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator -(Vect a) { return new Vect(-a.x, -a.y, -a.z, -a.w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect Lerp(Vect a, Vect b, Num t) { return a + (b - a) * t; }

    private static Vect Min(Vect a, Vect b)
    {
        return new Vect(Num.Min(a.x, b.x), Num.Min(a.y, b.y), Num.Min(a.z, b.z), Num.Min(a.w, b.w));
    }

    private static Vect Max(Vect a, Vect b)
    {
        return new Vect(Num.Max(a.x, b.x), Num.Max(a.y, b.y), Num.Max(a.z, b.z), Num.Max(a.w, b.w));
    }

    public static Vect Clamp(Vect value, Vect min, Vect max) { return Min(Max(value, min), max); }

    public static Vect Abs(Vect value)
    {
        return new Vect(Num.Abs(value.x), Num.Abs(value.y), Num.Abs(value.z), Num.Abs(value.w));
    }

    // Optimized operators using SIMD when available
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator +(Vect a, Vect b) { return new Vect(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator -(Vect a, Vect b) { return new Vect(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w); }

    public static Vect Rotate(Vect v, Num angle)
    {
        Num cos = Num.Cos(angle);
        Num sin = Num.Sin(angle);
        return new Vect(v.x * cos - v.y * sin, v.x * sin + v.y * cos, v.z, v.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator *(Vect a, Num b) { return new Vect(a.x * b, a.y * b, a.z * b, a.w * b); }

    public static Vect operator +(Vect a, Num b) { return new Vect(a.x + b, a.y + b, a.z + b, a.w + b); }

    public static Vect operator +(Num a, Vect b) { return new Vect(a + b.x, a + b.y, a + b.z, a + b.w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator *(Vect a, Vect b) { return new Vect(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator /(Vect a, Num b)
    {
        Num invB = 1        / b;
        return new Vect(a.x * invB, a.y * invB, a.z * invB, a.w * invB);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator /(Vect a, Vect b) { return new Vect(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator *(Num a, Vect b) { return b * a; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator /(Num a, Vect b) { return new Vect(a / b.x, a / b.y, a / b.z, a / b.w); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Num Dot(Vect a, Vect b) { return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect Cross(Vect a, Vect b)
    {
        return new Vect(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Num Distance(Vect a, Vect b) { return (a - b).Magnitude(); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Num DistanceSqr(Vect a, Vect b)
    {
        Num dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z, dw = a.w - b.w;
        return dx * dx + dy * dy + dz * dz + dw * dw;
    }

    // Efficient comparison operators using squared magnitude
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Vect a, Vect b) { return a.SqrMagnitude() > b.SqrMagnitude(); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vect operator -(Vect a, Num b) { return new Vect(a.x - b, a.y - b, a.z - b, a.w - b); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Vect a, Vect b) { return a.SqrMagnitude() < b.SqrMagnitude(); }

    public static implicit operator Vect((Num x, Num y, Num z) tuple) { return new Vect(tuple.x, tuple.y, tuple.z); }

    public static implicit operator Vect((Num x, Num y) tuple) { return new Vect(tuple.x, tuple.y); }

    public static implicit operator Vect(Vector2 v) { return new Vect(v.X, v.Y); }

    public static implicit operator Vect(Microsoft.Xna.Framework.Vector2 v) { return new Vect(v.X, v.Y); }

    public static implicit operator Vect(Vector3 v) { return new Vect(v.X, v.Y, v.Z); }

    public static implicit operator Vect(Microsoft.Xna.Framework.Vector3 v) { return new Vect(v.X, v.Y, v.Z); }

    public static implicit operator Vector2(Vect v) { return new Vector2(v.x, v.y); }

    public static implicit operator Microsoft.Xna.Framework.Vector2(Vect v)
    {
        return new Microsoft.Xna.Framework.Vector2(v.x, v.y);
    }

    public static implicit operator Vector3(Vect v) { return new Vector3(v.x, v.y, v.z); }

    public static implicit operator Microsoft.Xna.Framework.Vector3(Vect v)
    {
        return new Microsoft.Xna.Framework.Vector3(v.x, v.y, v.z);
    }

    public override bool Equals(object obj)
    {
        if (obj is Vect vect) { return vect == this; }

        return false;
    }

    public static bool operator ==(Vect a, Vect b) { return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w; }

    public static bool operator !=(Vect a, Vect b) { return !(a == b); }

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

    private static readonly string VectorFormat = "({0:F6}, {1:F6}, {2:F6}, {3:F6})";
    public override         string ToString() { return string.Format(VectorFormat, x, y, z, w); }

    public bool Equals(Vect other)
    {
        return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w) && Nullable.Equals(cachedMagnitude, other.cachedMagnitude);
    }
}