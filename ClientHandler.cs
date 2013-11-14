using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Proxy
{//
    public delegate void OnTextChangeHandler(OnTextChange e);

    public class OnTextChange : EventArgs
    {
        private readonly string _text;


        public OnTextChange(string text)
        {
            _text = text;
        }

        public string GetText()
        {
            return _text;
        }
    }

    class ClientHandler
    {
        public event OnTextChangeHandler OnTextChangeEvent;
        private readonly Socket _client;
        public  Thread HandleClientThread;
        //private IPAddress _serverIp;
        //private string _host;
        //private string _getRequest;

        public ClientHandler(Socket client, OnTextChangeHandler onTextChangeEvent)
        {
            _client = client;
            OnTextChangeEvent = onTextChangeEvent;
        }

        public void StartHandling()
        {
            HandleClientThread = new Thread(Handle) {Priority = ThreadPriority.AboveNormal};
            HandleClientThread.Start();
        }

        public void Handle()
        {
            bool continueReceiving = true;
            const string requestEnd = "\r\n";
            var clientIPAddress  = _client.RemoteEndPoint.ToString();
            clientIPAddress = clientIPAddress.Split(':')[0].Trim();


            string requestPayload = "";
            string requestTempLine = "";
            var requestLines = new List<string>();
            var requestBuffer = new byte[1];
            var responseBuffer = new byte[1];
            var destIp = "";
            var destPort = 0;
            

            requestLines.Clear();

            try
            {
                //State 0: Handle Request from Client
                while (continueReceiving)
                {
                    _client.Receive(requestBuffer);
                    string fromByte = Encoding.ASCII.GetString(requestBuffer);
                    requestPayload += fromByte;
                    requestTempLine += fromByte;

                    if (requestTempLine.EndsWith(requestEnd))
                    {
                        requestLines.Add(requestTempLine.Trim());
                        requestTempLine = "";
                    }

                    if (requestPayload.EndsWith(requestEnd + requestEnd))
                    {
                        continueReceiving = false;
                    }
                }
                OnTextChangeEvent(new OnTextChange("Raw request received"));
                OnTextChangeEvent(new OnTextChange(requestPayload));
                string remoteHost = requestLines[0].Split(' ')[1].Replace("http://", "").Split('/')[0];
                int remotePort = 80;
                if(remoteHost.Split(':')[0].Equals("localhost")){
                    remotePort = Convert.ToInt32(remoteHost.Split(':')[1]);
                    remoteHost = "localhost";
                }

                requestPayload = "";
                foreach (string line in requestLines)
                {
                    requestPayload += line;
                    requestPayload += requestEnd;
                }

                var destServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                int NumberOfAvailableServers = HealthChecker.activeServers.Count;
                var hash = Algorithm.AlgorithmChooser(NumberOfAvailableServers) - 1;
                var server = HealthChecker.activeServers[hash];
                destPort = server.port;
                destIp = server.address;

                if (Session.saveSession)
                {
                    if (!Session.sessionTable.ContainsKey(clientIPAddress))
                    {
                        Session.sessionTable.Add(clientIPAddress, destIp + ":" + destPort);
                    }
                    else if (Session.sessionTable.ContainsKey(clientIPAddress))
                    {
                        var destIpAndPort = Session.sessionTable[clientIPAddress];
                        var destIpAndPortArray = destIpAndPort.Split(':');
                        destIp = destIpAndPortArray[0];
                        destPort = Convert.ToInt32(destIpAndPortArray[1]);
                    }
                }
                destServerSocket.Connect(destIp, destPort);
          
                destServerSocket.Send(Encoding.ASCII.GetBytes(requestPayload));

                while (destServerSocket.Receive(responseBuffer) != 0)
                {
                    if (_client.Send(responseBuffer) == 0) break;
                }

                destServerSocket.Shutdown(SocketShutdown.Both);
                destServerSocket.Close();
                _client.Shutdown(SocketShutdown.Both);
                _client.Close();
            }
            catch(SocketException e){
                Debug.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                OnTextChangeEvent(new OnTextChange("Error occured: " + e.Message));
                OnTextChangeEvent(new OnTextChange("Stack Trace: " + e.StackTrace));
            }
        }
    }

}
