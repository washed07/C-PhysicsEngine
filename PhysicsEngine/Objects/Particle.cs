using System;
using Shapes;
using Types;

namespace Particles;

// Represents a particle in the physics engine
public class Particle
{
    public readonly  Polygon Polygon;   // Texture of the particle
    private readonly Num     _lifeTime; // Total Life span
    private          Num     _age;      // Current age
    public           Vect    Position;
    public           Num     Rotation;
    private          Vect    _pivot;
    public           Vect    Velocity;
    public           Num     AngVelocity;
    public           Vect    Acceleration;
    public           Num     Torque;
    public           Num     Mass;
    public           Num     Restitution; // Coefficient of restitution
    private readonly Num     _damping = 0.995f; // Damping factor

    // Properties
    public Vect Centroid => Polygon.GetCentroid();
    public Num  Inertia  => Polygon.CalculateMomentOfInertia(Polygon.Vertices, Mass);

    // Constructor to initialize the particle
    public Particle(Polygon polygon, Num lifeTime, Num mass, Vect position, Vect initialVelocity = default(Vect), Num restitution = default(Num))
    {
        Polygon   = polygon;
        _lifeTime = lifeTime;
        Mass      = mass;
        Position  = position + this.Centroid;
        Velocity  = initialVelocity;
        Restitution = restitution;
        this.Initialize();
    }

    // Initialize the particle (empty for now)
    public void Initialize()
    {
        Polygon.Position = Position;
        _pivot           = Polygon.GetCentroid();
    }

    // Load the content (texture) for the particle
    public void LoadContent() { }

    // Integrate the particle's motion over time
    public void Integrate(Engine engine)
    {
        _age += Engine.TimeStep;
        if (_age >= _lifeTime && _lifeTime >= 0)
        {
            engine.AgedParticles.Add(this); // Mark particle for removal if its age exceeds its lifetime
            return;
        }

        Velocity += Acceleration * Engine.TimeStep;
        AngVelocity += Torque * Engine.TimeStep;
        Velocity *= _damping;
        AngVelocity *= _damping;
        Position += Velocity * Engine.TimeStep;
        Rotation += AngVelocity * Engine.TimeStep;
        Acceleration = Vect.Zero;
        Torque = 0;
    }

    // Apply a force to the particle
    public void Impose(Vect force)       { Acceleration += force / Mass; }
    public void ImposeAngular(Num force) { Torque += force / Mass; }

    public void Update()
    {
        Polygon.Position = Position;
        Polygon.Rotate(Rotation, _pivot);
    }

    public bool IsMassInf()            { return Mass <= 0 || Mass >= 1e10; }
    public Vect GetPivotTo(Vect point) { return point - Position; }
}

// Represents an accumulator that emits particles
public class Accumulator(Engine engine, Vect position)
{
    // Reference to the engine
    // Position of the accumulator
    public  Num  CurrentAmount; // Current amount of particles emitted
    private Num  _lifeTime;     // Lifetime of emitted particles
    private Vect _vel;          // Velocity of emitted particles

    // Emit particles with specified properties
    public void Emit
    (
        bool isRandom, // required
        Num amount,    // required
        Polygon polygon,
        Num mass, // required
        Num radius = default(Num),
        Num lifeTime = default(Num), // Particle life time
        Num maxLife = default(Num),
        Num minLife = default(Num),
        Vect vel = default(Vect), // Velocity
        Num maxVelx = default(Num),
        Num maxVely = default(Num),
        Num minVelx = default(Num),
        Num minVely = default(Num)
    )
    {
        if (CurrentAmount >= amount) { return; }

        if (isRandom)
        {
            Random random = new Random();
            if (radius == 0) { random.Next(2, 10); }

            if (lifeTime == 0)
            {
                Num Max;
                Num Min;
                if (maxLife != 0) { Max = maxLife; } else { Max = 5f; }

                if (minLife != 0) { Min = minLife; } else { Min = 1f; }

                _lifeTime = random.Next(Min, Max);
            } else { _lifeTime = lifeTime; }

            if (vel == (0, 0, 0))
            {
                Num MaxX;
                Num MinX;
                Num MaxY;
                Num MinY;
                if (maxVelx != 0) { MaxX = maxVelx; } else { MaxX = 100; }

                if (minVelx != 0) { MinX = minVelx; } else { MinX = -100; }

                if (maxVely != 0) { MaxY = maxVely; } else { MaxY = 100; }

                if (minVely != 0) { MinY = minVely; } else { MinY = -100; }

                _vel = (random.Next(MinX, MaxX), random.Next(MinY, MaxY));
            } else { _vel = vel; }
        }

        CreateParticle(polygon, mass, _lifeTime, _vel);
        CurrentAmount++;
    }

    // Create a new particle and add it to the engine
    private void CreateParticle(Polygon polygon, Num mass, Num lifeTime, Vect vel)
    {
        Particle particle = new Particle(polygon, lifeTime, mass, position, vel);
        particle.LoadContent();
        engine.Particles.Add(particle);
    }
}