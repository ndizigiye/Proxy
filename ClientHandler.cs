using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Proxy
{
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
        private IPAddress _serverIp;
        private string _host;
        private string _getRequest;

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
            bool recvRequest = true;
            const string requestEnd = "\r\n";

            string requestPayload = "";
            string requestTempLine = "";
            var requestLines = new List<string>();
            var requestBuffer = new byte[1];
            var responseBuffer = new byte[1];
            var responseHeaderFirstPart = "";
            var responseHeaderSecondPart = "";
            var responseLines = new List<string>();
            

            requestLines.Clear();

            try
            {
                //State 0: Handle Request from Client
                while (recvRequest)
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
                        recvRequest = false;
                    }
                }
                OnTextChangeEvent(new OnTextChange("Raw request received"));
                OnTextChangeEvent(new OnTextChange(requestPayload));

                //State 1: Rebuilding Request Information and Create Connection to Destination Server
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
                var portTest = 80 + Algorithm.AlgorithmChooser();
                destServerSocket.Connect("localhost", portTest);

                //State 2: Sending New Request Information to Destination Server and Relay Response to Client            
                destServerSocket.Send(Encoding.ASCII.GetBytes(requestPayload));
                //Console.WriteLine("Begin Receiving Response...");
                while (destServerSocket.Receive(responseBuffer) != 0)
                {
                    var responseHeader = Encoding.ASCII.GetString(responseBuffer);

                    if (responseHeader == "\r")
                    {
                        Debug.WriteLine("-----Ends with----");
                    }
                    var newResponseBuffer = responseBuffer;
                    //newResponseBuffer = Encoding.ASCII.GetBytes(responseHeader);
                    //Debug.Write("=="+responseHeader);
                    _client.Send(newResponseBuffer);
                }
                Debug.WriteLine(responseHeaderFirstPart + responseHeaderSecondPart);
                Debug.WriteLine("finished============================================================");
                destServerSocket.Disconnect(false);
                destServerSocket.Dispose();
                _client.Disconnect(false);
                _client.Dispose();
            }
            catch (Exception e)
            {
                OnTextChangeEvent(new OnTextChange("Error occured: " + e.Message));
                OnTextChangeEvent(new OnTextChange("Stack Trace: " + e.StackTrace));
            }
        }

        public void HandleClient()
        {
            var request = "";

            while (true)
            {
                var buffer = new byte[1];
                _client.Receive(buffer);
                request += Encoding.ASCII.GetString(buffer);
                if (request.IndexOf("\r\n\r\n", StringComparison.Ordinal) > -1 ||
                    request.IndexOf("\n\n", StringComparison.Ordinal) > -1)
                { 
                    break;
                }
            }
            GetServerIp(request);
            OnTextChangeEvent(
            new OnTextChange(String.Format("Request header received to {0}", _getRequest)));
            HandleRequestInternal(request);
        }

        private void HandleRequest(string request)
        {
            var parameterizedThread = new ParameterizedThreadStart(HandleRequestInternal);
            var handleRequestThread = new Thread(parameterizedThread);
            handleRequestThread.Start(request);
        }

        private void HandleRequestInternal(object o)
        {
            var request = o as String;
            var proxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            proxy.Connect(_serverIp, 80);
            var receivedData = "";
            proxy.Connect(_serverIp, 80);
            var buffer = new byte[3000];
            if (request != null)
            {
                var headerBuffer = Encoding.ASCII.GetBytes(request);
                proxy.Send(headerBuffer);
            }

            while (proxy.Receive(buffer) != 0)
            {
                proxy.Receive(buffer);
                receivedData += Encoding.ASCII.GetString(buffer);
                if (_client.Send(buffer) == 0)
                {
                    break;
                }
               
            }
            OnTextChangeEvent(new OnTextChange("data received from server!!:"+ receivedData));
            proxy.Disconnect(false);
            proxy.Dispose();
            _client.Disconnect(false);
            _client.Dispose();
            OnTextChangeEvent(new OnTextChange("finished sending data to client"));
        }

        private void GetServerIp(string request)
        {
            string[] lines = request.Split('\n');
            _getRequest = lines[0];
            foreach (var elem in lines)
            {
                var elemLines = elem.Split(':');
                if (elemLines[0].ToLower().Trim().Equals("host"))
                {
                    _host = elemLines[1].Trim();
                    //host = host.Replace("www", "");
                }
            }
            _serverIp = Dns.GetHostAddresses(_host)[0];
        }
    }

}
