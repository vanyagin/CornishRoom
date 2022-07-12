using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CornishRoom
{
    public class Light         
    {
        public Point point_light;   
        public Point color_light;

        public Light(Point p, Point c)
        {
            point_light = new Point(p);
            color_light = new Point(c);
        }

        public Point Shade(Point hit_point, Point normal, Point material_color, float diffuse_coef)
        {
            Point dir = point_light - hit_point;
            dir = Point.norm(dir);
            Point diff = diffuse_coef * color_light * Math.Max(Point.scalar(normal, dir), 0);
            return new Point(diff.x * material_color.x, diff.y * material_color.y, diff.z * material_color.z);
        }
    }
}
