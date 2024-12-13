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

            Particle platform = new Particle(Shape.Square(200, 20), Physics.Laws.None, -1f, 10f, -10, (400, 400), 1f);
            Particle box = new Particle(Shape.Square(50, 20), Physics.Laws.Global, -1f, -1f, 1f, (500, 200), 1f);
            Particles.Add(platform);
            Particles.Add(box);
            //Particles.Add(new Particle(Shape.Square(20, 20), Motion.Static, Laws.None, -1f, 10f, 10000f, (500, 300), 1f));
            //Particles.Add(new Particle(Shape.Square(20, 20), Motion.Static, Laws.None, -1f, 10f, 100f, (500, 500), 1f));
            
            foreach (Particle particle in Particles)
            {
                particle.Initialize();
            }
            box.rotation = 0f;
            platform.rotation = 80f;
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

        public enum Laws {Global, Local, Individual, None} // GET RID OF

        public Force Gravity = F.Gravity(100f, Vect.Down);

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
                if (particle.IsMassInf() == true) {continue;}
                particle.Impose(Gravity);

                ResolveCollisions(particle);
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

        public void UsingOtherParticles(Particle excludedParticle, Action<Particle, Particle> function)
        {
            foreach (Particle otherParticle in getOtherParticles(excludedParticle))
            {
                function(excludedParticle, otherParticle);
            }
        }

        public void UsingOtherParticles(Particle excludedParticle, Action<Particle> function)
        {
            foreach (Particle otherParticle in getOtherParticles(excludedParticle))
            {
                function(otherParticle);
            }
        }

        public void ResolveCollisions(Particle particle)
        {
            foreach (Particle otherParticle in getOtherParticles(particle))
            {
                Collision.MTV collision = Collision.Resolve(particle.polygon, otherParticle.polygon);
                if (collision != null) 
                {
                    F.Collision(particle, otherParticle, collision.Point, Gravity);
                }
            }
        }
    }
}