/*
original source by Frank Quednau: https://gist.github.com/flq/369432
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace psx_cpl
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;

    public class HttpFileServer : IDisposable
    {
        //private readonly string rootPath;
        private string rootPath;
        private const int bufferSize = 1024 * 512; //512KB
        private readonly HttpListener http;

        public string RootPath
        {
            get { return rootPath; }
            set { rootPath = value; }
        }

        public HttpFileServer(string rootPath, int port, string[] PrefixIPs, bool ElfLoader)
        {
            Console.WriteLine("rootPath: " + rootPath);

            this.RootPath = rootPath;
            http = new HttpListener();
            if (!ElfLoader)
            {
                MainWindow.AddToLogWeb("start serving as: http://localhost:" + port + "/");
                http.Prefixes.Add("http://localhost:" + port + "/");
            }
            if (!ElfLoader)
            {
                MainWindow.AddToLogWeb("start serving as: http://127.0.0.1:" + port + "/");
                http.Prefixes.Add("http://127.0.0.1:" + port + "/");
            }

            if (PrefixIPs != null && PrefixIPs.Length > 0)
            {
                foreach (string ip in PrefixIPs)
                {
                    Console.WriteLine("Prefix-IP: " + ip);
                    MainWindow.AddToLogWeb("start serving as: http://" + ip + ":" + port + "/");
                    http.Prefixes.Add("http://" + ip + ":" + port + "/");
                }
            }

            http.Start();
            if(ElfLoader) http.BeginGetContext(requestWaitELFloader, null);
            else http.BeginGetContext(requestWait, null);
        }


        public void Dispose()
        {
            http.Stop();
        }

        private void requestWait(IAsyncResult ar)
        {
            if (!http.IsListening)
                return;
            var c = http.EndGetContext(ar);
            http.BeginGetContext(requestWait, null);

            var url = tuneUrl(c.Request.RawUrl);
            Console.WriteLine("[INFO] requestWait c.Request.RawUrl: " + c.Request.RawUrl);
            MainWindow.AddToLogWeb("Request.RawUrl: " + c.Request.RawUrl);

            var fullPath = string.IsNullOrEmpty(url) ? RootPath : Path.Combine(RootPath, url);

            Console.WriteLine("[INFO] requestWait fullPath: " + fullPath);

            if (Directory.Exists(fullPath))
                returnDirContents(c, fullPath);
            else if (File.Exists(fullPath))
                returnFile(c, fullPath);
            else if (fullPath.Contains(".html?"))
            {
                string FilePathWithoutParams = String.Empty;

                int index = fullPath.IndexOf("?");
                if (index > 0)
                {
                    FilePathWithoutParams = fullPath.Remove(index, (fullPath.Length - index));
                    Console.WriteLine("[INFO] requestWait FilePathWithoutParams: " + FilePathWithoutParams);
                }

                returnFile(c, FilePathWithoutParams);
            }
            else
                return404(c);
        }


        private void requestWaitELFloader(IAsyncResult ar)
        {
            Console.WriteLine("[INFO] requestWaitELFloader Start");

            if (!http.IsListening)
                return;
            var c = http.EndGetContext(ar);
            http.BeginGetContext(requestWaitELFloader, null);

            var url = tuneUrl(c.Request.RawUrl);
            Console.WriteLine("[INFO] requestWaitELFloader c.Request.RawUrl: " + c.Request.RawUrl);
            MainWindow.AddToLogWeb("Request.RawUrl: " + c.Request.RawUrl);

            var fullPath = string.IsNullOrEmpty(url) ? RootPath : Path.Combine(RootPath, url);

            Console.WriteLine("[INFO] requestWaitELFloader fullPath: " + fullPath);

            if (Directory.Exists(fullPath))
                returnDirContents(c, fullPath);
            else if (File.Exists(fullPath))
            {
                Console.WriteLine("[INFO] requestWaitELFloader - File.Exists(fullPath): " + fullPath);
                Console.WriteLine("[INFO] requestWaitELFloader - c.Request: " + c.Request);
                Console.WriteLine("[INFO] requestWaitELFloader - c.Response: " + c.Response);

                returnFileELFloader(c, fullPath);
            }
            else if (fullPath.Contains(".html?"))
            {
                string FilePathWithoutParams = String.Empty;

                int index = fullPath.IndexOf("?");
                if (index > 0)
                {
                    FilePathWithoutParams = fullPath.Remove(index, (fullPath.Length - index));
                    Console.WriteLine("[INFO] requestWaitELFloader FilePathWithoutParams: " + FilePathWithoutParams);
                }

                returnFile(c, FilePathWithoutParams);
            }
            else
                return404(c);
        }

        private void returnDirContents(HttpListenerContext context, string dirPath)
        {
            Console.WriteLine("[INFO] returnDirContents Start");

            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            using (var sw = new StreamWriter(context.Response.OutputStream))
            {
                sw.WriteLine("<html>");
                sw.WriteLine("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"><style>body {zoom: " + MainWindow.CssScaling + "; -moz-transform: scale(" + MainWindow.CssScaling + "); -moz-transform-origin: 0 0;}</style></head>");
                sw.WriteLine("<body><ul>");

                var dirs = Directory.GetDirectories(dirPath);
                foreach (var d in dirs)
                {
                    var link = d.Replace(RootPath, "").Replace('\\', '/');
                    sw.WriteLine("<li>&lt;DIR&gt; <a href=\"" + link + "\">" + Path.GetFileName(d) + "</a></li>");
                }

                var files = Directory.GetFiles(dirPath);
                foreach (var f in files)
                {
                    var link = f.Replace(RootPath, "").Replace('\\', '/');
                    sw.WriteLine("<li><a href=\"" + link + "\">" + Path.GetFileName(f) + "</a></li>");
                }

                sw.WriteLine("</ul></body></html>");
            }
            context.Response.OutputStream.Close();
        }

        private static void returnFile(HttpListenerContext context, string filePath)
        {
            Console.WriteLine("[INFO] returnFile Start");

            try
            {
                context.Response.ContentType = getcontentType(Path.GetExtension(filePath));
                var buffer = new byte[bufferSize];
                using (var fs = File.OpenRead(filePath))
                {
                    context.Response.ContentLength64 = fs.Length;
                    int read;
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, read);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] returnFile: " + ex.ToString());
            }


            context.Response.OutputStream.Close();
        }

        private static void returnFileELFloader(HttpListenerContext context, string filePath)
        {
            Console.WriteLine("[INFO] returnFileELFloader Start");

            try
            {
                context.Response.ContentType = getcontentTypeELFloader(Path.GetExtension(filePath));
                var buffer = new byte[bufferSize];
                using (var fs = File.OpenRead(filePath))
                {
                    context.Response.ContentLength64 = fs.Length;
                    int read;
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, read);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] returnFileELFloader: " + ex .ToString());
            }

            context.Response.OutputStream.Close();
        }

        private static void return404(HttpListenerContext context)
        {
            Console.WriteLine("[INFO] return404 Start");

            context.Response.StatusCode = 404;
            context.Response.Close();
        }

        private static string tuneUrl(string url)
        {
            Console.WriteLine("[INFO] tuneUrl Start");

            url = url.Replace('/', '\\');
            url = HttpUtility.UrlDecode(url, Encoding.UTF8);
            url = url.Substring(1);
            return url;
        }

        private static string getcontentType(string extension)
        {
            Console.WriteLine("[INFO] getcontentType Start - extension: " + extension);

            switch (extension)
            {
                case ".avi": return "video/x-msvideo";
                case ".css": return "text/css";
                case ".doc": return "application/msword";
                case ".gif": return "image/gif";
                case ".htm":
                case ".html": return "text/html";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".js": return "application/x-javascript";
                case ".mp3": return "audio/mpeg";
                case ".png": return "image/png";
                case ".pdf": return "application/pdf";
                case ".ppt": return "application/vnd.ms-powerpoint";
                case ".zip": return "application/zip";
                case ".txt": return "text/plain";
                default: return "application/octet-stream";
            }
        }

        private static string getcontentTypeELFloader(string extension)
        {
            Console.WriteLine("[INFO] getcontentTypeELFloader - extionsion: " + extension);

            if (extension.Contains(".html?"))
            {
                Console.WriteLine("[INFO] getcontentTypeELFloader - extionsion Contains .html?");
                return "text/html";
            }

            switch (extension)
            {
                case ".css": return "text/css";
                case ".htm":
                case ".html": return "text/html";
                case ".txt": return "text/plain";
                default: return "application/octet-stream";
            }
        }
    }
}
