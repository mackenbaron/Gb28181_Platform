using Gb28181_Client.Message;
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            SIPSqlite.Instance.Read();

            //NvrTable.Instance.Read();

            //NvrTable.NvrItem item = new NvrTable.NvrItem()
            //{
            //    Id = Guid.NewGuid(),
            //    NvrID = 1,
            //    NvrName = "Hik137",
            //    CamID = "",
            //    CamIP = "192.168.10.137",
            //    CamPort = 5060,
            //    CamUser = "admin",
            //    CamPassword = "12345",
            //    DevType = "Hik",
            //    OnvifAddress = "",
            //    IsAnalyzer = true,
            //    IsBackRecord = 0,
            //    LocalID = "",
            //    LocalIP = "",
            //    LocalPort = 5060
            //};

            //NvrTable.Instance.NvrItems.Add(item);

            //NvrTable.ChannelItem channel = new NvrTable.ChannelItem()
            //{
            //    Id = Guid.NewGuid(),
            //    Guid = 1,
            //    NvrID = 1,
            //    Channel = 1,
            //    Name = "gb28181_Hik151",
            //    FrameRate = 25,
            //    StreamFormat = "ES",
            //    AudioFormat = "",
            //    Rtsp1 = "",
            //    Rtsp2 = "",
            //    MainResolution = ImageResolution.R_Undefined,
            //    SubResolution = ImageResolution.R_Undefined,
            //    StreamType = StreamType.mainStream,
            //    CameraID = "",
            //    AreaName = "",
            //    IsBackRecord = 0
            //};
            //NvrTable.Instance.ChannelItems.Add(channel);


            SIPAssetPersistor<SIPAccount> account = SIPSqlite.Instance.SipAccount;
            Dictionary<string, PlatformConfig> platformList = new Dictionary<string, PlatformConfig>();
            _messageDaemon = new SIPMessageDaemon(account.Get,SIPSqlite.Instance.Accounts, SIPRequestAuthenticator.AuthenticateSIPRequest, platformList);
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

        private void button1_Click(object sender, EventArgs e)
        {
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
            return;
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
}
