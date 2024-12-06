using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;
using Physics.Particles;
using Shapes;
using Types;
using Forces;
using System.Runtime.Intrinsics.Arm;
using System.Numerics;
using Microsoft.Xna.Framework;

namespace Physics
{
    // Represents the physics engine
    public class Engine
    {
        private const int TPS = 60; // Ticks per second
        private const float Speed = 0.1f; // Speed factor
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

            Particles.Add(new Particle(Shape.Square(20, 20), Physics.Laws.None, -1f, 10f, 1e18, (500, 400), 1f)); // Massive masses attratction only works from 1e17 to 1e18. 
            Particles.Add(new Particle(Shape.Square(20, 20), Physics.Laws.Global, -1f, -1f, 1f, (400, 600), 1f));
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
                    accumulator.currentAmount--; // TODO: automate this within the accumulator
                }
                AgedParticles.Clear();
            }
        }
    }


    // Handles physics calculations
    public class Physics
    {
        private readonly Engine _Engine; // Reference to the engine

        public enum Laws {Global, Local, Individual, None} // Laws that can be applied to particles

        // Constructor that takes an Engine parameter
        public Physics(Engine engine)
        {
            this._Engine = engine;
        }

        // Perform physics calculations for each time step
        public void Tick(Num TimeStep)
        {
            foreach (Particle particle in _Engine.Particles)
            {
                if (!particle.IsMassInf())
                {
                    usingOtherParticles(particle, (p1, p2) => F.p(p1, p2, 100f, 100));
                }
            }
        }

        public List<Particle> getOtherParticles(Particle excludedParticle)
        {
            List<Particle> Particles = [];
            foreach (Particle otherParticle in _Engine.Particles)
            {
                if (otherParticle  != excludedParticle)
                {
                    Particles.Add(otherParticle);
                }
            }
            return Particles;
        }
        public void usingOtherParticles(Particle excludedParticle, Action<Particle, Particle> function)
        {
            foreach (Particle otherParticle in getOtherParticles(excludedParticle))
            {
                function(excludedParticle, otherParticle);
            }
        }
        public void usingOtherParticles(Particle excludedParticle, Action<Particle> function)
        {
            foreach (Particle otherParticle in getOtherParticles(excludedParticle))
            {
                function(otherParticle);
            }
        }
    }
}