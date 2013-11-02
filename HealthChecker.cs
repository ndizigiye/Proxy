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

        public static List<Server> availableServer;

        public static void checkHealth(string[] servers){

            var parameterizedThread = new ParameterizedThreadStart(checkHealthInternal);
            var checkHealthThread = new Thread(parameterizedThread);
            checkHealthThread.Start(servers);
        }

        public static void checkHealthInternal(object o){

            var servers = (string[]) o;

            foreach(var server in servers){

                var serverAddress = server.Split(':')[0].Trim();
                var port = server.Split(':')[1].Trim();
                string[] serverAddressAndPort = {serverAddress,port};

                var parameterizedThread = new ParameterizedThreadStart(PingServer);
                var pingThread = new Thread(parameterizedThread);
                pingThread.Start(serverAddressAndPort);
            }

        }

        public static void PingServer(object o)
        {
            var serverAddressAndPort = (string[])o;

            var serverAddress = serverAddressAndPort[0];
            var serverPort = Convert.ToInt32(serverAddressAndPort[1]);
            Server server = new Server(serverAddress,serverPort);
            var isActive = server.Ping();
        }

    }

        

    class Server
    {
        public event OnTextChangeHandler OnTextChangeEvent;

        private string address;
        private int port;

        public Server(string address, int port){
            this.address = address;
            this.port = port;
        }

        public bool Ping(){

            var destServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var testHeader = String.Format("GET http://{0}:{1}/ HTTP/1.1", address, port);
            var testHeaderBytes = Encoding.ASCII.GetBytes(testHeader);
            destServerSocket.Connect(address, port);
            destServerSocket.Send(testHeaderBytes);

            var responseBuffer = new byte[1024];

            try
            {
                while (destServerSocket.Receive(responseBuffer) != 0)
                {
                    var response = Encoding.ASCII.GetString(responseBuffer);
                    Debug.Write(Encoding.ASCII.GetString(responseBuffer));
                }
                destServerSocket.Disconnect(false);
                destServerSocket.Dispose();
            }
            catch (Exception e)
            {
                OnTextChangeEvent(new OnTextChange(e.Message));
            }
            return true;
        }
    }
}
