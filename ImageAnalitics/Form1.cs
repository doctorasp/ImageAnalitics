using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace ImageAnalitics
{
    public partial class Form1 : Form
    {
        int[] hist;
        private static SolidBrush histColor = new SolidBrush(Color.Black);

        public Form1()
        {
            InitializeComponent();
        }

        private static int[] GetHistogramm(Bitmap image)
        {
            int[] result = new int[256];
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    int i = (int)(255 * image.GetPixel(x, y).GetBrightness());
                    result[i]++;
                }

            return result;
        }


        private static void DrawHistogramm(Graphics g, Rectangle rect, int[] hist)
        {
            float max = hist.Max();
            if (max > 0)
                for (int i = 0; i < hist.Length; i++)
                {
                    float h = rect.Height * hist[i] / (float)max;
                    g.FillRectangle(histColor, i * rect.Width / (float)hist.Length, rect.Height - h, rect.Width / (float)hist.Length, h);
                }
        }

        public static void EdgeDetection(Bitmap b, float threshold)
        {
            Bitmap bSrc = (Bitmap)b.Clone();

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;

            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0;
                byte* pSrc = (byte*)(void*)bmSrc.Scan0;

                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width - 1;
                int nHeight = b.Height - 1;

                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        var p0 = ToGray(pSrc);
                        var p1 = ToGray(pSrc + 3);
                        var p2 = ToGray(pSrc + 3 + stride);

                        if (Math.Abs(p1 - p2) + Math.Abs(p1 - p0) > threshold)
                            p[0] = p[1] = p[2] = 255;
                        else
                            p[0] = p[1] = p[2] = 0;

                        p += 3;
                        pSrc += 3;
                    }
                    p += nOffset;
                    pSrc += nOffset;
                }
            }

            b.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);
        }

        static unsafe float ToGray(byte* bgr)
        {
            return bgr[2] * 0.3f + bgr[1] * 0.59f + bgr[0] * 0.11f;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "Image file|*.jpg;*.png;*.bmp" };
            Bitmap image;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                image = (Bitmap)Bitmap.FromFile(ofd.FileName);
                EdgeDetection(image, 10);
                this.pictureBox1.BackgroundImage = image;
            };

            image = (Bitmap)Image.FromFile(ofd.FileName);
            hist = GetHistogramm(image as Bitmap);

            this.pictureBox2.Paint += (o, g) => DrawHistogramm(g.Graphics, ClientRectangle, hist);
            this.pictureBox2.Resize += (o, g) => Refresh();
            Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                histColor = new SolidBrush(colorDialog1.Color);
                this.pictureBox2.Paint += (o, g) => DrawHistogramm(g.Graphics, ClientRectangle, hist);
                this.pictureBox2.Resize += (o, g) => Refresh();
                Refresh();
            }
        }
    }
}
