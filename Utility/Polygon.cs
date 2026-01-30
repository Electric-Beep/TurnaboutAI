using System;
using System.Linq;

namespace TurnaboutAI.Utility
{
    /// <summary>
    /// Represents a shape with at least 3 sides.
    /// </summary>
    public sealed class Polygon
    {
        private readonly PointF[] _points;

        /// <summary>
        /// Constructs a polygon with the given vertices.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the number of points is less than 3.</exception>
        public Polygon(PointF[] points)
        {
            if (points.Length < 3) throw new ArgumentException("A polygon must have at least 3 points.", nameof(points));

            _points = points;
        }

        /// <summary>
        /// Constructs a polygon from an <see cref="INSPECT_DATA"/> instance.
        /// </summary>
        public Polygon(INSPECT_DATA inspectData)
        {
            _points = new PointF[4]
            {
                new PointF(inspectData.x0, inspectData.y0),
                new PointF(inspectData.x1, inspectData.y1),
                new PointF(inspectData.x2, inspectData.y2),
                new PointF(inspectData.x3, inspectData.y3)
            };
        }

        /// <summary>
        /// Constructs a polygon from an <see cref="GSPoint4"/> instance.
        /// </summary>
        public Polygon(GSPoint4 point)
        {
            _points = new PointF[4]
            {
                new PointF(point.x0, point.y0),
                new PointF(point.x1, point.y1),
                new PointF(point.x2, point.y2),
                new PointF(point.x3, point.y3)
            };
        }

        public double MinX => _points.Min(p => p.X);

        /// <summary>
        /// Gets the center point of this polygon.
        /// </summary>
        public PointF Centroid()
        {
            double area = 0;
            double cx = 0;
            double cy = 0;

            for (int i = 0; i < _points.Length; i++)
            {
                var a = _points[i];
                var b = _points[(i + 1) % _points.Length];

                double c = (a.X * b.Y) - (b.X * a.Y);
                area += c;
                cx += (a.X + b.X) * c;
                cy += (a.Y + b.Y) * c;
            }

            double factor = 1.0 / (3.0 * area);

            return new PointF(
                cx * factor,
                cy * factor);
        }

        /// <summary>
        /// Checks if a point lies within this polygon.
        /// </summary>
        public bool Contains(PointF point)
        {
            double maxX = _points.Max(p => p.X);

            PointF point2 = new PointF(maxX + 1, 0);

            int intersects = 0;

            for (int i = 0; i < _points.Length; i++)
            {
                var s1 = _points[i];
                var s2 = _points[(i + 1) % _points.Length];

                if (DoesIntersect(s1, s2, point, point2)) intersects++;
            }

            return intersects % 2 != 0;
        }

        /// <summary>
        /// Checks if another polygon lies within or intersects with this polygon, or if this polygon lies within the other.
        /// </summary>
        public bool Intersects(Polygon other)
        {
            foreach (PointF p in other._points)
            {
                if (Contains(p)) return true;
            }

            foreach (PointF p in _points)
            {
                if (other.Contains(p)) return true;
            }

            for (int i = 0; i < _points.Length; i++)
            {
                var s1 = _points[i];
                var s2 = _points[(i + 1) % _points.Length];

                for (int o = 0; o < _points.Length; o++)
                {
                    var l1 = other._points[o];
                    var l2 = other._points[(o + 1) % other._points.Length];

                    if (DoesIntersect(s1, s2, l1, l2)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new polygon with all points scaled by the given factors.
        /// </summary>
        public Polygon Scale(double x, double y)
        {
            PointF[] points = new PointF[_points.Length];

            for (int i = 0; i < _points.Length; i++)
            {
                points[i] = _points[i].Scale(x, y);
            }

            return new Polygon(points);
        }

        /// <summary>
        /// Creates a new polygon translated by the given offsets.
        /// </summary>
        public Polygon Translate(double x, double y)
        {
            PointF[] points = new PointF[_points.Length];

            for (int i = 0; i < _points.Length; i++)
            {
                points[i] = _points[i].Translate(x, y);
            }

            return new Polygon(points);
        }

        public override string ToString()
        {
            return $"[{string.Join(",", _points.Select(p => p.ToString()).ToArray())}]";
        }

        private static bool OnSegment(PointF p, PointF q, PointF r)
        {
            return q.X <= Math.Max(p.X, r.X) &&
                q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) &&
                q.Y >= Math.Min(p.Y, r.Y);
        }

        private static int Orientation(PointF p, PointF q, PointF r)
        {
            double value = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);

            if (value == 0) return 0;

            return value > 0 ? 1 : 2;
        }

        private static bool DoesIntersect(PointF s1, PointF s2, PointF l1, PointF l2)
        {
            int o1 = Orientation(s1, s2, l1);
            int o2 = Orientation(s1, s2, l2);
            int o3 = Orientation(l1, l2, s1);
            int o4 = Orientation(l1, l2, s2);

            if (o1 != o2 && o3 != o4) return true;

            if (o1 == 0 && OnSegment(s1, l1, s2)) return true;
            if (o2 == 0 && OnSegment(s1, l2, s2)) return true;
            if (o3 == 0 && OnSegment(l1, s1, l2)) return true;
            if (o4 == 0 && OnSegment(l1, s2, l2)) return true;

            return false;
        }
    }
}
