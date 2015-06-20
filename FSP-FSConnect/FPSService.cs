using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NLog;
using FSUIPCServerLib;

namespace FSP_FSConnect
{
    public partial class FPSService : ServiceBase
    {
        Logger log;
        Server fsuipcServer;

        public FPSService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log = LogManager.GetCurrentClassLogger();

            log.Info("Starting FSUIPC Server...");

            fsuipcServer = new Server(5000);
            fsuipcServer.Test();
        }

        protected override void OnStop()
        {
            fsuipcServer.Stop();
        }
    }
}
