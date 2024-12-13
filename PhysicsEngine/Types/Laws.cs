using Types;
using Physics.Particles;
using Microsoft.Xna.Framework.Input;
using System.Drawing;
using System;
using Physics;
using System.Security.Cryptography.X509Certificates;

namespace Forces;

public class F
{
    public static Force Gravity(Num gravity, Vect direction) => new(new Vect(gravity, gravity) * direction); // Gravity

    public static Force Attraction (Particle particle, Particle _particle) // Mutual Attraction
    {
        // Force = G * (m1 * m2) / r^2
        // Force = GravitationalConstant * (Mass1 * Mass2) / Distance^2
        // explanation:
        // GravitationalConstant: constant of proportionality
        // Mass1, Mass2: masses of the particles
        // Distance: distance between the particles     
        // Constants
        Num m1 = particle.mass;
        Num m2 = _particle.mass;        
        // Dependents
        Num r = Vect.Distance(particle.position, _particle.position); // Distance between particles
        if (r < 100) {r = 100;} // Capped distance to prevent spikes in force at close range        
        Vect n = Vect.Normalize(_particle.position - particle.position); // Direction (Sin(theta), Cos(theta))      
        Num G = 6.67430e-11f; // Gravitational constant
        Num force = G * (m1 * m2) / (r * r); // Total force     
        return new (n * force);
    }

    public static Force gCursor (Particle particle, Num strength)
    {
        // Force = G * (m1 * m2) / r^2
        // Force = GravitationalConstant * (Mass1 * Mass2) / Distance^2
        // explanation:
        // GravitationalConstant: constant of proportionality
        // Mass1, Mass2: masses of the particles
        // Distance: distance between the particles   

        // Constants
        Num m1 = particle.mass;
        Num m2 = strength * 1e15; // Mass of cursor

        // Dependents
        Vect c = (Mouse.GetState().X, Mouse.GetState().Y); // Cursor position
        Num r = Vect.Distance(particle.position, c); // Distance between particles
        if (r < 100) {r = 100;} // Capped distance to prevent spikes in force at close range        
        Vect n = Vect.Normalize(c - particle.position); // Direction (Sin(theta), Cos(theta))      
        Num G = 6.67430e-11f; // Gravitational constant
        Num force = G * (m1 * m2) / (r * r); // Total force     
        return new (n * force);
    }

    public static Force Spring (Particle particle, Particle _particle, Num springConstant, Num restLength) // Spring
    {
        // Force = -(k/m) * (S * n) - (d/m) * v
        // Force = -(springConstant / Mass) * (Stretch * Normal) - (Damping / Mass) * Velocity 
        // explanation:
        // -(springConstant / Mass): sets force relative to the particles mass
        // (Stretch * Normal): stretch of the spring relative to the normal(theta); the actual force being applied
        // -(Damping / Mass) * v: damping applied to velocity relative to the particles mass    


        // Constants
        Num R = restLength; // Rest length of spring
        Num m = particle.mass; // Mass of particle
        Num k = springConstant; // Spring constant
        Num d = 0.5f; // Damping factor 
        
        // Dependents
        Vect T = _particle.position; // Anchor position
        Num L = Vect.Distance(particle.position, T); // Length of spring
        Num S = L - R; // Stretch of spring
        Vect p = particle.position; // Position of particle
        Vect v = particle.velocity; // Velocity of particle 
        Vect n = Vect.Normalize(p - T); // Direction (Sin(theta), Cos(theta))   
        Vect force = -(k/m) * (S * n) - (d/m) * v; // Total force   
        return new (force);
    }

    public static Force SpringAnchor (Particle particle, Vect position, Num springConstant, Num restLength) // Anchored Spring
    {
        // See Force Spring() for explanation   

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

    public static void Collision (Particle particle, Particle otherParticle, Vect collisionPoint, Force gravity) 
    {
        Particle x0 = particle;
        Particle x1 = otherParticle;

        Vect edge = x1.polygon.GetEdgeAtPoint(collisionPoint);
        Vect dir = edge.Perpendicular();  // Remove the sine multiplication

        Num e = 1f;  // Coefficient of restitution (0.5 = semi-elastic)

        Num rap = Vect.Distance(x0.position + x0.Centroid, collisionPoint);
        Num rbp = Vect.Distance(x1.position + x1.Centroid, collisionPoint);

        Num ma = x0.mass;
        Num Ia = x0.Inertia;

        Vect va1 = x0.velocity;
        Vect vb1 = x1.velocity;

        Num wa1 = x0.angularVelocity;
        Num wb1 = x1.angularVelocity;

        Vect vap1 = va1 + wa1 * rap;
        Vect vp1 = va1 + wa1 * rap - vb1 - wb1 * rbp;

        Vect j;

        if (!x1.IsMassInf()) 
        {
            Num mb = x1.mass;
            Num Ib = x1.Inertia;

            j = (-(1 + e) * Vect.Dot(vp1, dir) * dir) / (1/ma + 1/mb + ((rap * dir) * (rap * dir)) / Ia + ((rbp * dir) * (rbp * dir)) / Ib);
        }
        else
        {
            j = (-(1 + e) * Vect.Dot(vap1, dir) * dir) / ((1/ma + ((rap * dir) * (rap * dir))) / Ia);
        }

        particle.acceleration = Vect.Zero;
        particle.angularAcceleration = 0;

        // Calculate new velocity without eliminating tangential component
        Vect va2 = va1 + j / ma;
        x0.velocity = va2 * edge;  // Don't multiply by dir

        Num wa2 = wa1 + (rap * Vect.Dot(j, dir)) / Ia;
        x0.angularVelocity = wa2;
    } 

    public static Force n(Particle particle, Vect edge)
    {
        // Normal force
        // Fn = Fg * cos(theta)
        // Fn = Gravity * cos(theta)
        // explanation:
        // Fn: normal force
        // Fg: gravitational force
        // theta: angle between the normal and the gravitational force

        Vect dir = edge.Perpendicular();
        Num slant = Math.Sin(edge.x);
        Vect g = particle.velocity.Perpendicular() * slant;
        Console.WriteLine(Math.Sin(edge.x));
        return new(g * dir);
    }

    private static Num Theta = Math.PI/4;
    private static Num aVel = 0;

    public static void PendulumAnchor(Particle particle, Particle otherParticle, Num _g, Num _R, float b = 1) // Anored Pendulum
    {
        // Constants
        Num g = _g * particle.mass; // Gravity
        Num R = _R; // Length
        Particle x0 = otherParticle; // Pivot
        Particle x1  = particle; // Bob

        x1.angularAcceleration = -g/R * Math.Sin(Theta) * Engine.TimeStep; // x'' || Acceleration
        x1.angularVelocity += x1.angularAcceleration * Engine.TimeStep; // x' || Velocity
        Theta += aVel; // 0 (theta) || Angle

        aVel *= b; // Damping angular velocity

        // POSITION
        x1.position.x = x0.position.x + R * Math.Sin(Theta); 
        x1.position.y = x0.position.y - R * Math.Cos(Theta); 
    }

}