using System;
using System.Linq;
using Shapes;
using Types;

namespace Physics.Particles;

// Represents a particle in the physics engine
public class Particle
{
    public Polygon polygon; // Texture of the particle
    public Num radius;
    public Physics.Laws laws; // Laws applied to the particle (Global, Local, etc.)
    public Num lifeTime; // Total Life span
    public Num age; // Current age
    public Vect position;
    public Num rotation;
    public Vect pivot = (0, 0);
    public Vect velocity;
    public Num angularVelocity;
    public Vect acceleration; 
    public Num angularAcceleration;
    public Num mass;
    public Num damping = 0.9995f; // Damping factor

    // Properties
    public Vect Centroid => polygon.GetCentroid();
    public Num Inertia => Polygon.CalculateMomentOfInertia(polygon.Vertices, mass);

    // Constructor to initialize the particle
    public Particle(Polygon Polygon, Physics.Laws Laws, Num LifeTime, Num Radius, Num Mass, Vect Position, Num Damping, Vect InitalVelocity = default)
    {
        polygon = Polygon;
        laws = Laws;
        lifeTime = LifeTime;
        radius = Radius;
        mass = Mass;
        position = Position;
        polygon.Position = position;
        pivot = polygon.GetCentroid();
        velocity = InitalVelocity;
    }

    // Initialize the particle (empty for now)
    public void Initialize() {}

    // Load the content (texture) for the particle
    public void LoadContent() {}

    // Integrate the particle's motion over time
    public void Integrate(Engine engine)
    {
        age += Engine.TimeStep;
        if (age >= lifeTime && lifeTime >= 0) 
        {
            engine.AgedParticles.Add(this); // Mark particle for removal if its age exceeds its lifetime
            return;
        }


        velocity += acceleration * Engine.TimeStep;
        angularVelocity += angularAcceleration * Engine.TimeStep;

        velocity *= damping;
        angularVelocity *= damping;

        position += velocity * Engine.TimeStep;
        rotation += angularVelocity * Engine.TimeStep;

        acceleration = Vect.Zero;
        angularAcceleration = 0;
    }

    // Apply a force to the particle
    public void Impose(Vect force)
    {
        acceleration += force / mass;
    }

    public void ImposeAngular(Num force)
    {
        angularAcceleration += force / mass;
    }

    public void Update() {polygon.Position = position; polygon.Rotate(rotation, pivot);}

    public bool IsMassInf()
    {
        if (mass > 0 && mass < 1e10) {return false;} else {return true;}
    }

    public Vect getPivotTo(Vect point)
    {
        return point - position;
    }
    
}

// Represents an accumulator that emits particles
public class Accumulator
{
    private readonly Engine _Engine; // Reference to the engine
    public Vect position; // Position of the accumulator
    public Physics.Laws laws; // Laws applied to emitted particles

    // Constructor to initialize the accumulator
    public Accumulator(Engine Engine, Vect Position, Physics.Laws Laws = Physics.Laws.Global)
    {
        _Engine = Engine;
        position = Position;
        laws = Laws;
    }

    public Num radius; // Radius of emitted particles
    public Num amount; // Amount of particles to emit
    public Num currentAmount; // Current amount of particles emitted
    public Num lifeTime; // Lifetime of emitted particles
    public Vect vel; // Velocity of emitted particles

    // Emit particles with specified properties
    public void Emit
    (
        bool IsRandom, // required
        Num Amount, // required
        Polygon polygon,
        Num Mass, // required
        Num Radius = default,
        Num LifeTime = default, // Particle life time
        Num MaxLife = default,
        Num MinLife = default,
        Vect Vel = default, // Velocity
        Num MaxVelx = default, 
        Num MaxVely = default,
        Num MinVelx = default,
        Num MinVely = default
        
    )
    {   
        if (currentAmount < Amount)
        {
            if (IsRandom == true) 
            {
                Random random = new Random();
                if (Radius == 0) {radius = random.Next(2, 10);} else {radius = Radius;}
                if (LifeTime == 0)
                {
                    Num Max;
                    Num Min;
                    if (MaxLife != 0) {Max = MaxLife;} else {Max = 5f;}
                    if (MinLife != 0) {Min = MinLife;} else {Min = 1f;}
                    lifeTime = random.Next(Min, Max);
                } else {lifeTime = LifeTime;}
                if (Vel == (0, 0, 0)) 
                {
                    Num MaxX;
                    Num MinX;
                    Num MaxY;
                    Num MinY;
                    if (MaxVelx != 0) {MaxX = MaxVelx;} else {MaxX = 100;}
                    if (MinVelx != 0) {MinX = MinVelx;} else {MinX = -100;}
                    if (MaxVely != 0) {MaxY = MaxVely;} else {MaxY = 100;}
                    if (MinVely != 0) {MinY = MinVely;} else {MinY = -100;}
                    vel = (random.Next(MinX, MaxX), random.Next(MinY, MaxY));
                } else {vel = Vel;}
            }
            CreateParticle(polygon, Mass, radius, lifeTime, vel);
            currentAmount++;
        }
    }

    // Create a new particle and add it to the engine
    public void CreateParticle(Polygon polygon, Num Mass, Num Radius, Num LifeTime, Vect Vel)
    {
        Particle particle = new Particle(polygon, laws, LifeTime, Radius, Mass, position, 1f, Vel);
        particle.LoadContent();
        _Engine.Particles.Add(particle);
    }
}
