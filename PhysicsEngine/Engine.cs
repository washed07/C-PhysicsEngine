using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using System.Security.Cryptography.X509Certificates;
using Physics.Particles;
using Shapes;
using Types;

namespace Physics
{
    // Represents the physics engine
    public class Engine
    {
        private const int TPS = 60; // Ticks per second
        private const float Speed = 1f; // Speed factor
        private const int Iterations = 1; // Number of iterations per update

        public static float TimeStep => 1f / TPS / Iterations * Speed; // Time step for each update

        private float Timer { get; set; } = 0; // Timer for updates
        public float Runtime { get; private set; } = 0; // Total runtime
        public Accumulator accumulator; // Particle accumulator

        public List<Particle> Particles { get; set; } = new List<Particle>(); // List of particles
        public List<Particle> AgedParticles { get; set; } = new List<Particle>(); // List of particles to be removed
        private static Physics physics; // Physics handler

        // Initialize the engine
        public void Initialize()
        {
            physics = new Physics(this);

            accumulator = new Accumulator(this, (500, 500), Physics.Laws.Local);

            Particles.Add(new Particle(CreateShape.Square(20, 20), Physics.Laws.None, -1f, 10f, 9747479977426853f, (400, 400), 1f));
            //Particles.Add(new Particle(Shape.Square(20, 20), Motion.Static, Laws.None, -1f, 10f, 10000f, (600, 400), 1f));
            //Particles.Add(new Particle(Shape.Square(20, 20), Motion.Static, Laws.None, -1f, 10f, 10000f, (500, 300), 1f));
            //Particles.Add(new Particle(Shape.Square(20, 20), Motion.Static, Laws.None, -1f, 10f, 100f, (500, 500), 1f));
            
            foreach (Particle particle in Particles)
            {
                particle.Initialize();
            }
        }

        // Load content for all particles
        public void LoadContent()
        {
            foreach (Particle particle in Particles)
            {
                particle.LoadContent();
            }
        }

        // Update the engine
        public void Update(float dt)
        {
            Timer += dt;
            if (Timer > TimeStep)
            {
                Timer = 0;
                Runtime += TimeStep;
                for (int _Iteration = 0; _Iteration < Iterations; _Iteration++)
                {
                    accumulator.Emit 
                    (
                        true, 
                        20f, 
                        CreateShape.IsoTriangle(30, 30), 
                        78f,
                        LifeTime: -1f,
                        Vel: new Vect(-50, 0)
                    );
                    physics.Tick(TimeStep);
                }

                foreach (Particle particle in Particles)
                {
                    particle.Integrate(this);
                    particle.Update();
                }

                foreach (Particle particle in AgedParticles)
                {
                    Particles.Remove(particle);
                }
                AgedParticles.Clear();
            }
        }
    }


    // Handles physics calculations
    public class Physics
    {
        private readonly Engine engine; // Reference to the engine

        public enum Laws {Global, Local, Individual, None} // Laws that can be applied to particles

        // Constructor that takes an Engine parameter
        public Physics(Engine engine)
        {
            this.engine = engine;
        }

        // Perform physics calculations for each time step
        public void Tick(Num TimeStep)
        {
            foreach (Particle particle in engine.Particles)
            {
                if (particle.laws == Laws.Global) {particle.Impose(Force.Gravity * particle.mass);}
                if (particle.laws == Laws.Global || particle.laws == Laws.Local) 
                {
                    foreach (Particle _particle in engine.Particles)
                    {
                        if (particle != _particle)
                        {
                            particle.Impose(Force.Attraction(particle, _particle));
                        }
                    }
                }
            }
        }
    }

    public static class Force
    {
        public static Vect Gravity = new Vect(0, 100f);
        public static Num GravitationalConstant = 6.67430e-11f;
        public static Vect Attraction(Particle particle, Particle _particle)
        {
            Num distance = Vect.Distance(particle.position, _particle.position);
            if (distance < 100) {distance = 100;}

            Vect direction = Vect.Normalize(_particle.position - particle.position);

            Num force = GravitationalConstant * (particle.mass * _particle.mass) / (distance * distance);

            return direction * force;
        }
    }
}