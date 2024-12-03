using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Types;

namespace Shapes;

public struct MTV // Minimum Translation Vector
{
    public Vect Axis { get; set; }
    public float Overlap { get; set; }
    public Vect Point { get; set; }  // Add collision point

    public MTV(Vector2 axis, float overlap, Vector2 point)
    {
        Axis = axis;
        Overlap = overlap;
        Point = point;
    }
}
