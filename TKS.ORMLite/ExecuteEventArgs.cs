using System;
using System.Collections.Generic;
using System.Text;

namespace TKS.ORMLite
{
    /// <summary>
    ///     事件发生的时刻
    /// </summary>
    public enum ExecuteTime
    {
        /// <summary>
        ///     在执行命令前发生
        /// </summary>
        BeforeExecute,

        /// <summary>
        ///     在执行命令后发生
        /// </summary>
        AfterExecute
    }

    /// <summary>
    ///     命令执行参数
    /// </summary>
    public class ExecuteEventArgs:EventArgs 
    {
        /// <summary>
        ///     初始化一个新的<see cref="ExecuteEventArgs"/>实例
        /// </summary>
        /// <param name="time"></param>
        public ExecuteEventArgs(ExecuteTime time)
        {
            this._ExecuteTime = time;
        }

        /// <summary>
        ///     获取或设置该事件的执行时刻
        /// </summary>
        public ExecuteTime _ExecuteTime
        {
            get;
            set;
        }
    }
}
