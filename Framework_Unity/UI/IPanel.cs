using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace CLIP.Framework_Unity.UI
{
    /// <summary>
    /// UI面板接口，所有UI面板都需要实现此接口
    /// </summary>
    public interface IPanel
    {
        /// <summary>
        /// 面板的唯一标识符
        /// </summary>
        string PanelID { get; }



        /// <summary>
        /// 初始化面板
        /// </summary>
        void Initialize();

        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="data">传递的参数</param>
        void OpenPanel(params object[] data);

        /// <summary>
        /// 关闭面板
        /// </summary>
        void ClosePanel();

        /// <summary>
        /// 更新面板
        /// </summary>
        void UpdatePanel(params object[] data);
    }

    /// <summary>
    /// UI面板层级，数值越大显示在越上层
    /// </summary>
    public enum UIPanelLayer
    {
        Background = 0,
        Normal = 100,
        Popup = 200,
        Tips = 300,
        Loading = 400,
        Top = 500
    }

}
