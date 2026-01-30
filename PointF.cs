using System;

namespace TurnaboutAI
{
    /// <summary>
    /// Represents a point in space.
    /// </summary>
    public readonly struct PointF : IEquatable<PointF>
    {
        private readonly double _x;
        private readonly double _y;

        /// <summary>
        /// Constructs a point from the given coordinates.
        /// </summary>
        public PointF(double x, double y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// X coordinate.
        /// </summary>
        public double X => _x;

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public double Y => _y;

        /// <summary>
        /// Calculates the distance between this point and another.
        /// </summary>
        public double Distance(PointF other)
        {
            double x = Math.Pow(other.X - _x, 2);
            double y = Math.Pow(other.Y - _y, 2);
            return Math.Sqrt(x + y);
        }

        /// <summary>
        /// Creates a new point that is scaled by the given factor.
        /// </summary>
        public PointF Scale(double factor)
        {
            return Scale(factor, factor);
        }

        /// <summary>
        /// Creates a new point that is scaled by the given factors.
        /// </summary>
        public PointF Scale(double x, double y)
        {
            return new PointF(_x * x, _y * y);
        }

        /// <summary>
        /// Creates a new point that is translated by the given offsets.
        /// </summary>
        public PointF Translate(double x, double y)
        {
            return new PointF(_x + x, _y + y);
        }

        public override string ToString()
        {
            return $"({_x},{_y})";
        }

        public bool Equals(PointF other)
        {
            return other._x == _x && other._y == _y;
        }

        public override bool Equals(object obj)
        {
            if (obj is PointF p) return Equals(p);

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_x, _y);
        }

        public static bool operator ==(PointF left, PointF right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PointF left, PointF right)
        {
            return !(left == right);
        }
    }
}
