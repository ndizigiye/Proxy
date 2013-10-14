using System;
using System.Windows.Forms;

namespace Proxy
{
    public partial class Form1 : Form
    {
        private readonly HttpProxy _hp;
        delegate void SetTextCallBack(string e);

        public Form1()
        {
            InitializeComponent();
            _hp = new HttpProxy("127.0.0.1", 9000);
            _hp.OnTextChangeEvent += SetTextHandler;
            stopProxy.Enabled = false;
        }

        private void SetText(string text)
        {

            if (monitor.InvokeRequired)
            {
                SetTextCallBack setText = SetText;
                Invoke(setText, new object[] {text});
            }

            else
            {
                monitor.AppendText(text + "\n");
            }
        }

        void SetTextHandler(OnTextChange e)
        {
            SetText(e.GetText());
        }

        private void startProxy_Click(object sender, EventArgs e)
        {
            
            _hp.Start();
            _hp.HandleClient();
            startProxy.Enabled = false;
            stopProxy.Enabled = true;
        }

        private void stopProxy_Click(object sender, EventArgs e)
        {
            _hp.Stop();
            startProxy.Enabled = true;
            stopProxy.Enabled = false;
        }
    }
}
