using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AI_1
{
    public partial class Form1 : Form
    {        
        public Form1()
        {
            InitializeComponent();
            AddOwnedForm(form2);           
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();            
        }

        private void exit1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveAs1_Click(object sender, EventArgs e)
        {
            string s0 = saveFileDialog1.FileName;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string s = saveFileDialog1.FileName;
                if (s0.ToUpper().Equals(s.ToUpper()))
                {
                    s0 = Path.GetDirectoryName(s0) + "\\($$##$$).bmp";
                    pictureBox1.Image.Save(s0);
                    pictureBox1.Image.Dispose();
                    File.Delete(s);
                    File.Move(s0, s);
                    pictureBox1.Image = new Bitmap(s);
                }
                else                
                    pictureBox1.Image.Save(s);
                //
                if (radioButton1.Checked)
                {
                    smpX.Fill(pictureBox1.Image as Bitmap);
                    smpX.Save("UpTriangle.txt");
                }
                else
                {
                    smpY.Fill(pictureBox1.Image as Bitmap);
                    smpY.Save("DownTriangle.txt");
                }
            }
        }

        private void open1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string s = openFileDialog1.FileName;
                try
                {
                    Image im = new Bitmap(s);
                    if (pictureBox1.Image != null)
                        pictureBox1.Image.Dispose();
                    pictureBox1.Image = im;
                }
                catch
                {
                    MessageBox.Show("File " + s + " has a wrong format", "Error");
                    return;
                }
                saveFileDialog1.FileName = s;
                openFileDialog1.FileName = "";
            }
        }       

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {            
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (pt1.Y <= 0 || pt2.Y <= 0 || pt3.Y <= 0)
                        return;
                    pt1.Y -= shift;
                    pt2.Y -= shift;
                    pt3.Y -= shift;
            	    break;
                case Keys.Down:
                    double dh = Math.Floor(pictureBox1.Image.Height * .95);
                    if (pt1.Y >= dh || pt2.Y >= dh || pt3.Y >= dh)
                        return;
                    pt1.Y += shift;
                    pt2.Y += shift;
                    pt3.Y += shift;
            	    break;
                case Keys.Left:
                    if(pt1.X <= 0)
                        return;
                    pt1.X -= shift;
                    pt2.X -= shift;
                    pt3.X -= shift;
            	    break;
                case Keys.Right:
                    if (pt2.X >= pictureBox1.Image.Width - 1)
                        return;
                    pt1.X += shift;
                    pt2.X += shift;
                    pt3.X += shift;
            	    break;
                default:
                    return;
            }
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(pictureBox1.BackColor);
            g.DrawPolygon(pen, new Point[] { pt1, pt2, pt3 });
            g.FillPolygon(new SolidBrush(pen.Color), new Point[] { pt1, pt2, pt3 });
            pictureBox1.Invalidate();
        }

        private void new1_Click(object sender, EventArgs e)
        {
            form2.ActiveControl = form2.numericUpDown1;
            if (form2.ShowDialog() == DialogResult.OK)
            {
                saveFileDialog1.FileName = "";
                int w = (int)form2.numericUpDown1.Value;
                int h = (int)form2.numericUpDown2.Value;
                int centerMargin = Math.Min(w, h) / 4;
                Image im = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(im);                
                if (radioButton1.Checked)
                {
                    smpX = new Sample(h, w);
                    pt1 = new Point(w / 2 - centerMargin, h / 2 + centerMargin);
                    pt2 = new Point(w / 2 + centerMargin, h / 2 + centerMargin);
                    pt3 = new Point(w / 2, h / 2 - centerMargin);
                }
                else
                {
                    smpY = new Sample(h, w);
                    pt1 = new Point(w / 2 - centerMargin, h / 2 - centerMargin);
                    pt2 = new Point(w / 2 + centerMargin, h / 2 - centerMargin);
                    pt3 = new Point(w / 2, h / 2 + centerMargin);
                }                
                g.Clear(pictureBox1.BackColor);
                g.DrawPolygon(pen, new Point[] { pt1, pt2, pt3 });
                g.FillPolygon(new SolidBrush(pen.Color), new Point[] { pt1, pt2, pt3 });
                g.Dispose();
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                pictureBox1.Image = im;
                this.ActiveControl = this.pictureBox1;                
            }
        }
        /// <summary>
        /// создание обучающих выборок
        /// </summary>
        /// <param name="cnt">число элементов выборки</param>
        /// <returns>список, состоящий из 2 выборок разных классов изображений</returns>
        List<Sample> InitW(int cnt)
        {
            List<Sample> sampling = new List<Sample>();            
            for (int i = 1; i <= 2 * cnt; ++i)
            {                
                string s = "triangle" + i + "." + saveFileDialog1.DefaultExt;
                Bitmap bmp;
                try
                {
                    bmp = new Bitmap(@"..\\..\\images\\" + s);                   
                }
                catch
                {
                    MessageBox.Show("File " + s + " has a wrong format", "Error");
                    return null;
                }               
                Sample elem = new Sample(bmp.Height + 1, bmp.Width, true);
                elem.FillExtend(bmp, 1);
                if (i > cnt)
                    elem.Neg();                    
                sampling.Add(elem);                
            }
            return sampling;
        }        

        private void segregate1_Click(object sender, EventArgs e)
        {
            // Фаза 1 - проверка на линейную разделимость
            // шаг 1 - инициализация
            int classSize = 5; // кол-во элементов класса в выборке
            List<Sample> sampling = new List<Sample>(InitW(classSize));
            if (sampling == null)
                return;
            int m = (int)form2.numericUpDown1.Value;
            int n = (int)form2.numericUpDown2.Value;
            Sample l = new Sample(m + 1, n, true);
            l.FillExtend(1, 1);
            int h = 10;        // скорость обучения
            int iters = 10;    // кол-во итераций                        
            // шаг 2 - итеративный процесс
            bool stop = false;
            for (int p = 0; p <= iters; ++p)                        
            {
                int min = int.MaxValue;
                int idx = -1;
                for (int i = 0; i < sampling.Count; ++i)
                {
                    int sc = (int)l.scalarProduct(sampling[i]);
                    if (sc < min)
                    {
                        min = sc;
                        idx = i;
                    }
                }
                if (min < 0)
                {
                    l = l.Sum(sampling[idx].mulScalar(h));
                }
                else if (min > 0)
                {
                    stop = true;
                    break;
                }                
            }            
            if(!stop)
                MessageBox.Show("Классы линейно неразделимы");
            else
            {               
                // Фаза 2 - определение порога                
                l = Kozinets(toConvexHull(sampling));   // определение наилучшей разделяющей прямой
                // поиск диапазона порога: maxY < T <= minX
                double minX = double.MaxValue;
                for(int i = 0; i < classSize; ++i)
                {
                    double sc = l.scalarProduct(sampling[i]);
                    if (sc < minX)
                        minX = sc;
                }
                MessageBox.Show(minX.ToString());
                //double maxY = double.MinValue;
                //for (int i = classSize; i < sampling.Count; ++i)
                //{
                //    double sc = l.scalarProduct(sampling[i]);
                //    if (sc > maxY)
                //        maxY = sc;
                //}
            }
        }

        List<Sample> toConvexHull(List<Sample> ls)
        {
            List<Sample> w = new List<Sample>();
            int smpCnt = ls.Count / 2;
            for (int i = 0; i < smpCnt; ++i)
                for (int j = smpCnt; j < ls.Count; ++j)
                    w.Add(ls[i].Sum(ls[j]));
            return w;
        }

        double lambdaCalc(Sample w, Sample l)
        {
            Sample lw = l.Subtraction(w);
            double num = 0, denom = 0;
            for (int i = 0; i < lw.Height; ++i)
                for (int j = 0; j < lw.Width; ++j)
                {
                    num += w[i, j] * lw[i,j];
                    denom += lw[i, j] * lw[i, j];
                }
            return - num / denom;
        }
        /// <summary>
        /// определение наилучшей разделяющей прямой
        /// </summary>
        /// <param name="w">выборка из выпуклых оболочек</param>
        /// <returns>экземпляр Sample, соответствующий наилучшей разделяющей прямой</returns>
        Sample Kozinets(List<Sample> w)
        {
            // шаг 1 - инициализация
            double minNorma = double.MaxValue;
            int idx = -1;
            for (int i = 0; i < w.Count; ++i)
                if (w[i].Norma() < minNorma)
                {
                    minNorma = w[i].Norma();
                    idx = i;
                }
            if (idx < 0)
                return null;
            Sample l = w[idx];
            //  шаг 2 - итеративный процесс            
            foreach(Sample _w in w)
            {
                double lambda = lambdaCalc(_w, l);
                while (lambda > 0 && lambda < 1)
                {
                    l = l.mulScalar(lambda).Sum(_w.mulScalar(1 - lambda));
                    lambda = lambdaCalc(_w, l);
                }
            }
            // шаг 3 - окончание процесса            
            return l;
        }

        Form2 form2 = new Form2();
        Pen pen = Pens.Black;
        Point pt1, pt2, pt3;
        const int shift = 1;
        Sample smpX, smpY;
    }
}
