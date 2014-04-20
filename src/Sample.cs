using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AI_1
{
    public class Sample
    {                
        public Sample(int h, int w, bool isExt = false)
        {            
            this.m = h;
            this.n = w;
            this.isExtend = isExt;
            this.retina = new double[m, n];                        
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
            get { return retina[i, j]; }
        }

        public void Clear()
        {            
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    retina[i, j] = 0;
        }

        public void Fill(Bitmap bmp)
        {
            int hh = m;
            if (isExtend)
                --hh;
            for (int i = 0; i < hh; ++i)
                for (int j = 0; j < n; ++j)
                    retina[i, j] = bmp.GetPixel(i, j).Name.Equals("ff000000") ? 1 : 0;                
        }

        public void Fill(int val)
        {
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    retina[i, j] = val;            
        }

        public void FillExtend(int val, int extVal)
        {
            Fill(val);
            for (int j = 0; j < n; ++j)
                retina[m-1, j] = 0;
            retina[m-1, 0] = extVal;
        }

        public void FillExtend(Bitmap bmp, int extVal)
        {
            Fill(bmp);
            for (int j = 0; j < n; ++j)
                retina[m-1, j] = 0;
            retina[m-1, 0] = extVal;
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
                    sw.Write(retina[j, i] + " ");
                sw.WriteLine();
            }
            sw.Close();
        }
        /// <summary>
        /// Скалярное произведение сетчаток
        /// </summary>
        /// <param name="that">вектор, на который скалярно умножается сетчатка</param>
        /// <returns>Скалярное произведение двух сетчаток</returns>
        public double scalarProduct(Sample that)
        {
            double res = 0;
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res += this.retina[i, j] * that.retina[i, j];
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
                    res += Math.Pow(this.retina[i, j],2);
            return Math.Sqrt(res);
        }
        /// <summary>
        /// Скалярное умножение сетчатки на val
        /// </summary>
        /// <param name="val">Скаляр, на который умножается сетчатка</param>
        /// <returns>Объект Sample, сетчатка которой является результатом скалярного умножения</returns>
        public Sample mulScalar(double val)
        {
            Sample res = new Sample(m, n);
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res.retina[i, j] = this.retina[i, j] * val;
            return res;
        }
        /// <summary>
        /// Взятие сетчатки с противоположным знаком
        /// </summary>
        public void Neg()
        {
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    this.retina[i, j] = -this.retina[i, j];
        }
        /// <summary>
        /// Сумма двух сетчаток
        /// </summary>
        /// <param name="that">Второе слагаемое-сетчатка</param>
        /// <returns>Объект Sample, сетчатка которой является результатом суммы</returns>
        public Sample Sum(Sample that)
        {
            Sample res = new Sample(m, n);
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res.retina[i, j] = this.retina[i, j] + that.retina[i, j];
            return res;
        }
        /// <summary>
        /// Разность двух сетчаток
        /// </summary>
        /// <param name="that">Второе слагаемое-сетчатка</param>
        /// <returns>Объект Sample, сетчатка которой является результатом вычитания</returns>
        public Sample Subtraction(Sample that)
        {
            Sample res = new Sample(m, n);
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    res.retina[i, j] = this.retina[i, j] - that.retina[i, j];
            return res;
        }

        int m, n;
        double[,] retina;
        bool isExtend;
    }
}
