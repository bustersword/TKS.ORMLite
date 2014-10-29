using System;
using System.Collections.Generic;
using System.Text;

namespace TKS.ORMLite
{
    /// <summary>
    ///     执行参数，若无异常_Exception为null
    /// </summary>
    public class ExecuteLogEventArgs:EventArgs
    {
        /// <summary>
        ///     初始化一个<see cref="ExecuteExceptionEventArgs"/>的实例
        /// </summary>
        /// <param name="executeMethod">执行方法名</param>
        /// <param name="executeText">执行文本</param>
        public ExecuteLogEventArgs(string executeMethod, string executeText):this(executeMethod,executeText ,null )
        {
       
        }
        /// <summary>
        ///     初始化一个<see cref="ExecuteExceptionEventArgs"/>的实例
        /// </summary>
        /// <param name="executeMethod">执行方法名</param>
        /// <param name="executeText">执行文本</param>
        /// <param name="exception">异常信息</param>
        public ExecuteLogEventArgs(string executeMethod, string executeText, Exception exception)
        {
            this._Exception = exception;
            this.ExecuteMethod = executeMethod;
            this.ExecuteText = executeText;
        }

        /// <summary>
        ///     执行方法名
        /// </summary>
        public string ExecuteMethod
        {
            get;
            set;
        }

        /// <summary>
        ///     执行的sql语句或者存储过程名
        /// </summary>
        public string ExecuteText
        {
            get;
            set;
        }

        /// <summary>
        ///     异常信息
        /// </summary>
        public Exception _Exception
        {
            get;
            set;
        }
    }
}
