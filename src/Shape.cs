using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AI_1
{
    public abstract class Shape
    {
        protected Shape(Point pp)
        {
            this.p = pp;
        }
        // рисование фигуры
        public abstract void Draw(Graphics g);
        //
        protected Point p;        
    }

    public class CircleShape : Shape
    {
        public CircleShape(Point center, int radius)
            : base(center)
        {
            this.r = radius;
        }
        //
        public override void Draw(Graphics g)
        {
            g.FillEllipse(new SolidBrush(Color.Black), new Rectangle(new Point(p.X - r, p.Y - r), new Size(2 * r, 2 * r)));            
        }
        // радиус окружности
        int r;
    }

    public class RectangleShape : Shape
    {
        public RectangleShape(Point p1, Point p2)
            : base(p1)
        {
            this.p2 = p2;
        }
        //
        public override void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(p, new Size(p2.X - p.X, p2.Y - p.Y)));
        }
        // p - левый верхний угол
        Point p2; // правый нижний угол
    }

    public class TriangleShape : Shape
    {
        public TriangleShape(Point p1, Point p2, Point p3)
            : base(p1)
        {
            this.p2 = p2;
            this.p3 = p3;
        }
        //
        public override void Draw(Graphics g)
        {
            g.FillPolygon(new SolidBrush(Color.Black), new Point[] { p, p2, p3 });
        }
        // вершины основания равнобедренного треугольника
        Point p2, p3;
    }

    public class TextRectangleShape : RectangleShape
    {
        public TextRectangleShape(string s, Font f, Point p1, Point p2)
            : base(p1, p2)
        {
            this.text = s;
            this.font = f;
        }
        //
        public override void Draw(Graphics g)
        {
            g.DrawString(text, font, new SolidBrush(Color.Black), p);
        }
        //
        Font font;   // параметры шрифта
        string text; // строка для вывода
    }
}
