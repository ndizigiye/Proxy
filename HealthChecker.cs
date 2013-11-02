using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Proxy
{
    class HealthChecker
    {
        public static string[] activeServer;



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
                var checkHealthThread = new Thread(parameterizedThread);
                checkHealthThread.Start(serverAddressAndPort);
            }

        }

        public static void PingServer(object o)
        {
            var serverAddressAndPort = (string[]) o;

            var serverAddress = serverAddressAndPort[0];
            var serverPort = Convert.ToInt32(serverAddressAndPort[1]);

            var destServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var testHeader = String.Format("GET http://{0}:{1}/ HTTP/1.1", serverAddress, serverPort);
            var testHeaderBytes = Encoding.ASCII.GetBytes(testHeader);
            destServerSocket.Connect(serverAddress, serverPort);
            destServerSocket.Send(testHeaderBytes);
            
            var responseBuffer = 

            while (destServerSocket.Receive(responseBuffer) != 0)
            {
                //Console.Write(ASCIIEncoding.ASCII.GetString(responseBuffer));
                _client.Send(responseBuffer);
                Debug.Write(Encoding.ASCII.GetString(responseBuffer));
            }
        }
    }
}
