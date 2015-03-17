using System;
using System.Collections.Generic;
using System.Text;

namespace TKS.ORMLite.Attrs
{
    /// <summary>
    ///     用于标识实体的每个数据成员的一些加载信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ItemFieldAttribute : Attribute
    {
        /// <summary>
        ///     允许在加载时，找不到相应的匹配数据来源。（只用于值类型），此设置对于从数据库加载时无效。
        /// </summary>
        public bool AllowNotFoundOnLoad;
        /// <summary>
        ///     数据库中对应的字段名，如不指定，则与成员的名称相同。
        /// </summary>
        public string DbFieldName;
        /// <summary>
        ///     在加载数据时，不加载这个成员 
        /// </summary>
        public bool IgnoreLoad;
        /// <summary>
        ///     指示是否是一个子实体对象 
        /// </summary>
        internal bool IsSubItem;
        /// <summary>
        ///     从查询字符串或者从FROM加载时的Key 
        /// </summary>
        public string KeyName;
        /// <summary>
        ///     仅当设置为true时候有效，程序可控制是否加载该成员 
        /// </summary>
        public bool OnlyLoadAll;
        /// <summary>
        ///     在加载时，如果找不到相应的匹配数据来源，对于“字符串”类型来说，就设置为 String.Empty ，此设置对于从数据库加载时无效。
        /// </summary>
        public bool SetEmptyIfNotFoundOnLoad;

        /// <summary>
        ///    插入时，指示是否有默认值，如果有，则该字段不生成插入语句
        /// </summary>
        public bool HasDefault;
    }
}
