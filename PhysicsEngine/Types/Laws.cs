using Types;
using Physics.Particles;
using Microsoft.Xna.Framework.Input;
using System.Drawing;
using System;
using Physics;

namespace Forces;

public class F
{
    public static Force g(Num gravity, Vect direction) => new((gravity, gravity) * direction); // Gravity

    public static Force gPull (Particle particle, Particle _particle) // Mutual Attraction
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

    public static Force s (Particle particle, Particle _particle, Num springConstant, Num restLength) // Spring
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

    public static Force sAnchor (Particle particle, Vect position, Num springConstant, Num restLength) // Anchored Spring
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

    public static Force sCursor (Particle particle, Num springConstant, Num restLength) // Cursor Spring
    {
        // See Force Spring() for explanation   

        // Constants
        Num R = restLength; // Rest length of spring
        Num m = 1; // Mass of particle
        Num k = springConstant; // Spring constant
        Num d = 0.1f; // Damping factor 

        // Dependents
        Vect T = (Mouse.GetState().X, Mouse.GetState().Y); // Cursor position
        Num L = Vect.Distance(particle.position, T); // Length of spring
        Num S = L - R; // Stretch of spring
        Vect p = particle.position; // Position of particle
        Vect v = particle.velocity; // Velocity of particle 
        Vect n = Vect.Normalize(p - T); // Direction (Sin(theta), Cos(theta))   
        Vect force = -(k/m) * (S * n) - v;

        return new (force * particle.mass);
    }

    public static Force t(Particle particle, Particle otherParticle, Num _g, Num _R)
    {

        // Constants
        Num g = _g; // Gravity
        Num R = _R; // Length of torque arm
        Num m = particle.mass; // Mass

        // Dependents
        Vect p = particle.position; // 
        Vect b = otherParticle.position;

        // Direction control
        Vect d = p - b; particle.position = b + R * Vect.Normalize(d); //
        Num theta = d.x/R;

        Num xdir = d.x/Math.Abs(d.x);
        Vect n = Vect.Normalize((d.y * xdir, -d.x * xdir));

        Num I = m * Math.Pow(R, 2); // Intertia = m * R^2
        Num a = -g/R * Math.Sin(theta);

        Num torque = -R * m * g * Math.Sin(theta);

        Vect force = torque * n;
        
        return new(force);
    }

    private static Num Theta = Math.PI/4;
    private static Num aVel = 0;

    public static modForce p(Particle particle, Particle otherParticle, Num _g, Num _R, float b = 1)
    {
        Num g = _g; // Gravity
        Num R = _R; // Length
        Vect P = otherParticle.position; // Pivot position

        Num aForce = g * Math.Sin(Theta) / R; // Applied acceleration relitive to angle of the pendulum Against Vect.Down (0, 1)

        // KINEMATICS
        Num aAccel = -1 * aForce * Engine.TimeStep; // Acceleration of the bob (create new variable so its reset to 0 every iteration)
        aVel += aAccel; // Velocity of the bob
        Theta += aVel; // Angle of the bob

        aVel *= b; // Damping

        // Final position
        particle.position.x = R * Math.Sin(Theta) + P.x;
        particle.position.y = R * Math.Cos(Theta) + P.y;

        return new();
    }

}