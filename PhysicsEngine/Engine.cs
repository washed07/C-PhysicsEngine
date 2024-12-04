using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xna.Framework.Input;
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

            accumulator = new Accumulator(this, (500, 500), Physics.Laws.Global);

            Particles.Add(new Particle(CreateShape.Square(20, 20), Physics.Laws.None, -1f, 10f, 1f, (500, 400), 1f));
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
                        2f, 
                        CreateShape.IsoTriangle(30, 30), 
                        0.5f,
                        LifeTime: -1f
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
                if (particle.laws == Laws.Global) {particle.Impose(Gravity * particle.mass);}
                if (particle.laws == Laws.Global || particle.laws == Laws.Local) 
                {
                    foreach (Particle _particle in engine.Particles)
                    {
                        if (particle != _particle)
                        {
                            particle.Impose(Attraction(particle, _particle));
                        }
                    }
                }
            }
            Particle p0 = engine.Particles[0];
            Particle p1 = engine.Particles[1];
            MouseState mouseState = Mouse.GetState();
            p0.Impose(AnchoredSpring(p0, new Vect(mouseState.X, mouseState.Y), 20f, 1));
            p1.Impose(Connection(p1, p0, 100, 50));
        }

        public static Force Gravity => new((0, 100f));

        public static Force Attraction(Particle particle, Particle _particle)
        {
            Num distance = Vect.Distance(particle.position, _particle.position);
            if (distance < 100) {distance = 100;}

            Vect direction = Vect.Normalize(_particle.position - particle.position);

            Num GravitationalConstant = 6.67430e-11f;
            Num force = GravitationalConstant * (particle.mass * _particle.mass) / (distance * distance);

            return new (direction * force);
        }

        public static Force Spring(Particle particle, Particle _particle, Num springConstant, Num restLength)
        {
            Num R = restLength; // Rest length of spring
            Num m = particle.mass; // Mass of particle
            Num k = springConstant; // Spring constant
            Num d = 0.5f; // Damping factor

            Vect T = _particle.position; // Anchor position
            Num L = Vect.Distance(particle.position, T); // Length of spring
            Num S = L - R; // Stretch of spring
            Vect p = particle.position; // Position of particle
            Vect v = particle.velocity; // Velocity of particle

            Vect n = Vect.Normalize(p - T); // Normalized distance

            Vect force = -(k/m) * (S * n) - (d/m) * v;



            
            return new (force);
        }

        public static Force AnchoredSpring(Particle particle, Vect position, Num springConstant, Num restLength)
        {
            // Constants
            Num R = restLength; // Rest length of spring
            Vect T = position; // Anchor position
            Num m = particle.mass; // Mass of particle
            Num k = springConstant; // Spring constant
            Num d = 0.5f; // Damping factor

            // Dependents
            Num L = Vect.Distance(particle.position, T); // Length of spring
            Num S = L - R; // Stretch of spring
            Vect p = particle.position; // Position of particle
            Vect v = particle.velocity; // Velocity of particle

            Vect n = Vect.Normalize(p - T); // Direction (Sin(theta), Cos(theta))

            Vect force = -(k/m) * (S * n) - (d/m) * v;
            
            return new (force);
        }

        public static Force Connection(Particle particle, Particle _particle, Num Tension, Num Length)
        {
            // Constants
            Num T = Tension; // Tension of connection
            Num R = Length; // Rest length of connection

            // Dependents
            Vect p = _particle.position - particle.position; // Pivot
            Vect n = Vect.Normalize(p); // Normalized distance

            particle.position = _particle.position - R * n;
            particle.pivot = p;

            particle.ImposeAngular(n.Magnitude() * n.x);

            Vect force = (0, 0);

            return new (force);
        }

        public static Force AnchoredConnection(Particle particle, Vect position, Num connectLength)
        {
            // Constants
            Num R = connectLength; // Rest length of connection
            Vect T = position; // Anchor position

            // Dependents
            Vect p = T - particle.position; // Pivot
            Vect n = Vect.Normalize(p); // Normalized distance
            Num m = particle.mass; // Mass of particle
            Vect g = Gravity; // Gravity

            particle.position = T - (R * n);
            particle.pivot = p;

            Vect force = -(g/R) * n;

            return new (force);
        }

    }

    public struct Force
    {
        public Vect totalForce;
        public Num angularForce;

        public Force(Vect Force = default, Num AngularForce = default)
        {
            if (Force != default) {totalForce = Force;}
            if (AngularForce != default) {angularForce = AngularForce;}
        }

        public static implicit operator Vect(Force force) => force.totalForce;
        public static implicit operator Force(Vect force) => new(force);
        public static implicit operator Num(Force force) => force.angularForce;
        public static Force operator *(Force force, Num scalar) => new(force.totalForce * scalar);
    }
}