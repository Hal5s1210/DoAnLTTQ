﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace DoAnLTTQ
{

    public enum Tool
    {
        Pen, Eraser, Picker
    }
    public partial class DrawSpace : UserControl
    {
        private Bitmap processing;
        Graphics gFinal;
        Graphics gProcess;
        Graphics gTop;
        Graphics g;
        Size originalSize;
        float zoom;
        public float Zoom { get => zoom; }
        public Matrix ScaleMatrix { get; set; }
        public Tools.Tools Tools { get; set; }

        public DrawSpace()
        {
            InitializeComponent();
            ScaleMatrix = new Matrix();
            zoom = 1f;
        }

        public void Init()
        {
            processing = new Bitmap(this.Size.Width, this.Size.Height);
            g = Graphics.FromImage(processing);

            InitGraphic();

            originalSize = this.Size;
        }

        public void SetCenter()
        {
            WorkSpace ws = (WorkSpace)this.Parent;

            float z = 1f;
            float a = (float)originalSize.Width / originalSize.Height;

            if(a > 1)
            {
                z = (float)(ws.Width - 40) / originalSize.Width;
            }
            else z = (float)(ws.Height - 40) / originalSize.Height;

            Scaling(z);

            this.Left = ws.Width / 2 - this.Width / 2;
            this.Top = ws.Height / 2 - this.Height / 2;
        }

        void InitGraphic()
        {
            finalBox.Image = new Bitmap(this.Size.Width, this.Size.Height);
            gFinal = Graphics.FromImage(finalBox.Image);
            processBox.Image = new Bitmap(this.Size.Width, this.Size.Height);
            gProcess = Graphics.FromImage(processBox.Image);
            topBox.Image = new Bitmap(this.Size.Width, this.Size.Height);
            gTop = Graphics.FromImage(topBox.Image);
        }

        public void Scaling(float scale)
        {
            zoom = scale;
            Size newSize = new Size((int)(originalSize.Width * zoom), (int)(originalSize.Height * zoom));
            this.Size = newSize;
            InitGraphic();
            ScaleMatrix.Reset();
            ScaleMatrix.Scale(scale, scale);
            Invalidate();
        }

        PointF ScaledPoint(PointF p)
        {
            return new PointF(p.X / zoom, p.Y / zoom);
        }

        public PictureBox Event
        {
            get
            {
                return topBox;
            }
        }

        public Bitmap Final
        {
            get
            {
                return (Bitmap)finalBox.Image;
            }
        }

        public Graphics Final_Graphics
        {
            get
            {
                return gFinal;
            }
        }

        public bool CurrentVisible { get; set; }
        public Image ProcessBoxImage
        {
            set
            {
                g.DrawImage(value, 0, 0);
            }
            get
            {
                return processing;
            }
        }

        public void ClearProcess()
        {
            if (processing == null) return;
            g.Clear(Color.Transparent);
        }

        public void BGGenerator(Color c)
        {
            if(c==Color.Transparent)
            {
                int m = (int)Math.Ceiling((double)Size.Width / 512);
                int n = (int)Math.Ceiling((double)Size.Height / 512);
                Bitmap bg = new Bitmap(512 * m, 512 * n);
                using (Graphics g = Graphics.FromImage(bg))
                {
                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            g.DrawImage(Properties.Resources.TransparencyBG, i * 512, j * 512);
                        }
                    }
                    BackgroundImage = bg;
                }
            }
            else
            {
                Bitmap bg = new Bitmap(Size.Width, Size.Height);
                using (Graphics g = Graphics.FromImage(bg))
                {
                    using (SolidBrush brush = new SolidBrush(c))
                        g.FillRectangle(brush, 0, 0, Size.Width, Size.Height);
                    BackgroundImage = bg;
                }
            }
        }

        RectangleF displayRect;
        PointF p1;
        PointF p2;
        void processDisplay()
        {
            PointF p = new PointF(Math.Min(p1.X - (float)Tools.Size / 2 - 1, p2.X - (float)Tools.Size / 2 - 1),
                                Math.Min(p1.Y - (float)Tools.Size / 2 - 1, p2.Y - (float)Tools.Size / 2 - 1));

            displayRect = new RectangleF(p.X,
                                        p.Y,
                                        Math.Abs(p2.X - p1.X) + Tools.Size + 2,
                                        Math.Abs(p2.Y - p1.Y) + Tools.Size + 2);

            gProcess.MultiplyTransform(ScaleMatrix);
            gProcess.DrawImage(processing, p.X, p.Y, displayRect, GraphicsUnit.Pixel);
            gProcess.ResetTransform();
        }

        public void Mouse_Down(object sender, MouseEventArgs e)
        {
            if (processing == null)
            {
                processing = new Bitmap(this.Size.Width, this.Size.Height);
                g = Graphics.FromImage(processing);
            }

            switch (Tools.Tool)
            {
                case Tool.Pen:
                    {
                        p1 = ScaledPoint(e.Location);
                        Tools.Pen.GetLocation(ScaledPoint(e.Location));
                    }
                    break;
                case Tool.Picker:
                    {
                        Tools.Picker.GetColor((Bitmap)finalBox.Image, ScaledPoint(e.Location));
                    }
                    break;
                case Tool.Eraser:
                    {
                        p1 = ScaledPoint(e.Location);
                        Tools.Eraser.GetLocation(ScaledPoint(e.Location));
                    }
                    break;
                default:
                    break;
            }

        }

        public void Mouse_Move(object sender, MouseEventArgs e)
        {
            if (processing == null)
                return;

            if (e.Button == MouseButtons.Left)
            {
                switch (Tools.Tool)
                {
                    case Tool.Pen:
                        {
                            p2 = ScaledPoint(e.Location);
                            Tools.Pen.Draw(g, ScaledPoint(e.Location));
                            if (CurrentVisible)
                                processDisplay();
                            p1 = p2;
                        }
                        break;
                    case Tool.Eraser:
                        {
                            p2 = ScaledPoint(e.Location);
                            Tools.Eraser.Draw(g, ScaledPoint(e.Location));
                            if (CurrentVisible)
                                processDisplay();
                            p1 = p2;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (Tools.Tool == Tool.Pen || Tools.Tool == Tool.Eraser)
            {
                float n = Tools.Size * zoom;
                if (n != 0)
                {
                    gTop.Clear(Color.Transparent);
                    gTop.DrawEllipse(Pens.Black, new RectangleF(e.X - n / 2, e.Y - n / 2, n, n));
                    topBox.Invalidate();
                }
            }
        }

        public void Mouse_Up(object sender, MouseEventArgs e)
        {
            gProcess.Clear(Color.Transparent);
        }

        private void Mouse_Leave(object sender, EventArgs e)
        {
            gTop.Clear(Color.Transparent);
            topBox.Invalidate();
        }
    }
}
