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

namespace Browser
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Net.HttpWebRequest wr =(System.Net.HttpWebRequest) System.Net.WebRequest.Create("http://yahoo.co.jp");
            wr.Method = "GET";



            System.Net.WebResponse res = wr.GetResponse();
            System.IO.Stream rs = res.GetResponseStream();

            wb.NavigateToStream(rs);

            //wb.Source = rs;
            //wb.Navigate("https://www.msn.com/ja-jp");


        }
    }
}
