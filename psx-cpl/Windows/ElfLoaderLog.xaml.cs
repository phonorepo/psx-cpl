using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace psx_cpl.Windows
{
    public class EnumarableToTextConverter : IValueConverter
    {
        public object Convert(
          object value, Type targetType,
          object parameter, CultureInfo culture)
        {
            Console.WriteLine("EnumarableToTextConverter value: " + value);

            if (value is IEnumerable)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var s in value as IEnumerable)
                {
                    sb.AppendLine(s.ToString());
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(
          object value, Type targetType,
          object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public partial class ElfLoaderLog : Window, INotifyPropertyChanged
    {
        public ElfLoaderLog()
        {
            InitializeComponent();
            DataContext = MainWindow.Instance;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private int _value = 0;
        private string _text = "?";

        public int Value
        {
            get { return _value; }
        }
        
        private void ListViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        public enum DialogValues
        {
            No = 0,
            Yes = 1,
            NoToAll = 2,
            YesToAll = 3,
            Cancel = 4,
            OK = 5
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((Button)sender).Tag;
            _value = Int32.Parse(tag.ToString());
            this.Close();
        }

        private void btnConnectClient_Click(object sender, RoutedEventArgs e)
        {
            string ip = MainWindow.Instance.txtBoxPS4IP.Text;
            int EndpointPort = 5088; // Log Port

            if (MainWindow.client != null && !MainWindow.client.isConnected)
            {
                MainWindow.AddToLog("Trying to connect to PS4 (" + ip + ":" + EndpointPort + ")");
                if (MainWindow.Instance.btnConnectClient != null) MainWindow.Instance.btnConnectClient.IsEnabled = false;
                if (btnConnectClient != null) btnConnectClient.IsEnabled = false;
                MainWindow.client.StartRead(ip, EndpointPort);
            }
            else
            {
                MainWindow.AddToLog(MainWindow.ErrorTag + " btnConnectClient_Click - client is null or clinet is already connected");
            }
            MainWindow.OpenLogWindow();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.Instance.WindowLog = null;
        }
    }
}
