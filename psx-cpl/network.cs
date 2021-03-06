﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace psx_cpl
{
    public static class network
    {
        public static string[] GetAllLocalIPv4(bool FilterByNetworkInterfaceyType = false, NetworkInterfaceType _type = NetworkInterfaceType.Ethernet)
        {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    if (FilterByNetworkInterfaceyType == true && item.NetworkInterfaceType != _type)
                    {
                        //SKIP
                    }
                    else
                    {
                        foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                ipAddrList.Add(ip.Address.ToString());
                            }
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }

    }
}
