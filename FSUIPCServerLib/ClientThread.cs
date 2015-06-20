using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using NLog;
using System.Xml.Linq;
using FSUIPC;
using System.Text.RegularExpressions;

namespace FSUIPCServerLib
{
    public class ClientThread
    {
        private TcpClient tcpClient;
        private Logger log;

        public ClientThread(TcpClient client)
        {
            log = LogManager.GetCurrentClassLogger();
            this.tcpClient = client;
            m_offsets = new List<Offset_C>();
        }

        public void DoWork()
        {
            NetworkStream clientStream = tcpClient.GetStream();
            log.Info("Connection from {0} accepted", tcpClient.ToString());

            byte[] message = new byte[4096];
            int bytesRead;


            while (true)
            {
                bytesRead = 0;

                try
                {
                    //log.Info("Reading from stream");
                    bytesRead = clientStream.Read(message, 0, 4096);
                    //log.Info("Read {0} bytes from stream", bytesRead);
                }
                catch (Exception ex)
                {
                    log.Info("Exception in reading from stream: {0} ", ex.Message);
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                ASCIIEncoding encoder = new ASCIIEncoding();
                String rec = encoder.GetString(message, 0, bytesRead);
                //log.Info("Read data from stream: {0}", rec);

                String ret = ProcessMessage(rec);

                byte[] buffer = encoder.GetBytes(ret + Environment.NewLine);
                clientStream.Write(buffer, 0, buffer.Length);
                clientStream.Flush();

                //Thread a = new Thread(() => ProcessSendReturnThread(message, bytesRead, clientStream));
                //a.Start();

                log.Info("Send string return to client");
            }

            tcpClient.Close();
            log.Info("Connection with client has closed");
        }

        private void ProcessSendReturnThread(byte[] message, int size, NetworkStream stream)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            String rec = encoder.GetString(message, 0, size);

            String ret = ProcessMessage(rec);

            byte[] buffer = encoder.GetBytes(ret + Environment.NewLine);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        private List<Offset_C> m_offsets;

        private string ProcessMessage(String message)
        {
            string[] messages = Regex.Split(message, "</root>");
            string retMessage = "<root><UnknownCommand/></root>";

            if (messages.Length>0)
            {
                string s = messages[0];
                if (s.Length > 2)
                {
                    string s1 = s + "</root>";
                    try
                    {
                        XDocument doc = XDocument.Parse(s1);


                        if (doc.Root.Element("Connect") != null)
                        {
                            log.Info("Connect command found");
                            retMessage = Connect();
                        }

                        if (doc.Root.Element("FSUIPC") != null)
                        {
                            string cmd = ReadCommand(doc.Root.Element("FSUIPC"));

                            if (cmd == "Open")
                            {
                                retMessage = Open();
                            }

                            if (cmd == "Close")
                            {
                                retMessage = Close();
                            }

                            if (cmd == "ReadOffset")
                            {
                                retMessage = ReadOffsets(doc.Root.Element("FSUIPC"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Reading XML exception: {0}", ex.Message);
                    }
                }
            }

            return retMessage;
        }

        private string ReadCommand(XElement element)
        {
            if (element.Attribute("Command") != null)
            {
                return element.Attribute("Command").Value.ToString();
            }
            else
            {
                return "";
            }
        }

        private string ReadOffsets(XElement offsets)
        {
            string retMessage = "";
            string datagroup = "";

            foreach(XElement offs in offsets.Elements("Offsets").Elements("Offset"))
            {
                log.Info("Offset: {0}, Datagroup: {1}, DataType: {2}",
                    offs.Attribute("Address").Value,
                    offs.Attribute("Datagroup").Value,
                    offs.Attribute("Datatype").Value);

                int addr = Convert.ToInt32(offs.Attribute("Address").Value);
                string dt = offs.Attribute("Datatype").Value;
                var a = from aa  in m_offsets
                        where aa.Address == addr
                        select aa;

                if (a.Count() == 0)
                {
                    Datatype d = Datatype.Int32;
                    if (dt == "Byte") d = Datatype.Byte;
                    if (dt == "Int16") d = Datatype.Int16;
                    if (dt == "Int32") d = Datatype.Int32;
                    if (dt == "Int64") d = Datatype.Int64;
                    if (dt == "Single") d = Datatype.Single;
                    if (dt == "Double") d = Datatype.Double;
                    if (dt == "String") d = Datatype.String;
                    if (dt == "ByteArray") d = Datatype.ByteArray;
                    if (dt == "BitArray") d = Datatype.BitArray;

                    Offset_C offset = new Offset_C(Convert.ToInt32(offs.Attribute("Address").Value),
                        d, offs.Attribute("Datagroup").Value.ToString());
                    m_offsets.Add(offset);
                    datagroup = offs.Attribute("Datagroup").Value.ToString();
                }
                else
                {
                    Offset_C offset = a.First();
                    datagroup = offset.Datagroup;
                }

            }
            FSUIPCConnection.Process(datagroup);

            XDocument r = new XDocument();
            r.AddFirst(new XElement("root"));
            XElement or = new XElement("OffsetsRead");
            r.Root.Add(or);

            foreach (Offset_C of in m_offsets)
            {
                XElement o = new XElement("Offset");
                o.Value = of.Value.ToString();
                if (of.DataType == Datatype.Double) o.Value = o.Value.Replace(",", ".");
                o.SetAttributeValue("Address", of.Address);
                or.Add(o);
            }

            retMessage = r.ToString(SaveOptions.DisableFormatting);
            log.Info("OffsetsXML: {0}", retMessage);

            return retMessage;
        }

        private string Connect()
        {
            return "<root><Connected/></root>"; ;
        }

        private string Open()
        {
            string retMessage = "";
            try
            {
                FSUIPCConnection.Open(FlightSim.Any);
                log.Info("Connection to Flightsim opened");
                retMessage = "<root><FSUIPC_Opened/></root>";
            }
            catch (FSUIPCException fsE)
            {
                log.Error("Exception FSUIPC connection: {0}", fsE.Message);
                if(fsE.FSUIPCErrorCode != FSUIPCError.FSUIPC_ERR_OPEN)
                    retMessage = "<root>" +
                        "<ConnectError>" + fsE.Message + "</ConnectError>"
                        + "</root>";
                else
                    retMessage = "<root><FSUIPC_Opened/></root>";
            }
            return retMessage;
        }

        private string Close()
        {
            string retMessage = "";
            try
            {
                FSUIPCConnection.Close();
                log.Info("Connection to FSX is closed");
                retMessage = "<root><FSUIPC_Closed/></root>";
            }
            catch (FSUIPCException fsE)
            {
                log.Error("Exception FSUIPC connection: {0}", fsE.Message);
                retMessage = "<root>" +
                    "<ConnectError>" + fsE.Message + "</ConnectError>"
                    + "</root>";
            }

            return retMessage;
        }
    }
}
