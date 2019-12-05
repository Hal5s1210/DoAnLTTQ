﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAnLTTQ.Forms
{
    public partial class Pixelate : Form
    {
        private Form1 f;
        private LayerContainer lc;
        private Bitmap origin;
        private Bitmap adjusted;
        private System.Drawing.Imaging.BitmapData bmpData;
        private byte[] imagePixels;
        private int dataSize;

        public Pixelate(Form1 f, LayerContainer lc)
        {
            InitializeComponent();
            this.f = f;
            this.lc = lc;
        }
        public Bitmap Image
        {
            set
            {
                origin = new Bitmap(value);
                adjusted = new Bitmap(origin);
            }
            get
            {
                return adjusted;
            }
        }

        public void Initialize()
        {
            bmpData = origin.LockBits(new Rectangle(0, 0, origin.Width, origin.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, origin.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            dataSize = Math.Abs(bmpData.Stride) * origin.Height;
            imagePixels = new byte[dataSize];
            System.Runtime.InteropServices.Marshal.Copy(ptr, imagePixels, 0, dataSize);
            origin.UnlockBits(bmpData);
        }

        int pixel = 1;
        private void Adjust()
        {
            if (adjusted != null)
            {
                adjusted.Dispose();
                adjusted = null;
            }
            adjusted = new Bitmap(origin);

            if (pixel != 1)
            {
                int k = pixel / 2;
                using (Graphics g = Graphics.FromImage(adjusted))
                {
                    g.Clear(Color.Transparent);
                    for (int y = 0; y < adjusted.Height; y += pixel)
                    {
                        for (int x = 0; x < adjusted.Width; x += pixel)
                        {
                            g.FillRectangle(new SolidBrush(origin.GetPixel(x, y)), x - k, y - k, pixel, pixel);
                        }
                    }
                }
            }

            lc.ProcessUpdate(adjusted, true);
            f.DSUpdate();
        }

        private void PixelTrack_Scroll(object sender, EventArgs e)
        {
            pixel = pixelTrack.Value;
            label3.Text = pixelTrack.Value.ToString();
            Adjust();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            lc.ProcessUpdate(origin, true);
            f.DSUpdate();
        }
    }
}