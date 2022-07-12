using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CornishRoom
{
    public partial class Form1 : Form
    {
        public List<Figure> scene = new List<Figure>();
        public List<Light> lights = new List<Light>();
        public Color[,] pixels_color;
        public Point[,] pixels;
        public Point cameraPoint;
        public Point up_left, up_right, down_left, down_right;
        public int h, w;

        public Form1()
        {
            InitializeComponent();
            cameraPoint = new Point();
            up_left = new Point();
            up_right = new Point();
            down_left = new Point();
            down_right = new Point();
            h = pictureBox1.Height;
            w = pictureBox1.Width;
            pictureBox1.Image = new Bitmap(w, h);
            BuildScene();
            BackwardRayTracing();
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    (pictureBox1.Image as Bitmap).SetPixel(i, j, pixels_color[i, j]);
                }
                pictureBox1.Invalidate();
            }
        }
        public void BuildScene()
        {
            Figure room = Figure.get_Cube(10);
            up_left = room.sides[0].getPoint(0);
            up_right = room.sides[0].getPoint(1);
            down_right = room.sides[0].getPoint(2);
            down_left = room.sides[0].getPoint(3);

            Point normal = Side.norm(room.sides[0]);                            
            Point center = (up_left + up_right + down_left + down_right) / 4;   
            cameraPoint = center + normal * 11;

            room.SetPen(new Pen(Color.White));
            room.sides[0].drawing_pen = new Pen(Color.White);
            room.sides[1].drawing_pen = new Pen(Color.White);
            room.sides[2].drawing_pen = new Pen(Color.Blue);
            room.sides[3].drawing_pen = new Pen(Color.Red);
            room.fMaterial = new Material(0f, 0f, 0.1f, 0.8f);

            Light l1 = new Light(new Point(0f, 2f, 4.9f), new Point(1f, 1f, 1f));
            Light l2 = new Light(new Point(-4.9f, -4.9f, 4.9f), new Point(1f, 1f, 1f));
            lights.Add(l1);
            lights.Add(l2);

            Sphere s = new Sphere(new Point(-2.5f, -2, 2.5f), 2.5f);
            s.SetPen(new Pen(Color.Green));
            s.fMaterial = new Material(0.0f, 0f, 0.1f, 0.7f, 1f);
            //s.fMaterial = new Material(0.9f, 0f, 0.1f, 0.7f, 1f);

            Figure c1 = Figure.get_Cube(3.3f);
            c1.Offset(-1.5f, 1.5f, -3.9f);
            c1.RotateAround(70, "CZ");
            c1.SetPen(new Pen(Color.Gray));
            c1.fMaterial = new Material(0f, 0f, 0.1f, 1f, 1f);

            Figure c2 = Figure.get_Cube(1.5f);
            c2.Offset(2f, 2f, -4.3f);
            c2.RotateAround(-10, "CZ");
            c2.SetPen(new Pen(Color.Yellow));
            c2.fMaterial = new Material(0, 0f, 0.1f, 1f, 1f);
            //c2.fMaterial = new Material(0f, 0.7f, 0.1f, 1f, 1f);

            Sphere s2 = new Sphere(new Point(1.5f, -1f, -0.5f), 1.5f);
            s2.SetPen(new Pen(Color.Pink));
            s2.fMaterial = new Material(0.0f, 0f, 0.1f, 0.7f, 1f);

            scene.Add(room);
            scene.Add(c1);
            scene.Add(c2);
            scene.Add(s);
            scene.Add(s2);
        }

        public void BackwardRayTracing()
        {
            GetPixels();
            for (int i = 0; i < w; ++i)
                for (int j = 0; j < h; ++j)
                {
                    Ray r = new Ray(cameraPoint, pixels[i, j]);
                    r.start = new Point(pixels[i, j]);
                    Point color = RayTrace(r, 10, 1);
                    if (color.x > 1.0f || color.y > 1.0f || color.z > 1.0f)
                        color = Point.norm(color);
                    pixels_color[i, j] = Color.FromArgb((int)(255 * color.x), (int)(255 * color.y), (int)(255 * color.z));
                }
        }
        public void GetPixels()
        {
            pixels = new Point[w, h];
            pixels_color = new Color[w, h];
            Point step_up = (up_right - up_left) / (w - 1);
            Point step_down = (down_right - down_left) / (w - 1);
            Point up = new Point(up_left);
            Point down = new Point(down_left);
            for (int i = 0; i < w; ++i)
            {
                Point step_y = (up - down) / (h - 1);
                Point d = new Point(down);
                for (int j = 0; j < h; ++j)
                {
                    pixels[i, j] = d;
                    d += step_y;
                }
                up += step_up;
                down += step_down;
            }
        }

        public bool IsVisible(Point light_point, Point hit_point)
        {
            float max_t = (light_point - hit_point).length();
            Ray r = new Ray(hit_point, light_point);
            foreach (Figure fig in scene)
                if (fig.FigureIntersection(r, out float t, out Point n))
                    if (t < max_t && t > Figure.eps)
                        return false;
            return true;
        }

        public Point RayTrace(Ray r, int iter, float env)
        {
            if (iter <= 0)
                return new Point(0, 0, 0);
            float rey_fig_intersect = 0;
            Point normal = null;
            Material material = new Material();
            Point res_color = new Point(0, 0, 0);
            bool refract_out_of_figure = false;

            foreach (Figure fig in scene)
            {
                if (fig.FigureIntersection(r, out float intersect, out Point norm))
                    if (intersect < rey_fig_intersect || rey_fig_intersect == 0)
                    {
                        rey_fig_intersect = intersect;
                        normal = norm;
                        material = new Material(fig.fMaterial);
                    }
            }

            if (rey_fig_intersect == 0)
                return new Point(0, 0, 0);

            if (Point.scalar(r.direction, normal) > 0)
            {
                normal *= -1;
                refract_out_of_figure = true;
            }

            Point hit_point = r.start + r.direction * rey_fig_intersect;

            foreach (Light light in lights)
            {
                Point ambient_coef = light.color_light * material.ambient;
                ambient_coef.x = (ambient_coef.x * material.color.x);
                ambient_coef.y = (ambient_coef.y * material.color.y);
                ambient_coef.z = (ambient_coef.z * material.color.z);
                res_color += ambient_coef;
                if (IsVisible(light.point_light, hit_point))
                    res_color += light.Shade(hit_point, normal, material.color, material.diffuse);
            }

            if (material.reflection > 0)
            {
                Ray reflected_ray = r.Reflect(hit_point, normal);
                res_color += material.reflection * RayTrace(reflected_ray, iter - 1, env);
            }

            if (material.refraction > 0)
            {
                float refract_coef;
                if (refract_out_of_figure)
                    refract_coef = material.environment;
                else
                    refract_coef = 1 / material.environment;

                Ray refracted_ray = r.Refract(hit_point, normal, material.refraction, refract_coef);

                if (refracted_ray != null)
                    res_color += material.refraction * RayTrace(refracted_ray, iter - 1, material.environment);
            }
            return res_color;
        }
    }
}

