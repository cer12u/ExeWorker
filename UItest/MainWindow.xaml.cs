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

namespace UItest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Data d = new Data();

        /// <summary>
        /// ListBoxの動作テスト
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            d.lf = new List<Data.Files>();
            lbox.ItemsSource = d.lf;
            lbox.DisplayMemberPath = "id";
            foreach (int i in Enumerable.Range(0, 10))
            {
                Data.Files f = new Data.Files() { id = i, dir = i.ToString()};

                d.lf.Add(f);

                lbox.Items.Refresh();
                await Task.Delay(500);

            }

        }

        /// <summary>
        /// キャストのテストデータ
        /// </summary>
        public class Data
        {
            public List<Files> lf = new List<Files>();

            public class Files
            {
                public int id { get; set; } = 0;
                public string dir { get; set; } = string.Empty;
                public string fileName { get; set; } = string.Empty;
                public string status { get; set; } = "FALSE";
                public selector sel { get; set; } = new selector();
            }
        }

        /// <summary>
        /// DataGridテスト用
        /// </summary>
        public enum selector
        {
            a,
            b,
            c
            
        }

        
        /// <summary>
        /// DataGrid側のボタン操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            d.lf = new List<Data.Files>();
            dGrid.CanUserAddRows = false;

            dGrid.ItemsSource = d.lf;
            dGrid.DisplayMemberPath = "id,dir";


            foreach (int i in Enumerable.Range(0, 10))
            {
                Data.Files f = new Data.Files() { id = i, dir = i.ToString() };

                d.lf.Add(f);

                dGrid.Items.Refresh();
                await Task.Delay(500);

            }



        }


        /// <summary>
        /// バインディング時の設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "id":
                    e.Column.Header = "ID";
                    break;
                case "fileName":
                    e.Cancel = true;
                    break;
                case "dir":
                    e.Column.DisplayIndex = 0;
                    break;
                default:
                    break;
            }


        }
    }
}
