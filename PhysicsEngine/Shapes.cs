using System;
using System.Linq;
using System.Numerics;
using Types;

namespace Shapes;

public struct Shape
{
    public static Polygon Square(int width, Num height)
    {
        Vect[] vertices =
        [
            new Vect(0, 0),
            new Vect(width, 0),
            new Vect(width, height),
            new Vect(0, height)
        ];
        return new Polygon(vertices);
    }

    public static Polygon Triangle(Num sideLength)
    {
        Vect[] vertices =
        [
            new Vect(sideLength / 2, 0),
            new Vect(sideLength, sideLength),
            new Vect(0, sideLength)
        ];
        return new Polygon(vertices);
    }

    public static Polygon IsoTriangle(Num width, Num height)
    {
        Vect[] vertices =
        [
            new Vect(width / 2, 0),
            new Vect(width, height),
            new Vect(0, height)
        ];
        return new Polygon(vertices);
    }

    public static Polygon Circle(Num radius, int sides = 16) // Very performance heavy
    {
        Vect[] vertices = new Vect[sides];
        for (int i = 0; i < sides; i++)
        {
            float angle = MathF.PI                  * 2                        * i / sides;
            vertices[i] = new Vect(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
        }

        return new Polygon(vertices);
    }
}

public static class Extensions
{
    public static float ToRadians(this float degrees) { return degrees * MathF.PI / 180f; }
}

public class Polygon(Vect[] vertices, Vect position = default(Vect))
{
    public  Vect[] Vertices { get; }      = vertices;
    public  Vect   Position { get; set; } = position;
    private Vect   Pivot    { get; set; } = (0, 0); // Added pivot property
    private Num    Rotation { get; set; } = 0f; // Added rotation property
    public  Num    Radius   => Math.Max(Vertices.Select(v => (float)v.Magnitude()).Max(), 0f); // Added radius property

    public Vect[] GetAxes()
    {
        Vect[] axes             = new Vect[Vertices.Length];
        Vect[] transformedVerts = TransformedVertices();

        for (int i = 0; i < transformedVerts.Length; i++)
        {
            Vect current = transformedVerts[i];
            Vect next    = transformedVerts[(i + 1) % transformedVerts.Length];

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
        if (axis.SqrMagnitude() < 0.0001f) { return (0, 0); }

        Vect  normalizedAxis = Vect.Normalize(axis);
        float min            = float.MaxValue;
        float max            = float.MinValue;

        foreach (Vect vertex in TransformedVertices())
        {
            float projection = Vect.Dot(vertex, normalizedAxis);
            min = Math.Min(min, projection);
            max = Math.Max(max, projection);
        }

        return (min, max);
    }

    public void Rotate(Num angle, Vect pivot = default(Vect))
    {
        Rotation = angle.Radians(); // Convert degrees to radians
        Pivot    = pivot != default(Vect) ? pivot : GetCentroid();
    }

    public Vect[] TransformedVertices()
    {
        Vect[] transformedVertices = new Vect[Vertices.Length];

        // Cache trigonometric calculations
        float cos = MathF.Cos(Rotation);
        float sin = MathF.Sin(Rotation);

        for (int i = 0; i < Vertices.Length; i++)
        {
            // Translate to origin
            Vect vertex = Vertices[i] - Pivot;

            // Rotate
            Vect rotated = new Vect(vertex.x * cos - vertex.y * sin, vertex.x * sin + vertex.y * cos);

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
        if (vertices.Length < 3) { throw new ArgumentException("Polygon must have at least 3 vertices"); }

        Vect   centroid             = CalculateCentroid(vertices);
        double totalMomentOfInertia = 0;
        double totalArea            = CalculatePolygonArea(vertices);

        // Handle concave polygons by ensuring positive area triangles
        for (int i = 1; i < vertices.Length - 1; i++)
        {
            Vect[] triangle =
            [
                vertices[0],
                vertices[i],
                vertices[i + 1]
            ];

            double triangleArea = Math.Abs(CalculateTriangleArea(triangle));
            double triangleMass = mass * (triangleArea / totalArea);

            Vect   triangleCentroid = CalculateTriangleCentroid(triangle);
            double triangleMoment   = CalculateTriangleMomentOfInertia(triangle, triangleMass);

            double distanceSquared = Vect.DistanceSqr(triangleCentroid, centroid);
            totalMomentOfInertia += triangleMass * distanceSquared + triangleMoment;
        }

        return totalMomentOfInertia;
    }

    private static Vect CalculateCentroid(Vect[] vertices)
    {
        double sumX = 0, sumY = 0;
        foreach (Vect vertex in vertices)
        {
            sumX += vertex.x;
            sumY += vertex.y;
        }

        return new Vect((float)(sumX / vertices.Length), (float)(sumY / vertices.Length));
    }

    private static Vect CalculateTriangleCentroid(Vect[] triangle)
    {
        return new Vect
            ((triangle[0].x + triangle[1].x + triangle[2].x) / 3, (triangle[0].y + triangle[1].y + triangle[2].y) / 3);
    }

    private static double CalculatePolygonArea(Vect[] vertices)
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

    private static double CalculateTriangleArea(Vect[] triangle)
    {
        if (triangle == null || triangle.Length != 3) { throw new ArgumentException("Invalid triangle array"); }

        // Using vector cross product for better numerical stability
        Vect v1 = triangle[1] - triangle[0];
        Vect v2 = triangle[2] - triangle[0];

        return Num.Abs(v1.x * v2.y - v1.y * v2.x) / 2;
    }

    private static double CalculateTriangleMomentOfInertia(Vect[] triangle, double triangleMass)
    {
        if (triangle == null || triangle.Length != 3) { throw new ArgumentException("Invalid triangle array"); }

        if (triangleMass <= 0) { return 0; }

        // Calculate triangle sides
        double a = Vector2.Distance(triangle[0], triangle[1]);
        double b = Vector2.Distance(triangle[1], triangle[2]);
        double c = Vector2.Distance(triangle[2], triangle[0]);

        // Calculate area
        double s    = (a + b + c) / 2; // semi-perimeter
        double area = Math.Sqrt(s * (s - a) * (s - b) * (s - c));

        // Moment of inertia about centroid
        // Formula: (mass * (a² + b² + c²)) / 36
        return triangleMass * (a * a + b * b + c * c) / 36.0;
    }

    public Vect GetEdgeAtPoint(Vect point)
    {
        Vect[] vertices  = TransformedVertices();
        float  minDistSq = float.MaxValue;
        Vect   edge      = default(Vect);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vect v1 = vertices[i];
            Vect v2 = vertices[(i + 1) % vertices.Length];

            // Calculate point-to-line-segment distance squared
            Vect lineVec      = v2    - v1;
            Vect pointVec     = point - v1;
            Num  lineLengthSq = lineVec.SqrMagnitude();

            // Project point onto line segment
            Num  t          = Num.Max(0, Num.Min(1, Vect.Dot(pointVec, lineVec) / lineLengthSq));
            Vect projection = v1 + lineVec * t;
            Num  distSq     = Vect.DistanceSqr(point, projection);

            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                edge      = lineVec;
            }
        }

        return edge;
    }
}

public static class Collision
{
    public class MTV // Minimum Translation Vector
        (Vect axis, float overlap, Vect point)
    {
        public Vect  Axis    { get; set; } = axis;
        public float Overlap { get; set; } = overlap;
        public Vect  Point   { get; set; } = point; // New collision point property
    }

    public static MTV Resolve(Polygon poly1, Polygon poly2)
    {
        Vector2[] axes           = [.. poly1.GetAxes(), .. poly2.GetAxes()];
        float     minOverlap     = float.MaxValue;
        Vect      smallestAxis   = Vector2.Zero;

        foreach (Vector2 axis in axes)
        {
            (float min1, float max1) = poly1.Project(axis);
            (float min2, float max2) = poly2.Project(axis);

            float overlap = Math.Min(max1 - min2, max2 - min1);

            if (max1 < min2 || max2 < min1) { return null; }

            if (!(overlap < minOverlap)) { continue; }

            minOverlap   = overlap;
            smallestAxis = axis;

            Vector2 center1 = poly1.GetCentroid() + poly1.Position;
            Vector2 center2 = poly2.GetCentroid() + poly2.Position;
            if (Vector2.Dot(center2 - center1, smallestAxis) < 0) { smallestAxis = -smallestAxis; }
        }

        // Calculate collision point
        Vect[] vertices1 = poly1.TransformedVertices();
        Vect[] vertices2 = poly2.TransformedVertices();

        // Find the deepest penetrating vertices
        Vect deepestPoint1 = vertices1.OrderBy(v => Vector2.Dot(v, smallestAxis)).First();
        Vect deepestPoint2 = vertices2.OrderByDescending(v => Vector2.Dot(v, smallestAxis)).First();

        // Use the midpoint as the collision point
        Vect collisionPoint = (deepestPoint1 + deepestPoint2) * 0.5f;

        return new MTV(smallestAxis, minOverlap, collisionPoint);
    }
}