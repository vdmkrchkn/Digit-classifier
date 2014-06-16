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
    enum SHAPE_TYPE { TRIANGLE, DIGIT }

    public partial class Form1 : Form
    {
        Form2 form2 = new Form2();
        Form3 form3 = new Form3();
        Pen pen;
        Font font;
        Point[] figureContour;
        const int shift = 2;        // сдвиг по сетчатке               
        List<Retina> separationLine;// разделяющие прямые для соотвествующих классов
        List<double> threshold;     // пороги для соответствующих классов
        SHAPE_TYPE iType;
        int classes;

        public Form1()
        {
            InitializeComponent();
            AddOwnedForm(form2);
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            pen = Pens.Black;
            radioButton2.Checked = true;            
        }
        /// <summary>
        /// создание обучающих выборок
        /// </summary>
        /// <param name="m">кол-во классов</param>
        /// <param name="n">размер выборки</param>
        /// <returns>список, состоящий из m выборок разных классов изображений</returns>
        List<List<Retina>> sampleInit(int m, int n)
        {
            List<List<Retina>> res = new List<List<Retina>>();
            for (int j = 0; j < m; ++j)
            {
                List<Retina> sampling = new List<Retina>();
                for (int i = 0; i < n; ++i)
                {
                    string s = (iType == SHAPE_TYPE.DIGIT ? "digit" : "triangle") + j + "-" + i + "." + saveFileDialog1.DefaultExt;
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
                    Retina elem = new Retina(bmp.Height, bmp.Width);
                    elem.Fill(bmp);
                    sampling.Add(elem);
                }
                res.Add(sampling);
            }
            return res;
        }
        // формирование объединенной выборки для определения сепарабельности
        List<Retina> wSeparationInit(List<Retina> x, List<Retina> y)
        {
            List<Retina> w = new List<Retina>();
            for (int i = 0; i < x.Count; ++i)
            {
                Retina _x = new Retina(x[i].Height+1, x[i].Width, true);
                _x.FillExtend(x[i].Save(), 1);
                w.Add(_x);
            }
            for (int i = 0; i < y.Count; ++i)
            {
                Retina _y = new Retina(y[i].Height+1, y[i].Width, true);
                _y.FillExtend(y[i].Save(), 1);
                _y.Neg();
                w.Add(_y);
            }
            return w;
        }
        // формирование разностной выборки для алгоритма Козинца
        List<Retina> toConvexHull(List<Retina> x,List<Retina> y)
        {
            List<Retina> w = new List<Retina>();            
            for (int i = 0; i < x.Count; ++i)
                for (int j = 0; j < y.Count; ++j)
                    w.Add(x[i].Subtraction(y[j]));
            return w;
        }
        // формирование объединенной выборки
        List<Retina> wUnionInit(List<List<Retina>> l)
        {
            List<Retina> w = new List<Retina>();
            foreach (var sample in l)
                foreach (var retina in sample)
                    w.Add(retina);            
            return w;
        }

        double lambdaCalc(Retina w, Retina l)
        {
            Retina lw = l.Subtraction(w);
            double num = 0, denom = 0;
            for (int i = 0; i < lw.Height; ++i)
                for (int j = 0; j < lw.Width; ++j)
                {
                    num += w[i, j] * lw[i, j];
                    denom += lw[i, j] * lw[i, j];
                }
            return -num / denom;
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
            foreach (Retina _w in w)
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
        //
        static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            if (indexA == indexB)
                return;
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
        // проверка линейной разделимости выборок
        bool isLinearlySeparable(List<List<Retina>> sample,int classSize)
        {            
            // шаг 1 - инициализация           
            // искомая выборка nSample должна быть в начале списка
            //for (int nSample = 0; nSample < classes; ++nSample)
            //{
            //    Swap(sample, 0, nSample);
                //for (int k = nSample + 1; k < classes; ++k)                    
            List<Retina> sampling = wSeparationInit(sample[0], sample[1]);//new List<Retina>();
                //for (int i = 0; i < classSize; ++i)
                //    sampling.Add(sample[nSample][i]);
                //for (int i = 0; i < classSize; ++i)
                //    sampling.Add(sample[k][i]);
                //for (int j = 0; j < 2; ++j)
                //    for (int i = 0; i < classSize; ++i)
                //        sampling.Add(sample[j][i]);
                //for (int i = 2; i < classes; ++i)
                //    for (int j = 0; j < classSize; ++j)
                //        sampling[classSize + j] = sampling[classSize + j].Union(sample[i][j]);
                //// взятие элементов 2-ой выборки с отрицательным знаком
                //for (int i = 0; i < classSize; ++i)
                //    sampling[classSize + i].Neg();
                //
                int m = (int)form2.numericUpDown1.Value;
                int n = (int)form2.numericUpDown2.Value;
                Retina l = new Retina(m + 1, n, true);
                //threshold.Add(1);
                l.FillExtend(1, -1);
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
            //}
            return stop;
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
            if (figureContour == null)
                return;
            Point cntr = new Point();
            Retina mrot = null;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (iType == SHAPE_TYPE.TRIANGLE && figureContour.All(p => p.Y <= 0) || iType == SHAPE_TYPE.DIGIT && figureContour[0].Y <= 0)
                        return;
                    for (int i = 0; i < figureContour.Count(); ++i)
                        figureContour[i].Y -= shift;
                    break;
                case Keys.Down:
                    double dh = Math.Floor(pictureBox1.Image.Height * .95);
                    if (iType == SHAPE_TYPE.TRIANGLE && figureContour.All(p => p.Y >= dh) || iType == SHAPE_TYPE.DIGIT && figureContour[1].Y >= dh)
                        return;
                    for (int i = 0; i < figureContour.Count(); ++i)
                        figureContour[i].Y += shift;
                    break;
                case Keys.Left:
                    if (figureContour[0].X <= 0)
                        return;
                    for (int i = 0; i < figureContour.Count(); ++i)
                        figureContour[i].X -= shift;
                    break;
                case Keys.NumPad6:  // поворот по часовой стрелке  
                    cntr = bulkCenter(figureContour);
                    mrot = Retina.Translation(-cntr.X, -cntr.Y) * Retina.Rotation(Math.PI / 2) * Retina.Translation(cntr.X, cntr.Y);
                    for (int i = 0; i < figureContour.Count(); ++i)
                        figureContour[i] *= mrot;
                    break;
                case Keys.Right:
                    if (figureContour[1].X >= pictureBox1.Image.Width - 1)
                        return;
                    for (int i = 0; i < figureContour.Count(); ++i)
                        figureContour[i].X += shift;
                    break;
                case Keys.NumPad4:  // поворот против часовой стрелки
                    cntr = bulkCenter(figureContour);
                    mrot = Retina.Translation(-cntr.X, -cntr.Y) * Retina.Rotation(-Math.PI / 2) * Retina.Translation(cntr.X, cntr.Y);
                    for (int i = 0; i < figureContour.Count(); ++i)
                        figureContour[i] *= mrot;
                    break;
                default:
                    return;
            }
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(pictureBox1.BackColor);
            if (iType == SHAPE_TYPE.TRIANGLE)
            {
                g.DrawPolygon(pen, figureContour);
                g.FillPolygon(new SolidBrush(pen.Color), figureContour);
            }
            else
            {
                //g.DrawEllipse(pen, new Rectangle(figureContour[0], new Size(figureContour[1].X - figureContour[0].X, figureContour[1].Y - figureContour[0].Y)));
                //Rectangle rect = new Rectangle(figureContour[0], new Size(figureContour[1].X - figureContour[0].X, figureContour[1].Y - figureContour[0].Y));
                g.DrawString(form2.numericUpDown3.Value.ToString(), font, new SolidBrush(pen.Color), new Rectangle(figureContour[0], new Size(figureContour[1].X - figureContour[0].X, figureContour[1].Y - figureContour[0].Y)));
                //g.DrawRectangle(pen, rect);
            }
            pictureBox1.Invalidate();
        }

        private void new1_Click(object sender, EventArgs e)
        {
            form2.ActiveControl = form2.numericUpDown1;
            if (radioButton1.Checked)
                form2.groupBox1.Visible = true;
            else
                form2.groupBox2.Visible = true;
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
                Image im = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(im);
                g.Clear(pictureBox1.BackColor);
                int centerMargin = Math.Min(w, h) / 4;
                font = new Font(FontFamily.GenericSerif, (float)Math.Floor(w / 2.083));
                if (iType == SHAPE_TYPE.TRIANGLE)
                {
                    figureContour = new Point[3];
                    if (form2.radioButton1.Checked)
                    {
                        figureContour[0] = new Point(w / 2 - centerMargin, h / 2 + centerMargin);
                        figureContour[1] = new Point(w / 2 + centerMargin, h / 2 + centerMargin);
                        figureContour[2] = new Point(w / 2, h / 2 - centerMargin);
                    }
                    else
                    {
                        figureContour[0] = new Point(w / 2 - centerMargin, h / 2 - centerMargin);
                        figureContour[1] = new Point(w / 2 + centerMargin, h / 2 - centerMargin);
                        figureContour[2] = new Point(w / 2, h / 2 + centerMargin);
                    }
                    g.DrawPolygon(pen, figureContour);
                    g.FillPolygon(new SolidBrush(pen.Color), figureContour);
                }
                else
                {
                    figureContour = new Point[2];
                    figureContour[0] = new Point(w / 2 - centerMargin, h / 2 - (int)(1.5 * centerMargin));
                    figureContour[1] = new Point(w / 2 + centerMargin, h / 2 + (int)(1.5 * centerMargin));
                    //g.DrawEllipse(pen, new Rectangle(figureContour[0], new Size(2 * centerMargin, 3 * centerMargin)));                    
                    //Rectangle rect = new Rectangle(figureContour[0], new Size(2 * centerMargin, 3 * centerMargin));
                    g.DrawString(form2.numericUpDown3.Value.ToString(), font, new SolidBrush(pen.Color), new Rectangle(figureContour[0], new Size(2 * centerMargin, 3 * centerMargin)));
                    //g.DrawRectangle(pen, rect);
                }
                g.Dispose();
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                pictureBox1.Image = im;
                this.ActiveControl = this.pictureBox1;
            }
            form2.groupBox1.Visible = form2.groupBox2.Visible = false;
        }

        private void learn1_Click(object sender, EventArgs e)
        {
            if (form3.ShowDialog() == DialogResult.OK)
            {
                classes = radioButton1.Checked ? 2 : (int)form2.numericUpDown3.Maximum + 1;         // кол-во распознаваемых классов
                int classSize = int.Parse(form3.textBox1.Text); // кол-во элементов в классе
                var sample = sampleInit(classes, classSize);
                if (sample == null)
                    return;
                // Фаза 1 - проверка на линейную разделимость
                bool isSeparable = true;
                    //isLinearlySeparable(sample, classSize);
                if (isSeparable)
                {
                    // Фаза 2 - определение порогов и разделяющих прямых                
                    threshold = new List<double>();
                    separationLine = new List<Retina>();
                    // искомая выборка nSample должна быть в начале списка
                    for (int nSample = 0; nSample < classes; ++nSample)
                    {
                        Swap(sample, 0, nSample);
                        List<Retina> x = sample.First();
                        List<Retina> y = wUnionInit(sample.Skip(1).ToList());
                        Retina line = Kozinets(toConvexHull(x, y));   // определение наилучшей разделяющей прямой                        
                        separationLine.Add(line);
                        if (line == null)
                            return;
                        // поиск диапазона порога: maxY < T <= minX
                        double maxY = double.MinValue;
                        for (int i = 0; i < (classes - 1) * classSize; ++i)
                        {                            
                            double sc = line.scalarProduct(y[i]);
                            if (sc > maxY)
                                maxY = sc;
                        }
                        double minX = double.MaxValue;
                        for (int i = 0; i < classSize; ++i)
                        {
                            double sc = line.scalarProduct(x[i]);
                            if (sc < minX)
                                minX = sc;                            
                        }                        
                        threshold.Add((maxY + minX) / 2.0);
                        //Console.WriteLine(threshold);
                    }
                    MessageBox.Show("Обучение завершено.");
                    recognize1.Visible = true;
                }
                else
                {
                    MessageBox.Show("Классы линейно неразделимы ");
                    return;
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
            switch (iType)
            {
                case SHAPE_TYPE.DIGIT:
                    {
                        List<int> idx = new List<int>();
                        for (int i = 0; i < classes; ++i)
                            idx.Add(i);
                        bool recognized = false;
                        for (int i = 0; i < classes; ++i)
                        {
                            double sc = separationLine[i].scalarProduct(s);
                            if (sc >= threshold[i] && idx.Where(j => j != i).All(k => separationLine[k].scalarProduct(s) < threshold[k]))
                            {
                                MessageBox.Show(i.ToString());
                                recognized = true;
                                break;
                            }
                        }
                        if(!recognized)
                            MessageBox.Show("Неизвестный класс");
                        break;
                    }
                case SHAPE_TYPE.TRIANGLE:
                    {
                        double sc = separationLine[0].scalarProduct(s);
                        if (sc >= threshold[0])
                            MessageBox.Show("Up");
                        else
                            MessageBox.Show("Down");
                        break;
                    }
                default:
                    break;
            }
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

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            this.iType = sender == radioButton1 ? SHAPE_TYPE.TRIANGLE : SHAPE_TYPE.DIGIT;
            recognize1.Visible = false;
        }
    }
}