using System;
using System.Linq;

namespace Types;

public struct Shape
{
    public static Polygon Square(int width, num height)
    {
        Vector[] vertices =
        {
            new Vector(0, 0),
            new Vector(width, 0),
            new Vector(width, height),
            new Vector(0, height)
        };
        return new Polygon(vertices);
    }

    public static Polygon Triangle(num sideLength)
    {
        Vector[] vertices =
        {
            new Vector(sideLength / 2, 0),
            new Vector(sideLength, sideLength),
            new Vector(0, sideLength)
        };
        return new Polygon(vertices);
    }

    public static Polygon IsoTriangle(num width, num height)
    {
        Vector[] vertices =
        {
            new Vector(width / 2, 0),
            new Vector(width, height),
            new Vector(0, height)
        };
        return new Polygon(vertices);
    }

    public static Polygon Circle(num radius, int sides = 16) // Very performance heavy
    {
        Vector[] vertices = new Vector[sides];
        for (int i = 0; i < sides; i++)
        {
            float angle = num.PI                    * 2                      * i / sides;
            vertices[i] = new Vector(num.Cos(angle) * radius, num.Sin(angle) * radius);
        }

        return new Polygon(vertices);
    }
}

public static class Extensions
{
    public static float ToRadians(this float degrees) { return degrees * num.PI / 180f; }
}

public class Polygon(Vector[] vertices, Vector position = default(Vector))
{
    public  Vector[] Vertices { get; }      = vertices;
    public  Vector   Position { get; set; } = position;
    private Vector   Pivot    { get; set; } = (0, 0);                       // Added pivot property
    private num      Rotation { get; set; } = 0f;                           // Added rotation property
    public  num      Radius   => Vertices.Select(v => v.Magnitude()).Max(); // Added radius property

    public Vector[] GetAxes()
    {
        Vector[] axes             = new Vector[Vertices.Length];
        Vector[] transformedVerts = TransformedVertices();

        for (int i = 0; i < transformedVerts.Length; i++)
        {
            Vector current = transformedVerts[i];
            Vector next    = transformedVerts[(i + 1) % transformedVerts.Length];

            // Calculate edge vector
            Vector edge = next - current;
            // Get perpendicular vector (normal)
            Vector normal = new Vector(-edge.y, edge.x);
            axes[i] = Vector.Normalize(normal);
        }

        return axes;
    }

    public (float min, float max) Project(Vector axis)
    {
        if (axis.SqrMagnitude() < 0.0001f) { return (0, 0); }

        Vector normalizedAxis = Vector.Normalize(axis);
        float  min            = float.MaxValue;
        float  max            = float.MinValue;

        foreach (Vector vertex in TransformedVertices())
        {
            float projection = Vector.Dot(vertex, normalizedAxis);
            min = num.Min(min, projection);
            max = num.Max(max, projection);
        }

        return (min, max);
    }

    public void Rotate(num angle, Vector pivot = default(Vector))
    {
        Rotation = angle.ToRadians(); // Convert degrees to radians
        Pivot    = pivot != default(Vector) ? pivot : GetCentroid();
    }

    public Vector[] TransformedVertices()
    {
        Vector[] transformedVertices = new Vector[Vertices.Length];

        // Cache trigonometric calculations
        num   cos = num.Cos(Rotation);
        float sin = num.Sin(Rotation);

        for (int i = 0; i < Vertices.Length; i++)
        {
            // Translate to origin
            Vector vertex = Vertices[i] - Pivot;

            // Rotate
            Vector rotated = new Vector(vertex.x * cos - vertex.y * sin, vertex.x * sin + vertex.y * cos);

            // Translate back and apply position
            transformedVertices[i] = rotated + Pivot + Position;
        }

        return transformedVertices;
    }

    public Vector GetCentroid()
    {
        float centroidX = 0, centroidY = 0;
        foreach (Vector vertex in Vertices)
        {
            centroidX += vertex.x;
            centroidY += vertex.y;
        }

        return new Vector(centroidX / Vertices.Length, centroidY / Vertices.Length);
    }

    public static double CalculateMomentOfInertia(Vector[] vertices, double mass)
    {
        if (vertices.Length < 3) { throw new ArgumentException("Polygon must have at least 3 vertices"); }

        Vector centroid             = CalculateCentroid(vertices);
        double totalMomentOfInertia = 0;
        double totalArea            = CalculatePolygonArea(vertices);

        // Handle concave polygons by ensuring positive area triangles
        for (int i = 1; i < vertices.Length - 1; i++)
        {
            Vector[] triangle =
            {
                vertices[0],
                vertices[i],
                vertices[i + 1]
            };

            double triangleArea = Math.Abs(CalculateTriangleArea(triangle));
            double triangleMass = mass * (triangleArea / totalArea);

            Vector triangleCentroid = CalculateTriangleCentroid(triangle);
            double triangleMoment   = CalculateTriangleMomentOfInertia(triangle, triangleMass);

            double distanceSquared = Vector.DistanceSqr(triangleCentroid, centroid);
            totalMomentOfInertia += triangleMass * distanceSquared + triangleMoment;
        }

        return totalMomentOfInertia;
    }

    private static Vector CalculateCentroid(Vector[] vertices)
    {
        double sumX = 0, sumY = 0;
        foreach (Vector vertex in vertices)
        {
            sumX += vertex.x;
            sumY += vertex.y;
        }

        return new Vector((float)(sumX / vertices.Length), (float)(sumY / vertices.Length));
    }

    private static Vector CalculateTriangleCentroid(Vector[] triangle)
    {
        return new Vector
            ((triangle[0].x + triangle[1].x + triangle[2].x) / 3, (triangle[0].y + triangle[1].y + triangle[2].y) / 3);
    }

    private static double CalculatePolygonArea(Vector[] vertices)
    {
        if (vertices == null || vertices.Length < 3)
        {
            throw new ArgumentException("Invalid polygon: needs at least 3 vertices");
        }

        double area = 0;
        int    j    = vertices.Length - 1;

        // Shoelace formula (aka surveyor's formula)
        for (int i = 0; i < vertices.Length; j = i++)
        {
            area += vertices[j].x * vertices[i].y - vertices[i].x * vertices[j].y;
        }

        return Math.Abs(area / 2);
    }

    private static double CalculateTriangleArea(Vector[] triangle)
    {
        if (triangle == null || triangle.Length != 3) { throw new ArgumentException("Invalid triangle array"); }

        // Using vector cross product for better numerical stability
        Vector v1 = triangle[1] - triangle[0];
        Vector v2 = triangle[2] - triangle[0];

        return num.Abs(v1.x * v2.y - v1.y * v2.x) / 2;
    }

    private static double CalculateTriangleMomentOfInertia(Vector[] triangle, double triangleMass)
    {
        if (triangle == null || triangle.Length != 3) { throw new ArgumentException("Invalid triangle array"); }

        if (triangleMass <= 0) { return 0; }

        // Calculate triangle sides
        double a = Vector.Distance(triangle[0], triangle[1]);
        double b = Vector.Distance(triangle[1], triangle[2]);
        double c = Vector.Distance(triangle[2], triangle[0]);

        // Calculate area
        double s    = (a + b + c) / 2; // semi-perimeter
        double area = num.Sqrt(s * (s - a) * (s - b) * (s - c));

        // Moment of inertia about centroid
        // Formula: (mass * (a² + b² + c²)) / 36
        return triangleMass * (a * a + b * b + c * c) / 36.0;
    }

    public Vector GetEdgeAtPoint(Vector point)
    {
        Vector[] vertices  = TransformedVertices();
        float    minDistSq = float.MaxValue;
        Vector   edge      = default(Vector);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector v1 = vertices[i];
            Vector v2 = vertices[(i + 1) % vertices.Length];

            // Calculate point-to-line-segment distance squared
            Vector lineVec      = v2    - v1;
            Vector pointVec     = point - v1;
            num    lineLengthSq = lineVec.SqrMagnitude();

            // Project point onto line segment
            num    t          = num.Max(0, num.Min(1, Vector.Dot(pointVec, lineVec) / lineLengthSq));
            Vector projection = v1 + lineVec * t;
            num    distSq     = Vector.DistanceSqr(point, projection);

            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                edge      = lineVec;
            }
        }

        return edge;
    }
}