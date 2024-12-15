using System;
using System.Collections.Generic;
using System.Linq;
using Forces;
using Shapes;
using Types;

namespace Particles;

// Represents the physics engine
public class Engine
{
    private const  int            Tps        = 60;                           // Ticks per second
    private const  float          Speed      = 1f;                           // Speed factor
    private const  int            Iterations = 1;                            // Number of iterations per update
    public static  float          TimeStep => 1f / Tps / Iterations * Speed; // Time step for each update
    private        float          Timer    { get; set; }                     // Timer for updates
    private        float          Runtime  { get; set; }                     // Total runtime
    private        Accumulator    _accumulator;                              // Particle accumulator
    public         List<Particle> Particles     { get; set; } = [];          // List of particles
    public         List<Particle> AgedParticles { get; set; } = [];          // List of particles to be removed
    private static Physics        _physics;                                  // Physics handler

    // Initialize the engine
    public void Initialize()
    {
        _physics = new Physics(this);
        Particle platform = new Particle(Shape.Square(200, 20), -1f, -10, (400, 400));
        Particle box      = new Particle(Shape.Square(50,  20), -1f, 1f,  (500, 200));
        _accumulator = new Accumulator(this, (500, 500));
        Particles.Add(platform);
        Particles.Add(box);
        //Particles.Add(new Particle(Shape.Square(20, 20), Motion.Static, Laws.None, -1f, 10f, 10000f, (500, 300), 1f));
        //Particles.Add(new Particle(Shape.Square(20, 20), Motion.Static, Laws.None, -1f, 10f, 100f, (500, 500), 1f));
        foreach (Particle particle in Particles) { particle.Initialize(); }

        box.Rotation      = 0f;
        platform.Rotation = 40f;
    }

    // Load content for all particles
    public void LoadContent()
    {
        foreach (Particle particle in Particles) { particle.LoadContent(); }
    }

    // Update the engine
    public void Update(float dt)
    {
        Timer += dt;
        if (!(Timer > TimeStep)) { return; }

        Timer   =  0;
        Runtime += TimeStep;
        for (int _Iteration = 0; _Iteration < Iterations; _Iteration++) { _physics.Tick(); }

        foreach (Particle particle in Particles)
        {
            particle.Integrate(this);
            particle.Update();
        }

        foreach (Particle particle in AgedParticles)
        {
            Particles.Remove(particle);
            _accumulator.CurrentAmount--; // TODO: automate this within the accumulator
        }

        AgedParticles.Clear();
    }
}

// Handles physics calculations
public class Physics(Engine engine)
{
    private readonly Force _gravity = F.Gravity(100f, Vect.Down);

    // Perform physics calculations for each time step
    public void Tick()
    {
        foreach (Particle particle in engine.Particles.Where(particle => !particle.IsMassInf()))
        {
            particle.Impose(F.Gravity(100f, Vect.Down));
            ResolveCollisions(particle);
        }
    }

    private List<Particle> GetOtherParticles(Particle excludedParticle)
    {
        List<Particle> Particles = [];
        Particles.AddRange(engine.Particles.Where(otherParticle => otherParticle != excludedParticle));

        return Particles;
    }

    public void UsingOtherParticles(Particle excludedParticle, Action<Particle, Particle> function)
    {
        foreach (Particle otherParticle in GetOtherParticles(excludedParticle))
        {
            function(excludedParticle, otherParticle);
        }
    }

    public void UsingOtherParticles(Particle excludedParticle, Action<Particle> function)
    {
        foreach (Particle otherParticle in GetOtherParticles(excludedParticle)) { function(otherParticle); }
    }

    private void ResolveCollisions(Particle particle)
    {
        foreach (Particle otherParticle in GetOtherParticles(particle))
        {
            Collision.MTV collision = Collision.Resolve(particle.Polygon, otherParticle.Polygon);
            if (collision != null) { F.Collision(particle, otherParticle, collision.Point, _gravity); }
        }
    }
}