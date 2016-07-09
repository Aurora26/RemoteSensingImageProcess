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
    class memoryBitmap
    {
        private Bitmap source = null;
        private IntPtr intPtr = IntPtr.Zero;
        private BitmapData bitmapData = null;

        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public memoryBitmap(Bitmap source)
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
                    throw new ArgumentException("仅支持8、24或32位图图像");
                }

                // 锁定位图，返回位图数据
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);

                // 创建要复制像素值的字节数组
                int step = Depth / 8;
                Pixels = new byte[PixelCount * step];
                intPtr = bitmapData.Scan0;

                // 将数据复制到数组
                Marshal.Copy(intPtr, Pixels, 0, Pixels.Length);
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
                // 将数据从数组复制到指针
                Marshal.Copy(Pixels, 0, intPtr, Pixels.Length);

                // 解锁位图数据
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Color GetPixel(int x, int y)
        {
            Color color = Color.Empty;

            // 获取颜色组件数
            int colorCount = Depth / 8;

            // 获取指定像素的起始索引
            int i = ((y * Width) + x) * colorCount;

            if (i > Pixels.Length - colorCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte blue = Pixels[i];
                byte green = Pixels[i + 1];
                byte red = Pixels[i + 2];
                byte alpha = Pixels[i + 3]; 

                color = Color.FromArgb(alpha, red, green, blue);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte blue = Pixels[i];
                byte green = Pixels[i + 1];
                byte red = Pixels[i + 2];
                color = Color.FromArgb(red, green, blue);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                color = Color.FromArgb(c, c, c);
            }
            return color;
        }

        public void SetPixel(int x, int y, Color color)
        {
            // 获取颜色组件数
            int colorCount = Depth / 8;

            // 获取指定像素的起始索引
            int i = ((y * Width) + x) * colorCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }
    }
}
