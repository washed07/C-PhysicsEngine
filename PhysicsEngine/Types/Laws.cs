using System;
using Microsoft.Xna.Framework.Input;
using Particles;
using Particles;
using Types;

namespace Forces;

public static class F
{
    public static Force Gravity(Num gravity, Vect direction)
    {
        return new Force(new Vect(gravity, gravity) * direction);
        // Gravity
    }

    public static Force Attraction(Particle particle, Particle otherParticle) // Mutual Attraction
    {
        // Force = G * (m1 * m2) / r^2
        // Force = GravitationalConstant * (Mass1 * Mass2) / Distance^2
        // explanation:
        // GravitationalConstant: constant of proportionality
        // Mass1, Mass2: masses of the particles
        // Distance: distance between the particles     
        // Constants
        Num m1 = particle.Mass;
        Num m2 = otherParticle.Mass;
        // Dependents
        Num r = Vect.Distance(particle.Position, otherParticle.Position); // Distance between particles
        if (r < 100) { r = 100; } // Capped distance to prevent spikes in force at close range        

        Vect n = Vect.Normalize(otherParticle.Position - particle.Position); // Direction (Sin(theta), Cos(theta))      
        Num  G = 6.67430e-11f; // Gravitational constant
        Num  force = G * (m1 * m2) / (r * r); // Total force     
        return new Force(n * force);
    }
    
    public static Force FakeAttraction(Particle particle, Particle otherParticle, Force gravity) // Mutual Attraction
    {

        Vect n = Vect.Normalize(otherParticle.Position - particle.Position); // Direction (Sin(theta), Cos(theta))
        Num  force = gravity._totalForce.Magnitude(); // Total force     
        return new Force(n * force);
    }

    public static Force GCursor(Particle particle, Num strength)
    {
        // Force = G * (m1 * m2) / r^2
        // Force = GravitationalConstant * (Mass1 * Mass2) / Distance^2
        // explanation:
        // GravitationalConstant: constant of proportionality
        // Mass1, Mass2: masses of the particles
        // Distance: distance between the particles   

        // Constants
        Num m1 = particle.Mass;
        Num m2 = strength * 1e15; // Mass of cursor

        // Dependents
        Vect c = (Mouse.GetState().X, Mouse.GetState().Y); // Cursor position
        Num  r = Vect.Distance(particle.Position, c); // Distance between particles
        if (r < 100) { r = 100; } // Capped distance to prevent spikes in force at close range        

        Vect n     = Vect.Normalize(c - particle.Position); // Direction (Sin(theta), Cos(theta))      
        Num  G     = 6.67430e-11f;                          // Gravitational constant
        Num  force = G * (m1 * m2) / (r * r);               // Total force     
        return new Force(n         * force);
    }

    public static Force Spring(Particle particle, Particle otherParticle, Num springConstant, Num restLength) // Spring
    {
        // Force = -(k/m) * (S * n) - (d/m) * v
        // Force = -(springConstant / Mass) * (Stretch * Normal) - (Damping / Mass) * Velocity 
        // explanation:
        // -(springConstant / Mass): sets force relative to the particles mass
        // (Stretch * Normal): stretch of the spring relative to the normal(theta); the actual force being applied
        // -(Damping / Mass) * v: damping applied to velocity relative to the particles mass    

        // Constants
        Num R = restLength;     // Rest length of spring
        Num m = particle.Mass;  // Mass of particle
        Num k = springConstant; // Spring constant
        Num d = 0.5f;           // Damping factor 

        // Dependents
        Vect T = otherParticle.Position;              // Anchor position
        Num  L = Vect.Distance(particle.Position, T); // Length of spring
        Num  S = L - R;                               // Stretch of spring
        Vect p = particle.Position;                   // Position of particle
        Vect v = particle.Velocity;                   // Velocity of particle 
        Vect n = Vect.Normalize(p - T);               // Direction (Sin(theta), Cos(theta))   
        Vect F = -(k / m) * (S * n) - d / m * v;      // Total force   
        return new Force(F);
    }

    public static Force SpringAnchor
        (Particle particle, Vect position, Num springConstant, Num restLength) // Anchored Spring
    {
        // See Force Spring() for explanation   

        // Constants
        Num  R = restLength;     // Rest length of spring
        Vect T = position;       // Anchor position
        Num  m = particle.Mass;  // Mass of particle
        Num  k = springConstant; // Spring constant
        Num  d = 0.5f;           // Damping factor 

        // Dependents
        Num  L = Vect.Distance(particle.Position, T); // Length of spring
        Num  S = L - R;                               // Stretch of spring
        Vect p = particle.Position;                   // Position of particle
        Vect v = particle.Velocity;                   // Velocity of particle 
        Vect n = Vect.Normalize(p - T);               // Direction (Sin(theta), Cos(theta))   
        Vect F = -(k / m) * (S * n) - d / m * v;
        return new Force(F);
    }

    public static Force SCursor(Particle particle, Num springConstant, Num restLength)
    {
        // See Force Spring() for explanation   
        Vect T = (Mouse.GetState().X, Mouse.GetState().Y); // Anchor position
        // See Force Spring() for explanation   

        // Constants
        Num R = restLength;     // Rest length of spring
        Num m = particle.Mass;  // Mass of particle
        Num k = springConstant; // Spring constant
        Num d = 0.5f;           // Damping factor 

        // Dependents
        Num  L = Vect.Distance(particle.Position, T); // Length of spring
        Num  S = L - R;                               // Stretch of spring
        Vect p = particle.Position;                   // Position of particle
        Vect v = particle.Velocity;                   // Velocity of particle 
        Vect n = Vect.Normalize(p - T);               // Direction (Sin(theta), Cos(theta))   
        Vect F = -(k / m) * (S * n) - d / m * v;
        return new Force(F * n);
    }

    public static void Impulse(Particle particleA, Particle particleB, Vect collisionPoint)
    { // TODO: Fix not working at certain angles
        Num Mₐ = particleA.Mass;
        Num Mᵦ = particleB.Mass;

        //Num Iₐ = particleA.Inertia;
        //Num Iᵦ = particleB.Inertia;

        Vect vₐᵢ = particleA.Velocity;
        Vect vᵦᵢ = particleB.Velocity;

        Vect Vᵣ = vᵦᵢ - vₐᵢ;
        if (Vᵣ < Vect.Zero) { return; }

        //Num ωₐᵢ = particleA.AngVelocity;
        //Num ωᵦᵢ = particleB.AngVelocity;

        //Vect rₐ = particleA.Position - collisionPoint;
        //Vect rᵦ = particleB.Position - collisionPoint;

        Vect n = particleB.Polygon.GetEdgeAtPoint(collisionPoint).Normalize();
        Num  e = Num.Min(particleA.Restitution, particleB.Restitution); // Coefficient of restitution

        //Vect vₐₚ = vₐᵢ + ωₐᵢ * rₐ;
        //Vect vᵦₚ = vᵦᵢ + ωᵦᵢ * rᵦ;

        //Vect vₚ = vₐᵢ + vₐᵢ * rₐ - vᵦᵢ - ωᵦᵢ * rᵦ;

        Num j;
        
        if (particleB.IsMassInf()) {j = -(1 + e) * Vect.Dot(Vᵣ, n) / (1/Mₐ);} else
        {
            j = -(1 + e) * Vect.Dot(Vᵣ, n) / ((1/Mₐ) + (1/Mᵦ));
        }

        Vect vₐ = vₐᵢ + j * n / Mₐ;
        Vect vᵦ = vᵦᵢ - j * n / Mᵦ;

        //Num ωₐ = ωₐᵢ + Vect.Dot(rₐ, j * n) / Iₐ;

        particleA.Velocity    = vₐ;
        particleB.Velocity    = vᵦ;
        //particleA.AngVelocity = ωₐ;
    }

    public static Force Normal(Particle particle, Vect edge)
    {
        // Normal force
        // Fn = Fg * cos(theta)
        // Fn = Gravity * cos(theta)
        // explanation:
        // Fn: normal force
        // Fg: gravitational force
        // theta: angle between the normal and the gravitational force
        Vect dir   = edge.Perpendicular();
        Num  slant = Math.Sin(edge.x);
        Vect g     = particle.Velocity.Perpendicular() * slant;
        Console.WriteLine(Math.Sin(edge.x));
        return new Force(g * dir);
    }

    private static Num _theta = Math.PI / 4;
    private static Num _aVel  = 0;

    public static void PendulumAnchor
        (Particle particle, Particle otherParticle, Num gravity, Num r, float b = 1) // Anchored Pendulum
    {
        // Constants
        Num      g  = gravity * particle.Mass;                            // Gravity
        Num      R  = r;                                                  // Length
        Particle x0 = otherParticle;                                      // Pivot
        Particle x1 = particle;                                           // Bob
        x1.Torque      =  -g / R    * Math.Sin(_theta) * Engine.TimeStep; // x'' || Acceleration
        x1.AngVelocity += x1.Torque * Engine.TimeStep;                    // x' || Velocity
        _theta         += _aVel;                                          // 0 (theta) || Angle
        _aVel          *= b;                                              // Damping angular velocity

        // POSITION
        x1.Position.x = x0.Position.x + R * Math.Sin(_theta);
        x1.Position.y = x0.Position.y - R * Math.Cos(_theta);
    }
}