namespace TKS.ORMLite
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    /// <summary>
    ///     错误日志处理
    /// </summary>
    internal class LogForDB
    {
        private static object m_lock = new object();
        private static string fileName = "SqlLog" + DateTime.Now.ToString("yyyyMMdd");
        /// <summary>
        ///     获取或设置日志文件名，默认sqlLog
        /// </summary>
        public static string Filename
        {
            get { return fileName; }
            set { fileName = value; }
        }

        /// <summary>
        /// 写入sql查询日志
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLog(string message)
        {
           
            string path = (AppDomain.CurrentDomain.RelativeSearchPath==null ?AppDomain.CurrentDomain.BaseDirectory:
                AppDomain.CurrentDomain.RelativeSearchPath)+ @"\\Log\\Query";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string str2 = path + @"\" + Filename + ".txt";
            if (!File.Exists(str2))
            {
                File.Create(str2).Close();
            }
            StreamWriter writer = new StreamWriter(str2, true, Encoding.Default);
            lock (m_lock)
            {
                writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ">>>" + message);
                writer.WriteLine();
                writer.Flush();
                writer.Close();
            }

        }
    }
}
