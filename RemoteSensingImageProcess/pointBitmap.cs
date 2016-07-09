using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RemoteSensingImageProcess
{
    class pointBitmap
    {
        Bitmap source = null;
        IntPtr intPtr = IntPtr.Zero;
        BitmapData bitmapData = null;

        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public pointBitmap(Bitmap source)
        {
            this.source = source;
        }

        public void LockBits()
        {
            try
            {
                // 获取位图的宽度和高度
                Width = source.Width;
                Height = source.Height;

                // 获取锁定的总像素数
                int PixelCount = Width * Height;

                // 创建矩形锁
                Rectangle rect = new Rectangle(0, 0, Width, Height);

                // 获取源位图像素格式大小
                Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // 检查是否是8、 24 或 32位图
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);

                //得到首地址
                unsafe
                {
                    intPtr = bitmapData.Scan0;
                    //二维图像循环
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UnlockBits()
        {
            try
            {
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Color GetPixel(int x, int y)
        {
            unsafe
            {
                byte* ptr = (byte*)intPtr;
                ptr = ptr + bitmapData.Stride * y;
                ptr += Depth * x / 8;
                Color color = Color.Empty;
                if (Depth == 32)
                {
                    int alpha = ptr[3];
                    int red = ptr[2];
                    int green = ptr[1];
                    int blue = ptr[0];
                    color = Color.FromArgb(alpha, red, green, blue);
                }
                else if (Depth == 24)
                {
                    int red = ptr[2];
                    int green = ptr[1];
                    int blue = ptr[0];
                    color = Color.FromArgb(red, green, blue);
                }
                else if (Depth == 8)
                {
                    int r = ptr[0];
                    color = Color.FromArgb(r, r, r);
                }
                return color;
            }
        }

        public void SetPixel(int x, int y, Color color)
        {
            unsafe
            {
                byte* ptr = (byte*)intPtr;
                ptr = ptr + bitmapData.Stride * y;
                ptr += Depth * x / 8;
                if (Depth == 32)
                {
                    ptr[3] = color.A;
                    ptr[2] = color.R;
                    ptr[1] = color.G;
                    ptr[0] = color.B;
                }
                else if (Depth == 24)
                {
                    ptr[2] = color.R;
                    ptr[1] = color.G;
                    ptr[0] = color.B;
                }
                else if (Depth == 8)
                {
                    ptr[2] = color.R;
                    ptr[1] = color.G;
                    ptr[0] = color.B;
                }
            }
        }
    }
}
