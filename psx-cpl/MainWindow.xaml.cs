using System;
using System.Collections.Generic;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using DNS.Server;
using System.Linq;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Configuration;

namespace psx_cpl
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public static MainWindow instance;
        public static MainWindow Instance
        {
            get { return instance; }
        }


        public static string ErrorTag = "[ERROR]";
        public static string InfoTag = "[INFO]";
        public static string NL = Environment.NewLine;


        private string[] localIPs;
        public string[] LocalIPs
        {
            get { return localIPs; }
            set { localIPs = value; OnPropertyChanged("LocalIPs"); }
        }

        private string _selectedLocalIP;

        public string SelectedLocalIP
        {
            get { return _selectedLocalIP; }
            set
            {
                _selectedLocalIP = value;
                OnPropertyChanged("SelectedLocalIP");
            }
        }
        public string NewLocalIP
        {
            set
            {
                if (SelectedLocalIP != null)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(value))
                {
                    List<string> tempList = new List<string>();
                    tempList.Add(value);
                    if(LocalIPs != null && LocalIPs.Length > 0) tempList.AddRange(LocalIPs);
                    LocalIPs = tempList.ToArray();
                    SelectedLocalIP = value;
                }
            }
        }



        public static FileInfo payload;
        public static int PayloadLimitByte = 10000000;

        public static client client = new client();
        public static client clientPayload = new client();

        public static HttpFileServer webServer;
        public static HttpFileServer elfLoaderWebServer;
        public static int cssScaling = 3;
        public static int CssScaling
        {
            get { return cssScaling; }
            set { cssScaling = value; }
        }

        public static string AppDir
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static string www = Path.Combine(AppDir, "www");
        public static string wwwRootPath
        {
            get
            {
                if (Instance.ComboBoxFirmwareVersion != null && !string.IsNullOrEmpty(Instance.ComboBoxFirmwareVersion.Text))
                {
                    string retPath = Path.Combine(www, Instance.ComboBoxFirmwareVersion.Text);
                    if (Directory.Exists(retPath)) return retPath;
                    return www;
                }
                else
                {
                    return www;
                }
            }
        }

        public static string wwwElfLoaderRootPath
        {
            get
            {
                if (Instance.ComboBoxFirmwareVersion != null && !string.IsNullOrEmpty(Instance.ComboBoxFirmwareVersion.Text))
                {
                    string retPath = Path.Combine(www, Instance.ComboBoxFirmwareVersion.Text, "elfloader");
                    if (Directory.Exists(retPath)) return retPath;
                    return www;
                }
                else
                {
                    return www;
                }
            }
        }





        /// <summary>
        /// Configs
        /// </summary>
        private static string configFile;
        public static string ConfigFile
        {
            get { return configFile; }
            set { configFile = value; }
        }

        private Settings appSettings;
        public Settings AppSettings
        {
            get { return appSettings; }
            set { appSettings = value; OnPropertyChanged("AppSettings"); }
        }

        public static string FirmwareVersionsFilePath
        {
            get { return Path.Combine(AppDir, "config", "firmwareversions.txt"); }
        }


        public static FileInfo FirmwareVersionsFile;

        private static List<string> firmwareVersions = new List<string>();
        public List<string> FirmwareVersions
        {
            get { return firmwareVersions; }
            set { firmwareVersions = value; OnPropertyChanged("FirmwareVersions"); }
        }


        public int LastFirmware
        {
            get
            {
                if (FirmwareVersions != null && FirmwareVersions.Count > 0) return (FirmwareVersions.Count - 1);
                else return 0;
            }
        }


        public static FileInfo DomainsToRedirectFile;
        public static string[] domainsToRedirect = new string[] { "playstation.net", "psn.net", "manuals.playstation.net" };
        public static string DomainsToRedirectFilePath
        {
            get { return Path.Combine(AppDir, "config", "dns_redir.txt"); }
        }
        public string[] DomainsToRedirect
        {
            get { return domainsToRedirect; }
            set { domainsToRedirect = value; OnPropertyChanged("DomainsToRedirect"); }
        }

        public string DomainsToRedirectAsString
        {
            get { return string.Join(Environment.NewLine, DomainsToRedirect); }
            set
            {
                var tempList = new List<string>();

                string newValue = value;

                //check for important line that should not be removed
                string importantEntry = "dontremove.default.entry";

                if (!value.Contains(importantEntry))
                {
                    newValue = importantEntry + Environment.NewLine + value;
                }

                tempList.AddRange(newValue.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                DomainsToRedirect = tempList.ToArray();
                OnPropertyChanged("DomainsToRedirectAsString");
            }
        }



        public static bool DNSstarted = false;
        public static List<DnsServer> dnsServerList;
        public static string DnsBlackListFilePath
        {
            get { return Path.Combine(AppDir,"config", "dns_blacklist.txt"); }
        }


        public static FileInfo DnsBlackListFile;
        public static List<string> DnsBlackList
        {
            get; set;
        }
        public string DnsBlackListAsString
        {
            get { return string.Join(Environment.NewLine, DnsBlackList); }
            set
            {
                if (DnsBlackList == null) DnsBlackList = new List<string>();
                else DnsBlackList.Clear();
                DnsBlackList.AddRange(value.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                OnPropertyChanged("DnsBlackListAsString");
            }
        }


        public static FileInfo ps4ipfile;
        public static string PS4IPFilePath
        {
            get { return Path.Combine(AppDir, "config", "ps4ip.txt"); }
        }



        public static List<KeyValuePair<string,string>> payloads = new List<KeyValuePair<string, string>>();
        public static List<KeyValuePair<string, string>> Payloads
        {
            get
            {
                if (Instance.ComboBoxFirmwareVersion != null && Instance.ComboBoxFirmwareVersion.Items.Count > 0)
                {
                    Console.WriteLine("Payloads: " + Instance.ComboBoxFirmwareVersion.Text);

                    //return payloads.Where(kvp => kvp.Key == Instance.ComboBoxFirmwareVersion.Text).ToDictionary(x => x.Key, x => x.Value);
                    return payloads.Where(kvp => kvp.Key == Instance.ComboBoxFirmwareVersion.Text).ToList();
                }
                else
                {
                    return payloads;
                }
            }
        }


        /// <summary>
        /// Log
        /// </summary>

        public bool WindowLogIsOpen = false;

        private Windows.ElfLoaderLog windowLog;
        public Windows.ElfLoaderLog WindowLog
        {
            get { return windowLog; }
            set { windowLog = value; OnPropertyChanged("WindowLog"); }
        }

        private ObservableCollection<string> log;
        public ObservableCollection<string> Log
        {
            get { return log; }
            set { log = value; OnPropertyChanged("Log"); }
        }
        
        public static void AddToLog(string Text)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null)
                {
                    if (MainWindow.Instance.Log == null) MainWindow.Instance.Log = new ObservableCollection<string>();
                    MainWindow.Instance.Log.Add(Text);
                }

            }));
        }

        public static void CopyLogToClipboard()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null)
                {
                    if (MainWindow.Instance.Log != null) Clipboard.SetText(string.Join(Environment.NewLine, MainWindow.Instance.Log));
                }

            }));
        }

        public static void ClearLog()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.Log != null) MainWindow.Instance.Log.Clear();

            }));
        }

        public static void OpenLogWindow()
        {
            Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if(!Instance.WindowLogIsOpen)
                {
                    Instance.WindowLog = new Windows.ElfLoaderLog();
                    //WindowLog.Text.ItemsSource = Log;
                    Instance.WindowLog.Show();
                    Instance.WindowLogIsOpen = true;
                }
                else
                {
                    Instance.WindowLog.Activate();
                }
            }));
        }


        /// <summary>
        /// Log DNS
        /// </summary>

        private ObservableCollection<string> logDNS;
        public ObservableCollection<string> LogDNS
        {
            get { return logDNS; }
            set { logDNS = value; OnPropertyChanged("LogDNS"); }
        }

        public static void AddToLogDNS(string Text)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null)
                {
                    if (MainWindow.Instance.LogDNS == null) MainWindow.Instance.LogDNS = new ObservableCollection<string>();
                    MainWindow.Instance.LogDNS.Add(Text);
                }

            }));
        }

        public static void CopyLogDNSToClipboard()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null)
                {
                    if (MainWindow.Instance.LogDNS != null) Clipboard.SetText(string.Join(Environment.NewLine, MainWindow.Instance.LogDNS));
                }

            }));
        }

        public static void ClearLogDNS()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.LogDNS != null) MainWindow.Instance.LogDNS.Clear();

            }));
        }



        /// <summary>
        /// Log Web
        /// </summary>

        private ObservableCollection<string> logWeb;
        public ObservableCollection<string> LogWeb
        {
            get { return logWeb; }
            set { logWeb = value; OnPropertyChanged("LogWeb"); }
        }

        public static void AddToLogWeb(string Text)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null)
                {
                    if (MainWindow.Instance.LogWeb == null) MainWindow.Instance.LogWeb = new ObservableCollection<string>();
                    MainWindow.Instance.LogWeb.Add(Text);
                }

            }));
        }

        public static void CopyLogWebToClipboard()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null)
                {
                    if (MainWindow.Instance.LogWeb != null) Clipboard.SetText(string.Join(Environment.NewLine,MainWindow.Instance.LogWeb));
                }

            }));
        }

        public static void ClearLogWeb()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.LogWeb != null) MainWindow.Instance.LogWeb.Clear();
            }));
        }


        /// <summary>
        /// Log ProxyDump
        /// </summary>

        private ObservableCollection<string> logProxyDump;
        public ObservableCollection<string> LogProxyDump
        {
            get { return logProxyDump; }
            set { logProxyDump = value; OnPropertyChanged("LogProxyDump"); }
        }

        public static void AddToLogProxyDump(string Text)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null)
                {
                    if (MainWindow.Instance.LogProxyDump == null) MainWindow.Instance.LogProxyDump = new ObservableCollection<string>();
                    MainWindow.Instance.LogProxyDump.Add(Text);
                }

            }));
        }

        public static void CopyLogProxyDumpToClipboard()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null)
                {
                    if (MainWindow.Instance.LogProxyDump != null) Clipboard.SetText(string.Join(Environment.NewLine, MainWindow.Instance.LogProxyDump));
                }

            }));
        }

        public static void ClearLogProxyDump()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.LogProxyDump != null) MainWindow.Instance.LogProxyDump.Clear();
            }));
        }


        /// <summary>
        /// Info Window
        /// </summary>

        public bool WindowInfoIsOpen = false;

        private Windows.Info windowInfo;
        public Windows.Info WindowInfo
        {
            get { return windowInfo; }
            set { windowInfo = value; OnPropertyChanged("WindowInfo"); }
        }

        public static void OpenInfoWindow()
        {
            Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (!Instance.WindowInfoIsOpen)
                {
                    Instance.WindowInfo = new Windows.Info();
                    Instance.WindowInfo.Show();
                    Instance.WindowInfoIsOpen = true;
                }
                else
                {
                    Instance.WindowInfo.Activate();
                }
            }));
        }


        /// <summary>
        /// ProxyDump Window
        /// </summary>

        public bool WindowProxyDumpIsOpen = false;

        private int proxyPort = 8877;
        public int ProxyPort
        {
            get { return proxyPort; }
            set { proxyPort = value; OnPropertyChanged("ProxyPort"); }
        }
        private Windows.ProxyDumpWindow windowProxyDump;
        public Windows.ProxyDumpWindow WindowProxyDump
        {
            get { return windowProxyDump; }
            set { windowProxyDump = value; OnPropertyChanged("WindowProxyDump"); }
        }

        public static void OpenProxyDumpWindow()
        {
            Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                // if clicked OK button of warning window
                if (OpenProxyDumpInfoWindow() == 5)
                {
                    if (!Instance.WindowProxyDumpIsOpen)
                    {
                        Instance.WindowProxyDump = new Windows.ProxyDumpWindow();
                        Instance.WindowProxyDump.Show();
                        Instance.WindowProxyDumpIsOpen = true;
                    }
                    else
                    {
                        Instance.WindowProxyDump.Activate();
                    }
                }
            }));
        }

        private psx_cpl.ProxyDump.ProxyDump proxyDumpInstance;
        public psx_cpl.ProxyDump.ProxyDump ProxyDumpInstance
        {
            get { return proxyDumpInstance; }
            set { proxyDumpInstance = value; OnPropertyChanged("ProxyDumpInstance"); }
        }

        /// <summary>
        /// ProxyDump Info Window
        /// </summary>

        public bool WindowProxyDumpInfoIsOpen = false;

        private Windows.ProxyDumpInfo windowProxyDumpInfo;
        public Windows.ProxyDumpInfo WindowProxyDumpInfo
        {
            get { return windowProxyDumpInfo; }
            set { windowProxyDumpInfo = value; OnPropertyChanged("WindowProxyDumpInfo"); }
        }

        public static int OpenProxyDumpInfoWindow()
        {
            Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (!Instance.WindowProxyDumpInfoIsOpen)
                {
                    Instance.WindowProxyDumpInfo = new Windows.ProxyDumpInfo();
                    Instance.WindowProxyDumpInfo.Owner = Instance;
                    Instance.WindowProxyDumpInfo.ShowDialog();
                }
                else
                {
                    Instance.WindowProxyDumpInfo.Activate();
                }
                
            }));
            return Instance.WindowProxyDumpInfo.Value;
        }

        /// <summary>
        /// Settings Window
        /// </summary>

        public bool WindowSettingsIsOpen = false;

        private Windows.Settings windowSettings;
        public Windows.Settings WindowSettings
        {
            get { return windowSettings; }
            set { windowSettings = value; OnPropertyChanged("WindowSettings"); }
        }

        public static void OpenSettingsWindow()
        {
            Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (!Instance.WindowSettingsIsOpen)
                {
                    Instance.WindowSettings = new Windows.Settings();
                    Instance.WindowSettings.Show();
                    Instance.WindowSettingsIsOpen = true;
                }
                else
                {
                    Instance.WindowSettings.Activate();
                }
            }));
        }


        /// <summary>
        /// Enable/Disable Controls
        /// </summary>
        /// 
        public static void Disable_btn_SendPayload()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.btn_SendPayload != null)
                {
                    MainWindow.Instance.btn_SendPayload.IsEnabled = false;
                }

            }));
        }

        public static void Enable_btn_SendPayload()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.btn_SendPayload != null)
                {
                    MainWindow.Instance.btn_SendPayload.IsEnabled = true;
                }

            }));
        }

        public static void Disable_btnConnectClient()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.btnConnectClient != null)
                {
                    MainWindow.Instance.btnConnectClient.IsEnabled = false;
                }

            }));
        }

        public static void Enable_btnConnectClient()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.btnConnectClient != null)
                {
                    MainWindow.Instance.btnConnectClient.IsEnabled = true;
                }

            }));
        }

        public static void Disable_WindowLog_btnConnectClient()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.WindowLog != null && MainWindow.Instance.WindowLog.btnConnectClient != null)
                {
                    MainWindow.Instance.WindowLog.btnConnectClient.IsEnabled = false;
                }

            }));
        }

        public static void Enable_WindowLog_btnConnectClient()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                if (MainWindow.Instance != null && MainWindow.Instance.WindowLog != null && MainWindow.Instance.WindowLog.btnConnectClient != null)
                {
                    MainWindow.Instance.WindowLog.btnConnectClient.IsEnabled = true;
                }

            }));
        }



        /// <summary>
        /// MainWindow
        /// </summary>

        public MainWindow()
        {
            instance = this;

            try
            {
                LoadFirmwareVersions();
            }
            catch (Exception ex)
            {
                FirmwareVersions = new List<string>
                {
                "1.76",
                "4.05"
                };

                MessageBox.Show(ErrorTag + " loading FirmwareVersions from file failed, loaded defaults. " + ex.ToString());
            }

            DataContext = this;
            InitializeComponent();

            Initialize();


            if (AppDomain.CurrentDomain.SetupInformation.ConfigurationFile != null)
            {
                MainWindow.ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            }
            
            //Load Settings when Whindow is loaded ...

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }


        /// <summary>
        /// Stuff
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        public void mBox(string Message)
        {
            Application.Current.Dispatcher.Invoke(
           System.Windows.Threading.DispatcherPriority.Normal,
           new Action(() =>
           {
               MessageBox.Show(Message);
           }));

        }



        public static void Initialize()
        {
            Console.WriteLine(InfoTag + " Initialize Start");

            try
            {
                LoadPS4IP();
                LoadLocalIPs();
                LoadDnsBlackList();
                LoadDnsDomainsToRedirect();
                LoadPayloads();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ErrorTag + " Initialize: " + ex.ToString());
            }

        }


        public void LoadSettings()
        {
            if(AppSettings == null) AppSettings = new Settings();
            AppSettings.LoadSettings();

            if(AppSettings != null)
            {
                if (AppSettings.OpenLogAfterStart) OpenInfoWindow();
                if (AppSettings.OpenElfloaderLogAfterStart) OpenLogWindow();
                if (AppSettings.OpenProxyDumpAfterStart) OpenProxyDumpWindow();
                if (AppSettings.UseLocalIP && !String.IsNullOrEmpty(AppSettings.LocalIP)) comboBoxLocalIP.Text = AppSettings.LocalIP;
                if (AppSettings.ProxyDumpUsePort && AppSettings.ProxyDumpPort >= 0) ProxyPort = AppSettings.ProxyDumpPort;
                if (MainWindow.Instance.ProxyDumpInstance != null && AppSettings.ProxyDumpUseDefaultResponseFile) MainWindow.Instance.ProxyDumpInstance.LoadResponseFile();
                if (MainWindow.Instance.ProxyDumpInstance != null && AppSettings.ProxyDumpSplitSessions) MainWindow.Instance.ProxyDumpInstance.SplitSessionsFiles = AppSettings.ProxyDumpSplitSessions;

                if (AppSettings.PS4UseDefaultPort && AppSettings.PS4DefaultPort >= 0) txtBoxPS4Port.Text = AppSettings.PS4DefaultPort.ToString();

                if (AppSettings.PayloadUseDefaultFile && File.Exists(AppSettings.PayloadDefaultFile)) UseCustomPayload(AppSettings.PayloadDefaultFile);
                
                if (AppSettings.AutoStartDNS && AppSettings.UseLocalIP && !String.IsNullOrEmpty(AppSettings.LocalIP) && btn_DNSServer != null && btn_DNSServer.IsChecked == false) do_btn_DNSServer_Click(btn_DNSServer, true);
                if (AppSettings.AutoStartWebServer && btn_WebServer != null && btn_WebServer.IsChecked == false) do_btn_WebServer_Click(btn_WebServer, true);
                if (AppSettings.AutoStartElfloaderWebServer && btn_ElfLoaderServer != null && btn_ElfLoaderServer.IsChecked == false) do_btn_ElfLoaderServer_Click(btn_ElfLoaderServer, true);
            }
        }

        public static void LoadFirmwareVersions()
        {
            Console.WriteLine(InfoTag + " LoadFirmwareVersions Start");

            if (File.Exists(FirmwareVersionsFilePath))
            {
                FirmwareVersionsFile = new FileInfo(FirmwareVersionsFilePath);
                if (FirmwareVersionsFile != null && FirmwareVersionsFile.Length > 0)
                {
                    string[] FirmwareVersionsLines = File.ReadAllLines(FirmwareVersionsFilePath);
                    Instance.FirmwareVersions = new List<string>(FirmwareVersionsLines);
                }
            }
            else
            {
                Instance.FirmwareVersions = new List<string>
                {
                "1.76",
                "4.05"
                };

                MessageBox.Show(ErrorTag + " loading FirmwareVersions from file failed, loaded defaults. File doesn't exist: " + FirmwareVersionsFilePath);
            }
        }

        public static void LoadDnsBlackList()
        {
            Console.WriteLine(InfoTag + " LoadDnsBlackList Start");

            if (File.Exists(DnsBlackListFilePath))
            {
                DnsBlackListFile = new FileInfo(DnsBlackListFilePath);
                if(DnsBlackListFile != null && DnsBlackListFile.Length > 0)
                {
                    string[] DnsBlackListLines = File.ReadAllLines(DnsBlackListFilePath);
                    DnsBlackList = new List<string>(DnsBlackListLines);
                }
            }
            else
            {
                MessageBox.Show(ErrorTag + " loading DNS Blacklist from file failed. File doesn't exist: " + DnsBlackListFilePath);
            }
        }

        public static void LoadDnsDomainsToRedirect()
        {
            Console.WriteLine(InfoTag + " LoadDnsDomainsToRedirect Start");

            if (File.Exists(DomainsToRedirectFilePath))
            {
                DomainsToRedirectFile = new FileInfo(DomainsToRedirectFilePath);
                if (DomainsToRedirectFile != null && DomainsToRedirectFile.Length > 0)
                {
                    string[] DomainsToRedirectLines = File.ReadAllLines(DomainsToRedirectFilePath);
                    Instance.DomainsToRedirect = DomainsToRedirectLines;
                }
            }
            else
            {
                MessageBox.Show(ErrorTag + " loading DNS Domains to Redirect from file failed. File doesn't exist: " + DomainsToRedirectFilePath);
            }
        }



        public static void LoadPS4IP()
        {
            Console.WriteLine(InfoTag + " LoadPS4IP Start");

            if (File.Exists(PS4IPFilePath))
            {
                ps4ipfile = new FileInfo(PS4IPFilePath);
                if (ps4ipfile != null && ps4ipfile.Length > 0)
                {
                    string[] ps4ipfileLines = File.ReadAllLines(PS4IPFilePath);
                    string ipstring = String.Join("", ps4ipfileLines);
                    IPAddress ip;

                    //fixme: if needed add more checks
                    if (IPAddress.TryParse(ipstring, out ip))
                    {
                        Instance.txtBoxPS4IP.Text = ipstring;
                    }
                }
            }
        }


        public static void LoadLocalIPs()
        {
            Console.WriteLine(InfoTag + " LoadLoacalIPs Start");
            try
            {
                //Instance.LocalIPs = network.GetAllLocalIPv4(true, System.Net.NetworkInformation.NetworkInterfaceType.Ethernet); // filter only Ethernet
                Instance.LocalIPs = network.GetAllLocalIPv4(); // return ALL IPs that are online
                if(Instance.LocalIPs != null && Instance.LocalIPs.Length > 0)
                {
                    if(Instance.txtBoxPS4IP != null && !String.IsNullOrEmpty(Instance.txtBoxPS4IP.Text) && Instance.txtBoxPS4IP.Text.Count(x => x == '.') == 3)
                    {
                        string[] ipArray = Instance.txtBoxPS4IP.Text.Split('.');
                        if (ipArray != null && ipArray.Length > 3)
                        {
                            string TryToGuessNetwork = ipArray[0] + "." + ipArray[1] + "." + ipArray[2] + ".";
                            foreach (string ip in Instance.LocalIPs)
                            {
                                if (ip.StartsWith(TryToGuessNetwork)) Instance.SelectedLocalIP = ip;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ErrorTag + " LoadLoacalIPs: " + ex.ToString());
            }
        }



        public static void LoadPayloads()
        {
            if (Instance.FirmwareVersions != null && Instance.FirmwareVersions.Count > 0)
            {
                foreach (string fw in Instance.FirmwareVersions)
                {
                    //string payloadpath = Path.Combine(AppDir, "payloads", fw);
                    string payloadpath = Path.Combine("payloads", fw);
                    if (Directory.Exists(payloadpath))
                    {
                        Console.WriteLine(InfoTag + " LoadPayloads - payload found: " + payloadpath);

                        var payloadfiles = Directory.GetFiles(payloadpath);
                        if(payloadfiles != null && payloadfiles.Length > 0)
                        {
                            foreach (var file in payloadfiles)
                            {
                                payloads.Add(new KeyValuePair<string, string>(fw, file));
                            }
                        }
                    }
                }

                UpdatePayloads();
                UpdateSelectedPayload();
            }
        }


        public static void UpdatePayloads()
        {
            if (payloads != null && payloads.Count > 0 && Instance.ComboBoxPayLoad != null)
            {
                Console.WriteLine(InfoTag + " UpdatePayloads");

                Instance.ComboBoxPayLoad.ItemsSource = Payloads;
                Instance.ComboBoxPayLoad.DisplayMemberPath = "Value";
                Instance.ComboBoxPayLoad.SelectedIndex = 0;
            }
        }

        public static void UpdateSelectedPayload()
        {
            if (Instance.ComboBoxPayLoad != null && Instance.ComboBoxPayLoad.Items != null && Instance.ComboBoxPayLoad.SelectedItem != null && Instance.ComboBoxPayLoad.Items.Count > 0)
            {
                KeyValuePair<string, string> cbitem = (KeyValuePair<string, string>)Instance.ComboBoxPayLoad.SelectedItem;
                string payloadfile = cbitem.Value;

                if (payloadfile != null && File.Exists(payloadfile))
                {
                    payload = new FileInfo(payloadfile);
                    Console.WriteLine(InfoTag + " ComboBoxPayLoad_SelectionChanged - payloadfile: " + payloadfile);
                }

            }
        }

        public static void UseCustomPayload(string CustomPayloadFilePath)
        {
            if (Instance.ComboBoxPayLoad != null && Instance.ComboBoxPayLoad.Items != null && Instance.ComboBoxPayLoad.Items.Count > 0)
            {
                if (CustomPayloadFilePath != null && File.Exists(CustomPayloadFilePath))
                {
                    payload = new FileInfo(CustomPayloadFilePath);
                    Console.WriteLine(InfoTag + " UseCustomPayload - payloadfile: " + CustomPayloadFilePath);

                    Instance.ComboBoxPayLoad.ItemsSource = Payloads;
                    Instance.ComboBoxPayLoad.DisplayMemberPath = "Value";

                    if (payloads != null)
                    {
                        var newItem = new KeyValuePair<string, string>("0.0", CustomPayloadFilePath);
                        payloads.Add(newItem);
                        ComboBoxItem item = (ComboBoxItem)Instance.ComboBoxPayLoad.ItemContainerGenerator.ContainerFromItem(newItem);
                        Instance.ComboBoxPayLoad.SelectedItem = item;
                     }
                }

            }
        }



        /////
        /// WebServer
        /// 

        public static void StartWebServer()
        {
            if (webServer == null)
            {
                if (Instance.AppSettings != null && Instance.AppSettings.HTTPUseDefaultPort && Instance.AppSettings.HTTPDefaultPort > 0)
                {
                    if (Instance.LocalIPs != null && Instance.LocalIPs.Length > 0) webServer = new HttpFileServer(wwwRootPath, Instance.AppSettings.HTTPDefaultPort, Instance.LocalIPs, false);
                    else webServer = new HttpFileServer(wwwRootPath, Instance.AppSettings.HTTPDefaultPort, network.GetAllLocalIPv4(), false);
                }
                else
                {
                    if (Instance.LocalIPs != null && Instance.LocalIPs.Length > 0) webServer = new HttpFileServer(wwwRootPath, 80, Instance.LocalIPs, false);
                    else webServer = new HttpFileServer(wwwRootPath, 80, network.GetAllLocalIPv4(), false);
                }
            }
        }

        public static void StartElfLoaderWebServer()
        {
            if (elfLoaderWebServer == null)
            {
                if (Instance.AppSettings != null && Instance.AppSettings.HTTPElfloaderUseDefaultPort && Instance.AppSettings.HTTPElfloaderDefaultPort > 0)
                {
                    if (Instance.LocalIPs != null && Instance.LocalIPs.Length > 0) elfLoaderWebServer = new HttpFileServer(wwwElfLoaderRootPath, Instance.AppSettings.HTTPElfloaderDefaultPort, Instance.LocalIPs, true);
                    else elfLoaderWebServer = new HttpFileServer(wwwElfLoaderRootPath, Instance.AppSettings.HTTPElfloaderDefaultPort, network.GetAllLocalIPv4(), true);
                }
                else
                {
                    if (Instance.LocalIPs != null && Instance.LocalIPs.Length > 0) elfLoaderWebServer = new HttpFileServer(wwwElfLoaderRootPath, 5350, Instance.LocalIPs, true);
                    else elfLoaderWebServer = new HttpFileServer(wwwElfLoaderRootPath, 5350, network.GetAllLocalIPv4(), true);
                }
            }
        }

        public static void StopWebServer()
        {
            if (webServer != null)
            {
                webServer.Dispose();
                webServer = null;
            }
        }

        public static void StopElfLoaderWebServer()
        {
            if (elfLoaderWebServer != null)
            {
                elfLoaderWebServer.Dispose();
                elfLoaderWebServer = null;
            }
        }

        public static void UpdateWebServerRoot()
        {
            if (webServer != null)
            {
                Console.WriteLine(InfoTag + " UpdateWebServerRoot - NewRootPath: " + wwwRootPath);

                webServer.RootPath = wwwRootPath;
            }
            else
            {
                Console.WriteLine(ErrorTag + " UpdateWebServerRoot - webServer was null!" );
            }
        }

        ///
        /// DNS Server
        ///

        public static async void StartDNS()
        {
            if (!DNSstarted)
            {
                if (dnsServerList == null) dnsServerList = new List<DnsServer>();

                try
                {
                    DNSstarted = true;
                    if (Instance.comboBoxLocalIP != null && !string.IsNullOrEmpty(Instance.comboBoxLocalIP.Text))
                    {
                        //string[] ip = new string[] { Instance.comboBoxLocalIP.Text };
                        string ip = Instance.comboBoxLocalIP.Text;
                        Console.WriteLine(InfoTag + " StartDNS: Trying to start redirecting requests to selected IP: " + ip);
                        await dns.DNSAsync(Instance.DomainsToRedirect, ip, DnsBlackList, Instance.AppSettings.DNSForwardServer);
                    }
                    else
                    {
                        MessageBox.Show(ErrorTag + " StartDNS - couldn't get your local IP from the dropdown.");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ErrorTag + " StartDNS: " + ex.ToString());
                }
            }
        }

        public static async void StopDNS()
        {
            if (dnsServerList != null && dnsServerList.Count > 0)
            {
                try
                {
                    foreach (var d in dnsServerList)
                    {
                        await Task.Run(() => d.Stop());
                    }

                    DNSstarted = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ErrorTag + " StopDNS: " + ex.ToString());
                }
            }
        }

            
        public static void OpenPayload()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                if(!string.IsNullOrEmpty(selectedFile) && File.Exists(selectedFile))
                {
                    payload = new FileInfo(selectedFile);

                    if (payload != null && payload.Length <= PayloadLimitByte)
                    {
                        Instance.ComboBoxPayLoad.Text = "";
                        Instance.btn_SendPayload.IsEnabled = true;
                    }
                }
            }
        }




        private void btn_OpenPayload_Click(object sender, RoutedEventArgs e)
        {
            OpenPayload();
        }

        private void btn_SendPayload_Click(object sender, RoutedEventArgs e)
        {
            if(payload != null && File.Exists(payload.FullName))
            {
                string ip = Instance.txtBoxPS4IP.Text;
                string port = Instance.txtBoxPS4Port.Text;

                int EndpointPort = 0;

                Int32.TryParse(port, out EndpointPort);

                
                if (clientPayload != null && !clientPayload.isConnected && EndpointPort > 0)
                {
                    AddToLog("Trying to send payload to PS4 (" + ip + ":" + EndpointPort + ")");
                    MainWindow.Instance.btn_SendPayload.IsEnabled = false;
                    clientPayload.StartSend(ip, EndpointPort, payload.FullName);
                }
                else
                {
                    AddToLog(ErrorTag + " btn_SendPayload_Click - clientPayload is null or clientPayload is already connected");
                }

                OpenLogWindow();
            }
        }

        private void btn_StartWebServer_Click(object sender, RoutedEventArgs e)
        {
            StartWebServer();
        }

        private void btn_StartDNSServer_Click(object sender, RoutedEventArgs e)
        {
            StartDNS();
        }

        private void btn_StopWebServer_Click(object sender, RoutedEventArgs e)
        {
            StopWebServer();
        }

        private void ComboBoxFirmwareVersion_DropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine(InfoTag + " ComboBoxFirmwareVersion_SelectionChanged");
            if(AppSettings != null && AppSettings.GeneralSwitchPS4PortWithFirmwareVersion && !AppSettings.PS4UseDefaultPort && txtBoxPS4Port != null && ComboBoxFirmwareVersion != null && !String.IsNullOrEmpty(ComboBoxFirmwareVersion.Text))
            {
                if (ComboBoxFirmwareVersion.Text == "1.76") txtBoxPS4Port.Text = "5054"; // if selected 1.76 as Firmwareversion set port to Elfloader port as default
                if (ComboBoxFirmwareVersion.Text == "4.05") txtBoxPS4Port.Text = "9020"; // if selected 4.05 as Firmwareversion set port to IDC code exec port as default
            }
            UpdateWebServerRoot();
            UpdatePayloads();
            UpdateSelectedPayload();
        }

        private void ComboBoxPayLoad_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                UpdateSelectedPayload();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void btn_StartElfLoaderWebServer_Click(object sender, RoutedEventArgs e)
        {
            StartElfLoaderWebServer();
        }

        private void btn_StopElfLoaderWebServer_Click(object sender, RoutedEventArgs e)
        {
            StopElfLoaderWebServer();
        }

        private void txtBoxCssScaling_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Instance.txtBoxCssScaling != null && !String.IsNullOrEmpty(Instance.txtBoxCssScaling.Text))
            {
                int num;
                bool isNumber = Int32.TryParse(Instance.txtBoxCssScaling.Text, out num);

                if (isNumber)
                {
                    Console.WriteLine(InfoTag + " Set Scaling to {0}", num);
                    CssScaling = num;
                }
            }
        }

        private void btn_OpenLogWindow_Click(object sender, RoutedEventArgs e)
        {
            OpenLogWindow();
        }


        private void btnConnectClient_Click(object sender, RoutedEventArgs e)
        {
            string ip = Instance.txtBoxPS4IP.Text;
            int EndpointPort = 5088; // Log Port

            if (client != null && !client.isConnected)
            {
                AddToLog("Trying to connect to PS4 (" + ip + ":" + EndpointPort + ")");
                if (MainWindow.Instance.WindowLog != null && MainWindow.Instance.WindowLog.btnConnectClient != null) MainWindow.Instance.WindowLog.btnConnectClient.IsEnabled = false;
                if (btnConnectClient != null) btnConnectClient.IsEnabled = false;

                client.StartRead(ip, EndpointPort);
                
            }
            else
            {
                AddToLog(ErrorTag + " btnConnectClient_Click - client is null or clinet is already connected");
            }
            OpenLogWindow();
        }

        public void do_btn_DNSServer_Click(System.Windows.Controls.Primitives.ToggleButton tButton, bool checkButton = false)
        {
            if (checkButton) tButton.IsChecked = true;

            if (tButton.IsChecked ?? false)
            {
                if (Instance.comboBoxLocalIP != null && !string.IsNullOrEmpty(Instance.comboBoxLocalIP.Text))
                {
                    tButton.BorderThickness = new Thickness(4, 4, 4, 4);
                    tButton.Padding = new Thickness(4, 4, 4, 4);
                    btnDNSServerLabel2.Content = "Stop";
                    StartDNS();
                    Instance.comboBoxLocalIP.IsEnabled = false;
                }
                else if (string.IsNullOrEmpty(Instance.comboBoxLocalIP.Text))
                {
                    tButton.IsChecked = false;
                    MessageBox.Show(ErrorTag + " Couldn't get your local IP from the dropdown.");
                }
            }
            else
            {
                tButton.BorderThickness = new Thickness(1, 1, 1, 1);
                tButton.Padding = new Thickness(0, 0, 0, 0);
                btnDNSServerLabel2.Content = "Start";

                StopDNS();
                if (Instance.comboBoxLocalIP != null) Instance.comboBoxLocalIP.IsEnabled = true;
            }
        }

        private void btn_DNSServer_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton tButton = sender as System.Windows.Controls.Primitives.ToggleButton;
            do_btn_DNSServer_Click(tButton);

        }

        public void do_btn_WebServer_Click(System.Windows.Controls.Primitives.ToggleButton tButton, bool checkButton = false)
        {
            if (checkButton) tButton.IsChecked = true;

            if (tButton.IsChecked ?? false)
            {
                tButton.BorderThickness = new Thickness(4, 4, 4, 4);
                tButton.Padding = new Thickness(4, 4, 4, 4);
                btnWebServerLabel2.Content = "Stop";

                StartWebServer();
            }
            else
            {
                tButton.BorderThickness = new Thickness(1, 1, 1, 1);
                tButton.Padding = new Thickness(0, 0, 0, 0);
                btnWebServerLabel2.Content = "Start";

                StopWebServer();
            }
        }
        private void btn_WebServer_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton tButton = sender as System.Windows.Controls.Primitives.ToggleButton;
            do_btn_WebServer_Click(tButton);
        }

        public void do_btn_ElfLoaderServer_Click(System.Windows.Controls.Primitives.ToggleButton tButton, bool checkButton = false)
        {
            if (checkButton) tButton.IsChecked = true;

            if (tButton.IsChecked ?? false)
            {
                tButton.BorderThickness = new Thickness(4, 4, 4, 4);
                tButton.Padding = new Thickness(4, 4, 4, 4);
                btnElfLoaderServerLabel2.Content = "Stop";

                StartElfLoaderWebServer();
            }
            else
            {
                tButton.BorderThickness = new Thickness(1, 1, 1, 1);
                tButton.Padding = new Thickness(0, 0, 0, 0);
                btnElfLoaderServerLabel2.Content = "Start";

                StopElfLoaderWebServer();
            }
        }
        private void btn_ElfLoaderServer_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton tButton = sender as System.Windows.Controls.Primitives.ToggleButton;
            do_btn_ElfLoaderServer_Click(tButton);
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            OpenInfoWindow();
        }

        private void btn_OpenProxyDump_Click(object sender, RoutedEventArgs e)
        {
            OpenProxyDumpWindow();
        }

        private void btn_OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenSettingsWindow();
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
