using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class ProxyDumpWindow : Window, INotifyPropertyChanged
    {

        public ProxyDumpWindow()
        {
            InitializeComponent();
            DataContext = MainWindow.Instance;

            try
            {
                if (MainWindow.Instance.ProxyDumpInstance == null) MainWindow.Instance.ProxyDumpInstance = new ProxyDump.ProxyDump();
            }
            catch (Exception ex)
            {
                MainWindow.Instance.mBox(MainWindow.ErrorTag + ex.ToString());
            }
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
            MainWindow.Instance.WindowProxyDump = null;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.ProxyDumpInstance != null && MainWindow.Instance.ProxyPort >= 0) MainWindow.Instance.ProxyDumpInstance.Start(MainWindow.Instance.ProxyDumpInstance.FCSF, MainWindow.Instance.ProxyPort);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.ProxyDumpInstance != null) MainWindow.Instance.ProxyDumpInstance.DoQuit();
        }

        private void btnClearProxyDumpLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.ClearLogProxyDump();
            }
            catch (Exception ex)
            {
                Console.WriteLine(MainWindow.ErrorTag + " btnClearProxyDumpLog_Click: " + ex.ToString());
            }
        }

        private void btnCopyProxyDumpLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.CopyLogProxyDumpToClipboard();
            }
            catch (Exception ex)
            {
                Console.WriteLine(MainWindow.ErrorTag + " btnCopyProxyDumpLog_Click: " + ex.ToString());
            }
        }

        private void btn_ToggleDumpMode_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton tButton = sender as System.Windows.Controls.Primitives.ToggleButton;

            if (tButton.IsChecked ?? false)
            {
                if (MainWindow.Instance.ProxyDumpInstance != null)
                {
                    tButton.BorderThickness = new Thickness(4, 4, 4, 4);
                    tButton.Padding = new Thickness(4, 4, 4, 4);
                    //btnToggleDumpModeLabel2.Content = "Stop";
                    MainWindow.Instance.ProxyDumpInstance.DumpMode = true;
                }
            }
            else
            {
                tButton.BorderThickness = new Thickness(1, 1, 1, 1);
                tButton.Padding = new Thickness(0, 0, 0, 0);
                //btnToggleDumpModeLabel2.Content = "Start";
                if (MainWindow.Instance.ProxyDumpInstance != null) MainWindow.Instance.ProxyDumpInstance.DumpMode = false;
            }
        }

        private void btnTrustRootCert_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.ProxyDumpInstance != null) MainWindow.Instance.ProxyDumpInstance.TrustRootCert();
        }

        private void btnUninstallRootCert_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.ProxyDumpInstance != null) MainWindow.Instance.ProxyDumpInstance.UninstallCertificate();
        }

        public void SaveUriFilter()
        {
            try
            {
                if (MainWindow.Instance.ProxyDumpInstance != null && MainWindow.Instance.ProxyDumpInstance.URIFilterList != null && MainWindow.Instance.ProxyDumpInstance.URIFilterList.Count > 0)
                {
                    string LinesToSave = Regex.Replace(MainWindow.Instance.ProxyDumpInstance.URIFilterListAsString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);

                    File.WriteAllText(MainWindow.Instance.ProxyDumpInstance.URIFilterFilePath, LinesToSave);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnProxyDumpSaveUriFilter_Click(object sender, RoutedEventArgs e)
        {
            SaveUriFilter();
        }

        private void tabProxyDumpUriFilter_KeyDown(object sender, KeyEventArgs e)
        {
            tabProxyDumpUriFilter.Focus();
            UpdateLayout();
            tbProxyDumpUriFilter.Focus();
        }
    }
}
