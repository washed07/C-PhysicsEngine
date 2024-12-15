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

        Vect n     = Vect.Normalize(otherParticle.Position - particle.Position); // Direction (Sin(theta), Cos(theta))      
        Num  G     = 6.67430e-11f;                                           // Gravitational constant
        Num  force = G * (m1 * m2) / (r * r);                                // Total force     
        return new Force(n         * force);
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
        Vect T = otherParticle.Position;                  // Anchor position
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

    public static void Collision(Particle particle, Particle otherParticle, Vect collisionPoint, Force gravity)
    { // TODO: Fix not working at certain angles
        Particle x0   = particle;
        Particle x1   = otherParticle;
        Vect     edge = x1.Polygon.GetEdgeAtPoint(collisionPoint);
        Vect     dir  = edge.Perpendicular(); // Remove the sine multiplication
        Num      e    = 1f;                   // Coefficient of restitution (0.5 = semi-elastic)
        Num      rap  = Vect.Distance(x0.Position + x0.Centroid, collisionPoint);
        Num      rbp  = Vect.Distance(x1.Position + x1.Centroid, collisionPoint);
        Num      ma   = x0.Mass;
        Num      Ia   = x0.Inertia;
        Vect     va1  = x0.Velocity;
        Vect     vb1  = x1.Velocity;
        Num      wa1  = x0.AngVelocity;
        Num      wb1  = x1.AngVelocity;
        Vect     vap1 = va1             + wa1       * rap;
        Vect     vp1  = va1 + wa1 * rap - vb1 - wb1 * rbp;
        Vect     j;
        if (!x1.IsMassInf())
        {
            Num mb = x1.Mass;
            Num Ib = x1.Inertia;
            j = -(1 + e) * Vect.Dot(vp1, dir) * dir /
                (1 / ma + 1 / mb + rap * dir * (rap * dir) / Ia + rbp * dir * (rbp * dir) / Ib);
        } else { j = -(1 + e) * Vect.Dot(vap1, dir) * dir / ((1 / ma + rap * dir * (rap * dir)) / Ia); }

        particle.Acceleration = Vect.Zero;
        particle.Torque       = 0;

        // Calculate new velocity without eliminating tangential component
        Vect va2 = va1 + j / ma;
        x0.Velocity = va2 * edge; // Don't multiply by dir
        Num wa2 = wa1 + rap * Vect.Dot(j, dir) / Ia;
        x0.AngVelocity = wa2;
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
        Num      g  = gravity * particle.Mass;                                 // Gravity
        Num      R  = r;                                                 // Length
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