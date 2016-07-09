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
using System.Windows.Shapes;

using System.Windows.Forms;

namespace RemoteSensingImageProcess
{
    /// <summary>
    /// Interaction logic for GrayWindow.xaml
    /// </summary>
    public partial class GrayWindow : Window
    {
        public GrayWindow()
        {
            InitializeComponent();

            trackBarMinValue.Value = MainWindow.MinGrayValues;
            trackBarMaxValue.Value = MainWindow.MaxGrayValues;

            LableMinValue.Text = Convert.ToString(MainWindow.MinGrayValues);
            LableMaxValue.Text = Convert.ToString(MainWindow.MaxGrayValues);
        }

        ////禁用当前窗体的关闭按钮
        //private const int CP_NOCLOSE_BUTTON = 0x200;
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams myCp = base.CreateParams;
        //        myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
        //        return myCp;
        //    }
        //}

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Title_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (trackBarMinValue.Value > trackBarMaxValue.Value)
            {
                double Temp = trackBarMinValue.Value;
                trackBarMinValue.Value = trackBarMaxValue.Value;
                trackBarMaxValue.Value = Temp;
            }

            MainWindow.MinNewValues = Convert.ToByte(trackBarMinValue.Value);
            MainWindow.MaxNewValues = Convert.ToByte(trackBarMaxValue.Value);
            this.Close();
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MinNewValues = MainWindow.MinGrayValues;
            MainWindow.MaxNewValues = MainWindow.MaxGrayValues;
            this.Close();
        }

        private void trackBarMinValue_Scroll(object sender, EventArgs e)
        {
            LableMinValue.Text = Convert.ToString(trackBarMinValue.Value);
        }

        private void trackBarMaxValue_Scroll(object sender, EventArgs e)
        {
            LableMaxValue.Text = Convert.ToString(trackBarMaxValue.Value);
        }
    }
}
