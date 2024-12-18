using System;
using System.Collections.Generic;
using System.Linq;
using Forces;
using Types;

namespace Objects;

// Represents the physics engine
public class Engine
{
    private const  int            Tps        = 60; // Ticks per second
    private const  float          Speed      = 1f; // Speed factor
    private const  int            Iterations = 1; // Number of iterations per update
    private static Physics        _physics; // Physics handler
    public static  float          TimeStep      => 1f / Tps / Iterations * Speed; // Time step for each update
    private        float          Timer         { get; set; } // Timer for updates
    private        float          Runtime       { get; set; } // Total runtime// Particle accumulator
    public         List<Particle> Particles     { get; set; } = []; // List of particles
    public         List<Particle> AgedParticles { get; set; } = []; // List of particles to be removed

    // Initialize the engine
    public void Initialize() // Initialize the engine
    {
        _physics = new Physics(this);
        foreach (Particle particle in Particles) { particle.Initialize(); }
    }

    // Load content for all particles
    public void LoadContent() // Load content for all particles
    {
        foreach (Particle particle in Particles) { particle.LoadContent(); } // Load content for each particle
    }

    public void Update(float dt) // Update the engine
    {
        Timer += dt;
        if (!(Timer > TimeStep)) { return; }

        Timer   =  0;
        Runtime += TimeStep;
        for (int _Iteration = 0; _Iteration < Iterations; _Iteration++) { _physics.Tick(); }

        foreach (Particle particle in Particles) // Integrate particle physics
        {
            particle.Integrate(this);
            particle.Update();
        }

        foreach (Particle particle in AgedParticles)
        {
            Particles.Remove(particle);
            //_accumulator.CurrentAmount--; // TODO: automate this within the accumulator
        } // Remove aged particles

        AgedParticles.Clear();
    }
}

// Handles physics calculations
public class Physics(Engine engine)
{
    private readonly Force _gravity = F.Gravity(100f, Vector.Down);

    public void Tick() // perform physics calculations
    {
        foreach (Particle particle in engine.Particles.Where(particle => !particle.IsMassInf()))
        {
            UsingOtherParticles(particle, (p1, p2) => particle.Impose(F.FakeAttraction(p1, p2, _gravity)));
            //particle.Impose(_gravity);
            ResolveCollisions(particle);
        }
    }

    private List<Particle> GetOtherParticles
        (Particle excludedParticle) // Get all particles except the excluded particle
    {
        List<Particle> Particles = [];
        Particles.AddRange(engine.Particles.Where(otherParticle => otherParticle != excludedParticle));

        return Particles;
    }

    public void UsingOtherParticles
    (
        Particle                   excludedParticle,
        Action<Particle, Particle> function
    ) // Perform an action on all particles using the excluded particle
    {
        foreach (Particle otherParticle in GetOtherParticles(excludedParticle))
        {
            function(excludedParticle, otherParticle);
        }
    }

    public void UsingOtherParticles
    (
        Particle         excludedParticle,
        Action<Particle> function
    ) // Perform an action on all particles except the excluded particle
    {
        foreach (Particle otherParticle in GetOtherParticles(excludedParticle)) { function(otherParticle); }
    }

    private void ResolveCollisions(Particle particle) // Resolve collisions between particles
    {
        foreach (Particle otherParticle in GetOtherParticles(particle))
        {
            Collision.Mtv collision = Collision.Resolve(particle.Polygon, otherParticle.Polygon);
            if (collision != null) { F.Impulse(particle, otherParticle, collision.Point); }
        }
    }
}