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
    enum SHAPE_TYPE { TRIANGLE,DIGIT }

    public partial class Form1 : Form
    {
        Form2 form2 = new Form2();
        Form3 form3 = new Form3();
        Pen pen = Pens.Black;
        Point pt1, pt2, pt3;
        const int shift = 2;   // сдвиг по сетчатке        
        Retina separationLine; // разделяющая прямая
        double threshold = 1;  // порог
        SHAPE_TYPE iType = SHAPE_TYPE.DIGIT;

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
            Point cntr = new Point();
            Retina mrot = null;
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
                case Keys.NumPad6:  // поворот по часовой стрелке  
                    cntr = bulkCenter(new Point[] { pt1, pt2, pt3 });
                    mrot = Retina.Translation(-cntr.X, -cntr.Y) * Retina.Rotation(Math.PI / 4) * Retina.Translation(cntr.X, cntr.Y);
                    pt1 *= mrot;
                    pt2 *= mrot;
                    pt3 *= mrot;
                    break;
                case Keys.Right:
                    if (pt2.X >= pictureBox1.Image.Width - 1)
                        return;
                    pt1.X += shift;
                    pt2.X += shift;
                    pt3.X += shift;
            	    break;
                case Keys.NumPad4:  // поворот против часовой стрелки
                    cntr = bulkCenter(new Point[] { pt1, pt2, pt3 });
                    mrot = Retina.Translation(-cntr.X, -cntr.Y) * Retina.Rotation(-Math.PI / 4) * Retina.Translation(cntr.X, cntr.Y);
                    pt1 *= mrot;
                    pt2 *= mrot;
                    pt3 *= mrot;
                    break;
                default:
                    return;
            }
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(pictureBox1.BackColor);
            // треугольник, заданный 3-мя точками
            Point[] pp = new Point[] { pt1, pt2, pt3 };
            g.DrawPolygon(pen, pp);
            g.FillPolygon(new SolidBrush(pen.Color), pp);
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
                if (w != h)
                {
                    MessageBox.Show("Изображение должно быть квадратным. Пожалуйста, попробуйте еще раз");
                    return;
                }
                int centerMargin = Math.Min(w, h) / 4;
                Image im = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(im);                
                if (radioButton1.Checked)
                {                    
                    pt1 = new Point(w / 2 - centerMargin, h / 2 + centerMargin);
                    pt2 = new Point(w / 2 + centerMargin, h / 2 + centerMargin);
                    pt3 = new Point(w / 2, h / 2 - centerMargin);
                }
                else
                {                    
                    pt1 = new Point(w / 2 - centerMargin, h / 2 - centerMargin);
                    pt2 = new Point(w / 2 + centerMargin, h / 2 - centerMargin);
                    pt3 = new Point(w / 2, h / 2 + centerMargin);
                }                
                g.Clear(pictureBox1.BackColor);
                if (iType == SHAPE_TYPE.TRIANGLE)
                {
                    g.DrawPolygon(pen, new Point[] { pt1, pt2, pt3 });
                    g.FillPolygon(new SolidBrush(pen.Color), new Point[] { pt1, pt2, pt3 });
                }
                g.Dispose();
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                pictureBox1.Image = im;
                this.ActiveControl = this.pictureBox1;                
            }
        }       

        private void learn1_Click(object sender, EventArgs e)
        {
            if (form3.ShowDialog() == DialogResult.OK)
            {
                // Фаза 1 - проверка на линейную разделимость
                // шаг 1 - инициализация
                int classSize = int.Parse(form3.textBox1.Text); // кол-во элементов класса в выборке
                List<Retina> sampling = InitW(2, classSize);
                if (sampling == null)
                    return;
                int m = (int)form2.numericUpDown1.Value;
                int n = (int)form2.numericUpDown2.Value;

                Retina l = new Retina(m + 1, n, true);
                l.FillExtend(threshold, -threshold);
                int h = (int)form3.numericUpDown2.Value;        // скорость обучения
                int iters = (int)form3.numericUpDown1.Value;    // кол-во итераций                        
                // шаг 2 - итеративный процесс
                bool stop = false;
                for (int p = 1; p <= iters; ++p)
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
                        l = l.Sum(sampling[idx].mulScalar(h));
                    else if (min > 0)
                    {
                        stop = true;
                        break;
                    }
                }
                if (!stop)
                    MessageBox.Show("Классы линейно неразделимы");
                else
                {
                    MessageBox.Show("Классы линейно разделимы");
                    // Фаза 2 - определение порога                
                    separationLine = Kozinets(toConvexHull(sampling));   // определение наилучшей разделяющей прямой
                    if (separationLine == null)
                        return;
                    recognize1.Visible = true;
                    // поиск диапазона порога: maxY < T <= minX
                    double minX = double.MaxValue;
                    for (int i = 0; i < classSize; ++i)
                    {
                        double sc = separationLine.scalarProduct(sampling[i]);
                        if (sc < minX)
                            minX = sc;
                    }
                    double maxY = double.MinValue;
                    for (int i = classSize; i < sampling.Count; ++i)
                    {
                        sampling[i].Neg();
                        double sc = separationLine.scalarProduct(sampling[i]);
                        if (sc > maxY)
                            maxY = sc;
                    }
                    threshold = (maxY + minX) / 2.0;
                    //Console.WriteLine(threshold);
                }
            }
        }

        private void recognize1_Click(object sender, EventArgs e)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(pictureBox1.Image);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Для распознавания инициализируйте изображение.");
                return;
            }               
            Retina s = new Retina(bmp.Height + 1, bmp.Width, true);
            s.Fill(bmp);
            double sc = separationLine.scalarProduct(s);
            if(iType == SHAPE_TYPE.DIGIT)
                if (sc >= threshold)
                    MessageBox.Show("0");
                else
                    MessageBox.Show("1");
            else
                if (sc >= threshold)
                    MessageBox.Show("Up");
                else
                    MessageBox.Show("Down");
        }
        /// <summary>
        /// создание обучающих выборок
        /// </summary>
        /// <param name="m">кол-во классов</param>
        /// <param name="n">размер выборки</param>
        /// <returns>список, состоящий из m выборок разных классов изображений</returns>
        List<Retina> InitW(int m, int n)
        {
            List<Retina> sampling = new List<Retina>();
            for(int j = 0; j < m; ++j)
                for (int i = 0; i < n; ++i)
                {
                    string s = (iType == SHAPE_TYPE.DIGIT ? "digit" : "triangle") + j + "-" + i + "." + (iType == SHAPE_TYPE.DIGIT ? "jpg" : saveFileDialog1.DefaultExt);
                    Bitmap bmp;
                    try
                    {
                        bmp = new Bitmap(@"..\..\images\" + s);
                        //bmp = new Bitmap(@"images\" + s); 
                    }
                    catch
                    {
                        MessageBox.Show("File " + s + " has a wrong format", "Error");
                        return null;
                    }
                    Retina elem = new Retina(bmp.Height + 1, bmp.Width, true);
                    elem.FillExtend(bmp, 1);
                    if (j > 0)
                        elem.Neg();
                    sampling.Add(elem);
                }
            return sampling;
        }        

        List<Retina> toConvexHull(List<Retina> ls)
        {
            List<Retina> w = new List<Retina>();
            int smpCnt = ls.Count / 2;
            for (int i = 0; i < smpCnt; ++i)
                for (int j = smpCnt; j < ls.Count; ++j)
                    w.Add(ls[i].Sum(ls[j]));
            return w;
        }

        double lambdaCalc(Retina w, Retina l)
        {
            Retina lw = l.Subtraction(w);
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
        /// определение наилучшей разделяющей прямой алгоритмом Козинца
        /// </summary>
        /// <param name="w">выборка из выпуклых оболочек</param>
        /// <returns>Матрица, соответствующая наилучшей разделяющей прямой</returns>
        Retina Kozinets(List<Retina> w)
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
            Retina l = w[idx];
            //  шаг 2 - итеративный процесс            
            foreach(Retina _w in w)
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
        /// <summary>
        /// Вычисление центра масс точек набора
        /// </summary>
        /// <param name="p">массив точек</param>
        /// <returns>центр масс точек набора</returns>
        Point bulkCenter(Point[] p)
        {
            return new Point(p.Sum(e => e.X) / p.Length, p.Sum(e => e.Y) / p.Length);
        }

        private void noise1_Click(object sender, EventArgs e)
        {            
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(pictureBox1.Image);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Для зашумления инициализируйте изображение.");
                return;
            }
            // загрузка сетчатки
            Retina img = new Retina(bmp.Height, bmp.Height);
            img.Fill(bmp);
            img.Noise((int)numericUpDown1.Value);
            pictureBox1.Image.Dispose();
            pictureBox1.Image = img.Save();            
        }
    }
}