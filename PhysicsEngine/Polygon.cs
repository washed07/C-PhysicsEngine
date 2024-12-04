using System;
using System.Numerics;
using System.Linq;
using Types;

namespace Shapes;

public struct CreateShape
{
    public static Polygon Square(Num Width, Num Height)
    {
        Vect[] vertices = new Vect[]
        {
            new Vect(0, 0),
            new Vect(Width, 0),
            new Vect(Width, Height),
            new Vect(0, Height)
        };
        return new Polygon(vertices);
    }

    public static Polygon Triangle(Num SideLength)
    {
        Vect[] vertices = new Vect[]
        {
            new Vect(SideLength / 2, 0),
            new Vect(SideLength, SideLength),
            new Vect(0, SideLength)
        };
        return new Polygon(vertices);
    }

    public static Polygon IsoTriangle(Num Width, Num Height)
    {
        Vect[] vertices = new Vect[]
        {
            new Vect(Width / 2, 0),
            new Vect(Width, Height),
            new Vect(0, Height)
        };
        return new Polygon(vertices);
    }

    public static Polygon Circle(Num Radius, int Sides = 16) // Very performance heavy
    {
        Vect[] vertices = new Vect[Sides];
        for (int i = 0; i < Sides; i++)
        {
            float angle = MathF.PI * 2 * i / Sides;
            vertices[i] = new Vect(
                MathF.Cos(angle) * Radius,
                MathF.Sin(angle) * Radius
            );
        }
        return new Polygon(vertices);
    }

}



public class Polygon(Vect[] vertices, Vect position = default)
{
    public Vect[] Vertices { get; private set; } = vertices;
    public Vect Position { get; set; } = position;
    public Vect Pivot { get; set; } = position; // Added pivot property
    public Num Rotation { get; set; } = 0f;  // Added rotation property
    public Num radius => Math.Max(Vertices.Select(v => (float)v.Magnitude()).Max(), 0f); // Added radius property

    public Vect[] GetAxes()
    {
        Vect[] axes = new Vect[Vertices.Length];
        Vect[] transformedVerts = TransformedVertices();
        
        for (int i = 0; i < transformedVerts.Length; i++)
        {
            Vect current = transformedVerts[i];
            Vect next = transformedVerts[(i + 1) % transformedVerts.Length];

            // Calculate edge vector
            Vect edge = next - current;
            // Get perpendicular vector (normal)
            Vect normal = new Vect(-edge.y, edge.x);
            axes[i] = Vect.Normalize(normal);
        }

        return axes;
    }

    public (float min, float max) Project(Vect axis)
    {
        if (axis.SqrMagnitude() < 0.0001f) return (0, 0);
        
        Vect normalizedAxis = Vect.Normalize(axis);
        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (Vect vertex in TransformedVertices())
        {
            float projection = Vect.Dot(vertex, normalizedAxis);
            min = Math.Min(min, projection);
            max = Math.Max(max, projection);
        }

        return (min, max);
    }

    public void Rotate(Num angle, Vect pivot = default)
    {
        Rotation = angle;
        if (pivot != default)
        {
            Pivot = pivot;
        }
        else
        {
            Pivot = GetCentroid();
        }
    }

    public Vect[] TransformedVertices()
    {
        Vect[] transformedVertices = new Vect[Vertices.Length];
        
        for (int i = 0; i < Vertices.Length; i++)
        {
            // Translate to origin
            Vect vertex = Vertices[i] - Pivot;
            
            // Rotate
            float cos = MathF.Cos(Rotation);
            float sin = MathF.Sin(Rotation);
            Vect rotated = new(
                vertex.x * cos - vertex.y * sin,
                vertex.x * sin + vertex.y * cos
            );
            
            // Translate back and apply position
            transformedVertices[i] = rotated + Pivot + Position;
        }
        return transformedVertices;
    }

    public Vect GetCentroid()
    {
        float centroidX = 0, centroidY = 0;
        foreach (Vect vertex in Vertices)
        {
            centroidX += vertex.x;
            centroidY += vertex.y;
        }
        return new Vect(centroidX / Vertices.Length, centroidY / Vertices.Length);
    }

    public static double CalculateMomentOfInertia(Vect[] vertices, double mass)
    {
        if (vertices.Length < 3)
            throw new ArgumentException("Polygon must have at least 3 vertices");
            
        Vect centroid = CalculateCentroid(vertices);
        double totalMomentOfInertia = 0;
        double totalArea = CalculatePolygonArea(vertices);

        // Handle concave polygons by ensuring positive area triangles
        for (int i = 1; i < vertices.Length - 1; i++)
        {
            Vect[] triangle = new Vect[]
            {
                vertices[0],
                vertices[i],
                vertices[i + 1]
            };

            double triangleArea = Math.Abs(CalculateTriangleArea(triangle));
            double triangleMass = mass * (triangleArea / totalArea);
            
            Vect triangleCentroid = CalculateTriangleCentroid(triangle);
            double triangleMoment = CalculateTriangleMomentOfInertia(triangle, triangleMass);
            
            double distanceSquared = Vect.DistanceSqr(triangleCentroid, centroid);
            totalMomentOfInertia += triangleMass * distanceSquared + triangleMoment;
        }

        return totalMomentOfInertia;
    }

    private static Vect CalculateCentroid(Vect[] vertices)
    {
        double sumX = 0, sumY = 0;
        foreach (var vertex in vertices)
        {
            sumX += vertex.x;
            sumY += vertex.y;
        }
        return new Vect((float)(sumX / vertices.Length), (float)(sumY / vertices.Length));
    }

    private static Vect CalculateTriangleCentroid(Vect[] triangle)
    {
        return new Vect(
            (triangle[0].x + triangle[1].x + triangle[2].x) / 3,
            (triangle[0].y + triangle[1].y + triangle[2].y) / 3
        );
    }

    private static double CalculatePolygonArea(Vect[] vertices)
    {
        if (vertices == null || vertices.Length < 3)
            throw new ArgumentException("Invalid polygon: needs at least 3 vertices");

        double area = 0;
        int j = vertices.Length - 1;

        // Shoelace formula (aka surveyor's formula)
        for (int i = 0; i < vertices.Length; j = i++)
        {
            area += vertices[j].x * vertices[i].y - vertices[i].x * vertices[j].y;
        }

        return Math.Abs(area / 2);
    }

    private static double CalculateTriangleArea(Vect[] triangle)
    {
        if (triangle == null || triangle.Length != 3)
            throw new ArgumentException("Invalid triangle array");

        // Using vector cross product for better numerical stability
        Vect v1 = triangle[1] - triangle[0];
        Vect v2 = triangle[2] - triangle[0];
        
        return Math.Abs(v1.x * v2.y - v1.y * v2.x) / 2;
    }

    private static double CalculateTriangleMomentOfInertia(Vect[] triangle, double triangleMass)
    {
        if (triangle == null || triangle.Length != 3)
            throw new ArgumentException("Invalid triangle array");
        if (triangleMass <= 0)
            throw new ArgumentException("Mass must be positive");

        // Calculate triangle sides
        double a = Vector2.Distance(triangle[0], triangle[1]);
        double b = Vector2.Distance(triangle[1], triangle[2]);
        double c = Vector2.Distance(triangle[2], triangle[0]);
        
        // Calculate area
        double s = (a + b + c) / 2; // semi-perimeter
        double area = Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        
        // Moment of inertia about centroid
        // Formula: (mass * (a² + b² + c²)) / 36
        return (triangleMass * (a * a + b * b + c * c)) / 36.0;
    }

}