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
    /// Interaction logic for BandWindow.xaml
    /// </summary>
    public partial class BandWindow : Window
    {
        public delegate void BandChoiceEventHandle(int band1, int band2, int band3);
        public event BandChoiceEventHandle BandChoice;

        private List<ComboboxItem> ComboboxResource;
        private string[] metaData;
        public int[] bandIndex;

        public BandWindow(string[] metaData)
        {
            InitializeComponent();
            this.metaData = metaData;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ComboboxResource = new List<ComboboxItem>();

            for (int i = 0; i < this.metaData.Length - 1; i++)
            {
                ComboboxItem comboboxItem = new ComboboxItem();
                comboboxItem.Content = this.metaData[i].ToString();
                comboboxItem.Index = i;

                //MessageBox.Show(comboboxItem.Content);

                ComboboxResource.Add(comboboxItem);
            }

            this.cmb_bandR.ItemsSource = ComboboxResource;
            this.cmb_bandR.SelectedValuePath = "Index";
            this.cmb_bandR.DisplayMemberPath = "Content";

            this.cmb_bandG.ItemsSource = ComboboxResource;
            this.cmb_bandG.SelectedValuePath = "Index";
            this.cmb_bandG.DisplayMemberPath = "Content";

            this.cmb_bandB.ItemsSource = ComboboxResource;
            this.cmb_bandB.SelectedValuePath = "Index";
            this.cmb_bandB.DisplayMemberPath = "Content";

            if (ComboboxResource.Count != 0)
            {
                this.cmb_bandR.SelectedIndex = 0;
                this.cmb_bandG.SelectedIndex = 0;
                this.cmb_bandB.SelectedIndex = 0;
            }
        }

        private void Title_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            bandIndex = new int[3];

            bandIndex[0] = GetBandIndex(this.cmb_bandR.Text.ToString());
            bandIndex[1] = GetBandIndex(this.cmb_bandG.Text.ToString());
            bandIndex[2] = GetBandIndex(this.cmb_bandB.Text.ToString());

            if (BandChoice != null) BandChoice.Invoke(bandIndex[2] + 1, bandIndex[1] + 1, bandIndex[0] + 1);
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private int GetBandIndex(string bandName)
        {
            for (int i = 0; i < this.metaData.Length; i++)
            {
                if (metaData[i] == bandName)
                {
                    return i;
                }
            }
            return 0;
        }
    }

    public class ComboboxItem
    {
        private string content;
        public string Content
        {
            get { return content; }
            set { content = value; }
        }

        private int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
    }
}
