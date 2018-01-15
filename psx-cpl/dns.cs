using DNS.Client;
using DNS.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace psx_cpl
{
    public static class dns
    {
        public async static Task DNSAsync(string[] Domains, string[] localIPs, List<string>BlackList)
        {
            if (localIPs != null && localIPs.Length > 0)
            {
                // if one wants to use a remote DNS server for requests that cannot be solved:
                // DnsServer server = new DnsServer("8.8.8.8");
                DnsServer server = new DnsServer("0.0.0.0", true);
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
                
                foreach (string localIP in localIPs)
                {

                    Console.WriteLine(MainWindow.InfoTag + " DNSAsync localIP: " + localIP);

                    foreach (string domain in Domains)
                    {
                        Console.WriteLine();
                        MainWindow.AddToLogDNS(String.Format(MainWindow.InfoTag + " Redirecting {0} to " + localIP, domain));
                        server.MasterFile.AddIPAddressResourceRecord(domain, localIP);
                    }
                }

                server.Responded += (request, response) => MainWindow.AddToLogDNS(String.Format("{0} => {1}", request, response));
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
