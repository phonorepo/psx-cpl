using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
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
            MainWindow.Instance.WindowSettingsIsOpen = false;
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

        public void SaveDNSBlackList()
        {
            try
            {
                if (MainWindow.DnsBlackList != null && MainWindow.DnsBlackList.Count > 0)
                {
                    string LinesToSave = Regex.Replace(MainWindow.Instance.DnsBlackListAsString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);

                    File.WriteAllText(MainWindow.DnsBlackListFilePath, LinesToSave);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnDNSServerSaveBlackList_Click(object sender, RoutedEventArgs e)
        {
            SaveDNSBlackList();
        }

        public void SaveDNSMasterFile()
        {
            try
            {
                if (MainWindow.Instance.DomainsToRedirect != null && MainWindow.Instance.DomainsToRedirect.Length > 0)
                {
                    string LinesToSave = Regex.Replace(MainWindow.Instance.DomainsToRedirectAsString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);

                    //check for important line that should not be removed
                    string importantEntry = "dontremove.default.entry";
                    if (!LinesToSave.Contains(importantEntry))
                    {
                        LinesToSave = importantEntry + Environment.NewLine + LinesToSave;
                    }

                    File.WriteAllText(MainWindow.DomainsToRedirectFilePath, LinesToSave);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnDNSServerSavemasterFile_Click(object sender, RoutedEventArgs e)
        {
            SaveDNSMasterFile();
        }
    }
}
