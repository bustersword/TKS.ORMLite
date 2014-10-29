namespace TKS.ORMLite
{
    using System;
    using System.Reflection;
    using System.Xml;
    using TKS.ORMLite.Driver;
    /// <summary>
    ///     数据驱动工厂
    /// </summary>
    internal class DriverTypeFactory
    {
    
        /// <summary>
        ///     根据配置文件获取数据驱动
        /// </summary>
        /// <returns></returns>
        public static IDriverType  GetDriverType()
        {
            string DBType = string.Empty;
            string DBDSN = string.Empty;
            try
            {
                 DBType = GetValue("DBType");
                 DBDSN = GetValue("DBDSN");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            switch (DBType)
            { 
                case "mssql":
                    return new SqlDriver(DBDSN);
                case "oracle":
                    return new OracleDriver(DBDSN );
                default :
                    throw new Exception("未能正确配置数据库驱动配置信息，请检查DBConfig.xml");
            }

        }

        public static  string GetValue(string key)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(AppDomain.CurrentDomain.BaseDirectory  + @"\DBConfig.xml" );
            return GetValue(xmlDocument,"//appSettings/add",key );
        }
        /// <summary>
        /// 读取配置项
        /// </summary>
        /// <param name="xmlDocument">配置文件</param>
        /// <param name="selectPath">查询路径</param>
        /// <param name="key">键</param>
        /// <returns>键值</returns>
        public static   string GetValue(XmlDocument xmlDocument, string selectPath, string key)
        {
            string str = string.Empty;
            foreach (XmlNode node in xmlDocument.SelectNodes(selectPath))
            {
                if (node.Attributes["key"].Value.ToUpper().Equals(key.ToUpper()))
                {
                    return node.Attributes["value"].Value;
                }
            }
            return str;
        }
    }
}

