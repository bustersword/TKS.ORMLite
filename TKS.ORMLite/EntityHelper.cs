using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Data;
using System.Collections;
using System.Reflection;
using System.IO;
using TKS.ORMLite.Attrs;
namespace TKS.ORMLite
{
    /// <summary>
    ///     于数据实体的一些反射操作的辅助工具类，主要实现了从数据库加载，和从Request中加载实体的功能。
    /// </summary>
    internal static class EntityHelper
    {
        #region 从数据库加载实体的方法
        /// <summary>
        ///     存储一些实体描述信息
        /// </summary>
        private static Hashtable s_hashtbl = Hashtable.Synchronized(new Hashtable(0xc00));
        /// <summary>
        ///     最终要加载的数据项信息
        /// </summary>
        private sealed class LatestLoadDataItemInfo
        {
            public string[] ColumnNames;
            public EntityDescription EntityDescription;
        }
        /// <summary>
        ///     对实体类成员进行赋值
        /// </summary>
        /// <param name="row">一行数据</param>
        /// <param name="item">实体类</param>
        /// <param name="loadAllField">是否加载所有成员</param>
        /// <param name="checkSucessCount"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private static int Internal_LoadItemValuesFormDbRow(CommonDataAdapter row, object item, bool loadAllField, bool checkSucessCount, string prefix)
        {
            Type itemType = item.GetType();
            LatestLoadDataItemInfo userData = (LatestLoadDataItemInfo)row.UserData;
            EntityDescription entityDescription = userData.EntityDescription;
            if (userData.EntityDescription.ItemType != itemType)
            {
                userData.EntityDescription = GetItemDescription(itemType);
            }
            int num = 0;
            string fieldNames = string.Empty;
            foreach (KeyValuePair<string, EntityMemberDescription> pair in userData.EntityDescription.Dict)
            {
                //成员遍历
                if (pair.Value.MemberAttr.IgnoreLoad)
                {
                    if (pair.Value.MemberAttr.IsSubItem)
                    {
                        //如果是子实体对象
                        object val = Activator.CreateInstance(pair.Value.MemberType);
                        pair.Value.SetValue(item, val);
                        string str2 = prefix + pair.Value.MemberAttr.DbFieldName;
                        Internal_LoadItemValuesFormDbRow(row, val, loadAllField, checkSucessCount, str2);
                        userData.EntityDescription = entityDescription;
                    }
                }
                else if (loadAllField || !pair.Value.MemberAttr.OnlyLoadAll)  //默认为加载，当属性附加属性描述OnlyLoadAll=true时候，loadAllField设置才有效
                {
                    //对实体成员进行赋值
                    if (TrySetDataItemMemberValue(row, item, pair.Value, prefix))
                    {
                        num++;
                        continue;
                    }
                    if (checkSucessCount)
                    {
                        fieldNames = fieldNames + pair.Key + ";";
                    }
                }
            }
            if (checkSucessCount)
            {
                if (loadAllField)
                {
                    if (num != userData.EntityDescription.LoadALLValues_ExpectSuccessCount)
                    {
                        throw new LoadMemberFailedException(itemType, fieldNames);
                    }
                    return num;
                }
                if (num != userData.EntityDescription.LoadPartialValues_ExpectSuccessCount)
                {
                    throw new LoadMemberFailedException(itemType, fieldNames);
                }
            }
            return num;
        }

        /// <summary>
        ///     获取一个类型的具体描述。
        /// </summary>
        /// <param name="itemType">要加载的类型</param>
        /// <returns></returns>
        internal static EntityDescription GetItemDescription(Type itemType)
        {
            if (itemType == null)
            {
                throw new ArgumentNullException("itemType");
            }
            EntityDescription entityDescription = s_hashtbl[itemType] as EntityDescription;
            if (entityDescription == null)
            {
                entityDescription = InternalGetItemDescription(itemType);
                s_hashtbl[itemType] = entityDescription;
            }
            return entityDescription;
        }

        /// <summary>
        ///     获取一个类型的具体描述。
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static EntityDescription InternalGetItemDescription(Type itemType)
        {
            MemberInfo[] members = itemType.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            int memberCapacity = 0;
            foreach (MemberInfo info in members)
            {
                if ((info.MemberType == MemberTypes.Field) || (info.MemberType == MemberTypes.Property))
                {
                    memberCapacity++;
                }
            }
            int loadPart = 0;
            int loadAll = 0;
            EntityDescription description = new EntityDescription(itemType, memberCapacity);
            foreach (MemberInfo _memberInfo in members)
            {
                if ((_memberInfo.MemberType == MemberTypes.Field) || (_memberInfo.MemberType == MemberTypes.Property))
                {
                    EntityMemberDescription memberDescription = new EntityMemberDescription();
                    memberDescription.MemberInfo = _memberInfo;
                    ItemFieldAttribute itemAttr = null;
                    //获取一个属性或者字段,获取该字段或者属性的附加属性，如果没有则new一个
                    object[] customAttributes = _memberInfo.GetCustomAttributes(false);
                    for (int i = 0; i < customAttributes.Length; i++)
                    {
                        if (customAttributes[i].GetType() == TypeList._ItemFieldAttribute)
                        {
                            itemAttr = customAttributes[i] as ItemFieldAttribute;
                            HandleItemFieldAttribute(_memberInfo, ref itemAttr);
                            memberDescription.MemberAttr = itemAttr;
                        }
                        else if (customAttributes[i].GetType() == TypeList._PrimaryKeyAttribute)
                        {
                            memberDescription.IsPK = true;
                        }
                    }
                    if (memberDescription.MemberAttr == null)
                    {
                        itemAttr = new ItemFieldAttribute();
                        HandleItemFieldAttribute(_memberInfo, ref itemAttr);
                        memberDescription.MemberAttr = itemAttr;
                    }

                    if (!itemAttr.IgnoreLoad)
                    {
                        loadPart++;
                        if (!itemAttr.OnlyLoadAll)
                        {
                            loadAll++;
                        }
                    }

                    description.Dict[_memberInfo.Name] = memberDescription;
                }
            }
            description.SetSuccessCount(loadAll, loadPart);
            return description;
        }

        private static void HandleItemFieldAttribute(MemberInfo info2, ref ItemFieldAttribute attr)
        {
            //判断该成员是否加载
            if ((!attr.IgnoreLoad && (info2.MemberType == MemberTypes.Property)) && !((PropertyInfo)info2).CanWrite)
            {
                //property readonly 
                attr.IgnoreLoad = true;
            }

            if (!attr.IgnoreLoad)
            {
                Type type = (info2.MemberType == MemberTypes.Property) ? ((PropertyInfo)info2).PropertyType : ((FieldInfo)info2).FieldType;
                if (CheckItemIsNormalClass(type))
                {
                    //如果该成员是类，表示为子实体对象，并且不加载
                    attr.IsSubItem = true;
                    attr.IgnoreLoad = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(attr.DbFieldName))
                    {
                        //如果该成员有ItemFieldAttribute,并且设置了DbFieldName
                        attr.DbFieldName = info2.Name;
                    }
                    if (!attr.IgnoreLoad && IsEnumerableType(type))
                    {
                        //如果是枚举类型也不加载
                        attr.IgnoreLoad = true;
                    }
                }
            }
        }

        /// <summary>
        ///     测试一个类型是不是可枚举类型，但不包括 string、byte 类型 
        /// </summary>
        /// <param name="testType"></param>
        /// <returns></returns>
        internal static bool IsEnumerableType(Type testType)
        {
            if (testType == TypeList._string || testType == TypeList._byteArray)
            {
                return false;
            }
            return (testType.IsArray || TypeList._IEnumerable.IsAssignableFrom(testType));
        }

        /// <summary>
        ///     检查一个类型是否是一个“普通”的实体类（建议使用自定义基类的做法） 【警告】这个方法不检查参数是否为 null 。
        ///     如果不是实体类，不抛出异常
        /// </summary>
        /// <param name="itemType">要检查的类型</param>
        /// <returns></returns>
        public static bool CheckItemIsNormalClass(Type itemType)
        {
            return (((!itemType.IsPrimitive && !itemType.IsGenericType) && ((itemType != TypeList._string) && (itemType != TypeList._object))) && (itemType.IsClass && !itemType.IsArray));
        }

        /// <summary>
        ///     确认类型是一个数据实体类型，否则会抛出异常。 
        /// </summary>
        /// <param name="itemType">要检查的类型</param>
        internal static void EnsureIsDataItemType(Type itemType)
        {
            if (!CheckItemIsNormalClass(itemType))
            {
                throw new Exception("is not a valid entity class type.");
            }
        }

        /// <summary>
        ///     设置成员的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="item"></param>
        /// <param name="info"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private static bool TrySetDataItemMemberValue(CommonDataAdapter row, object item, EntityMemberDescription info, string prefix)
        {
            string[] columnNames = ((LatestLoadDataItemInfo)row.UserData).ColumnNames;
            string str = prefix + info.MemberAttr.DbFieldName;
            if (Array.IndexOf<string>(columnNames, str.ToUpper()) < 0)
            {
                return false;
            }
            object obj2 = row[str];//从数据源中获取指定字段名的值
            return (DBNull.Value.Equals(obj2) || SafeSetMemberValue(item, info, obj2));
        }

        /// <summary>
        ///     将值赋给成员
        /// </summary>
        /// <param name="item"></param>
        /// <param name="info"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        internal static bool SafeSetMemberValue(object item, EntityMemberDescription info, object val)
        {
            try
            {
                Type realType = GetRealType(info.MemberType);
                if (realType.IsEnum)
                {
                    info.SetValue(item, Convert.ToInt32(val));
                }
                else if (val.GetType() == realType)
                {
                    info.SetValue(item, val);
                }
                else
                {
                    info.SetValue(item, Convert.ChangeType(val, realType));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     返回一个类型的真实的类型，用于获取【可空类型】的参数类型。 
        /// </summary>
        /// <param name="testType"></param>
        /// <returns></returns>
        internal static Type GetRealType(Type testType)
        {
            if (testType.IsGenericType && (testType.GetGenericTypeDefinition() == TypeList._nullable))
            {
                return testType.GetGenericArguments()[0];
            }
            return testType;
        }

        #endregion

        #region WriteExtension
        internal class ParameterValue
        {
            public string Parameter
            {
                get;
                set;
            }
            public object Value
            {
                get;
                set;
            }
        }

        internal static List<ParameterValue> GetInsertDescription<T>(Type itemType, T obj)
        {
            List<ParameterValue> parameterValue = new List<ParameterValue>();
            EntityDescription description = GetItemDescription(itemType);
            foreach (var item in description.Dict)
            {
                string name = item.Key;
                EntityMemberDescription memberDescription = description.Dict[name];
                //数据库是否有默认值
                if (memberDescription.MemberAttr.HasDefault)
                    continue;

                var value = description.Dict[name].GetValue(obj);
                parameterValue.Add(new ParameterValue { Parameter = name, Value = value });
            }
            return parameterValue;
        }

        internal class SqlStatement
        {
            public string Sql { get; set; }
            public List<ParameterValue> Parameters { get; set; }
        }

        internal static SqlStatement GetUpdateStatement<T>(Type itemType, T obj, bool updateAllFields)
        {
            string updateSql = "update {0} set {1} {2}";
            EntityDescription description = GetItemDescription(itemType);
            StringBuilder sql = new StringBuilder();
            StringBuilder sqlFilter = new StringBuilder();
            List<ParameterValue> paras = new List<ParameterValue>();
            foreach (var item in description.Dict)
            {
                string name = item.Key;
                var value = description.Dict[item.Key].GetValue(obj);
                var defaultValue = description.Dict[item.Key].GetDefaultValue();
                if (item.Value.IsPK)
                {
                    if (sqlFilter.Length > 0)
                        sqlFilter.Append(" AND ");
                    sqlFilter.Append(name)
                     .Append("=@")
                     .Append(name);
                }
                else
                {
                    if (updateAllFields || (!updateAllFields && value != null && !value.Equals(defaultValue)))
                    {
                        if (sql.Length > 0)
                            sql.Append(",");
                        sql.Append(name).Append("=@").Append(name);
                    }
                    else if (!updateAllFields && (value == null || value.Equals(defaultValue)))
                    {
                        //不更新全部，并且没有值，过滤掉
                        continue;
                    }

                }
                paras.Add(new ParameterValue { Parameter = name, Value = value });
            }
            updateSql = string.Format(updateSql, itemType.Name, sql.ToString(),
                sqlFilter.Length > 0 ? " where " + sqlFilter.ToString() : "");
            return new SqlStatement { Sql = updateSql, Parameters = paras };
        }

        internal static SqlStatement GetUpdateStatement<T>(Type itemType, T obj, object anonType, bool updateAllFields)
        {
            string updateSql = "update {0} set {1} {2}";
            EntityDescription description = GetItemDescription(itemType);
            StringBuilder sql = new StringBuilder();
            StringBuilder sqlFilter = new StringBuilder();
            List<ParameterValue> paras = new List<ParameterValue>();

            List<string> parasName = new List<string>();

            foreach (var item in description.Dict)
            {
                string name = item.Key;
                var value = description.Dict[item.Key].GetValue(obj);
                var defaultValue = description.Dict[item.Key].GetDefaultValue();

                if (updateAllFields || value != null)
                {
                    if (!value.Equals(defaultValue))
                    {
                        if (sql.Length > 0)

                            sql.Append(",");
                        sql.Append(name).Append("=@").Append(name);
                        if (!parasName.Contains(name))
                        {
                            parasName.Add(name);
                            paras.Add(new ParameterValue { Parameter = name, Value = value });
                        }
                        else
                        {
                            parasName.Add(name);
                        }
                    }
                }
                else if (!updateAllFields && (value == null || value.Equals(defaultValue)))
                {
                    //不更新全部，并且没有值，过滤掉
                    continue;
                }

            }


            Type _anonType = anonType.GetType();
            var pis = _anonType.GetProperties();
            foreach (var item in pis)
            {
                var name = item.Name;
                if (!description.Dict.ContainsKey(name))
                {
                    throw new Exception("匿名类中的属性" + name + "不存在于" + itemType.Name);
                }
                var value = item.GetValue(anonType, null);
                 var defaultValue = description.Dict[name].GetDefaultValue();

                 if (value != null && !value.Equals(defaultValue))
                 {
                     if (sqlFilter.Length > 0)
                     {
                         sqlFilter.Append(" AND ");
                     }

                     sqlFilter.Append(name)
                     .Append("=@")
                     .Append(name);

                     if (!parasName.Contains(name))
                     {

                         parasName.Add(name);
                         paras.Add(new ParameterValue { Parameter = name, Value = value });
                     }
                     else
                     {
                         parasName.Add(name);
                     }
                 }
            }
            updateSql = string.Format(updateSql, itemType.Name, sql.ToString(),
                sqlFilter.Length > 0 ? " where " + sqlFilter.ToString() : "");
            return new SqlStatement { Sql = updateSql, Parameters = paras };
        }


        internal static SqlStatement GetDeleteNonDefultsStatement<T>(Type itemType, T obj)
        {
            string deleteSql = "delete from {0} {1}";
            EntityDescription description = GetItemDescription(itemType);
            List<ParameterValue> paras = new List<ParameterValue>();
            StringBuilder sqlFilter = new StringBuilder();
            foreach (var item in description.Dict)
            {
                string name = item.Key;
                var value = description.Dict[item.Key].GetValue(obj);
                var defaultValue = description.Dict[item.Key].GetDefaultValue();
                if (value != null && !value.Equals(defaultValue))
                {
                    if (sqlFilter.Length > 0)
                    {
                        sqlFilter.Append(" AND ");
                    }
                    sqlFilter.Append(name)
                    .Append("=@")
                    .Append(name);

                    paras.Add(new ParameterValue { Parameter = name, Value = value });
                }
            }

            deleteSql = string.Format(deleteSql, itemType.Name,
                sqlFilter.Length > 0 ? " where " + sqlFilter.ToString() : "");
            return new SqlStatement { Sql = deleteSql, Parameters = paras };

        }

        internal static SqlStatement GetDeleteStatement(Type itemType, object anonType)
        {
            string deleteSql = "delete from {0} {1}";
            Type _anonType = anonType.GetType();
            var pis = _anonType.GetProperties();
            EntityDescription description = GetItemDescription(itemType);
            List<ParameterValue> paras = new List<ParameterValue>();
            StringBuilder sqlFilter = new StringBuilder();
            foreach (var item in pis)
            {
                var name = item.Name;
                if (!description.Dict.ContainsKey(name))
                {
                    throw new Exception("匿名类中的属性" + name + "不存在于" + itemType.Name);
                }
                var value = item.GetValue(anonType, null);
                if (sqlFilter.Length > 0)
                {
                    sqlFilter.Append(" AND ");
                }
                sqlFilter.Append(name)
                .Append("=@")
                .Append(name);

                paras.Add(new ParameterValue { Parameter = name, Value = value });
            }
            deleteSql = string.Format(deleteSql, itemType.Name,
            sqlFilter.Length > 0 ? " where " + sqlFilter.ToString() : "");
            return new SqlStatement { Sql = deleteSql, Parameters = paras };
        }
        #endregion

        #region ReadExtension
        internal static SqlStatement GetSelectStatement<T>(Type itemType, T obj)
        {
            string selectSql = "select * from {0} {1}";
            EntityDescription description = GetItemDescription(itemType);
            List<ParameterValue> paras = new List<ParameterValue>();
            StringBuilder sqlFilter = new StringBuilder();
            if (obj != null)
            {
                foreach (var item in description.Dict)
                {
                    string name = item.Key;
                    var value = description.Dict[item.Key].GetValue(obj);
                    var defaultValue = description.Dict[item.Key].GetDefaultValue();
                    if (value != null && !value.Equals(defaultValue))
                    {
                        if (sqlFilter.Length > 0)
                        {
                            sqlFilter.Append(" AND ");
                        }
                        sqlFilter.Append(name)
                        .Append("=@")
                        .Append(name);

                        paras.Add(new ParameterValue { Parameter = name, Value = value });
                    }
                }
            }
            selectSql = string.Format(selectSql, itemType.Name,
                sqlFilter.Length > 0 ? " where " + sqlFilter.ToString() : "");
            return new SqlStatement { Sql = selectSql, Parameters = paras };
        }
        #endregion

        #region 从datarows、datatable、XML、加载
        /// <summary>
        ///     从一个DataRow数组中加载实体列表。
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="rows">要加载的数据行数组</param>
        /// <returns>实体列表</returns>
        public static List<T> LoadItemsFromDataRows<T>(DataRow[] rows) where T : class, new()
        {
            if (rows == null)
            {
                throw new ArgumentNullException("rows");
            }
            if (rows.Length == 0)
            {
                return new List<T>();
            }
            List<T> list = new List<T>(rows.Length);
            CommonDataAdapter adapter = new CommonDataAdapter(rows[0]);
            foreach (DataRow row in rows)
            {
                adapter.SetCurrentRow(row);
                T item = Activator.CreateInstance(typeof(T)) as T;
                LoadItemValuesFormDbRow(adapter, item, true, false);
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        ///     从一个数据表中加载实体列表。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> LoadItemsFromDataTable<T>(DataTable table) where T : class, new()
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            return LoadItemsFromDataRows<T>(table.Select());
        }

        /// <summary>
        ///     从一个XML文件中加载实体列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static List<T> LoadItemsFromXmlFile<T>(string xmlPath) where T : class, new()
        {
            if (!File.Exists(xmlPath))
            {
                return null;
            }
            DataSet set = new DataSet();
            set.ReadXml(xmlPath);
            if (set.Tables.Count == 0)
            {
                return null;
            }
            return LoadItemsFromDataRows<T>(set.Tables[0].Select());
        }

        /// <summary>
        ///     尝试从一个MyDataAdapter中加载实休对象的成员（一次加载一行信息） 
        /// </summary>
        /// <param name="row">CommonDataAdapter对象</param>
        /// <param name="item">实休对象的实例</param>
        /// <param name="loadAllField">是否要加载全部成员，可由ItemFieldAttribute.OnlyLoadAll控制</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns>返回成功加载的成员数量</returns>
        public static int LoadItemValuesFormDbRow(CommonDataAdapter row, object item, bool loadAllField, bool checkSucessCount)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (!(row.UserData is LatestLoadDataItemInfo))
            {
                LatestLoadDataItemInfo info = new LatestLoadDataItemInfo
                {
                    ColumnNames = row.GetColumnNames(true),
                    EntityDescription = GetItemDescription(item.GetType())
                };
                row.UserData = info;
            }
            return Internal_LoadItemValuesFormDbRow(row, item, loadAllField, checkSucessCount, null);
        }
        #endregion
    }
}
