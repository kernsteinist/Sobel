using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BMP
{

    public partial class Form1 : Form
    {

        FileStream fs;
        byte[] signature = new byte[2];
        byte[] sizefile = new byte[4];
        byte[] origo = new byte[4];
        byte[] widtharray = new byte[4];
        byte[] heightarray = new byte[4];
        byte[] sizedata = new byte[4];

        int height;
        int width;
        int padding;

        public byte[,] pixArray;
        public byte[,] paddingArray;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtFile.Text = openFileDialog1.FileName;
        }

        string tostr(byte[] source, int chk)
        {
            string ret = "";

            for (int i = 0; i < source.Length; i++)
            {
                if (chk == 0)
                {
                    ret += source[i].ToString("X") + " ";
                }
                else
                {
                    ret += source[i].ToString("X");
                }
            }

            return ret;
        }

        byte[,] sobeloperation(byte[,] pxArray)
        {
            byte[,] ret = new byte[height, width];
            byte[,] dest = new byte[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width * 3; j += 3)
                {
                    ret[i, j / 3] = pxArray[i, j];
                }
            }

            int gx;
            int gy;
            int val;

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    /* 
                     sobel-y --> [1   2   1]
                                 [0   0   0]
                                 [-1 -2  -1]
                     
                    sobel-x -->  [1 0 -1]
                                 [2 0 -2]
                                 [1 0 -1] 
                    
                     */

                    gx = ret[y - 1, x - 1] + 2 * ret[y, x - 1] + ret[y + 1, x - 1] - ret[y - 1, x + 1] - 2 * ret[y, x + 1] - ret[y + 1, x + 1];
                    gy = ret[y - 1, x - 1] + 2 * ret[y - 1, x] + ret[y - 1, x + 1] - ret[y + 1, x - 1] - 2 * ret[y + 1, x] - ret[y + 1, x + 1];

                    val = Math.Abs(gx) + Math.Abs(gy);
                    if (val > 255)
                        val = 255;
                    dest[y, x] = (byte)val;
                }
            }

            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 3; j < width * 3 - 3; j += 3)
                {
                    pxArray[i, j] = pxArray[i, j + 1] = pxArray[i, j + 2] = dest[i, j / 3];
                }
            }

            return pxArray;
        }

        void fill_pixArray_paddingArray(byte[] src)
        {
            int j = 0;

            for (int h = 0; h < height; h++)
            {
                int p = 0;
                for (int w = 0; w < width * 3; w++)
                {
                    pixArray[h, w] = src[j++];
                }
                while (p < padding)
                    paddingArray[h, p++] = src[j++];
            }
        }

        byte[] join_again(byte[,] px, byte[,] pad, int size)
        {
            byte[] ret = new byte[size];
            int z = 0;
            
            for (int i = 0; i < height; i++)
            {
                int p = 0;
                for (int j = 0; j < width * 3; j++)
                {
                    ret[z++] = px[i, j];
                }

                while (p < padding)
                    ret[z++] = pad[i, p++];
            }
            
            return ret;
        }


        byte[,] tograyscale(byte[,] pxarray)
        {
            byte[,] ret = new byte[height, width * 3];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width * 3; j += 3)
                {
                    byte val = (byte)((pxarray[i, j] + pxarray[i, j + 1] + pxarray[i, j + 2]) / 3);
                    ret[i, j] = ret[i, j + 1] = ret[i, j + 2] = val;
                }
            }

            return ret;
        }

        void reversing_meversing(byte[,] pixArray, byte[,] padArray)
        {
            byte[,] ret = new byte[height, width * 3];
            int top = height - 1;

            for (int i = 0; i < height / 2; i++)
            {
                int p = 0;

                for (int j = 0; j < width * 3; j++)
                {
                    byte tmp = pixArray[i, j];
                    pixArray[i, j] = pixArray[top, j];
                    pixArray[top, j] = tmp;
                }
                
                while (p < padding)
                {
                    byte tmp = padArray[i, p];
                    padArray[i, p] = paddingArray[top, p];
                    paddingArray[top, p] = tmp;
                    p++;
                }
                
                top--;
            }
        }

        void bmp()
        {
            fs = new FileStream(txtFile.Text, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            fs.Seek(0, SeekOrigin.Begin); signature = br.ReadBytes(2);
            fs.Seek(2, SeekOrigin.Begin); sizefile = br.ReadBytes(4);
            fs.Seek(10, SeekOrigin.Begin); origo = br.ReadBytes(4);
            fs.Seek(18, SeekOrigin.Begin); widtharray = br.ReadBytes(4);
            fs.Seek(22, SeekOrigin.Begin); heightarray = br.ReadBytes(4);
            fs.Seek(34, SeekOrigin.Begin); sizedata = br.ReadBytes(4);

            height = BitConverter.ToInt32(heightarray, 0);
            width = BitConverter.ToInt32(widtharray, 0);

            int siz_file = BitConverter.ToInt32(sizefile, 0);
            lblsignature.Text = tostr(signature, 0);
            lblsizefile.Text = BitConverter.ToInt32(sizefile, 0).ToString() + " bytes";
            lblwidth.Text = width.ToString();
            lblheight.Text = height.ToString();
            lbloffset.Text = tostr(origo, 0);
            lblsizedata.Text = BitConverter.ToInt32(sizedata, 0).ToString() + " bytes";
            lblheight.Text = BitConverter.ToInt32(heightarray, 0).ToString();

            fs.Seek(BitConverter.ToInt32(origo, 0), SeekOrigin.Begin);

            padding = (4 - ((width * 3) % 4));

            if (padding == 4)
                padding = 0;

            int rowsizewithpadding = width * 3 + padding;

            pixArray = new byte[height, width * 3];
            paddingArray = new byte[height, padding];
            byte[] witttiuu = br.ReadBytes(height * rowsizewithpadding);

            fill_pixArray_paddingArray(witttiuu);
            byte[,] grayimage = tograyscale(pixArray);
            reversing_meversing(grayimage, paddingArray);
            byte[,] lastexit = sobeloperation(grayimage);
            byte[] result = join_again(lastexit, paddingArray, witttiuu.Length);

            var bitmap = new Bitmap(width, height);

            var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            Marshal.Copy(result, 0, data.Scan0, witttiuu.Length);
            bitmap.UnlockBits(data);

            pictureBox1.Image = bitmap;
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            bmp();
        }
    }
}
