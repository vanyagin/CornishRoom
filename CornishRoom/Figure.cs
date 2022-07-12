﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CornishRoom
{

    public class Figure
    {
        public static float eps = 0.001f;
        public List<Point> points = new List<Point>(); 
        public List<Side> sides = new List<Side>();        
        public Material fMaterial;
        public Figure() { }

        public Figure(Figure f)
        {
            foreach (Point p in f.points)
                points.Add(new Point(p));

            foreach (Side s in f.sides)
            {
                sides.Add(new Side(s));
                sides.Last().host = this;
            }
        }

        public bool RayIntersectsTriangle(Ray r, Point p0, Point p1, Point p2, out float intersect)
        {
            intersect = -1;
            Point edge1 = p1 - p0;
            Point edge2 = p2 - p0;
            Point h = r.direction * edge2;
            float a = Point.scalar(edge1, h);
            if (a > -eps && a < eps)
                return false;
            float f = 1.0f / a;
            Point s = r.start - p0;
            float u = f * Point.scalar(s, h);
            if (u < 0 || u > 1)
                return false;
            Point q = s * edge1;
            float v = f * Point.scalar(r.direction, q);
            if (v < 0 || u + v > 1)
                return false;
            float t = f * Point.scalar(edge2, q);
            if (t > eps)
            {
                intersect = t;
                return true;
            }
            else
                return false;
        }

        public virtual bool FigureIntersection(Ray r, out float intersect, out Point normal)
        {
            intersect = 0;
            normal = null;
            Side side = null;
            foreach (Side figure_side in sides)
            {
                if (figure_side.points.Count == 3)
                {
                    if (RayIntersectsTriangle(r, figure_side.getPoint(0), figure_side.getPoint(1), figure_side.getPoint(2), out float t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        side = figure_side;
                    }
                }
                else if (figure_side.points.Count == 4)
                {
                    if (RayIntersectsTriangle(r, figure_side.getPoint(0), figure_side.getPoint(1), figure_side.getPoint(3), out float t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        side = figure_side;
                    }
                    else if (RayIntersectsTriangle(r, figure_side.getPoint(1), figure_side.getPoint(2), figure_side.getPoint(3), out t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        side = figure_side;
                    }
                }
            }
            if (intersect != 0)
            {
                normal = Side.norm(side);
                fMaterial.color = new Point(side.drawing_pen.Color.R / 255f, side.drawing_pen.Color.G / 255f, side.drawing_pen.Color.B / 255f);
                return true;
            }
            return false;
        }

        public float[,] GetMatrix()
        {
            var res = new float[points.Count, 4];
            for (int i = 0; i < points.Count; i++)
            {
                res[i, 0] = points[i].x;
                res[i, 1] = points[i].y;
                res[i, 2] = points[i].z;
                res[i, 3] = 1;
            }
            return res;
        }

        public void ApplyMatrix(float[,] matrix)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].x = matrix[i, 0] / matrix[i, 3];
                points[i].y = matrix[i, 1] / matrix[i, 3];
                points[i].z = matrix[i, 2] / matrix[i, 3];
            }
        }

        private Point GetCenter()
        {
            Point res = new Point(0, 0, 0);
            foreach (Point p in points)
            {
                res.x += p.x;
                res.y += p.y;
                res.z += p.z;

            }
            res.x /= points.Count();
            res.y /= points.Count();
            res.z /= points.Count();
            return res;
        }

        public void RotateArondRad(float rangle, string type)
        {
            float[,] mt = GetMatrix();
            Point center = GetCenter();
            switch (type)
            {
                case "CX":
                    mt = ApplyOffset(mt, -center.x, -center.y, -center.z);
                    mt = ApplyRotation_X(mt, rangle);
                    mt = ApplyOffset(mt, center.x, center.y, center.z);
                    break;
                case "CY":
                    mt = ApplyOffset(mt, -center.x, -center.y, -center.z);
                    mt = ApplyRotation_Y(mt, rangle);
                    mt = ApplyOffset(mt, center.x, center.y, center.z);
                    break;
                case "CZ":
                    mt = ApplyOffset(mt, -center.x, -center.y, -center.z);
                    mt = ApplyRotation_Z(mt, rangle);
                    mt = ApplyOffset(mt, center.x, center.y, center.z);
                    break;
                case "X":
                    mt = ApplyRotation_X(mt, rangle);
                    break;
                case "Y":
                    mt = ApplyRotation_Y(mt, rangle);
                    break;
                case "Z":
                    mt = ApplyRotation_Z(mt, rangle);
                    break;
                default:
                    break;
            }
            ApplyMatrix(mt);
        }

        public void RotateAround(float angle, string type)
        {
            RotateArondRad(angle * (float)Math.PI / 180, type);
        }

        public void Scale_axis(float xs, float ys, float zs)
        {
            float[,] pnts = GetMatrix();
            pnts = ApplyScale(pnts, xs, ys, zs);
            ApplyMatrix(pnts);
        }

        public void Offset(float xs, float ys, float zs)
        {
            ApplyMatrix(ApplyOffset(GetMatrix(), xs, ys, zs));
        }

        public virtual void SetPen(Pen dw)
        {
            foreach (Side s in sides)
                s.drawing_pen = dw;
        }

        public void ScaleAroundCenter(float xs, float ys, float zs)
        {
            float[,] pnts = GetMatrix();
            Point p = GetCenter();
            pnts = ApplyOffset(pnts, -p.x, -p.y, -p.z);
            pnts = ApplyScale(pnts, xs, ys, zs);
            pnts = ApplyOffset(pnts, p.x, p.y, p.z);
            ApplyMatrix(pnts);
        }

        public void LineRotateRad(float rang, Point p1, Point p2)
        {
            p2 = new Point(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z);
            p2 = Point.norm(p2);
            float[,] mt = GetMatrix();
            ApplyMatrix(RotateAroundLine(mt, p1, p2, rang));
        }

        public void LineRotate(float ang, Point p1, Point p2)
        {
            ang = ang * (float)Math.PI / 180;
            LineRotateRad(ang, p1, p2);
        }

        private static float[,] RotateAroundLine(float[,] transform_matrix, Point start, Point dir, float angle)
        {
            float cos_angle = (float)Math.Cos(angle);
            float sin_angle = (float)Math.Sin(angle);
            float val00 = dir.x * dir.x + cos_angle * (1 - dir.x * dir.x);
            float val01 = dir.x * (1 - cos_angle) * dir.y + dir.z * sin_angle;
            float val02 = dir.x * (1 - cos_angle) * dir.z - dir.y * sin_angle;
            float val10 = dir.x * (1 - cos_angle) * dir.y - dir.z * sin_angle;
            float val11 = dir.y * dir.y + cos_angle * (1 - dir.y * dir.y);
            float val12 = dir.y * (1 - cos_angle) * dir.z + dir.x * sin_angle;
            float val20 = dir.x * (1 - cos_angle) * dir.z + dir.y * sin_angle;
            float val21 = dir.y * (1 - cos_angle) * dir.z - dir.x * sin_angle;
            float val22 = dir.z * dir.z + cos_angle * (1 - dir.z * dir.z);
            float[,] rotateMatrix = new float[,] { { val00, val01, val02, 0 }, { val10, val11, val12, 0 }, { val20, val21, val22, 0 }, { 0, 0, 0, 1 } };
            return ApplyOffset(MultiplyMatrix(ApplyOffset(transform_matrix, -start.x, -start.y, -start.z), rotateMatrix), start.x, start.y, start.z);
        }

        private static float[,] MultiplyMatrix(float[,] m1, float[,] m2)
        {
            float[,] res = new float[m1.GetLength(0), m2.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); i++)
            {
                for (int j = 0; j < m2.GetLength(1); j++)
                {
                    for (int k = 0; k < m2.GetLength(0); k++)
                    {
                        res[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }
            return res;
        }

        private static float[,] ApplyOffset(float[,] transform_matrix, float offset_x, float offset_y, float offset_z)
        {
            float[,] translationMatrix = new float[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { offset_x, offset_y, offset_z, 1 } };
            return MultiplyMatrix(transform_matrix, translationMatrix);
        }

        private static float[,] ApplyRotation_X(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { 1, 0, 0, 0 }, { 0, (float)Math.Cos(angle), (float)Math.Sin(angle), 0 },
                { 0, -(float)Math.Sin(angle), (float)Math.Cos(angle), 0}, { 0, 0, 0, 1} };
            return MultiplyMatrix(transform_matrix, rotationMatrix);
        }

        private static float[,] ApplyRotation_Y(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { (float)Math.Cos(angle), 0, -(float)Math.Sin(angle), 0 }, { 0, 1, 0, 0 },
                { (float)Math.Sin(angle), 0, (float)Math.Cos(angle), 0}, { 0, 0, 0, 1} };
            return MultiplyMatrix(transform_matrix, rotationMatrix);
        }

        private static float[,] ApplyRotation_Z(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { (float)Math.Cos(angle), (float)Math.Sin(angle), 0, 0 }, { -(float)Math.Sin(angle), (float)Math.Cos(angle), 0, 0 },
                { 0, 0, 1, 0 }, { 0, 0, 0, 1} };
            return MultiplyMatrix(transform_matrix, rotationMatrix);
        }

        private static float[,] ApplyScale(float[,] transform_matrix, float scale_x, float scale_y, float scale_z)
        {
            float[,] scaleMatrix = new float[,] { { scale_x, 0, 0, 0 }, { 0, scale_y, 0, 0 }, { 0, 0, scale_z, 0 }, { 0, 0, 0, 1 } };
            return MultiplyMatrix(transform_matrix, scaleMatrix);
        }

        static public Figure get_Cube(float sz)
        {
            Figure res = new Figure();
            res.points.Add(new Point(sz / 2, sz / 2, sz / 2)); 
            res.points.Add(new Point(-sz / 2, sz / 2, sz / 2)); 
            res.points.Add(new Point(-sz / 2, sz / 2, -sz / 2)); 
            res.points.Add(new Point(sz / 2, sz / 2, -sz / 2)); 
            res.points.Add(new Point(sz / 2, -sz / 2, sz / 2));
            res.points.Add(new Point(-sz / 2, -sz / 2, sz / 2)); 
            res.points.Add(new Point(-sz / 2, -sz / 2, -sz / 2)); 
            res.points.Add(new Point(sz / 2, -sz / 2, -sz / 2)); 

            Side s = new Side(res);
            s.points.AddRange(new int[] { 3, 2, 1, 0 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 4, 5, 6, 7 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 2, 6, 5, 1 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 0, 4, 7, 3 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 1, 5, 4, 0 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 2, 3, 7, 6 });
            res.sides.Add(s);
            return res;
        }
    }

    public class Sphere : Figure
    {
        float radius;

        public Pen drawing_pen = new Pen(Color.Black);

        public Sphere(Point p, float r)
        {
            points.Add(p);
            radius = r;
        }

        public static bool RaySphereIntersection(Ray r, Point sphere_pos, float sphere_rad, out float t)
        {
            Point k = r.start - sphere_pos;
            float b = Point.scalar(k, r.direction);
            float c = Point.scalar(k, k) - sphere_rad * sphere_rad;
            float d = b * b - c;
            t = 0;
            if (d >= 0)
            {
                float sqrtd = (float)Math.Sqrt(d);
                float t1 = -b + sqrtd;
                float t2 = -b - sqrtd;

                float min_t = Math.Min(t1, t2);
                float max_t = Math.Max(t1, t2);

                t = (min_t > eps) ? min_t : max_t;
                return t > eps;
            }
            return false;
        }

        public override void SetPen(Pen dw)
        {
            drawing_pen = dw;
        }

        public override bool FigureIntersection(Ray r, out float t, out Point normal)
        {
            t = 0;
            normal = null;
            if (RaySphereIntersection(r, points[0], radius, out t) && (t > eps))
            {
                normal = (r.start + r.direction * t) - points[0];
                normal = Point.norm(normal);
                fMaterial.color = new Point(drawing_pen.Color.R / 255f, drawing_pen.Color.G / 255f, drawing_pen.Color.B / 255f);
                return true;
            }
            return false;
        }
    }

}
