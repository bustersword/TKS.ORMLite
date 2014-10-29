using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace TKS.ORMLite.Driver
{
    /// <summary>
    /// sqlserver驱动
    /// </summary>
    public class SqlDriver : IDriverType
    {
        public SqlDriver()
        {
        }
        /// <summary>
        ///     构造函数，连接字符串由<see cref="DriverTypeFactory"/>提供 
        /// </summary>
        public SqlDriver(string conn)
        {
            ConnectionString = conn;
            DBDSN = "mssql";
        }

        #region IDriverType Members

        /// <summary>
        /// 获取数据连接对象
        /// </summary>
        public System.Data.IDbConnection Connection
        {
            get { return new System.Data.SqlClient.SqlConnection(this.ConnectionString); }
        }



        /// <summary>
        /// 数据库连接字符串
        /// </summary>
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
            get
            {
                return "@";
            }
        }

        /// <summary>
        /// 获取相关参数的名称
        /// </summary>
        /// <param name="parametername">参数名称</param>
        /// <returns>string</returns>
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
            private set;
        }
        #endregion






       
    }
}
