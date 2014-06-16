using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AI_1
{
    public class Retina
    {                
        public Retina(int h, int w, bool isExt = false)
        {            
            this.m = h;
            this.n = w;
            this.isExtend = isExt;
            this.matr = new double[m, n];                        
        }

        public int Height
        {
            get { return m; }
        }

        public int Width
        {
            get { return n; }
        }

        public bool IsExtend
        {
            get { return isExtend; }
        }

        public double this[int i, int j]
        {
            get { return matr[i, j]; }
            set { matr[i, j] = value; }
        }

        public void Clear()
        {            
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    this[i, j] = 0;
        }

        public void Fill(Bitmap bmp)
        {
            int hh = m;
            if (isExtend)
                --hh;
            for (int i = 0; i < hh; ++i)
                for (int j = 0; j < n; ++j)
                    this[i, j] = bmp.GetPixel(i, j).Name.Equals("ff000000") ? 1 : 0;                
        }

        public void Fill(double val)
        {
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    this[i, j] = val;            
        }

        public void FillExtend(double val, double extVal)
        {
            Fill(val);
            for (int j = 0; j < n; ++j)
                this[m-1, j] = 0;
            this[m-1, 0] = extVal;
        }

        public void FillExtend(Bitmap bmp, int extVal)
        {
            Fill(bmp);
            for (int j = 0; j < n; ++j)
                this[m-1, j] = 0;
            this[m-1, 0] = extVal;
        }
        /// <summary>
        /// Сохранение сетчатки в файл.
        /// </summary>
        /// <param name="fName">имя файла</param>
        public void Save(string fName)
        {
            StreamWriter sw = new StreamWriter(fName);
            for (int i = 0; i < m; ++i)
            {
                for (int j = 0; j < n; ++j)
                    sw.Write(this[j, i] + " ");
                sw.WriteLine();
            }
            sw.Close();
        }
        /// <summary>
        /// Сохранение сетчатки в Bitmap.
        /// </summary>
        /// <returns>новый экземпляр Bitmap</returns>
        public Bitmap Save()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            for(int i = 0; i < Height; ++i)
                for(int j = 0; j < Width; ++j)
                    bmp.SetPixel(i,j,(this[i,j] == 1) ? Color.Black : Color.White);
            return bmp;
        }
        /// <summary>
        /// Зашумление сетчатки
        /// </summary>
        /// <param name="noise">% шума</param>
        public void Noise(int noise)
        {
            Random r = new Random();
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    if (r.NextDouble() > 1 - noise / 100.0)
                        this[i, j] = Math.Abs(this[i, j] - 1);                    
        }
        /// <summary>
        /// Скалярное произведение сетчаток
        /// </summary>
        /// <param name="that">вектор, на который скалярно умножается сетчатка</param>
        /// <returns>Скалярное произведение двух сетчаток</returns>
        public double scalarProduct(Retina that)
        {
            double res = 0;
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res += this[i, j] * that[i, j];
            return res;
        }
        /// <summary>
        /// Вычисление нормы сетчатки
        /// </summary>
        /// <returns>норма сетчатки</returns>
        public double Norma()
        {
            double res = 0;
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res += Math.Pow(this[i, j],2);
            return Math.Sqrt(res);
        }
        /// <summary>
        /// Скалярное умножение сетчатки на val
        /// </summary>
        /// <param name="val">Скаляр, на который умножается сетчатка</param>
        /// <returns>Объект Retina, сетчатка которой является результатом скалярного умножения</returns>
        public Retina mulScalar(double val)
        {
            Retina res = new Retina(m, n);
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res[i, j] = this[i, j] * val;
            return res;
        }
        /// <summary>
        /// Взятие сетчатки с противоположным знаком
        /// </summary>
        public void Neg()
        {
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    this[i, j] = -this[i, j];
        }
        /// <summary>
        /// Сумма двух сетчаток
        /// </summary>
        /// <param name="that">Второе слагаемое-сетчатка</param>
        /// <returns>Объект Retina, сетчатка которой является результатом суммы</returns>
        public Retina Sum(Retina that)
        {
            Retina res = new Retina(m, n);
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res[i, j] = this[i, j] + that[i, j];
            return res;
        }
        /// <summary>
        /// Разность двух сетчаток
        /// </summary>
        /// <param name="that">Второе слагаемое-сетчатка</param>
        /// <returns>Объект Retina, сетчатка которой является результатом вычитания</returns>
        public Retina Subtraction(Retina that)
        {
            Retina res = new Retina(m, n);
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res[i, j] = this[i, j] - that[i, j];
            return res;
        }
        /// <summary>
        /// Определение матрицы поворота на заданный угол 
        /// </summary>
        /// <param name="fi">угол поворота в градусах</param>
        /// <returns>Retina, соответствующий повороту fi</returns>
        public static Retina Rotation(double fi)
        {
            Retina res = new Retina(3, 3);
            double cos = Math.Cos(fi);
            double sin = Math.Sin(fi);
            res[0, 0] = res[1, 1] = cos;
            res[0, 1] = sin;
            res[1, 0] = -sin;
            res[2, 2] = 1;
            return res;
        }
        /// <summary>
        /// Параллельный перенос
        /// </summary>
        /// <param name="a">смещение по оси X</param>
        /// <param name="b">смещение по оси Y</param>
        /// <returns>Матрица, соответствующая параллельному переносу на (a,b)</returns>
        public static Retina Translation(double a, double b)
        {
            Retina m = new Retina(3, 3);
            m[0, 0] = m[1, 1] = m[2, 2] = 1;
            m[2, 0] = a;
            m[2, 1] = b;            
            return m;
        }        
        /// <summary>
        /// Перемножение двух сетчаток
        /// </summary>
        /// <param name="a">первое сетчатка-слагаемое</param>
        /// <param name="b">второе сетчатка-слагаемое</param>
        /// <returns>Произведение сетчаток. Null в случае несовпадения числа столбцов a и числа строк b</returns>
        public static Retina operator*(Retina a, Retina b)
        {
            if (a.Width != b.Height)    // проверка согласованности матриц
                return null;
            Retina res = new Retina(a.Height, b.Width);
            for (int i = 0; i < a.Height; ++i)
                for (int j = 0; j < b.Width; ++j)
                    for (int k = 0; k < a.Width; ++k)
                        res[i, j] += a[i, k] * b[k, j];
            return res;
        }
        //
        public static Point operator*(Point p, Retina that)
        {
            return new Point((int)Math.Round(p.X * that[0, 0] + p.Y * that[1, 0] + that[2, 0]), 
                (int)Math.Round(p.X * that[0, 1] + p.Y * that[1, 1] + that[2, 1]));
        }
        /// <summary>
        /// Объединение двух сетчаток, элементы которых содержат 0 или 1.
        /// </summary>
        /// <param name="that">Второе слагаемое-сетчатка</param>
        /// <returns>Объект Retina, сетчатка которой является результатом объединения</returns>
        public Retina Union(Retina that)
        {
            Retina res = new Retina(m, n);
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res[i, j] = (this[i, j] == 0 && that[i, j] == 0) ? 0 : 1;
            return res;
        }
        int m, n;
        double[,] matr;
        bool isExtend;
    }
}
