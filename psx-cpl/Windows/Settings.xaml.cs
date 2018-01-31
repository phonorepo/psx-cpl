using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace psx_cpl.Windows
{
    public partial class Settings : Window, INotifyPropertyChanged
    {
        public Settings()
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

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.Instance.WindowInfo = null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.AppSettings.Save();
            MainWindow.Instance.LoadSettings();
        }

        private void Defaults_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.AppSettings.LoadDefaultSettings();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.LoadSettings();
        }
    }
}
