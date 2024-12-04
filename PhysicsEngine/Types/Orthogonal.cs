using System;

namespace Types
{
    public struct Orthogonal // UNUSED
    {
        public Vect v1;
        public Vect v2;
        public Vect v3;

        public Orthogonal(Vect _v1, Vect _v2, Vect _v3 = default)
        {
            if (_v3 == default)
            {
                Vect c = Vect.Cross(_v1, _v2);
                Vect b = Vect.Cross(c, _v1);

                v1 = _v1;
                v2 = b;
                v3 = c;
            }
            else
            {
                if (_v2 == Vect.Cross(_v3, _v1))
                {
                    v1 = _v1;
                    v2 = _v2;
                    v3 = _v3;
                }
                else
                {
                    throw new Exception("Vectors are not orthogonal");
                }
            }
        }
    }
}
