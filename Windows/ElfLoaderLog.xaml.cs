using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace psx_cpl.Windows
{
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
