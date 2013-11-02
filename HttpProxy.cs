using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Proxy
{

    public class HttpProxy
    {
        public event OnTextChangeHandler OnTextChangeEvent;

        private TcpListener _server;
        private readonly string _ip;
        private readonly int _poort;
        private Socket _tcpClient;
        private Thread _serverListeningThread;
       

        public HttpProxy(string ip,int poort )
        {
            _ip = ip;
            _poort = poort;
        }

        public void HandleClient()
        {
            _serverListeningThread = new Thread(HandleClientInternal);
            _serverListeningThread.Start();
        }

        public void Start()
        {
         _server = new TcpListener(IPAddress.Parse(_ip),_poort);
         _server.Start();
         OnTextChangeEvent(new OnTextChange("Server started!!!"));
        }

        public void Stop()
        {
         _serverListeningThread.Abort();
         _server.Stop();
         OnTextChangeEvent(new OnTextChange("Server stopped!!!"));
            
        }

        public void HandleClientInternal()
        {
            while (true)
            {
                _tcpClient = _server.AcceptSocket();
                var client = new ClientHandler(_tcpClient, OnTextChangeEvent);
                client.StartHandling();
            }
// ReSharper disable FunctionNeverReturns
        }
// ReSharper restore FunctionNeverReturns
    }
}
