using Gb28181_Client.Message;
using Gb28181_Client.WayControls.IIS;
using log4net;
using SIPSorcery.GB28181.Net;
using SIPSorcery.GB28181.Persistence;
using SIPSorcery.GB28181.Servers;
using SIPSorcery.GB28181.Servers.SIPMessage;
using SIPSorcery.GB28181.SIP;
using SIPSorcery.GB28181.SIP.App;
using SIPSorcery.GB28181.Sys;
using SIPSorcery.GB28181.Sys.Config;
using SIPSorcery.GB28181.Sys.XML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.Net.Sockets;
namespace Gb28181_Client
{
    public delegate void SetSIPServiceText(string msg, SipServiceStatus state);
    public delegate void SetCatalogText(Catalog cata);
    public delegate void SetRecordText(RecordInfo record);

    public partial class Form1 : Form
    {
        private static ILog logger = AppState.logger;

        private SIPMessageDaemon _messageDaemon;

        event Action<Packet> OnPacketReady;

        public Form1()
        {
            InitializeComponent();
            this.OnPacketReady += Form1_OnPacketReady;
        }





        private void Form1_OnPacketReady(Packet packet)
        {
            logger.Debug(packet.TimeStamp + "\t" + packet.SeqNumber + "\t" + packet.Length);
        }

        private void Initialize()
        {
            //lvDev.Items.Clear();
            //ListViewItem lvItem = new ListViewItem(new string[] { "22", "微创球机7", "34020000001320000020" });
            //lvItem.ImageKey = "34020000001320000020";
            //lvDev.Items.Add(lvItem);

            SIPSqlite.Instance.Read();
            var account = SIPSqlite.Instance.Accounts.FirstOrDefault();
            if (account == null)
            {
                logger.Error("Account Config NULL SIP not started");
                return;
            }
            Dictionary<string, PlatformConfig> platformList = new Dictionary<string, PlatformConfig>();
            //PlatformConfig config = new PlatformConfig()
            //{
            //    ChannelName = "微创球机7",
            //    RemoteIP = "192.168.10.220",
            //    RemotePort = 5060
            //};
            //platformList.Add("34020000001320000020",config);
            _messageDaemon = new SIPMessageDaemon(account, SIPRequestAuthenticator.AuthenticateSIPRequest, platformList);
        }

        private void btnStart_Click(object sender, System.EventArgs e)
        {
            Initialize();
            _messageDaemon.Start();
            _messageDaemon.MessageCore.OnSIPServiceChanged += m_msgCore_SIPInitHandle;
            _messageDaemon.MessageCore.OnCatalogReceived += m_msgCore_OnCatalogReceived;
            _messageDaemon.MessageCore.OnRecordInfoReceived += MessageCore_OnRecordInfoReceived;
            lblStatus.Text = "sip服务已启动。。。";
            lblStatus.ForeColor = Color.FromArgb(0, 192, 0);
        }



        private void btnStop_Click(object sender, EventArgs e)
        {
            _messageDaemon.Stop();
            _messageDaemon.MessageCore.OnSIPServiceChanged -= m_msgCore_SIPInitHandle;
            _messageDaemon.MessageCore.OnCatalogReceived -= m_msgCore_OnCatalogReceived;
            lvDev.Items.Clear();
            lblStatus.Text = "sip服务已停止。。。";
            lblStatus.ForeColor = Color.Blue;
        }

        private void btnCatalog_Click(object sender, EventArgs e)
        {
            _devSN = 1;
            lvDev.Items.Clear();
            _messageDaemon.MessageCore.DeviceCatalogQuery();
        }

        private void m_msgCore_OnCatalogReceived(Catalog cata)
        {
            if (lvDev.InvokeRequired)
            {
                SetCatalogText setCata = new SetCatalogText(SetDevText);
                this.Invoke(setCata, cata);
            }
            else
            {
                SetDevText(cata);
            }
        }

        private void MessageCore_OnRecordInfoReceived(RecordInfo record)
        {
            if (lvRecord.InvokeRequired)
            {
                SetRecordText recordText = new SetRecordText(SetRecord);
                this.Invoke(recordText, record);
            }
            else
            {
                SetRecord(record);
            }
        }

        int _recordSN = 1;
        int _devSN = 1;
        /// <summary>
        /// 设置录像文件
        /// </summary>
        /// <param name="record"></param>
        private void SetRecord(RecordInfo record)
        {
            foreach (var item in record.RecordItems.Items)
            {
                ListViewItem lvItem = new ListViewItem(new string[] { _recordSN.ToString(), record.Name, item.DeviceID, item.StartTime, item.EndTime });
                lvRecord.Items.Add(lvItem);
                _recordSN++;
            }
        }

        /// <summary>
        /// 设置设备目录
        /// </summary>
        /// <param name="cata">设备目录</param>
        private void SetDevText(Catalog cata)
        {
            foreach (Catalog.Item item in cata.DeviceList.Items)
            {
                ListViewItem lvItem = new ListViewItem(new string[] { _devSN.ToString(), item.Name, item.DeviceID });
                lvItem.ImageKey = item.DeviceID;
                int lvTotal = 0;
                foreach (var v in lvDev.Items.Cast<ListViewItem>())
                {
                    if (v.ImageKey == lvItem.ImageKey)
                    {
                        lvTotal++;
                        break;
                    }
                }
                if (lvTotal > 0)
                {
                    continue;
                }
                _devSN++;
                lvDev.Items.Add(lvItem);
            }
        }

        private void m_msgCore_SIPInitHandle(string msg, SipServiceStatus state)
        {
            if (lblStatus.InvokeRequired)
            {
                SetSIPServiceText sipService = new SetSIPServiceText(SetSIPService);
                this.Invoke(sipService, msg, state);
            }
            else
            {
                SetSIPService(msg, state);
            }
        }

        /// <summary>
        /// 设置sip服务状态
        /// </summary>
        /// <param name="state">sip状态</param>
        private void SetSIPService(string msg, SipServiceStatus state)
        {
            if (state == SipServiceStatus.Wait)
            {
                lblStatus.Text = msg + "-SIP服务未初始化完成";
                lblStatus.ForeColor = Color.YellowGreen;
            }
            else
            {
                lblStatus.Text = msg + "-SIP服务已初始化完成";
                lblStatus.ForeColor = Color.Green;
            }
        }

        private void btnReal_Click(object sender, EventArgs e)
        {
            ListViewItem devItem = new ListViewItem();
            foreach (var item in lvDev.SelectedItems.Cast<ListViewItem>())
            {
                devItem = item;
            }
            string key = devItem.ImageKey + SIPSorcery.GB28181.Sys.XML.CommandType.Play;
            if (!_messageDaemon.MessageCore.MonitorService.ContainsKey(key))
            {
                return;
            }
            _messageDaemon.MessageCore.MonitorService[key].RealVideoReq();
        }

        private void btnBye_Click(object sender, EventArgs e)
        {
            ListViewItem devItem = new ListViewItem();
            foreach (var item in lvDev.SelectedItems.Cast<ListViewItem>())
            {
                devItem = item;
            }
            string key = devItem.ImageKey + SIPSorcery.GB28181.Sys.XML.CommandType.Play;
            if (!_messageDaemon.MessageCore.MonitorService.ContainsKey(key))
            {
                return;
            }
            _messageDaemon.MessageCore.MonitorService[key].ByeVideoReq();
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            ListViewItem devItem = new ListViewItem();
            foreach (var item in lvDev.SelectedItems.Cast<ListViewItem>())
            {
                devItem = item;
            }
            string key = devItem.ImageKey + SIPSorcery.GB28181.Sys.XML.CommandType.Playback;
            if (!_messageDaemon.MessageCore.MonitorService.ContainsKey(key))
            {
                return;
            }

            DateTime startTime = DateTime.Parse(txtStartTime.Text.Trim());
            DateTime stopTime = DateTime.Parse(txtStopTime.Text.Trim());
            _messageDaemon.MessageCore.MonitorService[key].BackVideoReq(startTime, stopTime);
        }

        private void btnStopRecord_Click(object sender, EventArgs e)
        {
            ListViewItem devItem = new ListViewItem();
            foreach (var item in lvDev.SelectedItems.Cast<ListViewItem>())
            {
                devItem = item;
            }
            string key = devItem.ImageKey + SIPSorcery.GB28181.Sys.XML.CommandType.Playback;
            if (!_messageDaemon.MessageCore.MonitorService.ContainsKey(key))
            {
                return;
            }
            _messageDaemon.MessageCore.MonitorService[key].ByeVideoReq();
        }

        List<Packet> _firstPackets = new List<Packet>();
        Queue<Packet> _packets = new Queue<Packet>();
        private void Write(byte[] buffer, int offset, short value)
        {
            unsafe
            {
                fixed (byte* ptr = &buffer[offset])
                {
                    *((short*)ptr) = value;
                }
            }
        }

        private void Write(byte[] buffer, int offset, byte value)
        {
            unsafe
            {
                fixed (byte* ptr = &buffer[offset])
                {
                    *((byte*)ptr) = value;
                }
            }
        }

        private void Write(byte[] buffer, int offset, int value)
        {
            unsafe
            {
                fixed (byte* ptr = &buffer[offset])
                {
                    *((int*)ptr) = value;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DirectoryEntry rootEntry = IISManagement.returnIISWebserver(5);
            // DirectoryEntry rootEntry = new DirectoryEntry("IIS://localhost/w3svc/5/ROOT");
            // 创建虚拟目录
            DirectoryEntry entry = rootEntry.Children.Add("appli", "IIsWebVirtualDir");
            entry.Invoke("AppCreate", true);
            //entry.Path = "D:\\baiduCloud\\那年青春我们正好";
            entry.CommitChanges();
            rootEntry.CommitChanges();
            //newVirDir.CommitChanges();
            //System.DirectoryServices.

            var writer = new BitWriter(48);

            writer.Write(2, 2);  //把12用5bit写入，此时二进制字符串为：01100

            writer.Write(0, 1);  //把8用16bit写入，此时二进制字符串为：011000000000000001000
            writer.Write(0, 5);
            var result = writer.GetData(); //8bit对齐为011000000000000001000000
            //返回结果为[96,0,64]

            byte[] bytes = new byte[48];
            Write(bytes, 0, (byte)0x80);
            Write(bytes, 1, (byte)0xC8);
            Write(bytes, 2, (byte)0x00);
            Write(bytes, 3, (byte)0x06);
            Write(bytes, 4, (ushort)0xFFFF);
            bytes[0] = 0x80;
            bytes[1] = 0xC8;
            bytes[2] = 0x00;
            bytes[3] = 0x06;
            bytes[4] = 0xFF;
            bytes[5] = 0xFF;
            bytes[6] = 0xC5;
            bytes[7] = 0x9C;
            return;
            XmlDocument xmlDoc = new XmlDocument();

            SipServer.Account account = new SipServer.Account()
            {
                id = Guid.NewGuid(),
                sipusername = "34020000001320000001",
                sippassword = "12345678",
                owner = "admin",
                sipdomain = "192.168.10.221:5060",
                localID = "34020000002000000001",
                localSocket = "192.168.10.245:5061"
            };
            List<SipServer.Account> accounts = new List<SipServer.Account>();
            accounts.Add(account);
            SipServer sip = new SipServer()
            {
                Accounts = accounts
            };

            SipServer.Instance.Save<SipServer>(sip);


            string id = Guid.NewGuid().ToString();
            Queue<Packet> packets = new Queue<Packet>();
            FileInfo file = new FileInfo("D:\\test.txt");
            FileStream fStream = file.OpenRead();
            byte[] buffer = new byte[fStream.Length];
            fStream.Read(buffer, 0, buffer.Length);
            string fileText = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            string[] lineText = fileText.Split('\n');
            foreach (var line in lineText)
            {
                if (line.Length == 0)
                {
                    break;
                }
                string rtpInfo = line.Substring(72, line.Length - 72);
                string[] text = rtpInfo.Split('\t');
                if (text.Length == 1)
                {
                    break;
                }
                Packet pack = new Packet()
                {
                    TimeStamp = uint.Parse(text[0]),
                    SeqNumber = int.Parse(text[1]),
                    Length = int.Parse(text[2])
                };
                _packets.Enqueue(pack);
            }
            Thread threadSeq = new Thread(new ThreadStart(ProcessSeqNumber));
            threadSeq.Start();
        }

        private void ProcessSeqNumber()
        {
            while (_packets.Count > 0)
            {
                Packet packet = null;
                lock (_packets)
                {
                    packet = _packets.Dequeue();
                }
                if (packet != null)
                {
                    lock (_firstPackets)
                    {
                        _firstPackets.Add(packet);
                        _firstPackets = _firstPackets.OrderBy(d => d.SeqNumber).ToList();
                    }
                }
                if (_firstPackets.Count > 30)
                {
                    lock (_firstPackets)
                    {
                        Packet pack = _firstPackets.FirstOrDefault();
                        _firstPackets.Remove(pack);
                        OnPacketReady(pack);
                    }
                }
                Thread.Sleep(1);
            }
        }
        private void btnRecordGet_Click(object sender, EventArgs e)
        {

            _recordSN = 1;
            lvRecord.Items.Clear();
            DateTime startTime = DateTime.Parse(txtStartTime.Text.Trim());
            DateTime stopTime = DateTime.Parse(txtStopTime.Text.Trim());
            ListViewItem devItem = new ListViewItem();
            foreach (var item in lvDev.SelectedItems.Cast<ListViewItem>())
            {
                devItem = item;
            }
            string key = devItem.ImageKey + SIPSorcery.GB28181.Sys.XML.CommandType.Playback;
            if (!_messageDaemon.MessageCore.MonitorService.ContainsKey(key))
            {
                return;
            }
            _messageDaemon.MessageCore.MonitorService[key].RecordFileQuery(startTime, stopTime);
        }

        Socket socket;
        bool Stop = false;
        private void button2_Click(object sender, EventArgs e)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            Thread recvThread = new Thread(new ThreadStart(Start)); ;
            recvThread.Start();
            Stop = true;
        }

        public void Start()
        {
            while (Stop)
            {
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = new byte[2048];
                if (socket.Poll(3000, SelectMode.SelectRead))
                {
                    int recvLength = socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEP);
                }
                Thread.Sleep(50);
            }
        }
    }

    public class BitWriter
    {
        private byte[] m_data = null;
        private int m_dataLen = 0;
        private int m_pos = 0;
        private BitArray bit_data = null;

        public BitWriter(int len)
        {
            m_dataLen = len;
            m_data = new byte[len];
            Array.Clear(m_data, 0, len);

            bit_data = new BitArray(len * 8);
        }

        public void Write(int data, int size)
        {
            //将传入数据转换成二进制位  
            int[] value = new int[1] { data };
            BitArray bit_temp = new BitArray(value);

            for (int i = 0; i < size; i++)
            {
                bit_data[m_pos + i] = bit_temp[size - i - 1];
            }

            m_pos += size;
        }

        public byte[] GetData()
        {
            for (int i = 0, y = 0; i < bit_data.Length / 8; i++)
            {
                m_data[i] = 0;

                if (bit_data[y])
                    m_data[i] |= (byte)(1 << 7);

                if (bit_data[y + 1])
                    m_data[i] |= (byte)(1 << 6);

                if (bit_data[y + 2])
                    m_data[i] |= (byte)(1 << 5);

                if (bit_data[y + 3])
                    m_data[i] |= (byte)(1 << 4);

                if (bit_data[y + 4])
                    m_data[i] |= (byte)(1 << 3);

                if (bit_data[y + 5])
                    m_data[i] |= (byte)(1 << 2);

                if (bit_data[y + 6])
                    m_data[i] |= (byte)(1 << 1);

                if (bit_data[y + 7])
                    m_data[i] |= (byte)(1);

                y += 8;
            }

            return m_data;
        }
    }
    public class Packet
    {
        public uint TimeStamp
        {
            get;
            set;
        }

        public int SeqNumber { get; set; }

        public int Length { get; set; }

    }


    namespace WayControls.IIS
    {
        /// <summary>
        /// IISWebServer的状态
        /// </summary>
        public enum IISServerState
        {
            /// <summary>
            ///
            /// </summary>
            Starting = 1,
            /// <summary>
            ///
            /// </summary>
            Started = 2,
            /// <summary>
            ///
            /// </summary>
            Stopping = 3,
            /// <summary>
            ///
            /// </summary>
            Stopped = 4,
            /// <summary>
            ///
            /// </summary>
            Pausing = 5,
            /// <summary>
            ///
            /// </summary>
            Paused = 6,
            /// <summary>
            ///
            /// </summary>
            Continuing = 7

        }
        /// <summary>
        /// IISManagement 的摘要说明。
        /// </summary>
        public class IISManagement
        {
            /// <summary>
            ///
            /// </summary>
            public IISWebServerCollection WebServers = new IISWebServerCollection();
            internal static string Machinename = "localhost";
            /// <summary>
            ///
            /// </summary>
            public IISManagement()
            {
                start();
            }


            /// <summary>
            ///
            /// </summary>
            /// <param name="MachineName">机器名,默认值为localhost</param>
            public IISManagement(string MachineName)
            {
                if (MachineName.ToString() != "")
                    Machinename = MachineName;
                start();
            }
            private void start()
            {

                DirectoryEntry Service = new DirectoryEntry("IIS://" + Machinename + "/W3SVC");
                DirectoryEntry Server;
                DirectoryEntry Root = null;
                DirectoryEntry VirDir;
                IEnumerator ie = Service.Children.GetEnumerator();
                IEnumerator ieRoot;
                IISWebServer item;
                IISWebVirtualDir item_virdir;
                bool finded = false;
                while (ie.MoveNext())
                {
                    Server = (DirectoryEntry)ie.Current;
                    if (Server.SchemaClassName == "IIsWebServer")
                    {
                        item = new IISWebServer();
                        item.index = Convert.ToInt32(Server.Name);

                        item.ServerComment = (string)Server.Properties["ServerComment"][0];
                        item.AccessRead = (bool)Server.Properties["AccessRead"][0];
                        item.AccessScript = (bool)Server.Properties["AccessScript"][0];
                        item.DefaultDoc = (string)Server.Properties["DefaultDoc"][0];
                        item.EnableDefaultDoc = (bool)Server.Properties["EnableDefaultDoc"][0];
                        item.EnableDirBrowsing = (bool)Server.Properties["EnableDirBrowsing"][0];
                        ieRoot = Server.Children.GetEnumerator();

                        while (ieRoot.MoveNext())
                        {
                            Root = (DirectoryEntry)ieRoot.Current;
                            if (Root.SchemaClassName == "IIsWebVirtualDir")
                            {
                                finded = true;
                                break;
                            }
                        }
                        if (finded)
                        {
                            item.Path = Root.Properties["path"][0].ToString();
                        }

                        item.Port = Convert.ToInt32(((string)Server.Properties["Serverbindings"][0]).Substring(1, ((string)Server.Properties["Serverbindings"][0]).Length - 2));
                        this.WebServers.Add_(item);
                        ieRoot = Root.Children.GetEnumerator();
                        while (ieRoot.MoveNext())
                        {
                            VirDir = (DirectoryEntry)ieRoot.Current;
                            if (VirDir.SchemaClassName != "IIsWebVirtualDir" && VirDir.SchemaClassName != "IIsWebDirectory")
                                continue;
                            item_virdir = new IISWebVirtualDir(item.ServerComment);
                            item_virdir.Name = VirDir.Name;
                            item_virdir.AccessRead = (bool)VirDir.Properties["AccessRead"][0];
                            item_virdir.AccessScript = (bool)VirDir.Properties["AccessScript"][0];
                            item_virdir.DefaultDoc = (string)VirDir.Properties["DefaultDoc"][0];
                            item_virdir.EnableDefaultDoc = (bool)VirDir.Properties["EnableDefaultDoc"][0];
                            if (VirDir.SchemaClassName == "IIsWebVirtualDir")
                            {
                                item_virdir.Path = (string)VirDir.Properties["Path"][0];
                            }
                            else if (VirDir.SchemaClassName == "IIsWebDirectory")
                            {
                                item_virdir.Path = Root.Properties["Path"][0] + "\\" + VirDir.Name;
                            }
                            item.WebVirtualDirs.Add_(item_virdir);
                        }
                    }
                }
            }

            /// <summary>
            /// 创建站点
            /// </summary>
            /// <param name="iisServer"></param>
            public static void CreateIISWebServer(IISWebServer iisServer)
            {
                if (iisServer.ServerComment.ToString() == "")
                    throw (new Exception("IISWebServer的ServerComment不能为空!"));
                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server;
                int i = 0;
                IEnumerator ie = Service.Children.GetEnumerator();

                while (ie.MoveNext())
                {
                    Server = (DirectoryEntry)ie.Current;
                    if (Server.SchemaClassName == "IIsWebServer")
                    {
                        if (Convert.ToInt32(Server.Name) > i)
                            i = Convert.ToInt32(Server.Name);
                        //     if( Server.Properties["Serverbindings"][0].ToString() == ":" + iisServer.Port + ":" ) 
                        //     {
                        //      Server.Invoke("stop",new object[0]);
                        //     }
                    }
                }

                i++;

                try
                {
                    iisServer.index = i;
                    Server = Service.Children.Add(i.ToString(), "IIsWebServer");
                    Server.Properties["ServerComment"][0] = iisServer.ServerComment;
                    Server.Properties["Serverbindings"].Add(":" + iisServer.Port + ":");
                    Server.Properties["AccessScript"][0] = iisServer.AccessScript;
                    Server.Properties["AccessRead"][0] = iisServer.AccessRead;
                    Server.Properties["EnableDirBrowsing"][0] = iisServer.EnableDirBrowsing;
                    Server.Properties["DefaultDoc"][0] = iisServer.DefaultDoc;
                    Server.Properties["EnableDefaultDoc"][0] = iisServer.EnableDefaultDoc;

                    DirectoryEntry root = Server.Children.Add("Root", "IIsWebVirtualDir");
                    root.Properties["path"][0] = iisServer.Path;


                    Service.CommitChanges();
                    Server.CommitChanges();
                    root.CommitChanges();
                    root.Invoke("AppCreate2", new object[1] { 2 });
                    //Server.Invoke("start",new object[0]);
                }
                catch (Exception es)
                {
                    throw (es);
                }
            }
            /// <summary>
            /// 删除IISWebServer
            /// </summary>
            public static void RemoveIISWebServer(string ServerComment)
            {
                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server;
                IEnumerator ie = Service.Children.GetEnumerator();

                ServerComment = ServerComment.ToLower();
                while (ie.MoveNext())
                {
                    Server = (DirectoryEntry)ie.Current;
                    if (Server.SchemaClassName == "IIsWebServer")
                    {
                        if (Server.Properties["ServerComment"][0].ToString().ToLower() == ServerComment)
                        {
                            Service.Children.Remove(Server);
                            Service.CommitChanges();
                            return;
                        }
                    }
                }
            }

            /// <summary>
            /// 删除IISWebServer
            /// </summary>
            public static void RemoveIISWebServer(int index)
            {
                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                try
                {
                    DirectoryEntry Server = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC/" + index);
                    if (Server != null)
                    {
                        Service.Children.Remove(Server);
                        Service.CommitChanges();
                    }
                    else
                    {
                        throw (new Exception("找不到此IISWebServer"));
                    }
                }
                catch
                {
                    throw (new Exception("找不到此IISWebServer"));
                }
            }

            /// <summary>
            /// 检查是否存在IISWebServer
            /// </summary>
            /// <param name="ServerComment">网站说明</param>
            /// <returns></returns>
            public static bool ExistsIISWebServer(string ServerComment)
            {
                ServerComment = ServerComment.Trim();
                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server = null;
                IEnumerator ie = Service.Children.GetEnumerator();

                string comment;
                while (ie.MoveNext())
                {
                    Server = (DirectoryEntry)ie.Current;
                    if (Server.SchemaClassName == "IIsWebServer")
                    {
                        comment = Server.Properties["ServerComment"][0].ToString().ToLower().Trim();
                        if (comment == ServerComment.ToLower())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            /// <summary>
            /// 返回指定的IISWebServer
            /// </summary>
            /// <param name="ServerComment"></param>
            /// <returns></returns>
            internal static DirectoryEntry returnIISWebserver(string ServerComment)
            {
                ServerComment = ServerComment.Trim();
                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server = null;
                IEnumerator ie = Service.Children.GetEnumerator();

                string comment;
                while (ie.MoveNext())
                {
                    Server = (DirectoryEntry)ie.Current;
                    if (Server.SchemaClassName == "IIsWebServer")
                    {
                        comment = Server.Properties["ServerComment"][0].ToString().ToLower().Trim();
                        if (comment == ServerComment.ToLower())
                        {
                            return Server;
                        }
                    }
                }

                return null;
            }


            /// <summary>
            ///  返回指定的IISWebServer
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            internal static DirectoryEntry returnIISWebserver(int index)
            {
                DirectoryEntry Server = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC/" + index+"/ROOT");
                try
                {
                    IEnumerator ie = Server.Children.GetEnumerator();
                    return Server;
                }
                catch
                {
                    return null;
                }
            }

            private static DirectoryEntry getRoot(DirectoryEntry Server)
            {
                foreach (DirectoryEntry child in Server.Children)
                {
                    string name = child.Name.ToLower();
                    if (name == "iiswebvirtualdir" || name == "root")
                    {
                        return child;
                    }
                }
                return null;
            }

            /// <summary>
            /// 修改与给定的IISWebServer具有相同网站说明的站点配置
            /// </summary>
            /// <param name="iisServer">给定的IISWebServer</param>
            public static void EditIISWebServer(IISWebServer iisServer)
            {
                if (iisServer.index == -1)
                    throw (new Exception("找不到给定的站点!"));
                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server;

                IEnumerator ie = Service.Children.GetEnumerator();

                while (ie.MoveNext())
                {
                    Server = (DirectoryEntry)ie.Current;
                    if (Server.SchemaClassName == "IIsWebServer")
                    {
                        if (Server.Properties["Serverbindings"][0].ToString() == ":" + iisServer.Port + ":")
                        {
                            Server.Invoke("stop", new object[0]);
                        }
                    }
                }

                Server = returnIISWebserver(iisServer.index);
                if (Server == null)
                {
                    throw (new Exception("找不到给定的站点!"));
                }

                try
                {

                    Server.Properties["ServerComment"][0] = iisServer.ServerComment;
                    Server.Properties["Serverbindings"][0] = ":" + iisServer.Port + ":";
                    Server.Properties["AccessScript"][0] = iisServer.AccessScript;
                    Server.Properties["AccessRead"][0] = iisServer.AccessRead;
                    Server.Properties["EnableDirBrowsing"][0] = iisServer.EnableDirBrowsing;
                    Server.Properties["DefaultDoc"][0] = iisServer.DefaultDoc;
                    Server.Properties["EnableDefaultDoc"][0] = iisServer.EnableDefaultDoc;

                    DirectoryEntry root = getRoot(Server);

                    Server.CommitChanges();
                    if (root != null)
                    {
                        root.Properties["path"][0] = iisServer.Path;
                        root.CommitChanges();
                    }

                    Server.Invoke("start", new object[0]);
                }
                catch (Exception es)
                {
                    throw (es);
                }
            }

            /// <summary>
            /// 返回所有站点的网站说明
            /// </summary>
            /// <returns></returns>
            public static ArrayList returnIISServerComment()
            {
                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server;

                ArrayList list = new ArrayList();
                IEnumerator ie = Service.Children.GetEnumerator();

                while (ie.MoveNext())
                {
                    Server = (DirectoryEntry)ie.Current;
                    if (Server.SchemaClassName == "IIsWebServer")
                    {
                        list.Add(Server.Properties["ServerComment"][0]);
                    }
                }

                return list;
            }

            /// <summary>
            /// 创建虚拟目录
            /// </summary>
            /// <param name="iisVir"></param>
            /// <param name="deleteIfExist"></param>
            public static void CreateIISWebVirtualDir(IISWebVirtualDir iisVir, bool deleteIfExist)
            {
                if (iisVir.Parent == null)
                    throw (new Exception("IISWebVirtualDir没有所属的IISWebServer!"));

                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server = returnIISWebserver(iisVir.Parent.index);

                if (Server == null)
                {
                    throw (new Exception("找不到给定的站点!"));
                }

                Server = getRoot(Server);
                if (deleteIfExist)
                {
                    foreach (DirectoryEntry VirDir in Server.Children)
                    {
                        if (VirDir.Name.ToLower().Trim() == iisVir.Name.ToLower())
                        {
                            Server.Children.Remove(VirDir);
                            Server.CommitChanges();
                            break;
                        }
                    }
                }

                try
                {
                    DirectoryEntry vir;
                    vir = Server.Children.Add(iisVir.Name, "IIsWebVirtualDir");
                    vir.Properties["Path"][0] = iisVir.Path;
                    vir.Properties["DefaultDoc"][0] = iisVir.DefaultDoc;
                    vir.Properties["EnableDefaultDoc"][0] = iisVir.EnableDefaultDoc;
                    vir.Properties["AccessScript"][0] = iisVir.AccessScript;
                    vir.Properties["AccessRead"][0] = iisVir.AccessRead;
                    vir.Invoke("AppCreate2", new object[1] { 2 });

                    Server.CommitChanges();
                    vir.CommitChanges();

                }
                catch (Exception es)
                {
                    throw (es);
                }

            }

            /// <summary>
            /// 删除虚拟目录
            /// </summary>
            /// <param name="WebServerComment">站点说明</param>
            /// <param name="VirtualDir">虚拟目录名称</param>
            public static void RemoveIISWebVirtualDir(string WebServerComment, string VirtualDir)
            {
                VirtualDir = VirtualDir.ToLower();
                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server = returnIISWebserver(WebServerComment);

                if (Server == null)
                {
                    throw (new Exception("找不到给定的站点!"));
                }

                Server = getRoot(Server);
                foreach (DirectoryEntry VirDir in Server.Children)
                {
                    if (VirDir.Name.ToLower().Trim() == VirtualDir)
                    {
                        Server.Children.Remove(VirDir);
                        Server.CommitChanges();
                        return;
                    }
                }

                throw (new Exception("找不到给定的虚拟目录!"));
            }
            /// <summary>
            /// 删除虚拟目录
            /// </summary>
            /// <param name="iisVir"></param>
            public static void RemoveIISWebVirtualDir(IISWebVirtualDir iisVir)
            {
                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server = returnIISWebserver(iisVir.Parent.index);

                if (Server == null)
                {
                    throw (new Exception("找不到给定的站点!"));
                }

                Server = getRoot(Server);
                foreach (DirectoryEntry VirDir in Server.Children)
                {
                    if (VirDir.Name.ToLower().Trim() == iisVir.Name.ToLower())
                    {
                        Server.Children.Remove(VirDir);
                        Server.CommitChanges();
                        return;
                    }
                }

                throw (new Exception("找不到给定的虚拟目录!"));
            }

        }
        /// <summary>
        ///
        /// </summary>
        public class IISWebServerCollection : CollectionBase
        {

            /// <summary>
            ///
            /// </summary>
            public IISWebServer this[int Index]
            {
                get
                {
                    return (IISWebServer)this.List[Index];

                }
            }

            /// <summary>
            ///
            /// </summary>
            public IISWebServer this[string ServerComment]
            {
                get
                {
                    ServerComment = ServerComment.ToLower().Trim();
                    IISWebServer list;
                    for (int i = 0; i < this.List.Count; i++)
                    {
                        list = (IISWebServer)this.List[i];
                        if (list.ServerComment.ToLower().Trim() == ServerComment)
                            return list;
                    }
                    return null;
                }
            }

            internal void Add_(IISWebServer WebServer)
            {
                this.List.Add(WebServer);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="WebServer"></param>
            public void Add(IISWebServer WebServer)
            {
                try
                {
                    this.List.Add(WebServer);
                    IISManagement.CreateIISWebServer(WebServer);
                }
                catch
                {
                    throw (new Exception("发生意外错误，可能是某节点将该节点的上级节点作为它自己的子级插入"));
                }

            }

            /// <summary>
            /// 是否包含指定的网站
            /// </summary>
            /// <param name="ServerComment"></param>
            /// <returns></returns>
            public bool Contains(string ServerComment)
            {
                ServerComment = ServerComment.ToLower().Trim();
                for (int i = 0; i < this.List.Count; i++)
                {
                    IISWebServer server = this[i];
                    if (server.ServerComment.ToLower().Trim() == ServerComment)
                        return true;
                }
                return false;
            }

            /// <summary>
            /// 是否包含指定的网站
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public bool Contains(int index)
            {
                for (int i = 0; i < this.List.Count; i++)
                {
                    IISWebServer server = this[i];
                    if (server.index == index)
                        return true;
                }
                return false;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="WebServers"></param>
            public void AddRange(IISWebServer[] WebServers)
            {
                for (int i = 0; i <= WebServers.GetUpperBound(0); i++)
                {
                    Add(WebServers[i]);
                }
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="WebServer"></param>
            public void Remove(IISWebServer WebServer)
            {
                for (int i = 0; i < this.List.Count; i++)
                {
                    if ((IISWebServer)this.List[i] == WebServer)
                    {
                        this.List.RemoveAt(i);
                        return;
                    }
                }
                IISManagement.RemoveIISWebServer(WebServer.index);
            }
        }


        //////////////////
        /// <summary>
        ///
        /// </summary>
        public class IISWebServer
        {
            /// <summary>
            ///
            /// </summary>
            internal int index = -1;
            /// <summary>
            ///
            /// </summary>
            public IISWebVirtualDirCollection WebVirtualDirs;
            /// <summary>
            /// 网站说明
            /// </summary>
            public string ServerComment = "Way";
            /// <summary>
            /// 脚本支持
            /// </summary>
            public bool AccessScript = true;
            /// <summary>
            /// 读取
            /// </summary>
            public bool AccessRead = true;
            /// <summary>
            /// 物理路径
            /// </summary>
            public string Path = "c:\\";
            /// <summary>
            /// 端口
            /// </summary>
            public int Port = 80;
            /// <summary>
            /// 目录浏览
            /// </summary>
            public bool EnableDirBrowsing = false;
            /// <summary>
            /// 默认文档
            /// </summary>
            public string DefaultDoc = "index.aspx";
            /// <summary>
            /// 使用默认文档
            /// </summary>
            public bool EnableDefaultDoc = true;

            /// <summary>
            /// IISWebServer的状态
            /// </summary>
            public IISServerState ServerState
            {
                get
                {
                    DirectoryEntry server = IISManagement.returnIISWebserver(this.index);
                    if (server == null)
                        throw (new Exception("找不到此IISWebServer"));
                    switch (server.Properties["ServerState"][0].ToString())
                    {
                        case "2":
                            return IISServerState.Started;
                        case "4":
                            return IISServerState.Stopped;
                        case "6":
                            return IISServerState.Paused;
                    }
                    return IISServerState.Stopped;
                }
            }

            /// <summary>
            /// 停止IISWebServer
            /// </summary>
            public void Stop()
            {
                DirectoryEntry Server;
                if (index == -1)
                    throw (new Exception("在IIS找不到此IISWebServer!"));
                try
                {
                    Server = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC/" + index);
                    if (Server != null)
                        Server.Invoke("stop", new object[0]);
                    else
                        throw (new Exception("在IIS找不到此IISWebServer!"));
                }
                catch
                {
                    throw (new Exception("在IIS找不到此IISWebServer!"));
                }
            }

            /// <summary>
            /// 把基本信息的更改更新到IIS
            /// </summary>
            public void CommitChanges()
            {
                IISManagement.EditIISWebServer(this);
            }

            /// <summary>
            /// 启动IISWebServer
            /// </summary>
            public void Start()
            {
                if (index == -1)
                    throw (new Exception("在IIS找不到此IISWebServer!"));

                DirectoryEntry Service = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC");
                DirectoryEntry Server;
                IEnumerator ie = Service.Children.GetEnumerator();

                while (ie.MoveNext())
                {
                    Server = (DirectoryEntry)ie.Current;
                    if (Server.SchemaClassName == "IIsWebServer")
                    {
                        if (Server.Properties["Serverbindings"][0].ToString() == ":" + this.Port + ":")
                        {
                            Server.Invoke("stop", new object[0]);
                        }
                    }
                }

                try
                {
                    Server = new DirectoryEntry("IIS://" + IISManagement.Machinename + "/W3SVC/" + index);
                    if (Server != null)
                        Server.Invoke("start", new object[0]);
                    else
                        throw (new Exception("在IIS找不到此IISWebServer!"));
                }
                catch
                {
                    throw (new Exception("在IIS找不到此IISWebServer!"));
                }
            }

            /// <summary>
            ///
            /// </summary>
            public IISWebServer()
            {
                WebVirtualDirs = new IISWebVirtualDirCollection(this);
            }
            ///////////////////////////////////////////
        }

        /// <summary>
        ///
        /// </summary>
        public class IISWebVirtualDirCollection : CollectionBase
        {
            /// <summary>
            ///
            /// </summary>
            public IISWebServer Parent = null;

            /// <summary>
            ///
            /// </summary>
            public IISWebVirtualDir this[int Index]
            {
                get
                {
                    return (IISWebVirtualDir)this.List[Index];

                }
            }

            /// <summary>
            ///
            /// </summary>
            public IISWebVirtualDir this[string Name]
            {
                get
                {
                    Name = Name.ToLower();
                    IISWebVirtualDir list;
                    for (int i = 0; i < this.List.Count; i++)
                    {
                        list = (IISWebVirtualDir)this.List[i];
                        if (list.Name.ToLower() == Name)
                            return list;
                    }
                    return null;
                }
            }


            internal void Add_(IISWebVirtualDir WebVirtualDir)
            {
                try
                {
                    this.List.Add(WebVirtualDir);
                }
                catch
                {
                    throw (new Exception("发生意外错误，可能是某节点将该节点的上级节点作为它自己的子级插入"));
                }

            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="WebVirtualDir"></param>
            public void Add(IISWebVirtualDir WebVirtualDir)
            {
                WebVirtualDir.Parent = this.Parent;
                try
                {
                    this.List.Add(WebVirtualDir);

                }
                catch
                {
                    throw (new Exception("发生意外错误，可能是某节点将该节点的上级节点作为它自己的子级插入"));
                }
                IISManagement.CreateIISWebVirtualDir(WebVirtualDir, true);

            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="WebVirtualDirs"></param>
            public void AddRange(IISWebVirtualDir[] WebVirtualDirs)
            {
                for (int i = 0; i <= WebVirtualDirs.GetUpperBound(0); i++)
                {
                    Add(WebVirtualDirs[i]);
                }
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="WebVirtualDir"></param>
            public void Remove(IISWebVirtualDir WebVirtualDir)
            {
                for (int i = 0; i < this.List.Count; i++)
                {
                    if ((IISWebVirtualDir)this.List[i] == WebVirtualDir)
                    {
                        this.List.RemoveAt(i);
                        IISManagement.RemoveIISWebVirtualDir(WebVirtualDir);
                        return;
                    }
                }
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="Parent"></param>
            public IISWebVirtualDirCollection(IISWebServer Parent)
            {
                this.Parent = Parent;
            }
        }


        ///////////////
        /// <summary>
        ///
        /// </summary>
        public class IISWebVirtualDir
        {
            /// <summary>
            ///
            /// </summary>
            public IISWebServer Parent = null;
            /// <summary>
            /// 虚拟目录名称
            /// </summary>
            public string Name = "Way";
            /// <summary>
            /// 读取
            /// </summary>
            public bool AccessRead = true;
            /// <summary>
            /// 脚本支持
            /// </summary>
            public bool AccessScript = true;
            /// <summary>
            /// 物理路径
            /// </summary>
            public string Path = "c:\\";
            /// <summary>
            /// 默认文档
            /// </summary>
            public string DefaultDoc = "index.aspx";
            /// <summary>
            /// 使用默认文档
            /// </summary>
            public bool EnableDefaultDoc = true;
            /// <summary>
            /// 所属的网站的网站说明
            /// </summary>
            public string WebServer = "";

            /// <summary>
            ///
            /// </summary>
            /// <param name="WebServerName"></param>
            public IISWebVirtualDir(string WebServerName)
            {
                if (WebServerName.ToString() == "")
                    throw (new Exception("WebServerName不能为空!"));
                this.WebServer = WebServerName;
            }
            /// <summary>
            ///
            /// </summary>
            public IISWebVirtualDir()
            {

            }
        }
    }
}
