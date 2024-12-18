namespace Types;

public struct Force(Vector force)
{
    public readonly Vector NetForce = force; // Total force

    public static implicit operator Vector(Force force) { return force.NetForce; }

    public static implicit operator Force(Vector force) { return new Force(force); }

    public static Force operator *(Force f, num n) { return new Force(f.NetForce * n); }

    public static Force operator *(Force f, double d) { return new Force(f.NetForce * d); }
}

public struct Torque(num torque)
{
    private readonly num _totalTorque = torque;

    public static implicit operator num(Torque torque) { return torque._totalTorque; }

    public static implicit operator Torque(num torque) { return new Torque(torque); }

    public static Torque operator *(Torque t, num n) { return new Torque(t._totalTorque * n); }
}