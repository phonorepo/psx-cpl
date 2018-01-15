﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace psx_cpl
{
    public class Client
    {
        public TcpClient tcpClient;
        public bool connecting = false;

        public bool isConnected
        {
            get
            {
                if (tcpClient != null) return tcpClient.Connected;
                else return false;
            }
        }
        public Client()
        {
            tcpClient = new TcpClient();
            tcpClient.SendTimeout = 5000;
            tcpClient.ReceiveTimeout = 5000;
        }

        public async void StartRead(string ip, int port)
        {
            await Initialize(ip, port);
            // Start reading task
            Task.Run(() => this.Read());

        }
        public async void StartSend(string ip, int port, string PayloadFilePath)
        {
            await Initialize(ip, port);
            // Start send task
            Task.Run(() => this.Send(PayloadFilePath));

        }


        public async Task Initialize(string ip, int port)
        {
            if(tcpClient == null) tcpClient = new TcpClient();
            if (!connecting)
            {
                connecting = true;
                try
                {
                    await tcpClient.ConnectAsync(ip, port);

                    Console.WriteLine("Connected to: {0}:{1}", ip, port);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(MainWindow.ErrorTag + " Client Initialize: " + ex.ToString());

                    MainWindow.Instance.mBox(MainWindow.ErrorTag + " Client Initialize: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine(MainWindow.ErrorTag + " Client already connecting");
            }
        }

        public async Task Disconnect()
        {
            if (tcpClient != null)
                Task.Run(() => tcpClient.Close());

            Console.WriteLine(MainWindow.InfoTag + " Client Disconnected");
        }

        public async Task Read()
        {
            String responseData = String.Empty;

            var buffer = new byte[4096];
            var ns = tcpClient.GetStream();
            while (true)
            {
                var bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) return; // Stream was closed
                Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                MainWindow.AddToLog("bytes received: " + bytesRead);

                responseData = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                MainWindow.AddToLog(String.Format("Received: {0}", responseData));
            }
        }

        public async Task Send(string PayloadFilePath)
        {
            Console.WriteLine(MainWindow.InfoTag + " Client Send PayloadFilePath: " + PayloadFilePath);

            if (!String.IsNullOrEmpty(PayloadFilePath) && File.Exists(PayloadFilePath))
            {

                try
                {
                    if (tcpClient != null && tcpClient.Connected == true)
                    {
                        tcpClient.Client.SendFile(PayloadFilePath);

                        await Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(MainWindow.ErrorTag + " Client Send: " + ex.ToString());
                }
            }
        }
    }
}