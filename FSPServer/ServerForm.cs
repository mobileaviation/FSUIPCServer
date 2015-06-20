using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NLog;
using FSUIPCServerLib;

namespace FSPServer
{
    public partial class ServerForm : Form
    {
        Logger log;
        Server fsuipcServer;

        public ServerForm()
        {
            InitializeComponent();

            log = LogManager.GetCurrentClassLogger();

            log.Info("Starting FSUIPC Server...");

            fsuipcServer = new Server(5000);
            fsuipcServer.Test();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            log.Info("Stopping FSUIPC server....");

            fsuipcServer.Stop();

            this.Close();
        }
    }
}
