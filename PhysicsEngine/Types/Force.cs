namespace Types;

public struct Force
{
    public Vect totalForce; // Total force

    public Force(Vect Force)
    {
        totalForce = Force;
    }

    public static implicit operator Vect(Force force) => force.totalForce;
    public static implicit operator Force(Vect force) => new(force);
    public static Force operator *(Force f, Num n) => new(f.totalForce * n);
}

public struct Torque
{
    public Num totalTorque;

    public Torque(Num Torque)
    {
        totalTorque = Torque;
    }

    public static implicit operator Num(Torque torque) => torque.totalTorque;
    public static implicit operator Torque(Num torque) => new(torque);
    public static Torque operator *(Torque t, Num n) => new(t.totalTorque * n);
}

public struct Change
    {

        public Change() {}

        public static implicit operator Vect(Change force) => Vect.Zero;
    }