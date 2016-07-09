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

namespace RemoteSensingImageProcess
{
    /// <summary>
    /// Interaction logic for HISWindow.xaml
    /// </summary>
    public partial class HISWindow : Window
    {
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

        public HISWindow()
        {
            InitializeComponent();
        }

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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            MainWindow.setIHS_I = trackBar_I_Value.Value / 10.0;
            MainWindow.setIHS_H = trackBar_H_Value.Value / 10.0;
            MainWindow.setIHS_S = trackBar_S_Value.Value / 10.0;

            this.Hide();
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            MainWindow.setIHS_I = 1;
            MainWindow.setIHS_H = 1;
            MainWindow.setIHS_S = 1;

            this.Close();
        }
    }
}
