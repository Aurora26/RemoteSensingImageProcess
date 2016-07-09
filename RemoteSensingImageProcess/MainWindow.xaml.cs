using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Drawing;
using OSGeo.GDAL;



namespace RemoteSensingImageProcess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool mouseDown;
        private Point mouseXY;
        private double min = 0.000001, max = 50; //最小/最大放大倍数

        private System.Drawing.Bitmap my_Bitmap;

        private struct IHS     //定义 IHS色彩空间数据
        {
            public double I;
            public double H;
            public double S;
        }

        public static byte MinGrayValues;   //直方图范围初始最小值
        public static byte MaxGrayValues;    //直方图范围初始最大值
        public static byte MinNewValues;       //直方图范围更改后最小值
        public static byte MaxNewValues;       //直方图范围更改后最大值

        //定义 IHS色彩空间数据设置的系数
        public static double setIHS_I = 1;
        public static double setIHS_H = 1;
        public static double setIHS_S = 1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SharpMap.GdalConfiguration.ConfigureGdal();
            SharpMap.GdalConfiguration.ConfigureOgr();

            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");

            setViewSize();
        }

        private void Title_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ChangeWindowSize();
            }
        }


        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            //Environment.exit(0);
            System.Windows.Application.Current.Shutdown();
        }

        private void btn_maximize_Click(object sender, RoutedEventArgs e)
        {
            ChangeWindowSize();
        }

        private void Domousemove(ContentControl img, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            var group = image_content.FindResource("TransformGroup") as TransformGroup;
            var transform = group.Children[1] as TranslateTransform;
            var position = e.GetPosition(img);
            transform.X -= mouseXY.X - position.X;
            transform.Y -= mouseXY.Y - position.Y;
            mouseXY = position;
        }

        private void DowheelZoom(TransformGroup group, System.Windows.Point point, double delta)
        {
            var pointToContent = group.Inverse.Transform(point);
            var transform = group.Children[0] as ScaleTransform;
            if (transform.ScaleX + delta < min) return;
            if (transform.ScaleX + delta > max) return;
            transform.ScaleX += delta;
            transform.ScaleY += delta;
            var transform1 = group.Children[1] as TranslateTransform;
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);
        }

        private void ContentControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            img.CaptureMouse();
            mouseDown = true;
            mouseXY = e.GetPosition(img);
        }

        private void ContentControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            img.ReleaseMouseCapture();
            mouseDown = false;
        }

        private void ContentControl_MouseMove(object sender, MouseEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            if (mouseDown)
            {
                Domousemove(img, e);
            }
        }

        private void ContentControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            var point = e.GetPosition(img);
            var group = image_content.FindResource("TransformGroup") as TransformGroup;
            var delta = e.Delta * 0.001;
            DowheelZoom(group, point, delta);
        }

        private void menu_openFile_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Microsoft.Win32.OpenFileDialog OpenFileDialog = new Microsoft.Win32.OpenFileDialog();

                OpenFileDialog.Filter = "所有图像文件 | *.bmp; *.pcx; *.png; *.jpg; *.gif; *.img; *.hdr;" +
                                        "位图( *.bmp; *.jpg; *.png;...) | *.bmp; *.pcx; *.png; *.jpg; *.gif; *.tif; *.ico|";

                OpenFileDialog.Title = "打开图像文件";

                if (OpenFileDialog.ShowDialog() == true)
                {
                    String filename = OpenFileDialog.FileName.ToString();

                    OSGeo.GDAL.Dataset dataSet = Gdal.Open(filename, Access.GA_ReadOnly);

                    if (dataSet == null)
                    {
                        MessageBox.Show("影像打开失败");
                        return;
                    }

                    System.Drawing.Rectangle pictureRect = new System.Drawing.Rectangle();
                    pictureRect.X = 0;
                    pictureRect.Y = 0;

                    pictureRect.Width = Convert.ToInt16(this.mainScrollv.Width);
                    pictureRect.Height = Convert.ToInt16(this.mainScrollv.Height);

                    int[] disband = { 3, 2, 1 };

                    my_Bitmap = ImageOperate.GetImage(dataSet, pictureRect, disband);   //遥感影像构建位图

                    //my_Bitmap = new System.Drawing.Bitmap(filename);

                    //image_content.Stretch = Stretch.Fill;

                    //this.image_content.Source = new BitmapImage(new Uri(filename));

                    image_content.Source = BitmapToBitmapSource(my_Bitmap);
                }
            }
            catch
            {
                MessageBox.Show("请导入正确的图像文件");
                return;
            }
        }

        private void menu_savaFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menu_savaAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menu_closeFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menu_grayStretch_Click(object sender, RoutedEventArgs e)
        {
            if (my_Bitmap != null)
            {
                this.Cursor = Cursors.Wait;

                my_Bitmap = GrayStretchImage(my_Bitmap);

                this.Cursor = Cursors.Arrow;

                if (MaxNewValues != MaxGrayValues || MinNewValues != MinGrayValues)
                {
                    //image_content.Stretch = Stretch.Fill;
                    image_content.Source = BitmapToBitmapSource(my_Bitmap);
                }
                else
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show("请先导入图像文件");
                return;
            }
        }

        private void menu_IHSConvert_Click(object sender, RoutedEventArgs e)
        {
            if (my_Bitmap != null)
            {
                IHS[] imageIHS;

                this.Cursor = Cursors.Wait;

                imageIHS = IHSImage(my_Bitmap);

                HISWindow HISWindow = new HISWindow();
                HISWindow.ShowDialog();

                this.Cursor = Cursors.Wait;

                if (setIHS_I == 1 && setIHS_H == 1 && setIHS_S == 1)
                {
                    this.Cursor = Cursors.Arrow;
                    return;
                }

                else
                {
                    for (int i = 0; i < (my_Bitmap.Width * my_Bitmap.Height); i++)
                    {
                        if (setIHS_I != 1)
                        {
                            imageIHS[i].I *= setIHS_I;
                        }
                        if (setIHS_H != 1)
                        {
                            imageIHS[i].H *= setIHS_H;
                        }
                        if (setIHS_S != 1)
                        {
                            imageIHS[i].S *= setIHS_S;
                        }
                    }

                    my_Bitmap = RGBImage(my_Bitmap, imageIHS);

                    this.Cursor = Cursors.Arrow;

                    //image_content.Stretch = Stretch.Fill;
                    image_content.Source = BitmapToBitmapSource(my_Bitmap);
                }
            }
            else
            {
                MessageBox.Show("请先导入图像文件");
                return;
            }
        }

        private void menu_Blur_Click(object sender, RoutedEventArgs e)
        {
            if (my_Bitmap != null)
            {
                this.Cursor = Cursors.Wait;

                my_Bitmap = SoftenImage(my_Bitmap);

                this.Cursor = Cursors.Arrow;

                //image_content.Stretch = Stretch.Fill;
                image_content.Source = BitmapToBitmapSource(my_Bitmap);
            }
            else
            {
                MessageBox.Show("请先导入图像文件");
                return;
            }
        }

        private void menu_Sharpen_Click(object sender, RoutedEventArgs e)
        {
            if (my_Bitmap != null)
            {
                this.Cursor = Cursors.Wait;

                my_Bitmap = SharpenImage(my_Bitmap);

                this.Cursor = Cursors.Arrow;

                //image_content.Stretch = Stretch.Fill;
                image_content.Source = BitmapToBitmapSource(my_Bitmap);
            }
            else
            {
                MessageBox.Show("请先导入图像文件");
                return;
            }
        }

        private void menu_Negative_Click(object sender, RoutedEventArgs e)
        {
            if (my_Bitmap != null)
            {
                this.Cursor = Cursors.Wait;

                my_Bitmap = NegativeImage(my_Bitmap);

                this.Cursor = Cursors.Arrow;

                //image_content.Stretch = Stretch.Fill;
                image_content.Source = BitmapToBitmapSource(my_Bitmap);
            }
            else
            {
                MessageBox.Show("请先导入图像文件");
                return;
            }
        }

        private void menu_Gray_Click(object sender, RoutedEventArgs e)
        {
            if (my_Bitmap != null)
            {
                this.Cursor = Cursors.Wait;

                my_Bitmap = BlackImage(my_Bitmap);

                this.Cursor = Cursors.Arrow;

                //image_content.Stretch = Stretch.Fill;
                image_content.Source = BitmapToBitmapSource(my_Bitmap);
            }
            else
            {
                MessageBox.Show("请先导入图像文件");
                return;
            }
        }

        private void menu_extent_Click(object sender, RoutedEventArgs e)
        {
            var group = image_content.FindResource("TransformGroup") as TransformGroup;

            var scaleTransform = group.Children[1] as TranslateTransform;
            scaleTransform.X = 0;
            scaleTransform.Y = 0;

            var translateTransform = group.Children[0] as ScaleTransform;
            translateTransform.ScaleX = 1;
            translateTransform.ScaleY = 1;
        }

        private void setViewSize()
        {
            mainScrollv.Width = this.ActualWidth;
            mainScrollv.Height = this.ActualHeight - 50;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            BitmapSource result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //release resource
            DeleteObject(ptr);

            return result;
        }

        private void ChangeWindowSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        //双击图片清空
        private void mainScrollv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.image_content = null;
        }

        //**功能**/**//////////////////////////////////////////////////////////////////////

        //灰度值拉伸
        private System.Drawing.Bitmap GrayStretchImage(System.Drawing.Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            MinGrayValues = 255;
            MaxGrayValues = 0;

            double slope = 1.0;

            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(width, height);

            pointBitmap pointBitMap = new pointBitmap(bitmap);
            pointBitmap newPointBitMap = new pointBitmap(newBitmap);

            pointBitMap.LockBits();
            newPointBitMap.LockBits();

            System.Drawing.Color pixel;

            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    pixel = pointBitMap.GetPixel(i, j);

                    if (MinGrayValues > pixel.R)
                        MinGrayValues = pixel.R;
                    if (MinGrayValues > pixel.G)
                        MinGrayValues = pixel.G;
                    if (MinGrayValues > pixel.B)
                        MinGrayValues = pixel.B;

                    if (MaxGrayValues < pixel.R)
                        MaxGrayValues = pixel.R;
                    if (MaxGrayValues < pixel.G)
                        MaxGrayValues = pixel.G;
                    if (MaxGrayValues < pixel.B)
                        MaxGrayValues = pixel.B;
                }
            }

            GrayWindow GrayWindow = new GrayWindow();
            GrayWindow.ShowDialog();

            this.Cursor = Cursors.Wait;

            if (MaxNewValues != MaxGrayValues || MinNewValues != MinGrayValues)
            {
                slope = Convert.ToDouble((MaxNewValues - MinNewValues)) / Convert.ToDouble((MaxGrayValues - MinGrayValues));

                for (int i = 1; i < width - 1; i++)
                {
                    for (int j = 1; j < height - 1; j++)
                    {
                        int RGB_R = 0, RGB_G = 0, RGB_B = 0;

                        pixel = pointBitMap.GetPixel(i, j);

                        RGB_R = Convert.ToInt16(slope * (pixel.R - MinGrayValues) + MinNewValues);
                        RGB_G = Convert.ToInt16(slope * (pixel.G - MinGrayValues) + MinNewValues);
                        RGB_B = Convert.ToInt16(slope * (pixel.B - MinGrayValues) + MinNewValues);

                        newPointBitMap.SetPixel(i - 1, j - 1, System.Drawing.Color.FromArgb(RGB_R, RGB_G, RGB_B));
                    }
                }

                pointBitMap.UnlockBits();
                newPointBitMap.UnlockBits();
                return newBitmap;
            }
            else
            {
                pointBitMap.UnlockBits();
                newPointBitMap.UnlockBits();
                return bitmap;
            }
        }

        //锐化
        private System.Drawing.Bitmap SharpenImage(System.Drawing.Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(width, height);

            pointBitmap pointBitMap = new pointBitmap(bitmap);
            pointBitmap newPointBitMap = new pointBitmap(newBitmap);

            pointBitMap.LockBits();
            newPointBitMap.LockBits();

            System.Drawing.Color pixel;

            int[] template;

            //Laplace算子
            int[] Laplacian1 = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
            int[] Laplacian2 = { 0, 1, 0, 1, -4, 1, 0, 1, 0 };

            //Prewitt算子
            int[] Prewitt1 = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            int[] Prewitt2 = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };

            //Soble算子
            int[] Soble1 = { 1, 2, 1, 0, 0, 0, -1, -2, -1 };
            int[] Soble2 = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };


            switch (0)
            {
                case 0://Laplace1算子
                    template = Laplacian1;
                    break;
                case 1://Laplace2算子
                    template = Laplacian2;
                    break;
                case 2://Prewitt1算子
                    template = Prewitt1;
                    break;
                case 3://Prewitt2算子
                    template = Prewitt1;
                    break;
                case 4://Soble1算子
                    template = Prewitt1;
                    break;
                case 5://Soble2算子
                    template = Prewitt1;
                    break;
            }

            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    int RGB_R = 0, RGB_G = 0, RGB_B = 0;
                    int Index = 0;

                    for (int column = -1; column <= 1; column++)
                    {
                        for (int row = -1; row <= 1; row++)
                        {
                            pixel = pointBitMap.GetPixel(i + row, j + column);

                            RGB_R += pixel.R * template[Index];
                            RGB_G += pixel.G * template[Index];
                            RGB_B += pixel.B * template[Index];

                            Index++;
                        }
                    }

                    //处理颜色值溢出
                    RGB_R = RGB_R > 255 ? 255 : RGB_R;
                    RGB_R = RGB_R < 0 ? 0 : RGB_R;

                    RGB_G = RGB_G > 255 ? 255 : RGB_G;
                    RGB_G = RGB_G < 0 ? 0 : RGB_G;

                    RGB_B = RGB_B > 255 ? 255 : RGB_B;
                    RGB_B = RGB_B < 0 ? 0 : RGB_B;

                    newPointBitMap.SetPixel(i - 1, j - 1, System.Drawing.Color.FromArgb(RGB_R, RGB_G, RGB_B));
                }
            }
            pointBitMap.UnlockBits();
            newPointBitMap.UnlockBits();
            return newBitmap;
        }

        //柔化
        private System.Drawing.Bitmap SoftenImage(System.Drawing.Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(width, height);

            pointBitmap pointBitMap = new pointBitmap(bitmap);
            pointBitmap newPointBitMap = new pointBitmap(newBitmap);

            pointBitMap.LockBits();
            newPointBitMap.LockBits();

            System.Drawing.Color pixel;

            int[] template;
            int ratio;

            //高斯算子
            int[] Gauss = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
            int ratioGauss = 16;

            //均值算子1
            int[] Average1 = { 9, 9, 9, 9, 9, 9, 9, 9, 9 };
            int ratioAverage1 = 9;

            //均值算子2
            int[] Average2 = { 8, 8, 8, 8, 0, 8, 8, 8, 8 };
            int ratioAverage2 = 8;

            switch (0)
            {
                case 0://高斯算子
                    template = Gauss;
                    ratio = ratioGauss;
                    break;
                case 1://均值算子1
                    template = Average1;
                    ratio = ratioAverage1;
                    break;
                case 2://均值算子2
                    template = Average2;
                    ratio = ratioAverage2;
                    break;
            }

            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    int RGB_R = 0, RGB_G = 0, RGB_B = 0;
                    int Index = 0;

                    for (int column = -1; column <= 1; column++)
                    {
                        for (int row = -1; row <= 1; row++)
                        {
                            pixel = pointBitMap.GetPixel(i + row, j + column);

                            RGB_R += pixel.R * template[Index];
                            RGB_G += pixel.G * template[Index];
                            RGB_B += pixel.B * template[Index];

                            Index++;
                        }
                    }
                    RGB_R /= ratio;
                    RGB_G /= ratio;
                    RGB_B /= ratio;

                    //处理颜色值溢出
                    RGB_R = RGB_R > 255 ? 255 : RGB_R;
                    RGB_R = RGB_R < 0 ? 0 : RGB_R;

                    RGB_G = RGB_G > 255 ? 255 : RGB_G;
                    RGB_G = RGB_G < 0 ? 0 : RGB_G;

                    RGB_B = RGB_B > 255 ? 255 : RGB_B;
                    RGB_B = RGB_B < 0 ? 0 : RGB_B;

                    newPointBitMap.SetPixel(i - 1, j - 1, System.Drawing.Color.FromArgb(RGB_R, RGB_G, RGB_B));
                }
            }
            pointBitMap.UnlockBits();
            newPointBitMap.UnlockBits();
            return newBitmap;
        }

        //灰度图
        private System.Drawing.Bitmap BlackImage(System.Drawing.Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(width, height);

            pointBitmap pointBitMap = new pointBitmap(bitmap);
            pointBitmap newPointBitMap = new pointBitmap(newBitmap);

            pointBitMap.LockBits();
            newPointBitMap.LockBits();

            System.Drawing.Color pixel;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    pixel = pointBitMap.GetPixel(i, j);
                    int RGB_R, RGB_G, RGB_B, Result = 0;

                    RGB_R = pixel.R;
                    RGB_G = pixel.G;
                    RGB_B = pixel.B;

                    switch (2)
                    {
                        case 0://平均值法
                            Result = ((RGB_R + RGB_G + RGB_B) / 3);
                            break;
                        case 1://最大值法
                            Result = RGB_R > RGB_G ? RGB_R : RGB_G;
                            Result = Result > RGB_B ? Result : RGB_B;
                            break;
                        case 2://加权平均值法
                            Result = ((int)(0.3 * RGB_R) + (int)(0.59 * RGB_G) + (int)(0.11 * RGB_B));
                            break;
                    }
                    newPointBitMap.SetPixel(i, j, System.Drawing.Color.FromArgb(Result, Result, Result));
                }
            }
            pointBitMap.UnlockBits();
            newPointBitMap.UnlockBits();
            return newBitmap;
        }

        //反相
        private System.Drawing.Bitmap NegativeImage(System.Drawing.Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(width, height);

            pointBitmap pointBitMap = new pointBitmap(bitmap);
            pointBitmap newPointBitMap = new pointBitmap(newBitmap);

            pointBitMap.LockBits();
            newPointBitMap.LockBits();

            System.Drawing.Color pixel;

            for (int i = 1; i < width; i++)
            {
                for (int j = 1; j < height; j++)
                {
                    int RGB_R, RGB_G, RGB_B;
                    pixel = pointBitMap.GetPixel(i, j);

                    RGB_R = 255 - pixel.R;
                    RGB_G = 255 - pixel.G;
                    RGB_B = 255 - pixel.B;

                    newPointBitMap.SetPixel(i, j, System.Drawing.Color.FromArgb(RGB_R, RGB_G, RGB_B));
                }
            }
            pointBitMap.UnlockBits();
            newPointBitMap.UnlockBits();
            return newBitmap;
        }

        //RGB色彩空间转IHS色彩空间
        private IHS[] IHSImage(System.Drawing.Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            IHS[] imageIHS = new IHS[height * width];

            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(width, height);

            pointBitmap pointBitMap = new pointBitmap(bitmap);
            pointBitmap newPointBitMap = new pointBitmap(newBitmap);

            pointBitMap.LockBits();
            newPointBitMap.LockBits();

            System.Drawing.Color pixel;

            double theta;
            double round = 180 / Math.PI;

            for (int i = 1; i < width; i++)
            {
                for (int j = 1; j < height; j++)
                {
                    pixel = pointBitMap.GetPixel(i, j);

                    double RGB_R = (pixel.R / 255.0);
                    double RGB_G = (pixel.G / 255.0);
                    double RGB_B = (pixel.B / 255.0);

                    double Max = Math.Max(RGB_R, Math.Max(RGB_G, RGB_B));
                    double Min = Math.Min(RGB_R, Math.Min(RGB_G, RGB_B));

                    //////////////////////////////////////////////////////////

                    imageIHS[i * height + j].I = (RGB_R + RGB_G + RGB_B) / 3;

                    if (RGB_R == RGB_G && RGB_R == RGB_B)
                    {
                        theta = 0;
                    }
                    else
                    {
                        theta = Math.Acos((((RGB_R - RGB_G) + (RGB_R - RGB_B)) / 2) / Math.Sqrt((RGB_R - RGB_G) * (RGB_R - RGB_G) + (RGB_R - RGB_B) * (RGB_G - RGB_B))) * round;
                    }

                    if (RGB_B <= RGB_G)
                    {
                        imageIHS[i * height + j].H = theta / 360;
                    }
                    else
                    {
                        imageIHS[i * height + j].H = 1 - theta / 360;
                    }

                    imageIHS[i * height + j].S = 1 - 3 * Min / (RGB_R + RGB_G + RGB_B);

                    //////////////////////////////////////////////////////////

                    //imageIHS[10 * i + j].H = 0.0;
                    //if (Max == RGB_R && RGB_G >= RGB_B)
                    //{
                    //    if (Max - Min == 0) imageIHS[10 * i + j].H = 0.0;
                    //    else imageIHS[10 * i + j].H = 60 * (RGB_G - RGB_B) / (Max - Min);
                    //}
                    //else if (Max == RGB_R && RGB_G < RGB_B)
                    //{
                    //    imageIHS[10 * i + j].H = 60 * (RGB_G - RGB_B) / (Max - Min) + 360;
                    //}
                    //else if (Max == RGB_G)
                    //{
                    //    imageIHS[10 * i + j].H = 60 * (RGB_B - RGB_R) / (Max - Min) + 120;
                    //}
                    //else if (Max == RGB_B)
                    //{
                    //    imageIHS[10 * i + j].H = 60 * (RGB_R - RGB_G) / (Max - Min) + 240;
                    //}

                    //imageIHS[10 * i + j].S = (Max == 0) ? 0.0 : (1.0 - ((double)Min / (double)Max));
                    //imageIHS[10 * i + j].I = Max;

                    //////////////////////////////////////////////////////////
                }
            }

            pointBitMap.UnlockBits();
            newPointBitMap.UnlockBits();

            return imageIHS;
        }

        //IHS色彩空间转RGB色彩空间
        private System.Drawing.Bitmap RGBImage(System.Drawing.Bitmap bitmap, IHS[] imageIHS)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            IHS[] newIHS = imageIHS;

            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(width, height);

            pointBitmap pointBitMap = new pointBitmap(bitmap);
            pointBitmap newPointBitMap = new pointBitmap(newBitmap);

            pointBitMap.LockBits();
            newPointBitMap.LockBits();

            System.Drawing.Color pixel;

            double round = 180 / Math.PI;

            for (int i = 1; i < width; i++)
            {
                for (int j = 1; j < height; j++)
                {
                    pixel = pointBitMap.GetPixel(i, j);

                    double RGB_R = 0;
                    double RGB_G = 0;
                    double RGB_B = 0;

                    //////////////////////////////////////////////////////////////

                    double I = newIHS[i * height + j].I;
                    double H = newIHS[i * height + j].H * 360;
                    double S = newIHS[i * height + j].S;



                    if (H <= 120 && H >= 0)
                    {
                        RGB_R = I * (1 + S * Math.Cos(H / round) / Math.Cos((60 - H) / round));
                        RGB_B = I * (1 - S);
                        RGB_G = 3 * I - RGB_R - RGB_B;
                    }
                    else if (H <= 240 && H >= 120)
                    {
                        RGB_G = I * (1 + S * Math.Cos((H - 120) / round) / Math.Cos((180 - H) / round));
                        RGB_R = I * (1 - S);
                        RGB_B = 3 * I - RGB_G - RGB_R;
                    }
                    else
                    {
                        RGB_B = I * (1 + S * Math.Cos((H - 240) / round) / Math.Cos((300 - H) / round));
                        RGB_G = I * (1 - S);
                        RGB_R = 3 * I - RGB_B - RGB_G;
                    }

                    //if (RGB_R > 1)
                    //{
                    //    RGB_R = 1;
                    //}
                    //if (RGB_R < 0)
                    //{
                    //    RGB_R = 0;
                    //}

                    //if (RGB_G > 1)
                    //{
                    //    RGB_G = 1;
                    //}
                    //if (RGB_G < 0)
                    //{
                    //    RGB_G = 0;
                    //}
                    //if (RGB_B > 1)
                    //{
                    //    RGB_B = 1;
                    //}
                    //if (RGB_B < 0)
                    //{
                    //    RGB_B = 0;
                    //}

                    //////////////////////////////////////////////////////////////

                    //if (imageIHS[i * height + j].S == 0)
                    //{
                    //    RGB_R = RGB_G = RGB_B = imageIHS[i * height + j].I;
                    //}
                    //else
                    //{
                    //    double sectorPos = imageIHS[i * height + j].H / 60.0;
                    //    int sectorNumber = (int)(Math.Floor(sectorPos));

                    //    double fractionalSector = sectorPos - sectorNumber;

                    //    double p = imageIHS[i * height + j].I * (1.0 - imageIHS[i * height + j].S);
                    //    double q = imageIHS[i * height + j].I * (1.0 - (imageIHS[i * height + j].S * fractionalSector));
                    //    double t = imageIHS[i * height + j].I * (1.0 - (imageIHS[i * height + j].S * (1 - fractionalSector)));

                    //    switch (sectorNumber)
                    //    {
                    //        case 0:
                    //            RGB_R = imageIHS[i * height + j].I;
                    //            RGB_G = t;
                    //            RGB_B = p;
                    //            break;
                    //        case 1:
                    //            RGB_R = q;
                    //            RGB_G = imageIHS[i * height + j].I;
                    //            RGB_B = p;
                    //            break;
                    //        case 2:
                    //            RGB_R = p;
                    //            RGB_G = imageIHS[i * height + j].I;
                    //            RGB_B = t;
                    //            break;
                    //        case 3:
                    //            RGB_R = p;
                    //            RGB_G = q;
                    //            RGB_B = imageIHS[i * height + j].I;
                    //            break;
                    //        case 4:
                    //            RGB_R = t;
                    //            RGB_G = p;
                    //            RGB_B = imageIHS[i * height + j].I;
                    //            break;
                    //        case 5:
                    //            RGB_R = imageIHS[i * height + j].I;
                    //            RGB_G = p;
                    //            RGB_B = q;
                    //            break;
                    //    }
                    //}

                    //////////////////////////////////////////////////////////////

                    if (double.IsNaN(RGB_R)) RGB_R = 0;
                    if (double.IsNaN(RGB_G)) RGB_G = 0;
                    if (double.IsNaN(RGB_B)) RGB_B = 0;

                    RGB_R *= 255;
                    RGB_G *= 255;
                    RGB_B *= 255;

                    int R = Convert.ToInt16(RGB_R);
                    int G = Convert.ToInt16(RGB_G);
                    int B = Convert.ToInt16(RGB_B);


                    R = R > 255 ? 255 : R;
                    R = R < 0 ? 0 : R;

                    G = G > 255 ? 255 : G;
                    G = G < 0 ? 0 : G;

                    B = B > 255 ? 255 : B;
                    B = B < 0 ? 0 : B;

                    newPointBitMap.SetPixel(i, j, System.Drawing.Color.FromArgb(R, G, B));
                }

            }
            pointBitMap.UnlockBits();
            newPointBitMap.UnlockBits();

            return newBitmap;
        }

        ////////////////////////////////////////////////////////////////////////
    }
}
