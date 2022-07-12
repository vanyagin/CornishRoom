using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CornishRoom
{
    public class Ray
    {
        public Point start, direction;

        public Ray(Point st, Point end)
        {
            start = new Point(st);
            direction = Point.norm(end - st);
        }

        public Ray() { }

        public Ray(Ray r)
        {
            start = r.start;
            direction = r.direction;
        }

        public Ray Reflect(Point hit_point, Point normal)
        {
            Point reflect_dir = direction - 2 * normal * Point.scalar(direction, normal);
            return new Ray(hit_point, hit_point + reflect_dir);
        }

        public Ray Refract(Point hit_point, Point normal,float refraction ,float refract_coef)
        {
            Ray res_ray = new Ray();
            float sclr = Point.scalar(normal, direction);
            float n1n2div = refraction / refract_coef;
            float theta_formula = 1 - n1n2div*n1n2div * (1 - sclr * sclr);
            if (theta_formula >= 0)
            {
                float cos_theta = (float)Math.Sqrt(theta_formula);
                res_ray.start = new Point(hit_point);
                res_ray.direction = Point.norm(direction * n1n2div - (cos_theta + n1n2div * sclr) * normal);
                return res_ray;
            }
            else
                return null;
        }
    }

}
