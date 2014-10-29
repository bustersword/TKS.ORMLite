using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;

namespace TKS.ORMLite.Driver
{
    /// <summary>
    /// Oracle驱动
    /// </summary>
    public class OracleDriver : IDriverType
    {
        public OracleDriver() { }

        public OracleDriver(string conn)
        {
            ConnectionString = conn;
            DBDSN = "oracle";
        }

        public System.Data.IDbConnection Connection
        {
            get { return new System.Data.OracleClient.OracleConnection(this.ConnectionString); }
        }

        public string ConnectionString
        {
            get;
            private set;
        }

        /// <summary>
        /// 参数开头标识符
        /// </summary>
        public string NamedPrefix
        {
            get { return ":"; }
        }

        public string FormatNameForParameter(string parametername)
        {
            return NamedPrefix + parametername;
        }

        /// <summary>
        /// 数据库标识
        /// </summary>
        public string DBDSN
        {
            get;
            private  set;
        }
    }
}
