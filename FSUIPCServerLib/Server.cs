using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using NLog;
using FSUIPC;

namespace FSUIPCServerLib
{
    public class Server
    {
        private TcpListener tcpListener;
        private Thread listenerThread;
        private Logger log;
        private int connectionCount = 0;
        private bool running;

        public Server(int port)
        {
            log = LogManager.GetCurrentClassLogger();
            running = true;
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenerThread = new Thread(new ThreadStart(ListenForClients));
            this.listenerThread.Start();
            log.Info("Starting listener");
        }

        public void Stop()
        {
            running = false;
            this.tcpListener.Stop();
        }
        
        public void ListenForClients()
        {
            this.tcpListener.Start();
            while (running)
            {
                try
                {
                    log.Info("waiting for connections: {0}", connectionCount);
                    TcpClient client = this.tcpListener.AcceptTcpClient();
                    connectionCount++;
                    ClientThread ct = new ClientThread(client);
                    Thread clientThread = new Thread(ct.DoWork);
                    clientThread.Start();
                }
                catch (Exception ee)
                {
                    log.Error("waiting for connections Exception: {0}", ee.Message);
                }
            }
        }

        public void Test()
        {
            try
            {
                FSUIPCConnection.Open();
                Offset<string> test = new Offset<string>("test", 12640, 24);
                FSUIPCConnection.Process("test");
                log.Info("Test: {0} {1}", test.Value, test.Address);
                FSUIPCConnection.Close();
            }
            catch (Exception ee)
            {
                log.Error("Test FSUIPC Error: " + ee.Message);
            }

        }
    }

    
}
