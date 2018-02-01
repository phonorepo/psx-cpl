using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;


namespace psx_cpl
{
    public class Settings: INotifyPropertyChanged
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


    // General Settings

    private string localIP;
        public string LocalIP
        {
            get { return localIP; }
            set { localIP = value; OnPropertyChanged("LocalIP"); }
        }

        private bool useLocalIP;
        public bool UseLocalIP
        {
            get { return useLocalIP; }
            set { useLocalIP = value; OnPropertyChanged("UseLocalIP"); }
        }

        private string ps4IP;
        public string PS4IP
        {
            get { return ps4IP; }
            set { ps4IP = value; OnPropertyChanged("PS4IP"); }
        }


        //httpSettings

        private string httpDefaultFile;
        public string HTTPDefaultFile
        {
            get { return httpDefaultFile; }
            set { httpDefaultFile = value; OnPropertyChanged("HTTPDefaultFile"); }
        }

        private bool httpUseDefaultFile;
        public bool HTTPUseDefaultFile
        {
            get { return httpUseDefaultFile; }
            set { httpUseDefaultFile = value; OnPropertyChanged("HTTPUseDefaultFile"); }
        }

        private int httpDefaultPort;
        public int HTTPDefaultPort
        {
            get { return httpDefaultPort; }
            set { httpDefaultPort = value; OnPropertyChanged("HTTPDefaultPort"); }
        }

        private bool httpUseDefaultPort;
        public bool HTTPUseDefaultPort
        {
            get { return httpUseDefaultPort; }
            set { httpUseDefaultPort = value; OnPropertyChanged("HTTPUseDefaultPort"); }
        }

        private int httpElfloaderDefaultPort;
        public int HTTPElfloaderDefaultPort
        {
            get { return httpElfloaderDefaultPort; }
            set { httpElfloaderDefaultPort = value; OnPropertyChanged("HTTPElfloaderDefaultPort"); }
        }

        private bool httpElfloaderUseDefaultPort;
        public bool HTTPElfloaderUseDefaultPort
        {
            get { return httpElfloaderUseDefaultPort; }
            set { httpElfloaderUseDefaultPort = value; OnPropertyChanged("HTTPElfloaderUseDefaultPort"); }
        }

        private string httpElfloaderDefaultFile;
        public string HTTPElfloaderDefaultFile
        {
            get { return httpElfloaderDefaultFile; }
            set { httpElfloaderDefaultFile = value; OnPropertyChanged("HTTPElfloaderDefaultFile"); }
        }

        private bool httpElfloaderUseDefaultFile;
        public bool HTTPElfloaderUseDefaultFile
        {
            get { return httpElfloaderUseDefaultFile; }
            set { httpElfloaderUseDefaultFile = value; OnPropertyChanged("HTTPElfloaderUseDefaultFile"); }
        }

        private int httpHTMLScaling;
        public int HTTPHTMLScaling
        {
            get { return httpHTMLScaling; }
            set { httpHTMLScaling = value; OnPropertyChanged("HTTPHTMLScaling"); }
        }



        // DNS Settings

        private bool dnsLocalOnly;
        public bool DNSLocalOnly
        {
            get { return dnsLocalOnly; }
            set { dnsLocalOnly = value; OnPropertyChanged("DNSLocalOnly"); }
        }

        private string dnsForwardServer;
        public string DNSForwardServer
        {
            get { return dnsForwardServer; }
            set { dnsForwardServer = value; OnPropertyChanged("DNSForwardServer"); }
        }


        // Payload Settings

        private int ps4DefaultPort;
        public int PS4DefaultPort
        {
            get { return ps4DefaultPort; }
            set { ps4DefaultPort = value; OnPropertyChanged("PS4DefaultPort"); }
        }

        private bool ps4UseDefaultPort;
        public bool PS4UseDefaultPort
        {
            get { return ps4UseDefaultPort; }
            set { ps4UseDefaultPort = value; OnPropertyChanged("PS4UseDefaultPort"); }
        }

        private string payloadDefaultFile;
        public string PayloadDefaultFile
        {
            get { return payloadDefaultFile; }
            set { payloadDefaultFile = value; OnPropertyChanged("PayloadDefaultFile"); }
        }

        private bool payloadUseDefaultFile;
        public bool PayloadUseDefaultFile
        {
            get { return payloadUseDefaultFile; }
            set { payloadUseDefaultFile = value; OnPropertyChanged("PayloadUseDefaultFile"); }
        }


        // ProxyDump Settings
        private string proxyDumpDefaultRepsonseFile;
        public string ProxyDumpDefaultRepsonseFile
        {
            get { return proxyDumpDefaultRepsonseFile; }
            set { proxyDumpDefaultRepsonseFile = value; OnPropertyChanged("ProxyDumpDefaultRepsonseFile"); }
        }

        private bool proxyDumpUseDefaultResponseFile;
        public bool ProxyDumpUseDefaultResponseFile
        {
            get { return proxyDumpUseDefaultResponseFile; }
            set { proxyDumpUseDefaultResponseFile = value; OnPropertyChanged("ProxyDumpUseDefaultResponseFile"); }
        }

        private int proxyDumpPort;
        public int ProxyDumpPort
        {
            get { return proxyDumpPort; }
            set { proxyDumpPort = value; OnPropertyChanged("ProxyDumpPort"); }
        }

        private bool proxyDumpUsePort;
        public bool ProxyDumpUsePort
        {
            get { return proxyDumpUsePort; }
            set { proxyDumpUsePort = value; OnPropertyChanged("ProxyDumpUsePort"); }
        }

        private bool proxyDumpSplitSessions;
        public bool ProxyDumpSplitSessions
        {
            get { return proxyDumpSplitSessions; }
            set { proxyDumpSplitSessions = value; OnPropertyChanged("ProxyDumpSplitSessions"); }
        }


        // Window Settings

        private bool openLogAfterStart;
        public bool OpenLogAfterStart
        {
            get { return openLogAfterStart; }
            set { openLogAfterStart = value; OnPropertyChanged("OpenLogAfterStart"); }
        }

        private bool openElfloaderLogAfterStart;
        public bool OpenElfloaderLogAfterStart
        {
            get { return openElfloaderLogAfterStart; }
            set { openElfloaderLogAfterStart = value; OnPropertyChanged("OpenElfloaderLogAfterStart"); }
        }

        private bool openProxyDumpAfterStart;
        public bool OpenProxyDumpAfterStart
        {
            get { return openProxyDumpAfterStart; }
            set { openProxyDumpAfterStart = value; OnPropertyChanged("OpenProxyDumpAfterStart"); }
        }

        // General Settings

        private bool generalSwitchPS4PortWithFirmwareVersion;
        public bool GeneralSwitchPS4PortWithFirmwareVersion
        {
            get { return generalSwitchPS4PortWithFirmwareVersion; }
            set { generalSwitchPS4PortWithFirmwareVersion = value; OnPropertyChanged("GeneralSwitchPS4PortWithFirmwareVersion"); }
        }


        /// <summary>
        /// Settings
        /// </summary>
        public Settings()
        {

        }



        /// <summary>
        /// SettingExist
        /// </summary>

        private static bool SettingExist(string settingName)
        {
            return Properties.Settings.Default.Properties.Cast<SettingsProperty>().Any(prop => prop.Name == settingName);
        }


        /// <summary>
        /// LoadDefaultSettings
        /// </summary>
        /// <returns></returns>
        public void LoadDefaultSettings()
        {
            try
            {
                if (SettingExist("httpDefaultFile")) HTTPDefaultFile = Properties.Settings.Default.httpDefaultFile;
                if (SettingExist("httpUseDefaultFile")) HTTPUseDefaultFile = Properties.Settings.Default.httpUseDefaultFile;

                if (SettingExist("httpDefaultPort")) HTTPDefaultPort = Properties.Settings.Default.httpDefaultPort;
                if (SettingExist("httpUseDefaultPort")) HTTPUseDefaultPort = Properties.Settings.Default.httpUseDefaultPort;

                if (SettingExist("httpElfloaderDefaultPort")) HTTPElfloaderDefaultPort = Properties.Settings.Default.httpElfloaderDefaultPort;
                if (SettingExist("httpElfloaderUseDefaultPort")) HTTPElfloaderUseDefaultPort = Properties.Settings.Default.httpElfloaderUseDefaultPort;

                if (SettingExist("httpElfloaderDefaultFile")) HTTPElfloaderDefaultFile = Properties.Settings.Default.httpElfloaderDefaultFile;
                if (SettingExist("httpElfloaderUseDefaultFile")) HTTPElfloaderUseDefaultFile = Properties.Settings.Default.httpElfloaderUseDefaultFile;

                if (SettingExist("httpHTMLScaling")) HTTPHTMLScaling = Properties.Settings.Default.httpHTMLScaling;
                if (SettingExist("ps4IP")) PS4IP = Properties.Settings.Default.ps4IP;
                if (SettingExist("dnsLocalOnly")) DNSLocalOnly = Properties.Settings.Default.dnsLocalOnly;
                if (SettingExist("dnsForwardServer")) DNSForwardServer = Properties.Settings.Default.dnsForwardServer;

                if (SettingExist("ps4DefaultPort")) PS4DefaultPort = Properties.Settings.Default.ps4DefaultPort;
                if (SettingExist("ps4UseDefaultPort")) PS4UseDefaultPort = Properties.Settings.Default.ps4UseDefaultPort;

                if (SettingExist("localIP")) LocalIP = Properties.Settings.Default.localIP;
                if (SettingExist("useLocalIP")) UseLocalIP = Properties.Settings.Default.useLocalIP;

                if (SettingExist("payloadDefaultFile")) PayloadDefaultFile = Properties.Settings.Default.payloadDefaultFile;
                if (SettingExist("payloadUseDefaultFile")) PayloadUseDefaultFile = Properties.Settings.Default.payloadUseDefaultFile;

                if (SettingExist("openLogAfterStart")) OpenLogAfterStart = Properties.Settings.Default.openLogAfterStart;
                if (SettingExist("openElfloaderLogAfterStart")) OpenElfloaderLogAfterStart = Properties.Settings.Default.openElfloaderLogAfterStart;
                if (SettingExist("openProxyDumpAfterStart")) OpenProxyDumpAfterStart = Properties.Settings.Default.openProxyDumpAfterStart;

                if (SettingExist("proxyDumpDefaultRepsonseFile")) ProxyDumpDefaultRepsonseFile = Properties.Settings.Default.proxyDumpDefaultRepsonseFile;
                if (SettingExist("proxyDumpUseDefaultResponseFile")) ProxyDumpUseDefaultResponseFile = Properties.Settings.Default.proxyDumpUseDefaultResponseFile;
                if (SettingExist("proxyDumpPort")) ProxyDumpPort = Properties.Settings.Default.proxyDumpPort;
                if (SettingExist("proxyDumpUsePort")) ProxyDumpUsePort = Properties.Settings.Default.proxyDumpUsePort;
                if (SettingExist("proxyDumpSplitSessions")) ProxyDumpSplitSessions = Properties.Settings.Default.proxyDumpSplitSessions;
                if (SettingExist("generalSwitchPS4PortWithFirmwareVersion")) GeneralSwitchPS4PortWithFirmwareVersion = Properties.Settings.Default.generalSwitchPS4PortWithFirmwareVersion;

            }
            catch (Exception ex)
            {
                Console.WriteLine(MainWindow.ErrorTag + " LoadDefaultSettings: " + ex.ToString());
            }
        }


        /// <summary>
        /// AppSettingExist
        /// </summary>

        private static bool AppSettingExist(string settingName)
        {
            return ConfigurationManager.AppSettings.AllKeys.Any(prop => prop == settingName);
        }

        public static class AppSettings
        {
            //usage
            //var debugSetting = AppSettings.Get<bool>("Debug");

            public static T Get<T>(string key)
            {
                var appSetting = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(appSetting)) throw new Exception(key);

                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)(converter.ConvertFromInvariantString(appSetting));
            }
        }


        /// <summary>
        /// LoadSettings
        /// </summary>
        /// <returns></returns>
        public void LoadSettings()
        {
            try
            {
                if (ConfigurationManager.AppSettings != null && ConfigurationManager.AppSettings.AllKeys.Length > 0)
                {
                    //var httpUseDefaultFile = Convert.ToBoolean(ConfigurationManager.AppSettings["httpUseDefaultFile"]);

                    if (AppSettingExist("httpDefaultFile")) HTTPDefaultFile = AppSettings.Get<string>("httpDefaultFile");
                    if (AppSettingExist("httpUseDefaultFile")) HTTPUseDefaultFile = AppSettings.Get<bool>("httpUseDefaultFile");

                    if (AppSettingExist("httpDefaultPort")) HTTPDefaultPort = AppSettings.Get<int>("httpDefaultPort");
                    if (AppSettingExist("httpUseDefaultPort")) HTTPUseDefaultPort = AppSettings.Get<bool>("httpUseDefaultPort");

                    if (AppSettingExist("httpElfloaderDefaultPort")) HTTPElfloaderDefaultPort = AppSettings.Get<int>("httpElfloaderDefaultPort");
                    if (AppSettingExist("httpElfloaderUseDefaultPort")) HTTPElfloaderUseDefaultPort = AppSettings.Get<bool>("httpElfloaderUseDefaultPort");

                    if (AppSettingExist("httpElfloaderDefaultFile")) HTTPElfloaderDefaultFile = AppSettings.Get<string>("httpElfloaderDefaultFile");
                    if (AppSettingExist("httpElfloaderUseDefaultFile")) HTTPElfloaderUseDefaultFile = AppSettings.Get<bool>("httpElfloaderUseDefaultFile");

                    if (AppSettingExist("httpHTMLScaling")) HTTPHTMLScaling = AppSettings.Get<int>("httpHTMLScaling");
                    if (AppSettingExist("ps4IP")) PS4IP = AppSettings.Get<string>("ps4IP");
                    if (AppSettingExist("dnsLocalOnly")) DNSLocalOnly = AppSettings.Get<bool>("dnsLocalOnly");
                    if (AppSettingExist("dnsForwardServer")) DNSForwardServer = AppSettings.Get<string>("dnsForwardServer");

                    if (AppSettingExist("ps4DefaultPort")) PS4DefaultPort = AppSettings.Get<int>("ps4DefaultPort");
                    if (AppSettingExist("ps4UseDefaultPort")) PS4UseDefaultPort = AppSettings.Get<bool>("ps4UseDefaultPort");

                    if (AppSettingExist("localIP")) LocalIP = AppSettings.Get<string>("localIP");
                    if (AppSettingExist("useLocalIP")) UseLocalIP = AppSettings.Get<bool>("useLocalIP");

                    if (AppSettingExist("payloadDefaultFile")) PayloadDefaultFile = AppSettings.Get<string>("payloadDefaultFile");
                    if (AppSettingExist("payloadUseDefaultFile")) PayloadUseDefaultFile = AppSettings.Get<bool>("payloadUseDefaultFile");

                    if (AppSettingExist("openLogAfterStart")) OpenLogAfterStart = AppSettings.Get<bool>("openLogAfterStart");
                    if (AppSettingExist("openElfloaderLogAfterStart")) OpenElfloaderLogAfterStart = AppSettings.Get<bool>("openElfloaderLogAfterStart");
                    if (AppSettingExist("openProxyDumpAfterStart")) OpenProxyDumpAfterStart = AppSettings.Get<bool>("openProxyDumpAfterStart");

                    if (AppSettingExist("proxyDumpDefaultRepsonseFile")) ProxyDumpDefaultRepsonseFile = AppSettings.Get<string>("proxyDumpDefaultRepsonseFile");
                    if (AppSettingExist("proxyDumpUseDefaultResponseFile")) ProxyDumpUseDefaultResponseFile = AppSettings.Get<bool>("proxyDumpUseDefaultResponseFile");
                    if (AppSettingExist("proxyDumpPort")) ProxyDumpPort = AppSettings.Get<int>("proxyDumpPort");
                    if (AppSettingExist("proxyDumpUsePort")) ProxyDumpUsePort = AppSettings.Get<bool>("proxyDumpUsePort");
                    if (AppSettingExist("proxyDumpSplitSessions")) ProxyDumpSplitSessions = AppSettings.Get<bool>("proxyDumpSplitSessions");
                    if (AppSettingExist("generalSwitchPS4PortWithFirmwareVersion")) GeneralSwitchPS4PortWithFirmwareVersion = AppSettings.Get<bool>("generalSwitchPS4PortWithFirmwareVersion");
                    


                }
                else
                {
                    Console.WriteLine(MainWindow.ErrorTag + " LoadSettings: No app settings found, loading defaults.");
                    LoadDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(MainWindow.ErrorTag + " LoadSettings: " + ex.ToString());
            }
        }


        public void Save()
        {
            try
            {
                // Open App.Config of executable
                System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                //remove old setting if exists (no exception if doens#T exist)
                config.AppSettings.Settings.Remove("ModificationDate");

                // Add an Application Setting.
                config.AppSettings.Settings.Add("ModificationDate", DateTime.Now.ToLongTimeString() + " ");




                config.AppSettings.Settings.Remove("httpDefaultFile");
                config.AppSettings.Settings.Add("httpDefaultFile", HTTPDefaultFile);

                config.AppSettings.Settings.Remove("httpUseDefaultFile");
                config.AppSettings.Settings.Add("httpUseDefaultFile", HTTPUseDefaultFile.ToString());

                config.AppSettings.Settings.Remove("httpDefaultPort");
                config.AppSettings.Settings.Add("httpDefaultPort", HTTPDefaultPort.ToString());

                config.AppSettings.Settings.Remove("httpUseDefaultPort");
                config.AppSettings.Settings.Add("httpUseDefaultPort", HTTPUseDefaultPort.ToString());

                config.AppSettings.Settings.Remove("httpElfloaderDefaultPort");
                config.AppSettings.Settings.Add("httpElfloaderDefaultPort", HTTPElfloaderDefaultPort.ToString());

                config.AppSettings.Settings.Remove("httpElfloaderUseDefaultPort");
                config.AppSettings.Settings.Add("httpElfloaderUseDefaultPort", HTTPElfloaderUseDefaultPort.ToString());

                config.AppSettings.Settings.Remove("httpElfloaderDefaultFile");
                config.AppSettings.Settings.Add("httpElfloaderDefaultFile", HTTPElfloaderDefaultFile);

                config.AppSettings.Settings.Remove("httpElfloaderUseDefaultFile");
                config.AppSettings.Settings.Add("httpElfloaderUseDefaultFile", HTTPElfloaderUseDefaultFile.ToString());


                config.AppSettings.Settings.Remove("httpHTMLScaling");
                config.AppSettings.Settings.Add("httpHTMLScaling", HTTPHTMLScaling.ToString());

                config.AppSettings.Settings.Remove("ps4IP");
                config.AppSettings.Settings.Add("ps4IP", PS4IP);

                config.AppSettings.Settings.Remove("dnsLocalOnly");
                config.AppSettings.Settings.Add("dnsLocalOnly", DNSLocalOnly.ToString());

                config.AppSettings.Settings.Remove("dnsForwardServer");
                config.AppSettings.Settings.Add("dnsForwardServer", DNSForwardServer);

                config.AppSettings.Settings.Remove("ps4DefaultPort");
                config.AppSettings.Settings.Add("ps4DefaultPort", PS4DefaultPort.ToString());

                config.AppSettings.Settings.Remove("ps4UseDefaultPort");
                config.AppSettings.Settings.Add("ps4UseDefaultPort", PS4UseDefaultPort.ToString());

                config.AppSettings.Settings.Remove("localIP");
                config.AppSettings.Settings.Add("localIP", LocalIP);

                config.AppSettings.Settings.Remove("useLocalIP");
                config.AppSettings.Settings.Add("useLocalIP", UseLocalIP.ToString());

                config.AppSettings.Settings.Remove("payloadDefaultFile");
                config.AppSettings.Settings.Add("payloadDefaultFile", PayloadDefaultFile);

                config.AppSettings.Settings.Remove("payloadUseDefaultFile");
                config.AppSettings.Settings.Add("payloadUseDefaultFile", PayloadUseDefaultFile.ToString());

                config.AppSettings.Settings.Remove("openLogAfterStart");
                config.AppSettings.Settings.Add("openLogAfterStart", OpenLogAfterStart.ToString());

                config.AppSettings.Settings.Remove("openElfloaderLogAfterStart");
                config.AppSettings.Settings.Add("openElfloaderLogAfterStart", OpenElfloaderLogAfterStart.ToString());

                config.AppSettings.Settings.Remove("openProxyDumpAfterStart");
                config.AppSettings.Settings.Add("openProxyDumpAfterStart", OpenProxyDumpAfterStart.ToString());

                config.AppSettings.Settings.Remove("proxyDumpDefaultRepsonseFile");
                config.AppSettings.Settings.Add("proxyDumpDefaultRepsonseFile", ProxyDumpDefaultRepsonseFile);

                config.AppSettings.Settings.Remove("proxyDumpUseDefaultResponseFile");
                config.AppSettings.Settings.Add("proxyDumpUseDefaultResponseFile", ProxyDumpUseDefaultResponseFile.ToString());

                config.AppSettings.Settings.Remove("proxyDumpPort");
                config.AppSettings.Settings.Add("proxyDumpPort", ProxyDumpPort.ToString());

                config.AppSettings.Settings.Remove("proxyDumpUsePort");
                config.AppSettings.Settings.Add("proxyDumpUsePort", ProxyDumpUsePort.ToString());

                config.AppSettings.Settings.Remove("proxyDumpSplitSessions");
                config.AppSettings.Settings.Add("proxyDumpSplitSessions", ProxyDumpSplitSessions.ToString());

                config.AppSettings.Settings.Remove("generalSwitchPS4PortWithFirmwareVersion");
                config.AppSettings.Settings.Add("generalSwitchPS4PortWithFirmwareVersion", GeneralSwitchPS4PortWithFirmwareVersion.ToString());

                // Save the changes in App.config file.
                config.Save(ConfigurationSaveMode.Modified);

                // Force a reload of a changed section.
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                Console.WriteLine(MainWindow.ErrorTag + " Save: " + ex.ToString());
            }
        }

        public void ApplySettings()
        {
            if (httpUseDefaultFile && string.IsNullOrEmpty(httpDefaultFile) && File.Exists(httpDefaultFile)) ; //fixme: write text to textbox
        }

    }
    
}
