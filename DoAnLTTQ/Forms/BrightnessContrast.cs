﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace DoAnLTTQ.Forms
{
    public partial class BrightnessContrast : Form
    {
        private LayerContainer lc;
        private Form1 f;
        private Bitmap origin;
        private Bitmap adjusted;
        public BrightnessContrast(Form1 f, LayerContainer lc)
        {
            InitializeComponent();
            this.lc = lc;
            this.f = f;
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

        private float brightness = 0f;
        private float contrast = 0f;
        private void Adjust()
        {
            if (adjusted != null)
            {
                adjusted.Dispose();
                adjusted = null;
            }
            
            adjusted = new Bitmap(origin);
            using (ImageAttributes imageAttributes = new ImageAttributes())
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix00 = matrix.Matrix11 = matrix.Matrix22 = contrast + 1f;
                matrix.Matrix33 = matrix.Matrix44 = 1f;
                matrix.Matrix40 = matrix.Matrix41 = matrix.Matrix42 = brightness;

                imageAttributes.SetColorMatrix(matrix);
                using (Graphics g = Graphics.FromImage(adjusted))
                {
                    g.DrawImage(adjusted, new Rectangle(0, 0, adjusted.Width, adjusted.Height), 0, 0, origin.Width, origin.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }

            lc.ProcessUpdate(adjusted, true);
            f.DSUpdate();
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            label3.Text = brightnessTrack.Value.ToString();
            brightness = (float)brightnessTrack.Value / 100;
            Adjust();
        }

        private void TrackBar2_Scroll(object sender, EventArgs e)
        {
            label4.Text = contrastTrack.Value.ToString();
            contrast = (float)contrastTrack.Value / 100;
            Adjust();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            brightness = brightnessTrack.Value = 0;
            label3.Text = brightnessTrack.Value.ToString();
            contrast = contrastTrack.Value = 0;
            label4.Text = contrastTrack.Value.ToString();
            Adjust();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            lc.ProcessUpdate(origin, true);
            f.DSUpdate();
        }
    }
}
