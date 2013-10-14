using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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

    public class HttpProxy
    {
        public event OnTextChangeHandler OnTextChangeEvent;

        private TcpListener _server;
        private readonly string _ip;
        private readonly int _poort;
        private Thread _handleClientThread;
        HttpWebRequest _webRequest;
        HttpWebResponse _webResponse;
        private IPAddress _serverIP;
       

        public HttpProxy(string ip,int poort )
        {
            _ip = ip;
            _poort = poort;
        }

        public void Start()
        {
         _server = new TcpListener(IPAddress.Parse(_ip),_poort);
         _server.Start();
         OnTextChangeEvent(new OnTextChange("Server started!!!"));
        }

        public void Stop()
        {
         _handleClientThread.Abort();
         _server.Stop();
         OnTextChangeEvent(new OnTextChange("Server stopped!!!"));
            
        }

        public void HandleClient()
        {
            _handleClientThread = new Thread(HandleClientInternal);
            _handleClientThread.Start();
            _handleClientThread.IsBackground = true;
        }
        public void HandleClientInternal()
        {
            var client = _server.AcceptTcpClient();
            var stream = client.GetStream();
            var buffer = new byte[1024];

            while (stream.CanRead)
            {
                stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.ASCII.GetString(buffer);
                OnTextChangeEvent(new OnTextChange(request));
                GetServerIp(request);
                var proxy = new TcpClient();
                proxy.Connect(_serverIP,80);
                var proxyStream = proxy.GetStream();
                proxyStream.Write(buffer, 0, buffer.Length);
                proxyStream.Read(buffer, 0, buffer.Length);
                while (proxyStream.CanRead)
                {
                    string requestProxy = Encoding.ASCII.GetString(buffer);
                    OnTextChangeEvent(new OnTextChange(requestProxy));
                    stream.Write(buffer, 0, buffer.Length);
                }

            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void GetServerIp(string request)
        {
            string[] lines = request.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string header = lines[0];
            OnTextChangeEvent(new OnTextChange(header));
            var ms = Regex.Matches(header, @"(www.+|http.+)([\s]|$)");
            var uriString = ms[0].Value;
            uriString = uriString.Remove(uriString.IndexOf("HTTP"));
            OnTextChangeEvent(new OnTextChange(uriString));
            var uri = new Uri(uriString);
            var host = uri.Host;
            _serverIP =  Dns.GetHostAddresses(host)[0];
        }
    }
}
