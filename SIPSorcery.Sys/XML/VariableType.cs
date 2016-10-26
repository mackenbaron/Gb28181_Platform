using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPSorcery.Sys.XML
{
    /// <summary>
    /// 指令
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 目录发送
        /// </summary>
        Catalog = 1,
        /// <summary>
        /// 心跳
        /// </summary>
        KeepAlive = 2,
        /// <summary>
        /// 实时视频
        /// </summary>
        RealMedia = 3,
        /// <summary>
        /// 录像文件列表
        /// </summary>
        FileList = 4,
        /// <summary>
        /// 云台控制
        /// </summary>
        PTZCommand = 5,
        /// <summary>
        /// 预置位查询
        /// </summary>
        PresetList = 6,
        /// <summary>
        /// 事件预定
        /// </summary>
        AlarmSubscribe = 7,
        /// <summary>
        /// 事件通知
        /// </summary>
        AlarmNotify = 8,
        /// <summary>
        /// 设备目录查询
        /// </summary>
        ItemList = 9,
        /// <summary>
        /// 设备信息查询
        /// </summary>
        DeviceInfo = 10,
        /// <summary>
        /// 平台接入单元流量查询
        /// </summary>
        BandWidth = 11
    }

    /// <summary>
    /// 命令类型
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 心跳
        /// </summary>
        Keepalive = 1,

        /// <summary>
        /// 设备目录
        /// </summary>
        Catalog = 2
    }
}
