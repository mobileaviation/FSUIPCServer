using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NLog;
using FSUIPCServerLib;

namespace FSUIPCServerConsole
{
    class Program
    {
        private static Logger log;

        static void Main(string[] args)
        {
            log = LogManager.GetCurrentClassLogger();

            log.Info("Starting FSUIPC Server...");

            Server fsuipcServer = new Server(5000);
            fsuipcServer.Test();

        }
    }
}
