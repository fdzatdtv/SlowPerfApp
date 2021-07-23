using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SlowPerfWpfApp
{
    public static class DrawingExtensions
    {
        // Add a Rectangle to a Canvas.
        public static Rectangle DrawRectangle(this Canvas canvas,
            Brush fill, Brush stroke, double stroke_thickness,
            Rect rect)
        {
            return canvas.DrawRectangle(fill, stroke,
                stroke_thickness, rect.Left, rect.Top,
                rect.Width, rect.Height);
        }

        // Add a Rectangle to a Canvas.
        public static Rectangle DrawRectangle(this Canvas canvas,
            Brush fill, Brush stroke, double stroke_thickness,
            double left, double top, double width, double height)
        {
            Rectangle rectangle = new Rectangle();
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            rectangle.Width = width;
            rectangle.Height = height;
            rectangle.Fill = fill;
            rectangle.Stroke = stroke;
            rectangle.StrokeThickness = stroke_thickness;
            canvas.Children.Add(rectangle);
            return rectangle;
        }

        // Add an Ellipse to a Canvas.
        public static Ellipse DrawEllipse(this Canvas canvas,
            Brush fill, Brush stroke, double stroke_thickness,
            Rect rect)
        {
            return canvas.DrawEllipse(fill, stroke,
                stroke_thickness, rect.Left, rect.Top,
                rect.Width, rect.Height);
        }
        
        // Add an Ellipse to a Canvas.
        public static Ellipse DrawEllipse(this Canvas canvas,
            Brush fill, Brush stroke, double stroke_thickness,
            double left, double top, double width, double height)
        {
            Ellipse ellipse = new Ellipse();
            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);
            ellipse.Width = width;
            ellipse.Height = height;
            ellipse.Fill = fill;
            ellipse.Stroke = stroke;
            ellipse.StrokeThickness = stroke_thickness;
            canvas.Children.Add(ellipse);
            return ellipse;
        }

        // Add a point (circle centers at a location) to a Canvas.
        public static Ellipse DrawPoint(this Canvas canvas,
            Brush fill, Brush stroke, double stroke_thickness,
            Point center, double radius)
        {
            return canvas.DrawEllipse(fill, stroke,
                stroke_thickness,
                center.X - radius, center.Y - radius,
                2 * radius, 2 * radius);
        }

        // Add an Arc to a Canvas.
        public static Path DrawArc(this Canvas canvas,
            Brush fill, Brush stroke, double stroke_thickness,
            Point start_point, Point end_point, Size size,
            double rotation_angle, bool is_large_arc,
            SweepDirection sweep_direction, bool is_stroked)
        {
            // Create a Path to hold the geometry.
            Path path = new Path();
            canvas.Children.Add(path);
            path.Fill = fill;
            path.Stroke = stroke;
            path.StrokeThickness = stroke_thickness;

            // Add a PathGeometry.
            PathGeometry path_geometry = new PathGeometry();
            path.Data = path_geometry;

            // Create a PathFigure.
            PathFigure path_figure = new PathFigure();
            path_geometry.Figures.Add(path_figure);

            // Start at the first point.
            path_figure.StartPoint = start_point;

            // Create a PathSegmentCollection.
            PathSegmentCollection path_segment_collection =
                new PathSegmentCollection();
            path_figure.Segments = path_segment_collection;

            // Create the ArcSegment.
            ArcSegment arc_segment = new ArcSegment(
                end_point, size, rotation_angle,
                is_large_arc, sweep_direction, is_stroked);
            path_segment_collection.Add(arc_segment);

            return path;
        }

        // Draw an elliptical arc. Return the end points.
        public static Path DrawArc(this Canvas canvas,
            Brush fill, Brush stroke, double stroke_thickness,
            Rect rect, double angle1, double angle2,
            bool is_large_arc, SweepDirection sweep_direction,
            out Point point1, out Point point2)
        {
            Point[] points = FindEllipsePoints(
                rect, angle1, angle2);
            point1 = points[0];
            point2 = points[1];

            Size size = new Size(rect.Width / 2, rect.Height / 2);
            return canvas.DrawArc(
                fill, stroke, stroke_thickness,
                points[0], points[1], size, 0, is_large_arc,
                sweep_direction, true);
        }

        // Find the points of on an ellipse
        // at the indicated angles from is center.
        private static Point[] FindEllipsePoints(
            Rect rect, double angle1, double angle2)
        {
            // Find the ellipse's center.
            Point center = new Point(
                rect.X + rect.Width / 2.0,
                rect.Y + rect.Height / 2.0);

            // Find segments from the center in the
            // desired directions and long enough to
            // cut the ellipse.
            double dist = rect.Width + rect.Height;
            Point pt1 = new Point(
                center.X + dist * Math.Cos(angle1),
                center.Y + dist * Math.Sin(angle1));
            Point pt2 = new Point(
                center.X + dist * Math.Cos(angle2),
                center.Y + dist * Math.Sin(angle2));

            // Find the points of intersection.
            Point[] intersections1 =
                FindEllipseSegmentIntersections(
                    rect, center, pt1, true);
            Point[] intersections2 =
                FindEllipseSegmentIntersections(
                    rect, center, pt2, true);
            return new Point[]
            {
                intersections1[0],
                intersections2[0]
            };
        }

        // Find the points of intersection between
        // an ellipse and a line segment.
        private static Point[] FindEllipseSegmentIntersections(
            Rect rect, Point pt1, Point pt2, bool segment_only)
        {
            // If the ellipse or line segment are empty, return no intersections.
            if ((rect.Width == 0) || (rect.Height == 0) ||
                ((pt1.X == pt2.X) && (pt1.Y == pt2.Y)))
                return new Point[] { };

            // Make sure the rectangle has non-negative width and height.
            if (rect.Width < 0)
            {
                rect.X = rect.Right;
                rect.Width = -rect.Width;
            }
            if (rect.Height < 0)
            {
                rect.Y = rect.Bottom;
                rect.Height = -rect.Height;
            }

            // Translate so the ellipse is centered at the origin.
            double cx = rect.Left + rect.Width / 2f;
            double cy = rect.Top + rect.Height / 2f;
            rect.X -= cx;
            rect.Y -= cy;
            pt1.X -= cx;
            pt1.Y -= cy;
            pt2.X -= cx;
            pt2.Y -= cy;

            // Get the semimajor and semiminor axes.
            double a = rect.Width / 2;
            double b = rect.Height / 2;

            // Calculate the quadratic parameters.
            double A = (pt2.X - pt1.X) * (pt2.X - pt1.X) / a / a +
                       (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y) / b / b;
            double B = 2 * pt1.X * (pt2.X - pt1.X) / a / a +
                       2 * pt1.Y * (pt2.Y - pt1.Y) / b / b;
            double C = pt1.X * pt1.X / a / a + pt1.Y * pt1.Y / b / b - 1;

            // Make a list of t values.
            List<double> t_values = new List<double>();

            // Calculate the discriminant.
            double discriminant = B * B - 4 * A * C;
            if (discriminant == 0)
            {
                // One real solution.
                t_values.Add(-B / 2 / A);
            }
            else if (discriminant > 0)
            {
                // Two real solutions.
                t_values.Add((double)((-B + Math.Sqrt(discriminant)) / 2 / A));
                t_values.Add((double)((-B - Math.Sqrt(discriminant)) / 2 / A));
            }

            // Convert the t values into points.
            List<Point> points = new List<Point>();
            foreach (double t in t_values)
            {
                // If the points are on the segment (or we
                // don't care if they are), add them to the list.
                if (!segment_only || ((t >= 0f) && (t <= 1f)))
                {
                    double x = pt1.X + (pt2.X - pt1.X) * t + cx;
                    double y = pt1.Y + (pt2.Y - pt1.Y) * t + cy;
                    points.Add(new Point(x, y));
                }
            }

            // Return the points.
            return points.ToArray();
        }

        // Draw an elliptical pie slice. Return the end points.
        public static Path DrawPieSlice(this Canvas canvas,
            Brush fill, Brush stroke, double stroke_thickness,
            Rect rect, double angle1, double angle2,
            bool is_large_arc, SweepDirection sweep_direction,
            out Point point1, out Point point2)
        {
            // Draw the arc.
            Path path = canvas.DrawArc(fill, stroke,
                stroke_thickness, rect, angle1, angle2,
                is_large_arc, sweep_direction,
                out point1, out point2);

            // Find the path's PathFigure collection.
            PathGeometry path_geometry = (PathGeometry)path.Data;
            PathFigureCollection path_figure_collection =
                path_geometry.Figures;
            PathFigure path_figure = path_figure_collection[0];
            PathSegmentCollection path_segment_collection =
                path_figure.Segments;

            // Add the pie slice edges.
            Point center = new Point(
                (rect.Left + rect.Right) / 2,
                (rect.Top + rect.Bottom) / 2);
            LineSegment line_seg1 =
                new LineSegment(center, true);
            path_segment_collection.Add(line_seg1);
            
            LineSegment line_seg2 = 
                new LineSegment(point1, true);
            path_segment_collection.Add(line_seg2);

            return path;
        }

        public static Line DrawLine(this Canvas canvas,
            Brush stroke, double stroke_thickness,
            Point pt1, Point pt2)
        {
            var line = new Line();
            line.X1 = pt1.X;
            line.Y1 = pt1.Y;
            line.X2 = pt2.X;
            line.Y2 = pt2.Y;
            line.Stroke = stroke;
            line.StrokeThickness = stroke_thickness;
            canvas.Children.Add(line);
            return line;
        }
    }
}
