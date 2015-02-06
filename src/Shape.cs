using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AI_1
{
    public abstract class Shape
    {        
        // рисование фигуры
        public abstract void Draw(Graphics g);
        // сдвиг фигуры вверх на shiftVal единиц
        public abstract void ShiftUp(int shiftVal);
        // сдвиг фигуры вниз на shiftVal единиц
        public abstract void ShiftDown(int shiftVal, int maxValue);
        // сдвиг фигуры влево на shiftVal единиц
        public abstract void ShiftLeft(int shiftVal);
        // сдвиг фигуры вправо на shiftVal единиц
        public abstract void ShiftRight(int shiftVal, int maxValue);
        // Вычисление точки центра масс фигуры
        public virtual Point getCenterPoint()
        {
            return new Point(points.Sum(e => e.X) / points.Length, points.Sum(e => e.Y) / points.Length);
        }
        //
        public Point[] points;        
    }

    public class CircleShape : Shape
    {
        public CircleShape(Point center, int radius)           
        {
            this.points = new Point[] { center };
            this.r = radius;
        }
        //
        public override void Draw(Graphics g)
        {
            g.FillEllipse(new SolidBrush(Color.Black), new Rectangle(new Point(points.First().X - r, points.First().Y - r), new Size(2 * r, 2 * r)));            
        }
        //
        public override void ShiftUp(int shiftVal)
        {
            if (this.points.First().Y >= shiftVal && this.points.First().Y > r)
            {
                Point pTmp = points.First();
                pTmp.Y -= shiftVal;
                points[0] = pTmp;
            }             
        }
        //
        public override void ShiftDown(int shiftVal, int maxValue)
        {
            if(points.First().Y < maxValue - r)
            {
                Point pTmp = points.First();
                pTmp.Y += shiftVal;
                points[0] = pTmp;
            }                
        }
        //
        public override void ShiftLeft(int shiftVal)
        {
            if (this.points.First().X >= shiftVal && this.points.First().X > r)
            {
                Point pTmp = points.First();
                pTmp.X -= shiftVal;
                points[0] = pTmp;
            }                
        }
        //
        public override void ShiftRight(int shiftVal, int maxValue)
        {
            if(points.First().X < maxValue - r)
            {
                Point pTmp = points.First();
                pTmp.X += shiftVal;
                points[0] = pTmp;
            }                
        }
        //
        public override Point getCenterPoint()
        {
            return points.First();
        }
        // радиус окружности
        int r;
    }

    public class RectangleShape : Shape
    {                
        /// <summary>
        /// Создание прямоугольной фигуры
        /// </summary>
        /// <param name="p1">левая верхняя точка</param>
        /// <param name="p2">правая нижняя точка</param>
        public RectangleShape(Point p1, Point p2)            
        {
            this.points = new Point[] { p1, p2 };            
        }
        //
        public override void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(points[0], new Size(points[1].X - points[0].X, points[1].Y - points[0].Y)));
        }
        //
        public override void ShiftUp(int shiftVal)
        {
            if (this.points.All(p => p.Y >= shiftVal))
            {
                for (int i = 0; i < points.Length; ++i )
                {
                    Point pTmp = points[i];
                    pTmp.Y -= shiftVal;
                    points[i] = pTmp;                    
                }
            }
        }
        //
        public override void ShiftDown(int shiftVal, int maxValue)
        {
            if (points[1].Y < maxValue)
            {
                for (int i = 0; i < points.Length; ++i)
                {
                    Point pTmp = points[i];
                    pTmp.Y += shiftVal;
                    points[i] = pTmp;
                }                
            }
        }
        //
        public override void ShiftLeft(int shiftVal)
        {
            if (points[0].X >= shiftVal)
            {
                for (int i = 0; i < points.Length; ++i)
                {
                    Point pTmp = points[i];
                    pTmp.X -= shiftVal;
                    points[i] = pTmp;
                }                 
            }                            
        }
        //
        public override void ShiftRight(int shiftVal, int maxValue)
        {
            if (points[1].X < maxValue)
            {
                for (int i = 0; i < points.Length; ++i)
                {
                    Point pTmp = points[i];
                    pTmp.X += shiftVal;
                    points[i] = pTmp;
                }                  
            }
        }                        
    }

    public class TriangleShape : Shape
    {
        /// <summary>
        /// Создание треугольной фигуры
        /// </summary>
        /// <param name="p1">1-я вершина</param>
        /// <param name="p2">левая вершина основания треугольника</param>
        /// <param name="p3">правая вершина основания треугольника</param>
        public TriangleShape(Point p1, Point p2, Point p3)            
        {
            this.points = new Point[] { p1, p2, p3 };            
        }
        //
        public override void Draw(Graphics g)
        {
            g.FillPolygon(new SolidBrush(Color.Black), points);
        }
        //
        public override void ShiftUp(int shiftVal)
        {
            if (this.points.All(p => p.Y >= shiftVal))
            {
                for (int i = 0; i < points.Length; ++i)                
                    points[i].Y -= shiftVal;                                    
            }
        }
        //
        public override void ShiftDown(int shiftVal, int maxValue)
        {
            if (this.points.All(p => p.Y < maxValue))
            {
                for (int i = 0; i < points.Length; ++i)                
                    points[i].Y += shiftVal;                                    
            }
        }
        //
        public override void ShiftLeft(int shiftVal)
        {
            if (points[1].X >= shiftVal)
            {
                for (int i = 0; i < points.Length; ++i)                
                    points[i].X -= shiftVal;                                    
            }         
        }
        //
        public override void ShiftRight(int shiftVal, int maxValue)
        {
            if (points[2].X < maxValue)
            {
                for (int i = 0; i < points.Length; ++i)                                    
                    points[i].X += shiftVal;                                    
            }  
        }               
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
            g.DrawString(text, font, new SolidBrush(Color.Black), points.First());            
            //g.DrawRectangle(new Pen(Color.Black), new Rectangle(points[0], new Size(points[1].X - points[0].X, points[1].Y - points[0].Y)));
        }        
        //
        Font font;   // параметры шрифта
        string text; // строка для вывода
    }
}
