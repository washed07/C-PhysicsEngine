using Microsoft.Xna.Framework.Input;
using Objects;
using Types;

namespace Forces;

public static class F
{
    public static Force Gravity(num gravity, Vector direction)
    {
        return new Force(new Vector(gravity, gravity) * direction);
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
        num m1 = particle.Mass;
        num m2 = otherParticle.Mass;
        // Dependents
        num r = Vector.Distance(particle.Position, otherParticle.Position); // Distance between particles
        if (r < 100) { r = 100; } // Capped distance to prevent spikes in force at close range        

        Vector n = Vector.Normalize
            (otherParticle.Position - particle.Position); // Direction (Sin(theta), Cos(theta))      
        num G     = 6.67430e-11f;                         // Gravitational constant
        num force = G * (m1 * m2) / (r * r);              // Total force     
        return new Force(n        * force);
    }

    public static Force FakeAttraction(Particle particle, Particle otherParticle, Force gravity) // Mutual Attraction
    {
        Vector n = Vector.Normalize(otherParticle.Position - particle.Position); // Direction (Sin(theta), Cos(theta))
        num    force = gravity.NetForce.Magnitude(); // Total force     
        return new Force(n * force);
    }

    public static Force GCursor(Particle particle, num strength)
    {
        // Force = G * (m1 * m2) / r^2
        // Force = GravitationalConstant * (Mass1 * Mass2) / Distance^2
        // explanation:
        // GravitationalConstant: constant of proportionality
        // Mass1, Mass2: masses of the particles
        // Distance: distance between the particles   

        // Constants
        num m1 = particle.Mass;
        num m2 = strength * 1e15; // Mass of cursor

        // Dependents
        Vector c = (Mouse.GetState().X, Mouse.GetState().Y); // Cursor position
        num    r = Vector.Distance(particle.Position, c); // Distance between particles
        if (r < 100) { r = 100; } // Capped distance to prevent spikes in force at close range        

        Vector n     = Vector.Normalize(c - particle.Position); // Direction (Sin(theta), Cos(theta))      
        num    G     = 6.67430e-11f;                            // Gravitational constant
        num    force = G * (m1 * m2) / (r * r);                 // Total force     
        return new Force(n           * force);
    }

    public static Force Spring(Particle particle, Particle otherParticle, num springConstant, num restLength) // Spring
    {
        // Force = -(k/m) * (S * n) - (d/m) * v
        // Force = -(springConstant / Mass) * (Stretch * Normal) - (Damping / Mass) * Velocity 
        // explanation:
        // -(springConstant / Mass): sets force relative to the particles mass
        // (Stretch * Normal): stretch of the spring relative to the normal(theta); the actual force being applied
        // -(Damping / Mass) * v: damping applied to velocity relative to the particles mass    

        // Constants
        num R = restLength;     // Rest length of spring
        num m = particle.Mass;  // Mass of particle
        num k = springConstant; // Spring constant
        num d = 0.5f;           // Damping factor 

        // Dependents
        Vector T = otherParticle.Position;                // Anchor position
        num    L = Vector.Distance(particle.Position, T); // Length of spring
        num    S = L - R;                                 // Stretch of spring
        Vector p = particle.Position;                     // Position of particle
        Vector v = particle.Velocity;                     // Velocity of particle 
        Vector n = Vector.Normalize(p - T);               // Direction (Sin(theta), Cos(theta))   
        Vector F = -(k / m) * (S * n) - d / m * v;        // Total force   
        return new Force(F);
    }

    public static Force SpringAnchor
        (Particle particle, Vector position, num springConstant, num restLength) // Anchored Spring
    {
        // See Force Spring() for explanation   

        // Constants
        num    R = restLength;     // Rest length of spring
        Vector T = position;       // Anchor position
        num    m = particle.Mass;  // Mass of particle
        num    k = springConstant; // Spring constant
        num    d = 0.5f;           // Damping factor 

        // Dependents
        num    L = Vector.Distance(particle.Position, T); // Length of spring
        num    S = L - R;                                 // Stretch of spring
        Vector p = particle.Position;                     // Position of particle
        Vector v = particle.Velocity;                     // Velocity of particle 
        Vector n = Vector.Normalize(p - T);               // Direction (Sin(theta), Cos(theta))   
        Vector F = -(k / m) * (S * n) - d / m * v;
        return new Force(F);
    }

    public static Force SCursor(Particle particle, num springConstant, num restLength)
    {
        // See Force Spring() for explanation   
        Vector T = (Mouse.GetState().X, Mouse.GetState().Y); // Anchor position
        // See Force Spring() for explanation   

        // Constants
        num R = restLength;     // Rest length of spring
        num m = particle.Mass;  // Mass of particle
        num k = springConstant; // Spring constant
        num d = 0.5f;           // Damping factor 

        // Dependents
        num    L = Vector.Distance(particle.Position, T); // Length of spring
        num    S = L - R;                                 // Stretch of spring
        Vector p = particle.Position;                     // Position of particle
        Vector v = particle.Velocity;                     // Velocity of particle 
        Vector n = Vector.Normalize(p - T);               // Direction (Sin(theta), Cos(theta))   
        Vector F = -(k / m) * (S * n) - d / m * v;
        return new Force(F * n);
    }

    public static void Impulse(Particle particleA, Particle particleB, Vector collisionPoint)
    { // TODO: Fix not working at certain angles
        num Mₐ = particleA.Mass;
        num Mᵦ = particleB.Mass;

        //Num Iₐ = particleA.Inertia;
        //Num Iᵦ = particleB.Inertia;

        Vector vₐᵢ = particleA.Velocity;
        Vector vᵦᵢ = particleB.Velocity;

        Vector Vᵣ = vᵦᵢ - vₐᵢ;
        if (Vᵣ < Vector.Zero) { return; }

        //Num ωₐᵢ = particleA.AngVelocity;
        //Num ωᵦᵢ = particleB.AngVelocity;

        //Vect rₐ = particleA.Position - collisionPoint;
        //Vect rᵦ = particleB.Position - collisionPoint;

        Vector n = particleB.Polygon.GetEdgeAtPoint(collisionPoint).Normalize();
        num    e = num.Min(particleA.Restitution, particleB.Restitution); // Coefficient of restitution

        //Vect vₐₚ = vₐᵢ + ωₐᵢ * rₐ;
        //Vect vᵦₚ = vᵦᵢ + ωᵦᵢ * rᵦ;

        //Vect vₚ = vₐᵢ + vₐᵢ * rₐ - vᵦᵢ - ωᵦᵢ * rᵦ;

        num j;

        if (particleB.IsMassInf()) { j = -(1 + e) * Vector.Dot(Vᵣ, n) / (1 / Mₐ); } else
        {
            j = -(1 + e) * Vector.Dot(Vᵣ, n) / ((1 / Mₐ) + (1 / Mᵦ));
        }

        Vector vₐ = vₐᵢ + j * n / Mₐ;
        Vector vᵦ = vᵦᵢ - j * n / Mᵦ;

        //Num ωₐ = ωₐᵢ + Vect.Dot(rₐ, j * n) / Iₐ;

        particleA.Velocity = vₐ;
        particleB.Velocity = vᵦ;
        //particleA.AngVelocity = ωₐ;
    }

    /*public static Force Normal(Particle particle, Vect edge)
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
    */

    // public static void PendulumAnchor
    /*(Particle particle, Particle otherParticle, Num gravity, Num r, float b = 1) // Anchored Pendulum
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
}*/
}