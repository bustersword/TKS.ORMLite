using System;
using System.Collections.Generic;
using System.Text;

namespace TKS.ORMLite.Driver
{
    /// <summary>
    /// 数据库类型提供描述接口
    /// </summary>
    public interface IDriverType
    {
        /// <summary>
        /// 获取数据连接对象
        /// </summary>
        System.Data.IDbConnection Connection
        {

            get;

        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        string ConnectionString
        {
            get;
           
        }

        /// <summary>
        /// 参数开头标识符
        /// </summary>
        string NamedPrefix { get; }

        /// <summary>
        /// 格式化参数名
        /// </summary>
        /// <param name="parametername"></param>
        /// <returns></returns>
        string FormatNameForParameter(string parametername);

        string DBDSN
        {
            get;
        }
    }
}
