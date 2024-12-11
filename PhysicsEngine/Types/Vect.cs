using System;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vect
    {
        public Num x, y, z, w;
        private readonly Num? cachedMagnitude;

        public static readonly Vect Zero = (0, 0);

        public static readonly Vect Up = (0, -1);

        public static readonly Vect Down = (0, 1);

        public static readonly Vect Right = (1, 0);

        public static readonly Vect Left = (-1, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vect(Num x, Num y, Num z = default, Num w = default)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
            this.cachedMagnitude = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vect WithMagnitude(Num magnitude)
        {
            Num currentMag = Magnitude();
            if (currentMag == 0) return this;
            Num scale = magnitude / currentMag;
            return this * scale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Num Magnitude()
        {
            if (cachedMagnitude.HasValue) return cachedMagnitude.Value;
            return (Num)Math.Sqrt(SqrMagnitude());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Num SqrMagnitude() => x * x + y * y + z * z + w * w;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect Normalize(Vect vector)
        {
            Num mag = vector.Magnitude();
            if (mag == 0) return vector;
            return vector / mag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator -(Vect a) => new(-a.x, -a.y, -a.z, -a.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect Lerp(Vect a, Vect b, Num t) => a + (b - a) * t;

        public static Vect Min(Vect a, Vect b) => new(
            Num.Min(a.x, b.x),
            Num.Min(a.y, b.y),
            Num.Min(a.z, b.z),
            Num.Min(a.w, b.w)
        );

        public static Vect Max(Vect a, Vect b) => new(
            Num.Max(a.x, b.x),
            Num.Max(a.y, b.y),
            Num.Max(a.z, b.z),
            Num.Max(a.w, b.w)
        );

        public static Vect Clamp(Vect value, Vect min, Vect max) => Min(Max(value, min), max);

        public static Vect Abs(Vect value) => new(Num.Abs(value.x), Num.Abs(value.y), Num.Abs(value.z), Num.Abs(value.w));

        // Optimized operators using SIMD when available
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator +(Vect a, Vect b) => new(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator -(Vect a, Vect b) => new(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);

        public static Vect Rotate(Vect v, Num angle)
        {
            Num cos = Num.Cos(angle);
            Num sin = Num.Sin(angle);
            return new Vect(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos,
                v.z,
                v.w
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator *(Vect a, Num b) => new(a.x * b, a.y * b, a.z * b, a.w * b);

        public static Vect operator +(Vect a, Num b) => new(a.x + b, a.y + b, a.z + b, a.w + b);

        public static Vect operator +(Num a, Vect b) => new(a + b.x, a + b.y, a + b.z, a + b.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator *(Vect a, Vect b) => new(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator /(Vect a, Num b)
        {
            Num invB = 1 / b;
            return new Vect(a.x * invB, a.y * invB, a.z * invB, a.w * invB);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator /(Vect a, Vect b) => new(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator *(Num a, Vect b) => b * a;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator /(Num a, Vect b) => new(a / b.x, a / b.y, a / b.z, a / b.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Num Dot(Vect a, Vect b) => a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect Cross(Vect a, Vect b) => new(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Num Distance(Vect a, Vect b) => (a - b).Magnitude();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Num DistanceSqr(Vect a, Vect b)
        {
            Num dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z, dw = a.w - b.w;
            return dx * dx + dy * dy + dz * dz + dw * dw;
        }

        // Efficient comparison operators using squared magnitude
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Vect a, Vect b) => a.SqrMagnitude() > b.SqrMagnitude();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vect operator -(Vect a, Num b) => new(a.x - b, a.y - b, a.z - b, a.w - b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Vect a, Vect b) => a.SqrMagnitude() < b.SqrMagnitude();

        public static implicit operator Vect((Num x, Num y, Num z) tuple)
        {
            return new Vect(tuple.x, tuple.y, tuple.z);
        }

        public static implicit operator Vect((Num x, Num y) tuple)
        {
            return new Vect(tuple.x, tuple.y);
        }

        public static implicit operator Vect(System.Numerics.Vector2 v)
        {
            return new Vect(v.X, v.Y);
        }

        public static implicit operator Vect(Microsoft.Xna.Framework.Vector2 v)
        {
            return new Vect(v.X, v.Y);
        }

        public static implicit operator Vect(System.Numerics.Vector3 v)
        {
            return new Vect(v.X, v.Y, v.Z);
        }

        public static implicit operator Vect(Microsoft.Xna.Framework.Vector3 v)
        {
            return new Vect(v.X, v.Y, v.Z);
        }

        public static implicit operator System.Numerics.Vector2(Vect v)
        {
            return new System.Numerics.Vector2((float)v.x, (float)v.y);
        }

        public static implicit operator Microsoft.Xna.Framework.Vector2(Vect v)
        {
            return new Microsoft.Xna.Framework.Vector2((float)v.x, (float)v.y);
        }

        public static implicit operator System.Numerics.Vector3(Vect v)
        {
            return new System.Numerics.Vector3((float)v.x, (float)v.y, (float)v.z);
        }

        public static implicit operator Microsoft.Xna.Framework.Vector3(Vect v)
        {
            return new Microsoft.Xna.Framework.Vector3((float)v.x, (float)v.y, (float)v.z);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vect)
            {
                return (Vect)obj == this;
            }
            return false;
        }

        public static bool operator ==(Vect a, Vect b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }

        public static bool operator !=(Vect a, Vect b)
        {
            return !(a == b);
        }

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
        public override string ToString() => string.Format(VectorFormat, x, y, z, w);
    }
}
