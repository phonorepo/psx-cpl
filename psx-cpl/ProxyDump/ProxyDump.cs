/*
* This program is using the FiddlerCore and Ionic.Zip.Reduced libraries.
* You will find both license files in the lisenses folder.
* It is compiled with support for the SAZ File format by
* defining the token SAZ_SUPPORT in the list of
* Conditional Compilation symbols on the project's BUILD tab.
* 
* For this it is needed to add either SAZ-DOTNETZIP.cs or SAZXCEEDZIP.cs to your project,
* depending on which ZIP library you want to use. You must also ensure to set the 
* Fiddler.RequiredVersionAttribute on your assembly, or your transcoders will be 
* ignored.
*/

//FIXME: Register eventhandlers only once

using System;
using Fiddler;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.IO;
using System.ComponentModel;

namespace psx_cpl.ProxyDump
{
    public class ProxyDump : INotifyPropertyChanged
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

        private bool debugMessages = false;
        public bool DebugMessages
        {
            get { return debugMessages; }
            set { debugMessages = value; OnPropertyChanged("DebugMessages"); }
        }

        private bool running = false;
        public bool Running
        {
            get { return running; }
            set { running = value; OnPropertyChanged("Running"); }
        }

        private bool handlerRegistered = false;
        public bool HandlerRegistered
        {
            get { return handlerRegistered; }
            set { handlerRegistered = value; OnPropertyChanged("HandlerRegistered"); }
        }

        private string uriFilterFilePath = "responses/urifilter.txt";
        public string URIFilterFilePath
        {
            get { return uriFilterFilePath; }
            set { uriFilterFilePath = value; OnPropertyChanged("URIFilterFilePath"); }
        }

        private List<string> uriFilterList = new List<string>();
        public List<string> URIFilterList
        {
            get { return uriFilterList; }
            set { uriFilterList = value; OnPropertyChanged("uriFilterList"); }
        }

        private string currentResponseFile;
        public string CurrentResponseFile
        {
            get { return currentResponseFile; }
            set { currentResponseFile = value; OnPropertyChanged("CurrentResponseFile"); WriteCommandResponse("Response file changed to: " + currentResponseFile); }
        }


        private bool splitSessionsFiles = false;
        public bool SplitSessionsFiles
        {
            get { return splitSessionsFiles; }
            set { splitSessionsFiles = value; OnPropertyChanged("SplitSessionsFiles"); }
        }

        private string[] _sAZfiles = { };
        public string[] _SAZfiles
        {
            get { return _sAZfiles; }
            set { _sAZfiles = value; OnPropertyChanged("_SAZfiles"); }
        }

        private string responsePath = @"responses";
        public string ResponsePath
        {
            get { return responsePath; }
            set { responsePath = value; OnPropertyChanged("ResponsePath"); }
        }

        // List holding all sessions
        private List<Fiddler.Session> _oAllSessions = new List<Fiddler.Session>();
        public List<Fiddler.Session> oAllSessions
        {
            get { return _oAllSessions; }
            set { _oAllSessions = value; OnPropertyChanged("oAllSessions"); }
        }

        // List for single dumped session
        private List<Fiddler.Session> _oSingleSession = new List<Fiddler.Session>();
        public List<Fiddler.Session> oSingleSession
        {
            get { return _oSingleSession; }
            set { _oSingleSession = value; OnPropertyChanged("oSingleSession"); }
        }

        private bool filteractive = true;
        public bool Filteractive
        {
            get { return filteractive; }
            set { filteractive = value; OnPropertyChanged("Filteractive"); }
        }

        private bool fiddlermessages = true;
        public bool Fiddlermessages
        {
            get { return fiddlermessages; }
            set { fiddlermessages = value; OnPropertyChanged("Fiddlermessages"); }
        }

        private bool connectionmessages = true;
        public bool Connectionmessages
        {
            get { return connectionmessages; }
            set { connectionmessages = value; OnPropertyChanged("Connectionmessages"); }
        }

        private bool dumpMode = false;
        public bool DumpMode
        {
            get { return dumpMode; }
            set { dumpMode = value; OnPropertyChanged("DumpMode"); }
        }

        private bool bupdateTitle = true;
        public bool bUpdateTitle
        {
            get { return bupdateTitle; }
            set { bupdateTitle = value; OnPropertyChanged("bUpdateTitle"); }
        }


        private Proxy pSecureEndpoint;
        public Proxy PSecureEndpoint
        {
            get { return pSecureEndpoint; }
            set { pSecureEndpoint = value; OnPropertyChanged("PSecureEndpoint"); }
        }

        // For forward-compatibility with updated FiddlerCore libraries, it is strongly recommended that you 
        // start with the DEFAULT options and manually disable specific unwanted options.
        private FiddlerCoreStartupFlags fCSF = FiddlerCoreStartupFlags.Default;

        public FiddlerCoreStartupFlags FCSF
        {
            get { return fCSF; }
            set { fCSF = value; OnPropertyChanged("FCSF"); }
        }

        private string sSecureEndpointHostname = "localhost";
        public string SSecureEndpointHostname
        {
            get { return sSecureEndpointHostname; }
            set { sSecureEndpointHostname = value; OnPropertyChanged("SSecureEndpointHostname"); }
        }

        private int iSecureEndpointPort = 7777;
        public int ISecureEndpointPort
        {
            get { return iSecureEndpointPort; }
            set { iSecureEndpointPort = value; OnPropertyChanged("ISecureEndpointPort"); }
        }

        public void WriteCommandResponse(string s)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ForegroundColor = oldColor;
            MainWindow.AddToLogProxyDump(s);
        }

        public void LoadURIFilter()
        {
            // Declare new List.
            List<string> lines = new List<string>();

            try
            {
                if (File.Exists(URIFilterFilePath))
                {
                    // Use using StreamReader for disposing.
                    using (StreamReader r = new StreamReader(URIFilterFilePath))
                    {
                        // Use while != null pattern for loop
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            // "line" is a line in the file. Add it to our List.
                            lines.Add(line);
                            WriteCommandResponse("URI Filter added: " + line);
                        }
                    }

                    if (lines.Count > 0)
                    {
                        URIFilterList = new List<string>();
                        URIFilterList.AddRange(lines);
                    }

                    // Print out all the lines.
                    foreach (string s in lines)
                    {
                        if (DebugMessages) Console.WriteLine(s);
                    }
                }
                else
                {
                    WriteCommandResponse("Error: URIFilter file not found: " + URIFilterFilePath);
                }
            }
            catch (SystemException ex)
            {
                Console.WriteLine("Error loading URIFilterFile: " + ex + Environment.NewLine);
                WriteCommandResponse("Error while loading URIFilter: " + ex.ToString());
            }
        }


        public void LoadResponseFile()
        {
            try
            {
                string actualSAZfile = "none";

                if (Directory.Exists(ResponsePath))
                {
                    _SAZfiles = Directory.GetFiles(ResponsePath, "*.saz");
                    if (_SAZfiles != null && _SAZfiles.Length > 0) actualSAZfile = _SAZfiles[0];
                    if (MainWindow.Instance.AppSettings.ProxyDumpUseDefaultResponseFile && !String.IsNullOrEmpty(MainWindow.Instance.AppSettings.ProxyDumpDefaultRepsonseFile))
                    {
                        if (File.Exists(MainWindow.Instance.AppSettings.ProxyDumpDefaultRepsonseFile)) actualSAZfile = MainWindow.Instance.AppSettings.ProxyDumpDefaultRepsonseFile;
                        else if (File.Exists(@"responses\" + MainWindow.Instance.AppSettings.ProxyDumpDefaultRepsonseFile)) actualSAZfile = MainWindow.Instance.AppSettings.ProxyDumpDefaultRepsonseFile;
                    }
                    CurrentResponseFile = actualSAZfile;
                }
                else
                {
                    WriteCommandResponse("Error repsonse directory not found: " + ResponsePath);
                }
            }
            catch (SystemException ex)
            {
                WriteCommandResponse("responses missing: " + ex);
            }

            WriteCommandResponse("Using for AutoResponse: " + CurrentResponseFile);
        }


        public void DoQuit()
        {
            WriteCommandResponse("Shutting down...");
            try
            {
                if (null != PSecureEndpoint) PSecureEndpoint.Dispose();
                Fiddler.FiddlerApplication.Shutdown();
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                WriteCommandResponse("Error: " + ex.ToString());
            }
            finally
            {
                Running = false;
            }
        }
        private static string Ellipsize(string s, int iLen)
        {
            if (s.Length <= iLen) return s;
            return s.Substring(0, iLen - 3) + "...";
        }

#if SAZ_SUPPORT
        private void ReadSessions(List<Fiddler.Session> oAllSessions)
        {
            TranscoderTuple oImporter = FiddlerApplication.oTranscoders.GetImporter("SAZ");
            if (null != oImporter)
            {
                Dictionary<string, object> dictOptions = new Dictionary<string, object>();
                //dictOptions.Add("Filename", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\ToLoad.saz");
                dictOptions.Add("Filename", @"responses\ToLoad.saz");

                Session[] oLoaded = FiddlerApplication.DoImport("SAZ", false, dictOptions, null);

                if ((oLoaded != null) && (oLoaded.Length > 0))
                {
                    oAllSessions.AddRange(oLoaded);
                    WriteCommandResponse("Loaded: " + oLoaded.Length + " sessions.");
                }
            }
        }


        private void SaveSessionsToDumpFolder(List<Fiddler.Session> Sessions, bool FullSession = false)
        {
            bool bSuccess = false;


            if (!Directory.Exists("proxydumps"))
            {
                try
                {
                    Directory.CreateDirectory("proxydumps");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] ExportSessions: " + ex.ToString());
                }
            }

            string sFilename = @"proxydumps\" + DateTime.Now.ToString("hh-mm-ss-fff") + "_dump_SPLIT.saz";
            if (FullSession) sFilename = @"proxydumps\" + DateTime.Now.ToString("hh-mm-ss-fff") + "_dump_FULL.saz";
            try
            {
                try
                {
                    Monitor.Enter(Sessions);
                    TranscoderTuple oExporter = FiddlerApplication.oTranscoders.GetExporter("SAZ");

                    if (null != oExporter)
                    {
                        Dictionary<string, object> dictOptions = new Dictionary<string, object>();
                        dictOptions.Add("Filename", sFilename);
                        // dictOptions.Add("Password", "pencil");

                        bSuccess = FiddlerApplication.DoExport("SAZ", Sessions.ToArray(), dictOptions, null);
                    }
                    else
                    {
                        Console.WriteLine("Save failed because the SAZ Format Exporter was not available.");
                    }
                }
                finally
                {
                    Monitor.Exit(Sessions);
                }

                WriteCommandResponse(bSuccess ? ("Wrote: " + sFilename) : ("Failed to save: " + sFilename));
            }
            catch (Exception eX)
            {
                Console.WriteLine("Save failed: " + eX.Message);
            }
        }

        private void SaveSessionsToDesktop(List<Fiddler.Session> oAllSessions)
        {
            bool bSuccess = false;
            string sFilename = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                            + @"\" + DateTime.Now.ToString("hh-mm-ss") + ".saz";
            try
            {
                try
                {
                    Monitor.Enter(oAllSessions);
                    TranscoderTuple oExporter = FiddlerApplication.oTranscoders.GetExporter("SAZ");

                    if (null != oExporter)
                    {
                        Dictionary<string, object> dictOptions = new Dictionary<string, object>();
                        dictOptions.Add("Filename", sFilename);
                        // dictOptions.Add("Password", "pencil");

                        bSuccess = FiddlerApplication.DoExport("SAZ", oAllSessions.ToArray(), dictOptions, null);
                    }
                    else
                    {
                        Console.WriteLine("Save failed because the SAZ Format Exporter was not available.");
                    }
                }
                finally
                {
                    Monitor.Exit(oAllSessions);
                }

                WriteCommandResponse(bSuccess ? ("Wrote: " + sFilename) : ("Failed to save: " + sFilename));
            }
            catch (Exception eX)
            {
                Console.WriteLine("Save failed: " + eX.Message);
            }
        }
#endif

        private void WriteSessionList(List<Fiddler.Session> Sessions)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Session list contains...");
            try
            {
                Monitor.Enter(Sessions);
                foreach (Session oS in Sessions)
                {
                    Console.Write(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 60), oS.responseCode, oS.oResponse.MIMEType));
                    WriteCommandResponse(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 60), oS.responseCode, oS.oResponse.MIMEType));
                }
            }
            finally
            {
                Monitor.Exit(Sessions);
            }
            Console.WriteLine();
            Console.ForegroundColor = oldColor;
        }

        private string ReadFile(string file)
        {
            StreamReader streamReader = new StreamReader(file);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            return text;
        }

        public ProxyDump()
        {
            // List holding all sessions
            oAllSessions = new List<Fiddler.Session>();
            // List for single dumped session
            oSingleSession = new List<Fiddler.Session>();

            int _counter = 0;

            try
            {
                if (!Directory.Exists(ResponsePath)) Directory.CreateDirectory(ResponsePath);
            }
            catch (Exception ex)
            {
                WriteCommandResponse("Error while creating directory: " + ResponsePath + Environment.NewLine + ex);
            }

            WriteCommandResponse("ProxyDump [Alpha] 2018 by phono");


            //Load URIFilter
            LoadURIFilter();

            LoadResponseFile();

            #region AttachEventListeners
            //
            // It is important to understand that FiddlerCore calls event handlers on session-handling
            // background threads.  If you need to properly synchronize to the UI-thread (say, because
            // you're adding the sessions to a list view) you must call .Invoke on a delegate on the 
            // window handle.
            // 
            // If you are writing to a non-threadsafe data structure (e.g. List<t>) you must
            // use a Monitor or other mechanism to ensure safety.
            //

            // Simply echo notifications to the console.  Because Fiddler.CONFIG.QuietMode=true 
            // by default, we must handle notifying the user ourselves.
            if (fiddlermessages && !HandlerRegistered)
            {
                Fiddler.FiddlerApplication.OnNotification += delegate (object sender, NotificationEventArgs oNEA) { Console.WriteLine("** NotifyUser: " + oNEA.NotifyString); WriteCommandResponse("**NotifyUser: " + oNEA.NotifyString); };
                Fiddler.FiddlerApplication.Log.OnLogString += delegate (object sender, LogEventArgs oLEA) { Console.WriteLine("** LogString: " + oLEA.LogString); WriteCommandResponse("**LogString: " + oLEA.LogString); };
            }

            if (!HandlerRegistered)
            {
                Fiddler.FiddlerApplication.BeforeRequest += delegate (Fiddler.Session oS)
                {
                    HandlerRegistered = true;

                    // Console.WriteLine("Before request for:\t" + oS.fullUrl);
                    // In order to enable response tampering, buffering mode MUST
                    // be enabled; this allows FiddlerCore to permit modification of
                    // the response in the BeforeResponse handler rather than streaming
                    // the response to the client as the response comes in.
                    oS.bBufferResponse = false;
                    Monitor.Enter(oAllSessions);
                    oAllSessions.Add(oS);
                    Monitor.Exit(oAllSessions);

                    #region OwnCode
                    if (connectionmessages) Console.Write(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 60), oS.responseCode, oS.oResponse.MIMEType));
                    WriteCommandResponse(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 60), oS.responseCode, oS.oResponse.MIMEType));

                    //string URIFilterFilePath = "responses/urifilter.txt";
                    // Declare new List.
                    List<string> lines = new List<string>();

                    if (URIFilterList != null && URIFilterList.Count > 0) lines.AddRange(URIFilterList);

                    foreach (string s in lines)
                    {
                        if (DebugMessages) WriteCommandResponse("Filter is active: " + filteractive);
                        if (DebugMessages) WriteCommandResponse("oS.uriContains(s): " + oS.uriContains(s) + " " + oS.url);
                        if (oS.uriContains(s) && filteractive)
                        {
                            WriteCommandResponse("INTERCEPTED CALL TO: " + Environment.NewLine);
                            WriteCommandResponse(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 60), oS.responseCode, oS.oResponse.MIMEType));


                            // if DumpMode add Session and directly save it and clear it to only write a single session for each file
                            if (DumpMode)
                            {
                                Monitor.Enter(oSingleSession);
                                oSingleSession.Add(oS);
                                Monitor.Exit(oSingleSession);
                            }

                            if (DebugMessages) WriteCommandResponse("DumpMode is active: " + DumpMode);
                            if (!DumpMode)
                            {
                                //Send SINGLE RESPONSE

                                oS.utilCreateResponseAndBypassServer();

                                // Inside your main object, create a list to hold the sessions
                                // This generic list type requires your source file includes #using System.Collections.Generic.
                                List<Fiddler.Session> ReplaySessions = new List<Fiddler.Session>();
                                Session[] ReplaySession;

                                TranscoderTuple oImporter = FiddlerApplication.oTranscoders.GetImporter("SAZ");
                                if (null != oImporter && CurrentResponseFile != null)
                                {
                                    Dictionary<string, object> dictOptions = new Dictionary<string, object>();
                                    dictOptions.Add("Filename", CurrentResponseFile);

                                    ReplaySession = FiddlerApplication.DoImport("SAZ", false, dictOptions, null);

                                    if ((ReplaySession != null) && (ReplaySession.Length > 0))
                                    {
                                        ReplaySessions.AddRange(ReplaySession);

                                        oS.responseBodyBytes = ReplaySession[0].responseBodyBytes;
                                        oS.oResponse.headers = (HTTPResponseHeaders)ReplaySession[0].oResponse.headers.Clone();

                                        string shortResponseBody = System.Text.Encoding.UTF8.GetString(ReplaySession[0].responseBodyBytes).Substring(0, 100);
                                        WriteCommandResponse(ReplaySession[0].oResponse.headers.ToString());
                                        WriteCommandResponse(shortResponseBody + " ...");
                                        WriteCommandResponse("Loaded: " + ReplaySession.Length + " sessions.");
                                    }
                                }
                                else if (CurrentResponseFile == null)
                                {
                                    WriteCommandResponse("Error: no recorded sessions found");
                                }
                            }//END if (!DumpMode)


                        }
                    }
                    #endregion OwnCode



                    /* If the request is going to our secure endpoint, we'll echo back the response.
                
                    Note: This BeforeRequest is getting called for both our main proxy tunnel AND our secure endpoint, 
                    so we have to look at which Fiddler port the client connected to (pipeClient.LocalPort) to determine whether this request 
                    was sent to secure endpoint, or was merely sent to the main proxy tunnel (e.g. a CONNECT) in order to *reach* the secure endpoint.
                
                    As a result of this, if you run the demo and visit https://localhost:7777 in your browser, you'll see
                
                    Session list contains...
                 
                        1 CONNECT http://localhost:7777
                        200                                         <-- CONNECT tunnel sent to the main proxy tunnel, port 8877

                        2 GET https://localhost:7777/
                        200 text/html                               <-- GET request decrypted on the main proxy tunnel, port 8877

                        3 GET https://localhost:7777/               
                        200 text/html                               <-- GET request received by the secure endpoint, port 7777
                    */

                    if ((oS.oRequest.pipeClient.LocalPort == iSecureEndpointPort) && (oS.hostname == sSecureEndpointHostname))
                    {
                        oS.utilCreateResponseAndBypassServer();
                        oS.oResponse.headers.HTTPResponseStatus = "200 Ok";
                        oS.oResponse["Content-Type"] = "text/html; charset=UTF-8";
                        oS.oResponse["Cache-Control"] = "private, max-age=0";
                        oS.utilSetResponseBody("<html><body>Request for httpS://" + sSecureEndpointHostname + ":" + iSecureEndpointPort.ToString() + " received. Your request was:<br /><plaintext>" + oS.oRequest.headers.ToString());
                    }
                }; // END before Request
            }

            /*
                // The following event allows you to examine every response buffer read by Fiddler. Note that this isn't useful for the vast majority of
                // applications because the raw buffer is nearly useless; it's not decompressed, it includes both headers and body bytes, etc.
                //
                // This event is only useful for a handful of applications which need access to a raw, unprocessed byte-stream
                Fiddler.FiddlerApplication.OnReadResponseBuffer += new EventHandler<RawReadEventArgs>(FiddlerApplication_OnReadResponseBuffer);
            */

            /*
            Fiddler.FiddlerApplication.BeforeResponse += delegate(Fiddler.Session oS) {
                // Console.WriteLine("{0}:HTTP {1} for {2}", oS.id, oS.responseCode, oS.fullUrl);
                
                // Uncomment the following two statements to decompress/unchunk the
                // HTTP response and subsequently modify any HTTP responses to replace 
                // instances of the word "Microsoft" with "Bayden". You MUST also
                // set bBufferResponse = true inside the beforeREQUEST method above.
                //
                //oS.utilDecodeResponse(); oS.utilReplaceInResponse("Microsoft", "Bayden");
            };*/

            Fiddler.FiddlerApplication.AfterSessionComplete += delegate (Fiddler.Session oS)
            {
                //Console.WriteLine("Finished session:\t" + oS.fullUrl); 
                if (bUpdateTitle)
                {
                    try
                    {
                        Console.Title = ("Session list contains: " + oAllSessions.Count.ToString() + " sessions");
                    }
                    catch (Exception ex)
                    {
                        //not running in console
                    }
                }

                // if DumpMode add Session and directly save it and clear it to only write a single session for each file
                if (DumpMode)
                {
                    if (SplitSessionsFiles)
                    {
                        WriteSessionList(oSingleSession);

                        if (oSingleSession.Count > 0)
                        {
                            //SaveSessionsToDesktop(oSingleSession);
                            SaveSessionsToDumpFolder(oSingleSession);
                        }
                        else
                        {
                            WriteCommandResponse("No sessions have been captured");
                        }

                        Monitor.Enter(oSingleSession);
                        oSingleSession.Clear();
                        Monitor.Exit(oSingleSession);
                    }

                    //test dump all sessions
                    SaveSessionsToDumpFolder(oAllSessions, true);
                }

            };

            // Tell the system console to handle CTRL+C by calling our method that
            // gracefully shuts down the FiddlerCore.
            //
            // Note, this doesn't handle the case where the user closes the window with the close button.
            // See http://geekswithblogs.net/mrnat/archive/2004/09/23/11594.aspx for info on that...
            //
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            #endregion AttachEventListeners

            string sSAZInfo = "NoSAZ";
#if SAZ_SUPPORT
            // If this demo was compiled with a SAZ-Transcoder, then the following lines will load the
            // Transcoders into the available transcoders. You can load other types of Transcoders from
            // a different assembly if you'd like, using the ImportTranscoders(string AssemblyPath) overload.
            // See https://www.fiddler2.com/dl/FiddlerCore-BasicFormats.zip for an example.
            //
            if (!FiddlerApplication.oTranscoders.ImportTranscoders(Assembly.GetExecutingAssembly()))
            {
                Console.WriteLine("This assembly was not compiled with a SAZ-exporter");
            }
            else
            {
                sSAZInfo = SAZFormat.GetZipLibraryInfo();
            }
#endif

            if (fiddlermessages) Console.WriteLine(String.Format("Starting {0} ({1})...", Fiddler.FiddlerApplication.GetVersionString(), sSAZInfo));

            // For the purposes of this demo, we'll forbid connections to HTTPS 
            // sites that use invalid certificates. Change this from the default only
            // if you know EXACTLY what that implies.
            Fiddler.CONFIG.IgnoreServerCertErrors = false;

            // ... but you can allow a specific (even invalid) certificate by implementing and assigning a callback...
            // FiddlerApplication.OverrideServerCertificateValidation += new OverrideCertificatePolicyHandler(FiddlerApplication_OverrideServerCertificateValidation);

            FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);

            // For forward-compatibility with updated FiddlerCore libraries, it is strongly recommended that you 
            // start with the DEFAULT options and manually disable specific unwanted options.
            //FiddlerCoreStartupFlags oFCSF = FiddlerCoreStartupFlags.Default;
            // E.g. uncomment the next line if you don't want FiddlerCore to act as the system proxy
            FCSF = (FCSF & ~FiddlerCoreStartupFlags.RegisterAsSystemProxy);
            // or uncomment the next line if you don't want to decrypt SSL traffic.
            //FCSF = (FCSF & ~FiddlerCoreStartupFlags.DecryptSSL);

            //Start(oFCSF);

            /*
            // Console App

            bool bDone = false;
            do
            {
                Console.WriteLine("\nEnter a command [C=Clear; L=List; d=Toggle Connection Messages;\n\tF=Toggle Filters; G=Collect Garbage; H=Toggle ResponseSessionFile;\n\tI=Toggle Infomessages; J=Toggle DumpMode; W=write SAZ; R=read SAZ;\n\tS=Toggle Forgetful Streaming; T=Toggle Title Counter; Q=Quit]:");
                Console.Write(">");
                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine();
                switch (cki.KeyChar)
                {
                    case 'c':
                        Monitor.Enter(oAllSessions);
                        oAllSessions.Clear();
                        Monitor.Exit(oAllSessions);
                        WriteCommandResponse("Clear...");
                        FiddlerApplication.Log.LogString("Cleared session list.");
                        break;

                    case 'l':
                        WriteSessionList(oAllSessions);
                        break;

                    case 'd':
                        connectionmessages = !connectionmessages;
                        WriteCommandResponse("Show Connection Messages: " + connectionmessages);
                        break;

                    case 'f':
                        filteractive = !filteractive;
                        WriteCommandResponse("Filters active: " + filteractive);
                        break;

                    case 'h':
                        actualSAZfile = _SAZfiles[_counter++ % _SAZfiles.Length];
                        WriteCommandResponse("Using for AutoResponse: " + actualSAZfile);
                        break;

                    case 'i':
                        fiddlermessages = !fiddlermessages;
                        WriteCommandResponse("Show Infomessages: " + fiddlermessages);
                        break;

                    case 'j':
                        DumpMode = !DumpMode;
                        WriteCommandResponse("DumpMode active: " + DumpMode);
                        break;

                    case 'g':
                        Console.WriteLine("Working Set:\t" + Environment.WorkingSet.ToString("n0"));
                        Console.WriteLine("Begin GC...");
                        GC.Collect();
                        Console.WriteLine("GC Done.\nWorking Set:\t" + Environment.WorkingSet.ToString("n0"));
                        break;

                    case 'q':
                        bDone = true;
                        DoQuit();
                        break;

                    case 'r':
#if SAZ_SUPPORT
                        ReadSessions(oAllSessions);
#else
                        WriteCommandResponse("This demo was compiled without SAZ_SUPPORT defined");
#endif
                        break;

                    case 'w':
#if SAZ_SUPPORT
                        if (oAllSessions.Count > 0)
                        {
                            SaveSessionsToDesktop(oAllSessions);
                        }
                        else
                        {
                            WriteCommandResponse("No sessions have been captured");
                        }
#else
                        WriteCommandResponse("This demo was compiled without SAZ_SUPPORT defined");
#endif
                        break;

                    case 't':
                        bUpdateTitle = !bUpdateTitle;
                        Console.Title = (bUpdateTitle) ? "Title bar will update with request count..." :
                            "Title bar update suppressed...";
                        break;

                    // Forgetful streaming
                    case 's':
                        bool bForgetful = !FiddlerApplication.Prefs.GetBoolPref("fiddler.network.streaming.ForgetStreamedData", false);
                        FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.ForgetStreamedData", bForgetful);
                        Console.WriteLine(bForgetful ? "FiddlerCore will immediately dump streaming response data." : "FiddlerCore will keep a copy of streamed response data.");
                        break;

                }
            } while (!bDone);
            */
        }

        /*
        /// <summary>
        /// This callback allows your code to evaluate the certificate for a site and optionally override default validation behavior for that certificate.
        /// You should not implement this method unless you understand why it is a security risk.
        /// </summary>
        /// <param name="sExpectedCN">The CN expected for this session</param>
        /// <param name="ServerCertificate">The certificate provided by the server</param>
        /// <param name="ServerCertificateChain">The certificate chain of that certificate</param>
        /// <param name="sslPolicyErrors">Errors from default validation</param>
        /// <param name="bTreatCertificateAsValid">TRUE if you want to force the certificate to be valid; FALSE if you want to force the certificate to be invalid</param>
        /// <returns>TRUE if you want to override default validation; FALSE if bTreatCertificateAsValid should be ignored</returns>
        static bool FiddlerApplication_OverrideServerCertificateValidation(Session oS, string sExpectedCN, System.Security.Cryptography.X509Certificates.X509Certificate ServerCertificate, System.Security.Cryptography.X509Certificates.X509Chain ServerCertificateChain, System.Net.Security.SslPolicyErrors sslPolicyErrors, out bool bTreatCertificateAsValid)
        {
            if (null != ServerCertificate)
            {
                Console.WriteLine("Certificate for " + sExpectedCN + " was for site " + ServerCertificate.Subject + " and errors were " + sslPolicyErrors.ToString());

                if (ServerCertificate.Subject.Contains("fiddler2.com"))
                {
                    Console.WriteLine("Got a certificate for fiddler2.com and we'll say this is also good for, say, fiddlertool.com");
                    bTreatCertificateAsValid = true;
                    return true;
                }
            }

            bTreatCertificateAsValid = false;
            return false;
        }
        */

        /*
        // This event handler is called on every socket read for the HTTP Response. You almost certainly don't want
        // to sync on this event handler, but the code below shows how you can use it to mess up your HTTP traffic.
        static void FiddlerApplication_OnReadResponseBuffer(object sender, RawReadEventArgs e)
        {
            // NOTE: arrDataBuffer is a fixed-size array. Only bytes 0 to iCountOfBytes should be read/manipulated.
            //
            // Just for kicks, lowercase every byte. Note that this will obviously break any binary content.
            for (int i = 0; i < e.iCountOfBytes; i++)
            {
                if ((e.arrDataBuffer[i] > 0x40) && (e.arrDataBuffer[i] < 0x5b))
                {
                    e.arrDataBuffer[i] = (byte)(e.arrDataBuffer[i] + (byte)0x20);
                }
            }
            Console.WriteLine(String.Format("Read {0} response bytes for session {1}", e.iCountOfBytes, e.sessionOwner.id));
        }
        */

        /// <summary>
        /// When the user hits CTRL+C, this event fires.  We use this to shut down and unregister our FiddlerCore.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DoQuit();
        }

        public void Start(FiddlerCoreStartupFlags Flag, int ProxyPort = 8877)
        {
            if (!Running)
            {
                try
                {
                    //
                    // NOTE: Because we haven't disabled the option to decrypt HTTPS traffic, makecert.exe 
                    // must be present in this executable's folder.

                    // NOTE: In the next line, you can pass 0 for the port (instead of 8877) to have FiddlerCore auto-select an available port
                    //Fiddler.FiddlerApplication.Startup(8877, Flag);
                    Fiddler.FiddlerApplication.Startup(ProxyPort, Flag);
                    WriteCommandResponse(MainWindow.InfoTag + " Proxy Port: " + ProxyPort);
                    WriteCommandResponse(MainWindow.InfoTag + " Response File: " + CurrentResponseFile);

                    if (fiddlermessages) FiddlerApplication.Log.LogString("Starting with settings: [" + Flag + "]");
                    if (fiddlermessages) FiddlerApplication.Log.LogString("Using Gateway: " + ((CONFIG.bForwardToGateway) ? "TRUE" : "FALSE"));

                    Console.WriteLine("Hit CTRL+C to end session.");

                    // oSecureEndpoint = FiddlerApplication.CreateProxyEndpoint(iSecureEndpointPort, true, sSecureEndpointHostname);
                    if (null != PSecureEndpoint)
                    {
                        if (fiddlermessages) FiddlerApplication.Log.LogString("Created secure end point listening on port " + iSecureEndpointPort.ToString() + ", using a HTTPS certificate for '" + sSecureEndpointHostname + "'");
                    }

                    Running = true;
                }
                catch (Exception ex)
                {
                    WriteCommandResponse("Error: " + ex.ToString());
                }
            }
            else
            {
                WriteCommandResponse("Error: already running");
            }
        }


        public void TrustRootCert()
        {
            //Installs root cert
            WriteCommandResponse("Result: " + Fiddler.CertMaker.trustRootCert().ToString());
        }

        public bool UninstallCertificate()
        {
            if (CertMaker.rootCertExists())
            {
                if (!CertMaker.removeFiddlerGeneratedCerts(true))
                    return false;
            }
            return true;
        }

    }



}
