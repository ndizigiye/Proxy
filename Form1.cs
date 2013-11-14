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
            roundRobin.Checked = true;
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

       public void SetTextHandler(OnTextChange e)
        {
            SetText(e.GetText());
        }

        private void startProxy_Click(object sender, EventArgs e)
        {
            
            _hp.Start();
            HealthChecker.checkHealthInternal(servers.Text.Split(','));
            _hp.HandleClient();
            startProxy.Enabled = false;
            stopProxy.Enabled = true;
            servers.Enabled = false;
        }

        private void stopProxy_Click(object sender, EventArgs e)
        {
            _hp.Stop();
            startProxy.Enabled = true;
            stopProxy.Enabled = false;
            servers.Enabled = true;
        }

        private void saveSession_CheckedChanged(object sender, EventArgs e)
        {
            Session.saveSession = saveSession.Checked;
        }

        private void roundRobin_CheckedChanged(object sender, EventArgs e)
        {
            Algorithm.algorithmName = "roundRobin";
        }

        private void random_CheckedChanged(object sender, EventArgs e)
        {
            Algorithm.algorithmName = "random";
        }

    }
}
