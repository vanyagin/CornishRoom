﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CornishRoom
{
    public class Side
    {
        public Figure host = null;
        public List<int> points = new List<int>();
        public Pen drawing_pen = new Pen(Color.Black);
        public Point Normal;

        public Side(Figure h = null)
        {
            host = h;
        }

        public Side(Side s)
        {
            points = new List<int>(s.points);
            host = s.host;
            drawing_pen = s.drawing_pen.Clone() as Pen;
            Normal = new Point(s.Normal);
        }

        public Point getPoint(int index)
        {
            if (host != null)
                return host.points[points[index]];
            return null;
        }

        public static Point norm(Side S)
        {
            if (S.points.Count() < 3)
                return new Point(0, 0, 0);
            Point U = S.getPoint(1) - S.getPoint(0);
            Point V = S.getPoint(S.points.Count - 1) - S.getPoint(0);
            Point normal = U * V;
            return Point.norm(normal);
        }

        public void CalculateSideNormal()
        {
            Normal = norm(this);
        }
    }
}
