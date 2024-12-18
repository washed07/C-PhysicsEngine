namespace Types;

public struct Force(Vect force)
{
    public readonly Vect _totalForce = force; // Total force

    public static implicit operator Vect(Force force) { return force._totalForce; }

    public static implicit operator Force(Vect force) { return new Force(force); }

    public static Force operator *(Force f, Num n) { return new Force(f._totalForce * n); }

    public static Force operator *(Force f, double d) { return new Force(f._totalForce * d); }
}

public struct Torque(Num torque)
{
    private readonly Num _totalTorque = torque;

    public static implicit operator Num(Torque torque) { return torque._totalTorque; }

    public static implicit operator Torque(Num torque) { return new Torque(torque); }

    public static Torque operator *(Torque t, Num n) { return new Torque(t._totalTorque * n); }
}