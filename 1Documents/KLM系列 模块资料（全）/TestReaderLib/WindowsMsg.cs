using System;
using System.Collections.Generic;
using System.Text;

namespace RFID_Reader_Csharp
{
    public enum WindowsMsg
    {
        WM_NULL=0,
        /// <summary>
        /// 应用程序创建一个窗口
        /// </summary>
        WM_CREATE=1,
        /// <summary>
        /// 一个窗口被销毁
        /// </summary>
        WM_DESTROY=2,
        /// <summary>
        /// 移动一个窗口
        /// </summary>
        WM_MOVE=3,
        /// <summary>
        /// 改变一个窗口的大小
        /// </summary>
        WM_SIZE=5,
        /// <summary>
        /// 一个窗口被激活或失去激活状态
        /// </summary>
        WM_ACTIVATE=6,
        /// <summary>
        /// 获得焦点后
        /// </summary>
        WM_SETFOCUS=7,
        /// <summary>
        /// 失去焦点
        /// </summary>
        WM_KILLFOCUS=8,
        /// <summary>
        /// 改变enable状态
        /// </summary>
        WM_ENABLE=0x0A,
        /// <summary>
        /// 设置窗口是否能重画
        /// </summary>
        WM_SETREDRAW=0x0B,
        /// <summary>
        /// 按下鼠标右键
        /// </summary>
        WM_RBUTTONDOWN=0x0204,
        /// <summary>
        /// 剪切数据
        /// </summary>
        WM_CUT=0x0300,
        /// <summary>
        /// 复制数据
        /// </summary>
        WM_COPY = 0x0301,
        /// <summary>
        /// 粘贴数据
        /// </summary>
        WM_PASTE = 0x0302,
    }
}
