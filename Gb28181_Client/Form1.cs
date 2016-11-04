using Gb28181_Client.Message;
using log4net;
using SIPSorcery.GB28181.Net;
using SIPSorcery.GB28181.Persistence;
using SIPSorcery.GB28181.Servers.SIPMessage;
using SIPSorcery.GB28181.SIP;
using SIPSorcery.GB28181.SIP.App;
using SIPSorcery.GB28181.Sys;
using SIPSorcery.GB28181.Sys.XML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Gb28181_Client
{
    public delegate void SetSIPServiceText(string msg, SipServiceStatus state);
    public delegate void SetCatalogText(Catalog cata);

    public partial class Form1 : Form
    {
        private static readonly string m_storageTypeKey = SIPSorceryConfiguration.PERSISTENCE_STORAGETYPE_KEY;
        private static readonly string m_connStrKey = SIPSorceryConfiguration.PERSISTENCE_STORAGECONNSTR_KEY;

        private static readonly string m_sipAccountsXMLFilename = SIPSorcery.GB28181.SIP.AssemblyState.XML_SIPACCOUNTS_FILENAME;
        private static readonly string m_sipRegistrarBindingsXMLFilename = SIPSorcery.GB28181.SIP.AssemblyState.XML_REGISTRAR_BINDINGS_FILENAME;

        private static ILog logger = AppState.logger;

        private static StorageTypes m_sipRegistrarStorageType;
        private static string m_sipRegistrarStorageConnStr;
        private SIPMessageDaemon _messageDaemon;

        public Form1()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            m_sipRegistrarStorageType = (AppState.GetConfigSetting(m_storageTypeKey) != null) ? StorageTypesConverter.GetStorageType(AppState.GetConfigSetting(m_storageTypeKey)) : StorageTypes.Unknown;
            m_sipRegistrarStorageConnStr = AppState.GetConfigSetting(m_connStrKey);

            if (m_sipRegistrarStorageType == StorageTypes.Unknown || m_sipRegistrarStorageConnStr.IsNullOrBlank())
            {
                throw new ApplicationException("The SIP Registrar cannot start with no persistence settings.");
            }

            SIPAssetPersistor<SIPAccount> sipAccountsPersistor = SIPAssetPersistorFactory<SIPAccount>.CreateSIPAssetPersistor(m_sipRegistrarStorageType, m_sipRegistrarStorageConnStr, m_sipAccountsXMLFilename);
            SIPDomainManager sipDomainManager = new SIPDomainManager(m_sipRegistrarStorageType, m_sipRegistrarStorageConnStr);
            SIPAssetPersistor<SIPRegistrarBinding> sipRegistrarBindingPersistor = SIPAssetPersistorFactory<SIPRegistrarBinding>.CreateSIPAssetPersistor(m_sipRegistrarStorageType, m_sipRegistrarStorageConnStr, m_sipRegistrarBindingsXMLFilename);

            Dictionary<string, string> devList = new Dictionary<string, string>();
            devList.Add("34020000001320000011", "大华151");
            devList.Add("34020000001320000012", "大华20");

            foreach (var item in devList)
            {
                ListViewItem lvItem = new ListViewItem(new string[] { item.Value, item.Key });
                lvItem.ImageKey = item.Key;
                lvDev.Items.Add(lvItem);
            }

            _messageDaemon = new SIPMessageDaemon(sipDomainManager.GetDomain, sipAccountsPersistor.Get, sipRegistrarBindingPersistor, SIPRequestAuthenticator.AuthenticateSIPRequest, devList);
        }

        private void btnStart_Click(object sender, System.EventArgs e)
        {
            Initialize();
            _messageDaemon.Start();
            _messageDaemon.MessageCore.OnSIPServiceChanged += m_msgCore_SIPInitHandle;
            _messageDaemon.MessageCore.OnCatalogReceived += m_msgCore_OnCatalogReceived;
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
            _messageDaemon.MessageCore.DeviceCatalogQuery(txtDeviceId.Text.Trim());
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

        /// <summary>
        /// 设置设备目录
        /// </summary>
        /// <param name="cata">设备目录</param>
        private void SetDevText(Catalog cata)
        {
            foreach (Catalog.Item item in cata.DeviceList.Items)
            {
                ListViewItem lvItem = new ListViewItem(new string[] { item.Name, item.DeviceID });
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
            if (!_messageDaemon.MessageCore.MonitorService.ContainsKey(devItem.ImageKey))
            {
                return;
            }
            _messageDaemon.MessageCore.MonitorService[devItem.ImageKey].RealVideoReq();
        }

        private void btnBye_Click(object sender, EventArgs e)
        {
            ListViewItem devItem = new ListViewItem();
            foreach (var item in lvDev.SelectedItems.Cast<ListViewItem>())
            {
                devItem = item;
            }
            if (!_messageDaemon.MessageCore.MonitorService.ContainsKey(devItem.ImageKey))
            {
                return;
            }
            _messageDaemon.MessageCore.MonitorService[devItem.ImageKey].ByeVideoReq();
        }



        private void button1_Click(object sender, EventArgs e)
        {
            DateTime date = new DateTime(2016, 11, 1, 10, 0, 0);

            uint startTime = TimeConvert.DateToTimeStamp(date);
            uint stopTime = TimeConvert.DateToTimeStamp(date.AddHours(2));
            string localIp = "192.168.10.104";

            SDPConnectionInformation sdpConn = new SDPConnectionInformation(localIp);

            SDP sdp = new SDP();
            sdp.Version = 0;
            sdp.SessionId = "0";
            sdp.Username = "34010000002000000001";
            sdp.SessionName = SIPSorcery.GB28181.Sys.XML.CommandType.Playback.ToString();
            sdp.Connection = sdpConn;
            sdp.Timing = startTime + " " + stopTime;
            sdp.Address = localIp;

            SDPMediaFormat psFormat = new SDPMediaFormat(SDPMediaFormatsEnum.PS);
            psFormat.IsStandardAttribute = false;
            SDPMediaFormat h264Format = new SDPMediaFormat(SDPMediaFormatsEnum.H264);
            h264Format.IsStandardAttribute = false;
            SDPMediaAnnouncement media = new SDPMediaAnnouncement();

            media.Media = SDPMediaTypesEnum.video;

            media.MediaFormats.Add(psFormat);
            media.MediaFormats.Add(h264Format);
            media.AddExtra("a=recvonly");
            media.AddFormatParameterAttribute(psFormat.FormatID, psFormat.Name);
            media.AddFormatParameterAttribute(h264Format.FormatID, h264Format.Name);
            media.Port = 10000;

            sdp.Media.Add(media);

            string sdpBody = sdp.ToString();
        }
    }
}
