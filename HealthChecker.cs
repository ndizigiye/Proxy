using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Proxy
{
    

    class HealthChecker
    {
        public event OnTextChangeHandler OnTextChangeEvent;

        public static List<Server> activeServers = new List<Server>();

        public static void checkHealthInternal(string[] servers)
        {
            var parameterizedThread = new ParameterizedThreadStart(checkHealth);
            var checkHealthThread = new Thread(parameterizedThread);
            checkHealthThread.Start(servers);
        }
        public static void checkHealth(object o){

            var servers = (string[])o;
            while(true){

                foreach(var server in servers){

                var serverAddress = server.Split(':')[0].Trim();
                var port = server.Split(':')[1].Trim();
                string[] serverAddressAndPort = { serverAddress, port };

                var parameterizedThread = new ParameterizedThreadStart(PingServer);
                var PingServerThread = new Thread(parameterizedThread);
                PingServerThread.Start(serverAddressAndPort);
                }
                Thread.Sleep(5000);
            }
        }

        public static void PingServer(object o)
        {
            var serverAddressAndPort = (string[])o;
            var serverAddress = serverAddressAndPort[0];
            var serverPort = Convert.ToInt32(serverAddressAndPort[1]);
            Server server = new Server(serverAddress, serverPort);
            var isActive = server.Ping();
            var result = activeServers.Find(item => item.port == server.port);
            if (isActive && result == null)
            {
                activeServers.Add(server);
            }
            else if (!isActive && result != null)
            {
                activeServers.Remove(result);
            }
        }
    }

        

    class Server
    {
        public event OnTextChangeHandler OnTextChangeEvent;

        public string address { get; set; }
        public int port { get; set; }

        public Server(string address, int port){
            this.address = address;
            this.port = port;
        }

        public bool Ping(){
            var serverIsAlive = false;
            var responseHeader = "";
            var destServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var testHeader = String.Format("GET http://{0}:{1} HTTP/1.1" +"\r"+ "\n" +
                                           "Host: {0}:{1}" + "\r" + "\n" +"\r"+ "\n", address, port);

            var testHeaderBytes = Encoding.ASCII.GetBytes(testHeader);
            var responseBuffer = new byte[1];

            try
            {
                destServerSocket.Connect(address, port);
                destServerSocket.Send(testHeaderBytes);

                while (destServerSocket.Receive(responseBuffer) != 0)
                {
                    responseHeader += Encoding.ASCII.GetString(responseBuffer);
                    if (responseHeader.EndsWith("\r\n\r\n"))
                    {
                        break;
                    }
                }

                if(responseHeader.Length != 0){
                    serverIsAlive = true;
                }

                destServerSocket.Disconnect(false);
                destServerSocket.Dispose();
            }
            catch (SocketException e)
            {
                serverIsAlive = false;
            }
            return serverIsAlive;
        }
    }

    
}
