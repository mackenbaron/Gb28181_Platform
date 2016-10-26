using Gb28181_Client.Register;
using log4net;
using SIPSorcery.Persistence;
using SIPSorcery.Servers.SIPMessage;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
using SIPSorcery.Sys;
using SIPSorcery.Sys.XML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Gb28181_Client
{
    public delegate void SetSIPServiceText(SipServiceStatus state);
    public delegate void SetCatalogText(Catalog cata);

    public partial class Form1 : Form
    {
        private static readonly string m_storageTypeKey = SIPSorceryConfiguration.PERSISTENCE_STORAGETYPE_KEY;
        private static readonly string m_connStrKey = SIPSorceryConfiguration.PERSISTENCE_STORAGECONNSTR_KEY;

        private static readonly string m_sipAccountsXMLFilename = SIPSorcery.SIP.App.AssemblyState.XML_SIPACCOUNTS_FILENAME;
        private static readonly string m_sipRegistrarBindingsXMLFilename = SIPSorcery.SIP.App.AssemblyState.XML_REGISTRAR_BINDINGS_FILENAME;

        private static ILog logger = AppState.logger;

        private static StorageTypes m_sipRegistrarStorageType;
        private static string m_sipRegistrarStorageConnStr;
        private SIPRegistrarDaemon _registerDaemon;

        public Form1()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            m_sipRegistrarStorageType = (AppState.GetConfigSetting(m_storageTypeKey) != null) ? StorageTypesConverter.GetStorageType(AppState.GetConfigSetting(m_storageTypeKey)) : StorageTypes.Unknown; ;
            m_sipRegistrarStorageConnStr = AppState.GetConfigSetting(m_connStrKey);

            if (m_sipRegistrarStorageType == StorageTypes.Unknown || m_sipRegistrarStorageConnStr.IsNullOrBlank())
            {
                throw new ApplicationException("The SIP Registrar cannot start with no persistence settings.");
            }

            SIPAssetPersistor<SIPAccount> sipAccountsPersistor = SIPAssetPersistorFactory<SIPAccount>.CreateSIPAssetPersistor(m_sipRegistrarStorageType, m_sipRegistrarStorageConnStr, m_sipAccountsXMLFilename);
            SIPDomainManager sipDomainManager = new SIPDomainManager(m_sipRegistrarStorageType, m_sipRegistrarStorageConnStr);
            SIPAssetPersistor<SIPRegistrarBinding> sipRegistrarBindingPersistor = SIPAssetPersistorFactory<SIPRegistrarBinding>.CreateSIPAssetPersistor(m_sipRegistrarStorageType, m_sipRegistrarStorageConnStr, m_sipRegistrarBindingsXMLFilename);

            _registerDaemon = new SIPRegistrarDaemon(sipDomainManager.GetDomain, sipAccountsPersistor.Get, sipRegistrarBindingPersistor, SIPRequestAuthenticator.AuthenticateSIPRequest);
        }

        private void btnStart_Click(object sender, System.EventArgs e)
        {
            Initialize();
            Thread daemonThread = new Thread(_registerDaemon.Start);
            daemonThread.Start();
            Thread.Sleep(100);
            _registerDaemon.m_msgCore.OnSIPServiceChanged += m_msgCore_SIPInitHandle;
            _registerDaemon.m_msgCore.OnCatalogReceived += m_msgCore_OnCatalogReceived;
            lblStatus.Text = "sip服务已启动。。。";
            lblStatus.ForeColor = Color.FromArgb(0, 192, 0);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _registerDaemon.Stop();
            lblStatus.Text = "sip服务已停止。。。";
            lblStatus.ForeColor = Color.Blue;
        }

        private void btnCatalog_Click(object sender, EventArgs e)
        {
            _registerDaemon.m_msgCore.DeviceCatalogQuery(txtDeviceId.Text.Trim());
            lblStatus.Text = "查询设备目录请求";
        }

        private void m_msgCore_OnCatalogReceived(Catalog cata)
        {
            if (lBoxCatalog.InvokeRequired)
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
                lBoxCatalog.Items.Add(item.Name + "---" + item.DeviceID);
            }
        }

        private void m_msgCore_SIPInitHandle(SipServiceStatus state)
        {
            if (lblStatus.InvokeRequired)
            {
                SetSIPServiceText sipService = new SetSIPServiceText(SetSIPService);
                this.Invoke(sipService, state);
            }
            else
            {
                SetSIPService(state);
            }
        }

        /// <summary>
        /// 设置sip服务状态
        /// </summary>
        /// <param name="state">sip状态</param>
        private void SetSIPService(SipServiceStatus state)
        {
            if (state == SipServiceStatus.Wait)
            {
                lblStatus.Text = "SIP服务未初始化完成";
                lblStatus.ForeColor = Color.YellowGreen;
            }
            else
            {
                lblStatus.Text = "SIP服务已初始化完成";
                lblStatus.ForeColor = Color.ForestGreen;
            }
        }

        private void btnReal_Click(object sender, EventArgs e)
        {
            _registerDaemon.m_msgCore.RealVideoReq(txtDeviceId.Text.Trim());
        }

        private void btnBye_Click(object sender, EventArgs e)
        {
            _registerDaemon.m_msgCore.ByeVideoReq(txtDeviceId.Text.Trim());
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }
    }
}
