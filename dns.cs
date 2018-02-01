using DNS.Client;
using DNS.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace psx_cpl
{
    public static class dns
    {
        public async static Task DNSAsync(string[] Domains, string localIP, List<string>BlackList, string DNSForwardingServer)
        {
            if (!String.IsNullOrEmpty(localIP))
            {
                // if one wants to use a remote DNS server for requests that cannot be solved:
                // DnsServer server = new DnsServer("8.8.8.8");
                string ForwardingIP = "0.0.0.0";

                if (!String.IsNullOrEmpty(DNSForwardingServer))
                {
                    IPAddress ForwardingIPAddress;
                    IPAddress.TryParse(DNSForwardingServer, out ForwardingIPAddress);
                    if (ForwardingIPAddress != null) ForwardingIP = ForwardingIPAddress.ToString();
                }

                bool RedirectUnknownToLocal = true;
                if (!MainWindow.Instance.AppSettings.DNSLocalOnly) RedirectUnknownToLocal = false;

                DnsServer server = new DnsServer(ForwardingIP, MainWindow.Instance.AppSettings.DNSLocalOnly, RedirectUnknownToLocal);

                server.LocalIP = localIP;
                MainWindow.dnsServerList.Add(server);

                if(BlackList != null && BlackList.Count > 0)
                {
                    foreach (string blacklistentry in BlackList)
                    {
                        Console.WriteLine(MainWindow.InfoTag + " Redirecting {0} to 0.0.0.0", blacklistentry);
                        MainWindow.AddToLogDNS(String.Format(MainWindow.InfoTag + " Redirecting {0} to 0.0.0.0", blacklistentry));
                        server.MasterFile.AddIPAddressResourceRecord(blacklistentry, "0.0.0.0");
                    }
                }
                


                Console.WriteLine(MainWindow.InfoTag + " DNSAsync localIP: " + localIP);

                foreach (string domain in Domains)
                {
                    Console.WriteLine();
                    MainWindow.AddToLogDNS(String.Format(MainWindow.InfoTag + " Redirecting {0} to " + localIP, domain));
                    server.MasterFile.AddIPAddressResourceRecord(domain, localIP);
                }

                //server.Responded += (request, response) => Console.WriteLine("{0} => {1}", request, response);
                //server.Responded += (request, response) => MainWindow.AddToLogDNS(String.Format("{0} => {1}", request, response));
                server.Responded += (request, response) => MainWindow.AddToLogDNS(String.Format("{0} => {1}", DNS.Protocol.Utils.ObjectStringifier.Stringify(request.Questions), DNS.Protocol.Utils.ObjectStringifier.Stringify(response.AnswerRecords)));
                server.Errored += (e) =>
                {
                    Console.WriteLine(MainWindow.ErrorTag + ": {0}", e);
                    MainWindow.AddToLogDNS(String.Format(MainWindow.ErrorTag + ": {0}", e));
                    ResponseException responseError = e as ResponseException;
                    if (responseError != null) Console.WriteLine(responseError.Response);
                };

                try
                {
                    await server.Listen();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(MainWindow.ErrorTag + " DNSAsync: " + ex.ToString());
                }
            }
        }

    }
}
